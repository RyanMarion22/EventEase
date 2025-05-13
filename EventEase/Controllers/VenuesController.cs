using Azure.Storage.Blobs;
using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly EventEaseContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly ILogger<VenuesController> _logger;

        public VenuesController(EventEaseContext context, IConfiguration configuration, ILogger<VenuesController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;

            // Retrieve the connection string
            string? connectionString = configuration.GetConnectionString("AzureBlobStorage");
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Azure Blob Storage connection string 'AzureBlobStorage' not found in configuration.");
                throw new InvalidOperationException("Azure Blob Storage connection string 'AzureBlobStorage' not found in configuration.");
            }
            _blobServiceClient = new BlobServiceClient(connectionString);

            // Retrieve the container name
            _containerName = configuration.GetSection("AzureBlobStorage:ContainerName").Value
                ?? "images";
            if (string.IsNullOrEmpty(_containerName))
            {
                _logger.LogWarning("Container name is empty or null, defaulting to 'images'.");
                _containerName = "images";
            }

            // Ensure the container exists
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            try
            {
                containerClient.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create or access container '{ContainerName}'.", _containerName);
                throw;
            }
        }

        // GET: Venues
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venue.ToListAsync());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FirstOrDefaultAsync(m => m.VenueID == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueID,VenueName,Location,Capacity")] Venue venue, IFormFile venueImage)
        {
            if (ModelState.IsValid)
            {
                if (venueImage != null && venueImage.Length > 0)
                {
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                    if (containerClient == null)
                    {
                        _logger.LogError("Container client is null for container '{ContainerName}'.", _containerName);
                        return View(venue);
                    }

                    string blobName = $"{Guid.NewGuid()}_{venueImage.FileName}";
                    var blobClient = containerClient.GetBlobClient(blobName);

                    try
                    {
                        using (var stream = venueImage.OpenReadStream())
                        {
                            await blobClient.UploadAsync(stream, overwrite: true);
                        }

                        venue.VenueImage = blobClient.Uri.ToString();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upload image for venue '{VenueID}'.", venue.VenueID);
                        return View(venue);
                    }
                }

                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueID,VenueName,Location,Capacity,VenueImage")] Venue venue, IFormFile venueImage)
        {
            if (id != venue.VenueID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVenue = await _context.Venue.AsNoTracking().FirstOrDefaultAsync(v => v.VenueID == id);

                    if (venueImage != null && venueImage.Length > 0)
                    {
                        // Declare containerClient once
                        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                        if (containerClient == null)
                        {
                            _logger.LogError("Container client is null for container '{ContainerName}'.", _containerName);
                            return View(venue);
                        }

                        // Delete the old image if it exists
                        if (!string.IsNullOrEmpty(existingVenue?.VenueImage))
                        {
                            try
                            {
                                var oldBlobUri = new Uri(existingVenue.VenueImage);
                                var oldBlobName = Path.GetFileName(oldBlobUri.AbsolutePath);
                                var oldBlobClient = containerClient.GetBlobClient(oldBlobName);

                                // Delete the old image
                                await oldBlobClient.DeleteIfExistsAsync();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to delete old image for venue '{VenueID}'.", venue.VenueID);
                            }
                        }

                        // Upload the new image
                        string blobName = $"{Guid.NewGuid()}_{venueImage.FileName}";
                        var blobClient = containerClient.GetBlobClient(blobName);

                        try
                        {
                            using (var stream = venueImage.OpenReadStream())
                            {
                                await blobClient.UploadAsync(stream, overwrite: true);
                            }

                            venue.VenueImage = blobClient.Uri.ToString();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to upload new image for venue '{VenueID}'.", venue.VenueID);
                            return View(venue);
                        }
                    }
                    else
                    {
                        venue.VenueImage = existingVenue?.VenueImage;
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FirstOrDefaultAsync(m => m.VenueID == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }

            // Check for active bookings
            bool hasBookings = await _context.Booking.AnyAsync(b => b.VenueID == id);
            if (hasBookings)
            {
                TempData["Error"] = "Cannot delete venue because it has active bookings.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(venue.VenueImage))
            {
                try
                {
                    var blobUri = new Uri(venue.VenueImage);
                    var blobName = Path.GetFileName(blobUri.AbsolutePath);
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                    var blobClient = containerClient.GetBlobClient(blobName);

                    // Delete the blob
                    await blobClient.DeleteIfExistsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete image for venue '{VenueID}' during deletion.", venue.VenueID);
                }
            }

            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueID == id);
        }
    }
}
