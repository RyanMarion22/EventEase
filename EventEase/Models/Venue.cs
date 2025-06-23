using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        [Key]
        public int VenueID { get; set; }
        public string? VenueName { get; set; }
        public string? Location { get; set; }
        public int Capacity { get; set; }
        public string? VenueImage { get; set; }
        public List<Booking> Bookings { get; set; } = new();
    }
}