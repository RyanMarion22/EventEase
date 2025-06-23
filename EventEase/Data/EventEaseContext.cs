using Microsoft.EntityFrameworkCore;
using EventEase.Models;

namespace EventEase.Data
{
    public class EventEaseContext : DbContext
    {
        public EventEaseContext(DbContextOptions<EventEaseContext> options)
            : base(options)
        {
        }

        public DbSet<Booking> Booking { get; set; } = default!;
        public DbSet<Venue> Venue { get; set; } = default!;
        public DbSet<Event> Event { get; set; } = default!;

    }

}