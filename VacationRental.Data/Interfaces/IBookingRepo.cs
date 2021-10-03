using System.Threading.Tasks;
using System.Collections.Generic;
using VacationRental.Data.Models;

namespace VacationRental.Data.Interfaces
{
    public interface IBookingRepo
    {
        Task<BookingDataModel> GetById(int id);
        Task<IEnumerable<BookingDataModel>> GetByRentalId(int rentalId);
        Task<int> Add(BookingDataModel model);
    }
}