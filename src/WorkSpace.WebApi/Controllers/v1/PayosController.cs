using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Hubs;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services; // Thêm namespace cho IEmailService
using WorkSpace.Application.DTOs.Email; // Thêm namespace cho EmailRequest
using VNPAY.NET.Utilities;
using PayOS.Models.V2.PaymentRequests;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/payos")]
    [ApiController]
    public class PayosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly PayOSClient _payOS;
        private readonly IBookingRepository _bookingRepository;
        private readonly IBlockedTimeSlotRepository _blockedTimeSlotRepository;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly IEmailService _emailService; // Khai báo EmailService tương tự VnpayController

        private const int PayOSPaymentMethodID = 3;
        private const int ConfirmedStatusId = 3;

        public PayosController(
          IConfiguration configuration,
          PayOSClient payOS,
          IBookingRepository bookingRepository,
          IBlockedTimeSlotRepository blockedTimeSlotRepository,
          IHubContext<OrderHub> hubContext,
          IEmailService emailService) // Inject EmailService qua Constructor
        {
            _configuration = configuration;
            _payOS = payOS;
            _bookingRepository = bookingRepository;
            _blockedTimeSlotRepository = blockedTimeSlotRepository;
            _hubContext = hubContext;
            _emailService = emailService;
        }

        [HttpGet("create-payment-url")]
        public async Task<ActionResult<object>> CreatePaymentUrl(int bookingId)
        {
            try
            {
                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);

                if (booking == null)
                    return NotFound("Booking not found");

                var payOSCallbackUrl = $"{_configuration["Vnpay:ReturnUrl"]!.Replace("vnpay/callback", "payos/callback")}";
                string clientReturnUrl = _configuration["Vnpay:ClientReturnUrl"] ?? "http://localhost:3000";

                long orderCode = DateTimeOffset.Now.ToUnixTimeSeconds();
                string orderCodeString = orderCode.ToString();

                booking.PaymentTransactionId = orderCodeString;
                await _bookingRepository.UpdateBookingAsync(booking.Id, booking);

                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = (int)booking.FinalAmount,
                    Description = $"Booking-{bookingId}",
                    ReturnUrl = payOSCallbackUrl,
                    CancelUrl = $"{clientReturnUrl}/payment-result/failed?bookingCode={booking.BookingCode}"
                };

                var paymentResult = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

                var response = new
                {
                    url = paymentResult.CheckoutUrl,
                };
                return Created(paymentResult.CheckoutUrl, response);
            }
            catch (Exception ex)
            {
                return BadRequest($"PayOS Error: {ex.Message}");
            }
        }

        [HttpGet("callback")]
        public async Task<IActionResult> PayOSCallback()
        {
            var query = Request.Query;

            try
            {
                if (!query.TryGetValue("code", out var code) || code.ToString() != "00")
                {
                    return RedirectToClientResult(null, false, "Thanh toán PayOS không thành công");
                }

                if (!query.TryGetValue("orderCode", out var orderCodeString) ||
                  !long.TryParse(orderCodeString, out long orderCode))
                {
                    return RedirectToClientResult(null, false, "Thông tin đơn hàng PayOS không hợp lệ");
                }

                int bookingId = await ExtractBookingIdFromOrderCode(orderCode);

                if (bookingId == 0)
                    return RedirectToClientResult(null, false, "Booking ID không tìm thấy trong OrderCode");

                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
                if (booking == null)
                    return RedirectToClientResult(null, false, "Booking không tồn tại");

                if (booking.BookingStatusId != ConfirmedStatusId)
                {
                    booking.BookingStatusId = ConfirmedStatusId;
                    booking.PaymentMethodID = PayOSPaymentMethodID;
                    booking.PaymentTransactionId = orderCode.ToString(); // Lưu mã giao dịch PayOS

                    await _bookingRepository.UpdateBookingAsync(booking.Id, booking);

                    // Chặn lịch phòng sau khi thanh toán thành công
                    await _blockedTimeSlotRepository.CreateBlockedTimeForBookingAsync(
                      booking.WorkSpaceRoomId,
                      bookingId,
                      booking.StartTimeUtc,
                      booking.EndTimeUtc);

                    // Thông báo SignalR
                    await _hubContext.Clients.Group("Staff")
                      .SendAsync("New Booking", $"Booking #{bookingId} đã thanh toán thành công qua PayOS.");

                    // --- BẮT ĐẦU LOGIC GỬI EMAIL (Format Blue Theme tương tự VNPAY) ---
                    try
                    {
                        var recipientEmail = booking.Customer?.Email ?? booking.Guest?.Email;
                        var displayName = booking.Customer != null
                            ? $"{booking.Customer.FirstName} {booking.Customer.LastName}"
                            : "Quý khách";

                        if (!string.IsNullOrEmpty(recipientEmail))
                        {
                            var emailRequest = new EmailRequest
                            {
                                To = recipientEmail,
                                Subject = $"[WorkSpace] Xác nhận thanh toán thành công - Mã đơn: {booking.BookingCode}",
                                Body = $@"
                        <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 20px auto; border: 1px solid #e0e0e0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05);'>
                            <div style='background: linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%); color: white; padding: 30px; text-align: center;'>
                                <h1 style='margin: 0; font-size: 28px; letter-spacing: 1px;'>Thanh toán thành công!</h1>
                                <p style='margin-top: 10px; opacity: 0.9;'>Cảm ơn bạn đã lựa chọn WorkSpace</p>
                            </div>
                            <div style='padding: 30px; background-color: #ffffff;'>
                                <p style='font-size: 16px;'>Xin chào <strong>{displayName}</strong>,</p>
                                <p>Yêu cầu đặt phòng của bạn đã được hệ thống xác nhận thanh toán qua cổng <strong>PayOS</strong>. Dưới đây là thông tin chi tiết:</p>
                                
                                <div style='background-color: #f0f7ff; border-radius: 8px; padding: 20px; margin: 25px 0; border: 1px solid #d1e9ff;'>
                                    <table style='width: 100%; border-collapse: collapse;'>
                                        <tr>
                                            <td style='padding: 8px 0; color: #64748b; width: 40%;'>Mã đặt phòng:</td>
                                            <td style='padding: 8px 0; font-weight: bold; color: #1e3a8a;'>{booking.BookingCode}</td>
                                        </tr>
                                        <tr>
                                            <td style='padding: 8px 0; color: #64748b;'>Thời gian bắt đầu:</td>
                                            <td style='padding: 8px 0; font-weight: 500;'>{booking.StartTimeUtc.ToLocalTime():dd/MM/yyyy HH:mm}</td>
                                        </tr>
                                        <tr>
                                            <td style='padding: 8px 0; color: #64748b;'>Thời gian kết thúc:</td>
                                            <td style='padding: 8px 0; font-weight: 500;'>{booking.EndTimeUtc.ToLocalTime():dd/MM/yyyy HH:mm}</td>
                                        </tr>
                                        <tr>
                                            <td style='padding: 8px 0; color: #64748b;'>Tổng số tiền:</td>
                                            <td style='padding: 8px 0; color: #2563eb; font-weight: bold; font-size: 20px;'>{booking.FinalAmount:N0} VND</td>
                                        </tr>
                                        <tr>
                                            <td style='padding: 8px 0; color: #64748b;'>Mã giao dịch (PayOS):</td>
                                            <td style='padding: 8px 0; font-size: 13px; color: #94a3b8;'>{orderCode}</td>
                                        </tr>
                                    </table>
                                </div>

                                <div style='border-top: 1px solid #eee; padding-top: 20px; margin-top: 20px;'>
                                    <p style='margin: 0; color: #475569;'>Mọi thắc mắc vui lòng liên hệ hỗ trợ:</p>
                                    <p style='margin: 5px 0; font-size: 18px; font-weight: bold; color: #1e3a8a;'>Hotline: 0357027134</p>
                                </div>
                                
                                <p style='margin-top: 25px; color: #64748b; font-style: italic; font-size: 13px;'>Lưu ý: Quý khách vui lòng cung cấp mã đặt phòng khi đến nhận phòng tại cơ sở.</p>
                            </div>
                            <div style='background-color: #f8fafc; padding: 20px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0;'>
                                <p style='margin: 0;'>Đây là email hệ thống tự động, vui lòng không trả lời.</p>
                                <p style='margin: 8px 0; font-weight: bold;'>© 2025 WorkSpace Platform</p>
                            </div>
                        </div>",
                                From = _configuration["MailSettings:EmailFrom"] ?? "noreply@workspace.com"
                            };

                            await _emailService.SendAsync(emailRequest);
                        }
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($"[PayOS Email Error]: {emailEx.Message}");
                    }
                    // --- KẾT THÚC LOGIC GỬI EMAIL ---
                }

                return RedirectToClientResult(booking.BookingCode, true, "Thanh toán thành công qua PayOS");
            }
            catch (Exception ex)
            {
                return RedirectToClientResult(null, false, $"Lỗi PayOS: {ex.Message}");
            }
        }

        private async Task<int> ExtractBookingIdFromOrderCode(long orderCode)
        {
            var orderCodeString = orderCode.ToString();
            var booking = await _bookingRepository.GetBookingByTransactionIdAsync(orderCodeString);

            if (booking != null)
            {
                return booking.Id;
            }
            return 0;
        }

        private IActionResult RedirectToClientResult(string? bookingCode, bool isSuccess, string message)
        {
            string clientReturnUrl = _configuration["Vnpay:ClientReturnUrl"] ?? "http://localhost:3000";

            if (isSuccess && !string.IsNullOrEmpty(bookingCode))
            {
                string redirectUrl = $"{clientReturnUrl}/payment-result/success?bookingCode={bookingCode}";
                return Redirect(redirectUrl);
            }
            else
            {
                var url = $"{clientReturnUrl}/payment-result/failed?message={Uri.EscapeDataString(message)}";
                return Redirect(url);
            }
        }
    }
}