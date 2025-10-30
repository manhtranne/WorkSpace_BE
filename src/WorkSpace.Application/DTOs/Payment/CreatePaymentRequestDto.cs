namespace WorkSpace.Application.DTOs.Payment;

public class CreatePaymentRequestDto
{
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "VNPay";
    public string? OrderInfo { get; set; }
}

