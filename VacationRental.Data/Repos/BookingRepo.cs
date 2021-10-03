using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VacationRental.Data.Interfaces;
using VacationRental.Data.Models;

namespace VacationRental.Data.Repos
{
    public class BookingRepo : IBookingRepo
    {
        private readonly IDictionary<int, BookingDataModel> _bookings;
        public BookingRepo(IDictionary<int, BookingDataModel> bookings)
        {
            _bookings = bookings;
        }

        public async Task<BookingDataModel> GetById(int id)
        {
            return await Task.Run(() =>
            {
                if (id <= _bookings.Keys.Count) return _bookings[id];
                return null;
            });
        }

        public async Task<IEnumerable<BookingDataModel>> GetByRentalId(int rentalId)
        {
            return await Task.Run(() =>
            {
                 return _bookings
                    .Where(b => b.Value.RentalId == rentalId)
                    .Select(b => b.Value);
            });
        }

        public async Task<int> Add(BookingDataModel model)
        {
            try
            {
                return await Task.Run(() =>
                {
                    model.Id = _bookings.Keys.Count + 1;
                    _bookings.Add(model.Id, model);
                    return model.Id;
                });
            }
            catch (Exception e)
            {
                return -1;
            }
        }
    }
}
