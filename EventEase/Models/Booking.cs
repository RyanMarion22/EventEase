using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }
        public DateTime BookingDate { get; set; }
        public int VenueID { get; set; }
        public Venue? Venue { get; set; }
        public int EventID { get; set; }
        public Event? Event { get; set; }
    }
}
