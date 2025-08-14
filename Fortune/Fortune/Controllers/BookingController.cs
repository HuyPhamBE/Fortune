using Fortune.DTOs;
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
        public async Task<IActionResult> CreateBooking([FromBody] BookingDTO booking)
        {
            if (booking == null)
            {
                return BadRequest("Booking cannot be null.");
            }
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            // Map BookingDTO to Booking model
            var bookingModel = new Booking
            {
                booking_id = Guid.NewGuid(),
                description = booking.description,
                type = booking.type,
                user_id = userId,
                status = booking.status,
                staff_id = booking.staff_id,
                minigame_id = booking.minigame_id,
                plan_id = booking.plan_id,
            };
            var result = await bookingService.CreateBookingAsync(bookingModel);
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetBookingById), new { id = bookingModel.booking_id }, booking);
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
