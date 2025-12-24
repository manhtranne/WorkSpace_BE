using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;
using WorkSpace.Application.DTOs.Email;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Hubs;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/vnpay")]
    [ApiController]
    public class VnpayController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IVnpay _vnpay;
        private readonly IBookingRepository _bookingRepository;
        private readonly IBlockedTimeSlotRepository _blockedTimeSlotRepository;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly IEmailService _emailService;
        public VnpayController(
            IConfiguration configuration,
            IVnpay vnpay,
            IBookingRepository bookingRepository,
            IBlockedTimeSlotRepository blockedTimeSlotRepository,
            IHubContext<OrderHub> hubContext,
            IEmailService emailService)
        {
            _configuration = configuration;
            _vnpay = vnpay;
            _bookingRepository = bookingRepository;
            _blockedTimeSlotRepository = blockedTimeSlotRepository;
            _hubContext = hubContext;
            _emailService = emailService;

            _vnpay.Initialize(
                _configuration["Vnpay:TmnCode"],
                _configuration["Vnpay:HashSecret"],
                _configuration["Vnpay:BaseUrl"],
                _configuration["Vnpay:ReturnUrl"]
            );
        }

        [HttpGet("create-payment-url")]
        public async Task<ActionResult<object>> CreatePaymentUrl(int bookingId)
        {
            try
            {
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext);
                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);

                if (booking == null)
                    return NotFound("Booking not found");

                var request = new PaymentRequest
                {
                    PaymentId = DateTime.Now.Ticks,
                    Money = (double)booking.FinalAmount,
                    Description = $"Booking-{bookingId}",
                    IpAddress = ipAddress,
                    BankCode = BankCode.ANY,
                    CreatedDate = DateTime.Now,
                    Currency = Currency.VND,
                    Language = DisplayLanguage.Vietnamese
                };

                var paymentUrl = _vnpay.GetPaymentUrl(request);

                var response = new
                {
                    url = paymentUrl
                };
                return Created(paymentUrl, response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("callback")]
        public async Task<IActionResult> VNPayCallback()
        {
            var query = Request.Query;

            try
            {
                var result = _vnpay.GetPaymentResult(query);
                var orderInfo = result.Description ?? string.Empty;
                int bookingId = ExtractBookingIdFromOrderInfo(orderInfo);

                if (bookingId == 0)
                    return RedirectWithError("Booking ID trong thông tin đơn hàng không hợp lệ");

                // Lấy thông tin Booking kèm theo thông tin Customer/Guest
                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
                if (booking == null)
                    return RedirectWithError("Booking không tồn tại");

                if (result.IsSuccess)
                {
                    // Trạng thái 3: Đã thanh toán (Confirmed/Paid)
                    if (booking.BookingStatusId != 3)
                    {
                        var confirmedStatusId = 3;
                        var vnpayMethodId = 2;

                        booking.BookingStatusId = confirmedStatusId;
                        booking.PaymentMethodID = vnpayMethodId;
                        booking.PaymentTransactionId = query["vnp_TransactionNo"];

                        await _bookingRepository.UpdateBookingAsync(booking.Id, booking);

                        // Chặn lịch phòng
                        await _blockedTimeSlotRepository.CreateBlockedTimeForBookingAsync(
                            booking.WorkSpaceRoomId,
                            bookingId,
                            booking.StartTimeUtc,
                            booking.EndTimeUtc);

                        // Thông báo SignalR
                        await _hubContext.Clients.Group("Staff")
                            .SendAsync("New Booking", $"Booking #{bookingId} đã thanh toán thành công.");

                        // Gửi Email xác nhận với giao diện Blue Theme
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
                                    <p>Yêu cầu đặt phòng của bạn đã được hệ thống xác nhận thanh toán qua cổng <strong>VNPay</strong>. Dưới đây là thông tin chi tiết:</p>
                                    
                                    <div style='background-color: #f0f7ff; border-radius: 8px; padding: 20px; margin: 25px 0; border: 1px solid #d1e9ff;'>
                                        <table style='width: 100%; border-collapse: collapse;'>
                                            <tr>
                                                <td style='padding: 8px 0; color: #64748b; width: 40%;'>Mã đặt phòng:</td>
                                                <td style='padding: 8px 0; font-weight: bold; color: #1e3a8a;'>{booking.BookingCode}</td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 8px 0; color: #64748b;'>Thời gian bắt đầu:</td>
                                                <td style='padding: 8px 0; font-weight: 500;'>{booking.StartTimeUtc:dd/MM/yyyy HH:mm}</td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 8px 0; color: #64748b;'>Thời gian kết thúc:</td>
                                                <td style='padding: 8px 0; font-weight: 500;'>{booking.EndTimeUtc:dd/MM/yyyy HH:mm}</td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 8px 0; color: #64748b;'>Tổng số tiền:</td>
                                                <td style='padding: 8px 0; color: #2563eb; font-weight: bold; font-size: 20px;'>{booking.FinalAmount:N0} VND</td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 8px 0; color: #64748b;'>Mã giao dịch:</td>
                                                <td style='padding: 8px 0; font-size: 13px; color: #94a3b8;'>{query["vnp_TransactionNo"]}</td>
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
                            Console.WriteLine($"[Email Error]: {emailEx.Message}");
                        }
                    }
                }
                else
                {
                    if (booking.BookingStatusId != 10 && booking.BookingStatusId != 3)
                    {
                        booking.BookingStatusId = 10;
                        await _bookingRepository.UpdateBookingAsync(booking.Id, booking);
                    }
                }

                string clientReturnUrl = _configuration["Vnpay:ClientReturnUrl"] ?? "http://localhost:3000";
                string redirectUrl = $"{clientReturnUrl}/payment-result/{(result.IsSuccess ? "success" : "failed")}?bookingCode={booking.BookingCode}";

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return Redirect($"{_configuration["Vnpay:ClientReturnUrl"]}?status=error&message={Uri.EscapeDataString(ex.Message)}");
            }
        }

        [HttpGet("ipn-action")]
        public IActionResult IpnAction()
        {
            if (!Request.QueryString.HasValue)
                return NotFound("No payment information found");

            try
            {
                var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                if (paymentResult.IsSuccess)
                {
                    return Ok();
                }

                return BadRequest("Payment failed");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int ExtractBookingIdFromOrderInfo(string orderInfo)
        {
            if (string.IsNullOrEmpty(orderInfo)) return 0;

            var match = Regex.Match(orderInfo, @"-(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int bookingId))
            {
                return bookingId;
            }
            return 0;
        }

        private IActionResult RedirectWithError(string message)
        {
            var clientReturnUrl = _configuration["Vnpay:ClientReturnUrl"] ?? "http://localhost:3000";
            var url = $"{clientReturnUrl}/payment/failed?message={Uri.EscapeDataString(message)}";
            return RedirectPermanent(url);
        }
    }
}