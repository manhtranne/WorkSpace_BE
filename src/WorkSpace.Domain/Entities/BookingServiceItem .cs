using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities
{
    public class BookingServiceItem : BaseEntity
    {
        public int BookingId { get; set; }
        public int ServiceId { get; set; }

        public int Quantity { get; set; }


        public decimal UnitPrice { get; set; }

        public virtual Booking? Booking { get; set; }
        public virtual WorkSpaceService? Service { get; set; }
    }
}