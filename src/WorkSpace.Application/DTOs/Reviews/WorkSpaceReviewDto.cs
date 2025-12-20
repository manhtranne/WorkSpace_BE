using System;

namespace WorkSpace.Application.DTOs.Reviews
{
    public class WorkSpaceReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

        public int UserId { get; set; }
        public string ReviewerName { get; set; }
        public string? ReviewerAvatar { get; set; }

        public int RoomId { get; set; }
        public string RoomName { get; set; } 
        public string RoomType { get; set; }
    }
}