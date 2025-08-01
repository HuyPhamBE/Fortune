using Fortune.Repository.Basic;
using Fortune.Repository.DBContext;
using Fortune.Repository.ModelExtension;
using Fortune.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fortune.Repository
{
    public class BookingRepository : GenericRepository<Booking>
    {
        public BookingRepository(FortuneContext context) : base(context)
        {
        }
        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings.ToListAsync();
        }
        public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
        {
            return await _context.Bookings
                .Include(b => b.user)
                .Include(b => b.Mini_games)
                .Include(b => b.Plans)
                .FirstOrDefaultAsync(b => b.booking_id == bookingId);
        }
        public async Task<List<Booking>> GetBookingsByUserIdAsync(Guid userId)
        {
            return await _context.Bookings
                .Where(b => b.user_id == userId)
                .Include(b => b.user)
                .Include(b => b.Mini_games)
                .Include(b => b.Plans)
                .ToListAsync();
        }
        public async Task<List<Booking>> GetBookingsByStaffIdAsync(Guid staffId)
        {
            return await _context.Bookings
                .Where(b => b.staff_id == staffId)
                .Include(b => b.user)
                .Include(b => b.Mini_games)
                .Include(b => b.Plans)
                .ToListAsync();
        }
       public async Task<PaginationResult<Booking>> SearchWithPagging(
            long status,
            bool type,
            int page,
            int pageSize)
        {
            var query = _context.Bookings.AsQueryable();
            if (status != 0)
            {
                query = query.Where(b => b.status == status);
            }
            if (type)
            {
                query = query.Where(b => b.type == type);
            }
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var bookings = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PaginationResult<Booking>
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                Items= bookings
            };
        }
    }
}
