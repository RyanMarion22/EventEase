using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        [Display(Name = "Event Name")]
        public string? EventName { get; set; }

        [Required]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Display(Name = "Description")]
        public string? EventDescription { get; set; }

        [Display(Name = "Event Type")]
        public string? EventType { get; set; }

        public List<Booking> Bookings { get; set; } = new();
    }
}