using System.Threading.Tasks;
using VacationRental.Logic.Models.BindingModels;
using VacationRental.Logic.Models.ServiceModels;

namespace VacationRental.Logic.Interfaces
{
    public interface IBookingService
    {
        Task<BookingServiceModel> GetById(int bookingId);
        Task<ResourceIdServiceModel> AddBooking(BookingBindingModel model);
    }
}