using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventEaseContext _context;

        public EventsController(EventEaseContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            return View(await _context.Event.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventEntity = await _context.Event
                .FirstOrDefaultAsync(m => m.EventID == id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            return View(eventEntity);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventID,EventName,EventDate,EventDescription")] Event eventEntity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eventEntity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eventEntity);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventEntity = await _context.Event.FindAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }
            return View(eventEntity);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventID,EventName,EventDate,EventDescription")] Event eventEntity)
        {
            if (id != eventEntity.EventID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventEntity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventEntity.EventID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(eventEntity);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventEntity = await _context.Event
                .FirstOrDefaultAsync(m => m.EventID == id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            return View(eventEntity);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventEntity = await _context.Event.FindAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            // Check for active bookings
            bool hasBookings = await _context.Booking.AnyAsync(b => b.EventID == id);
            if (hasBookings)
            {
                TempData["Error"] = "Cannot delete event because it has active bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Event.Remove(eventEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.EventID == id);
        }
    }
}