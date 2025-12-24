using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Email;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Application.Features.SupportTickets.Commands
{
    public class UpdateTicketStatusCommand : IRequest<Response<bool>>
    {
        public int TicketId { get; set; }
        public SupportTicketStatus NewStatus { get; set; }
        public int StaffUserId { get; set; }
    }

    public class UpdateTicketStatusCommandHandler : IRequestHandler<UpdateTicketStatusCommand, Response<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTimeService _dateTimeService;
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepository;

        public UpdateTicketStatusCommandHandler(
            IApplicationDbContext context,
            IDateTimeService dateTimeService,
            IEmailService emailService,
            IUserRepository userRepository)
        {
            _context = context;
            _dateTimeService = dateTimeService;
            _emailService = emailService;
            _userRepository = userRepository;
        }

        public async Task<Response<bool>> Handle(UpdateTicketStatusCommand request, CancellationToken cancellationToken)
        {
            // 1. Tìm Ticket trong cơ sở dữ liệu
            var ticket = await _context.SupportTickets
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

            if (ticket == null)
            {
                throw new ApiException($"Support Ticket with ID {request.TicketId} not found.");
            }

            // Lưu lại trạng thái cũ trước khi cập nhật
            var oldStatus = ticket.Status;

            // 2. Cập nhật thông tin Ticket
            ticket.Status = request.NewStatus;
            ticket.LastModifiedUtc = _dateTimeService.NowUtc;

            // Tự động gán nhân viên nếu ticket bắt đầu được xử lý
            if (request.NewStatus == SupportTicketStatus.InProgress && ticket.AssignedToStaffId == null)
            {
                ticket.AssignedToStaffId = request.StaffUserId;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // 3. Logic gửi email: Chỉ gửi khi trạng thái chuyển từ khác sang "Closed"
            if (request.NewStatus == SupportTicketStatus.Closed && oldStatus != SupportTicketStatus.Closed)
            {
                await SendClosureEmailAsync(ticket.SubmittedByUserId, ticket.Subject, ticket.Id);
            }

            return new Response<bool>(true, $"Ticket status updated to {request.NewStatus}.");
        }

        private async Task SendClosureEmailAsync(int userId, string subject, int ticketId)
        {
            // Lấy thông tin chi tiết người dùng
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                var emailRequest = new EmailRequest
                {
                    To = user.Email,
                    Subject = $"✅ [WorkSpace] Ticket #{ticketId} của bạn đã được xử lý xong",
                    Body = $@"
                        <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eee; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05);'>
                            
                            <div style='background-color: #FF8C00; padding: 30px 20px; text-align: center; color: white;'>
                                <h1 style='margin: 0; font-size: 28px; letter-spacing: 1px;'>CSB Support Team</h1>
                                <p style='margin: 5px 0 0; opacity: 0.9;'>Yêu cầu của bạn đã được giải quyết</p>
                            </div>

                            <div style='padding: 30px; color: #444; background-color: #ffffff;'>
                                <h2 style='color: #FF8C00; margin-top: 0;'>Chào {user.GetFullName()},</h2>
                                <p style='font-size: 16px; line-height: 1.6;'>Chúng tôi xin thông báo rằng yêu cầu hỗ trợ về vấn đề <b>""{subject}""</b> đã được đội ngũ nhân viên xử lý hoàn tất.</p>
                                
                                <div style='background-color: #fff9f2; border-left: 5px solid #FF8C00; padding: 20px; margin: 25px 0; border-radius: 4px;'>
                                    <table style='width: 100%; border-collapse: collapse;'>
                                        <tr>
                                            <td style='padding: 5px 0; color: #777; width: 120px;'>Mã Ticket:</td>
                                            <td style='padding: 5px 0; font-weight: bold; color: #333;'>#{ticketId}</td>
                                        </tr>
                                        <tr>
                                            <td style='padding: 5px 0; color: #777;'>Trạng thái:</td>
                                            <td style='padding: 5px 0;'>
                                                <span style='background-color: #FF8C00; color: white; padding: 2px 10px; border-radius: 12px; font-size: 12px; font-weight: bold;'>ĐÃ ĐÓNG (CLOSED)</span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style='padding: 5px 0; color: #777;'>Thời gian:</td>
                                            <td style='padding: 5px 0; color: #333;'>{DateTime.Now:dd/MM/yyyy HH:mm}</td>
                                        </tr>
                                    </table>
                                </div>

                                <p style='font-size: 15px; line-height: 1.6;'>Cảm ơn bạn đã tin tưởng dịch vụ của chúng tôi. Nếu bạn vẫn gặp khó khăn, vui lòng tạo một ticket mới trên ứng dụng.</p>
                                
                       
                            </div>

                            <div style='background-color: #f9f9f9; padding: 20px; text-align: center; font-size: 12px; color: #999; border-top: 1px solid #eee;'>
                                <p style='margin: 0;'>Đây là email tự động từ hệ thống, vui lòng không phản hồi trực tiếp.</p>
                                <p style='margin: 5px 0;'>© {DateTime.Now.Year} WorkSpace Team. All rights reserved.</p>
                            </div>
                        </div>"
                };

                // Thực hiện gửi email qua service
                await _emailService.SendAsync(emailRequest);
            }
        }
    }
}