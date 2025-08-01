using Fortune.Repository;
using Fortune.Repository.ModelExtension;
using Fortune.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fortune.Services
{
    public interface IBookingService
    {
        Task<List<Booking>> GetAllBookingsAsync();
        Task<Booking> GetBookingByIdAsync(Guid bookingId);
        Task<int> CreateBookingAsync(Booking booking);
        Task<int> UpdateBookingAsync(Booking booking);
        Task<bool> DeleteBookingAsync(Guid bookingId);
        Task<PaginationResult<Booking>> SearchWithPagging(long status, bool type, int page, int pageSize);
    }
    public class BookingService: IBookingService
    {
        private readonly BookingRepository bookingRepository;

        public BookingService(BookingRepository bookingRepository)
        {
            this.bookingRepository = bookingRepository;
        }
       public async Task<List<Booking>> GetAllBookingsAsync()
        {
            return await bookingRepository.GetAllBookingsAsync();
        }
        public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
        {
            return await bookingRepository.GetBookingByIdAsync(bookingId);
        }
        public async Task<int> CreateBookingAsync(Booking booking)
        {
            return await bookingRepository.CreateAsync(booking);
        }
        public async Task<int> UpdateBookingAsync(Booking booking)
        {
            return await bookingRepository.UpdateAsync(booking);
        }
        public async Task<bool> DeleteBookingAsync(Guid bookingId)
        {
            var existingBooking = await bookingRepository.GetBookingByIdAsync(bookingId);
            return await bookingRepository.RemoveAsync(existingBooking);
        }
        public async Task<PaginationResult<Booking>> SearchWithPagging(
            long status,
            bool type,
            int page,
            int pageSize)
        {
            var result = await bookingRepository.SearchWithPagging(status, type, page, pageSize);
            return result ?? new PaginationResult<Booking>
            {
                TotalItems = 0,
                TotalPages = 0,
                PageSize = pageSize,
                CurrentPage = page,
                Items = new List<Booking>()
            };
        }
    }
}
