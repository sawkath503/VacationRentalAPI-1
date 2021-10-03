using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using VacationRental.Data.Interfaces;
using VacationRental.Data.Models;
using VacationRental.Logic.Models.BindingModels;
using VacationRental.Logic.Interfaces;
using VacationRental.Logic.Utilities;
using VacationRental.Logic.Models.ServiceModels;

namespace VacationRental.Logic.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepo _bookingRepo;
        private readonly IRentalRepo _rentalRepo;

        public BookingService(IRentalRepo rentalRepository, IBookingRepo bookingRepository)
        {
            _bookingRepo = bookingRepository;
            _rentalRepo = rentalRepository;
        }

        public async Task<BookingServiceModel> GetById(int bookingId)
        {
            var bookingData = await _bookingRepo.GetById(bookingId).ConfigureAwait(false);

            if (bookingData == null)
                return ConvertToErrorModel<BookingServiceModel>.GetErrorModel("Booking not found. ");

            return TransformModels.GetServiceModel(bookingData);
        }

        public async Task<ResourceIdServiceModel> AddBooking(BookingBindingModel model)
        {
            var key = new ResourceIdServiceModel();
            
            var rental = await _rentalRepo.GetById(model.RentalId).ConfigureAwait(false);
            if (rental == null)
                return ConvertToErrorModel<ResourceIdServiceModel>.GetErrorModel("Rental not found. ");
            
            var bookings = await _bookingRepo.GetByRentalId(model.RentalId).ConfigureAwait(false);
            var hasConflicts = HasConflict(bookings, model, rental);

            if (!hasConflicts)
            {
                key.Id = await _bookingRepo.Add(TransformModels.GetDataModel(model)).ConfigureAwait(false);

                if (key.Id <= 0)
                    return ConvertToErrorModel<ResourceIdServiceModel>.GetErrorModel("database operation failed. ");
            }
            else
            {
                return ConvertToErrorModel<ResourceIdServiceModel>.GetErrorModel("Not available for selected date range. ");
            }

            return key;
        }

        private static bool HasConflict(IEnumerable<BookingDataModel> bookings, BookingBindingModel requestedBooking, RentalDataModel rental)
        {
            if (bookings.Any())
            {
                var bookingsByDateRange = bookings.Select(b => 
                    new { StartDt = b.StartDate, EndDt = b.StartDate.AddDays(b.NumberOfNights + rental.PreparationTimeInDays) }).ToList();

                var dateToProcess = requestedBooking.Start.Date;                
                var lastDt = requestedBooking.Start.Date.AddDays(requestedBooking.Nights + rental.PreparationTimeInDays); 

                // Go through each date. If number of booking on any day equals/exceeds number of unit, there is conflict
                while (dateToProcess <= lastDt)
                {
                    if (bookingsByDateRange.Where(b => dateToProcess >= b.StartDt && dateToProcess < b.EndDt).Count() >= rental.NumberOfUnits)
                        return true;
                    dateToProcess = dateToProcess.AddDays(1);
                }
            }

            return false;
        }
    }
}
