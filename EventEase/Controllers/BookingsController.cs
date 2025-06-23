using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly EventEaseContext _context;

        public BookingsController(EventEaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: Bookings
        public async Task<IActionResult> Index(string? searchEventName = null, string? eventType = null,
            DateTime? startDate = null, DateTime? endDate = null, bool? onlyAvailable = null)
        {
            var bookings = _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .Where(b => b.Event != null && b.Venue != null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchEventName))
            {
                bookings = bookings.Where(b => b.Event.EventName != null && b.Event.EventName.Contains(searchEventName));
            }

            if (!string.IsNullOrEmpty(eventType))
            {
                bookings = bookings.Where(b => b.Event.EventType == eventType);
            }

            // Use Booking.BookingDate for date range filter
            if (startDate.HasValue && endDate.HasValue)
            {
                bookings = bookings.Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate);
            }

            if (onlyAvailable == true)
            {
                bookings = bookings.Where(b =>
                    !_context.Booking.Any(other => other.VenueID == b.VenueID &&
                                                  other.BookingDate.Date == b.BookingDate.Date &&
                                                  other.BookingID != b.BookingID)
                );
            }

            ViewData["EventType"] = new SelectList(_context.Event.Select(e => e.EventType).Distinct());
            ViewData["CurrentFilter"] = searchEventName;
            ViewData["CurrentEventType"] = eventType;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentOnlyAvailable"] = onlyAvailable;

            return View(await bookings.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);

            if (booking == null || booking.Event == null || booking.Venue == null)
                return NotFound();

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewBag.VenueID = new SelectList(_context.Venue, "VenueID", "VenueName");
            ViewBag.EventID = new SelectList(_context.Event, "EventID", "EventName");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingID,BookingDate,VenueID,EventID")] Booking booking)
        {
            if (await _context.Booking.AnyAsync(b =>
                b.VenueID == booking.VenueID &&
                b.BookingDate.Date == booking.BookingDate.Date))
            {
                ModelState.AddModelError("", "This venue is already booked on that date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.VenueID = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);
            ViewBag.EventID = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
                return NotFound();

            ViewBag.VenueID = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);
            ViewBag.EventID = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingID,BookingDate,VenueID,EventID")] Booking booking)
        {
            if (id != booking.BookingID)
                return NotFound();

            if (await _context.Booking.AnyAsync(b =>
                b.VenueID == booking.VenueID &&
                b.BookingDate.Date == booking.BookingDate.Date &&
                b.BookingID != booking.BookingID))
            {
                ModelState.AddModelError("", "This venue is already booked on that date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await BookingExists(booking.BookingID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.VenueID = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);
            ViewBag.EventID = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> BookingExists(int id)
        {
            return await _context.Booking.AnyAsync(e => e.BookingID == id);
        }

        // Search by Booking ID
        public async Task<IActionResult> SearchByBookingId(string? searchString = null)
        {
            var bookings = _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .Where(b => b.Event != null && b.Venue != null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                if (int.TryParse(searchString, out int bookingId))
                {
                    bookings = bookings.Where(b => b.BookingID == bookingId);
                }
                else
                {
                    bookings = bookings.Where(b => b.BookingID.ToString().Contains(searchString));
                }
            }

            ViewData["CurrentFilter"] = searchString;
            return View("Index", await bookings.ToListAsync());
        }
    }
}