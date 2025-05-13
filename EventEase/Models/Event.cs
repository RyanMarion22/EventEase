using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        public string? EventName { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public string? EventDescription { get; set; }

        // Navigation property to Bookings
        public List<Booking> Bookings { get; set; } = new();
    }
}
