public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int SenderId { get; set; }
    public string SenderRole { get; set; }
    public DateTimeOffset CreateUtc { get; set; }
}
