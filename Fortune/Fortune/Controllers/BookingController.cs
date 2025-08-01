using Fortune.Repository.Models;
using Fortune.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fortune.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class BookingController : ControllerBase
    {
        private readonly IBookingService bookingService;

        public BookingController(IBookingService bookingService)
        {
            this.bookingService = bookingService;
        }
        [HttpGet]
        [Authorize(Roles = "3,2")]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await bookingService.GetAllBookingsAsync();
            return Ok(bookings);
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "3,2")]
        public async Task<IActionResult> GetBookingById(Guid id)
        {
            var booking = await bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return Ok(booking);
        }   
        [HttpPost]
        [Authorize(Roles = "3,2,1")]
        public async Task<IActionResult> CreateBooking([FromBody] Booking booking)
        {
            if (booking == null)
            {
                return BadRequest("Booking cannot be null.");
            }
            var result = await bookingService.CreateBookingAsync(booking);
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetBookingById), new { id = booking.booking_id }, booking);
            }
            return BadRequest("Failed to create booking.");
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "3,2,1")]
        public async Task<IActionResult> UpdateBooking(Guid id, [FromBody] Booking booking)
        {
            if (booking == null || booking.booking_id != id)
            {
                return BadRequest("Invalid booking data.");
            }
            var existingBooking = await bookingService.GetBookingByIdAsync(id);
            if (existingBooking == null)
            {
                return NotFound();
            }
            var result = await bookingService.UpdateBookingAsync(booking);
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest("Failed to update booking.");
        }
    }
}
