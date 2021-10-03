using System.Linq;
using System.Threading.Tasks;
using VacationRental.Data.Interfaces;
using VacationRental.Data.Models;
using VacationRental.Logic.Interfaces;
using VacationRental.Logic.Models.BindingModels;
using System.Collections.Generic;
using VacationRental.Logic.Utilities;
using VacationRental.Logic.Models.ServiceModels;

namespace VacationRental.Logic.Services
{
    public class RentalService : IRentalService
    {
        private readonly IBookingRepo _bookingRepo;
        private readonly IRentalRepo _rentalRepo;

        public RentalService(IRentalRepo rentalRepository, IBookingRepo bookingRepository)
        {
            _bookingRepo = bookingRepository;
            _rentalRepo = rentalRepository;
        }

        public async Task<RentalServiceModel> GetById(int rentalId)
        {
            var rentalData = await _rentalRepo.GetById(rentalId).ConfigureAwait(false);

            if (rentalData == null)            
                return ConvertToErrorModel<RentalServiceModel>.GetErrorModel("Rental not found. ");       

            return new RentalServiceModel() {Id = rentalData.Id, Units = rentalData.NumberOfUnits, PreparationTimeInDays = rentalData.PreparationTimeInDays };
        }

        public async Task<ResourceIdServiceModel> AddRental(RentalBindingModel model)
        {
            var newRentalId = await _rentalRepo.Add(new RentalDataModel() { NumberOfUnits = model.Units, PreparationTimeInDays = model.PreparationTimeInDays }).ConfigureAwait(false);
            
            if ( newRentalId <= 0)
                return ConvertToErrorModel<ResourceIdServiceModel>.GetErrorModel("Add operation failed. ");
            
            return new ResourceIdServiceModel() {Id = newRentalId};
        }

        public async Task<RentalServiceModel> UpdateRental(int id, RentalBindingModel model)
        {
            //If the length of preparation time is changed then it should be updated for all existing bookings. The request should fail if
            //decreasing the number of units or increasing the length of preparation time will produce overlapping between existing bookings 
            //and/or their preparation times.

            var existingRental = await GetById(id).ConfigureAwait(false);
            if (existingRental.HasError) return existingRental;

            var bookings = await _bookingRepo.GetByRentalId(id).ConfigureAwait(false);

            // proceed if currntly no booking, increasing units or decreasing preparation days
            if (!bookings.Any() || model.Units >= bookings.Count() ||
                (model.Units >= existingRental.Units && model.PreparationTimeInDays <= existingRental.PreparationTimeInDays))
            {
                return await UpdateRentalData(id, model).ConfigureAwait(false);
            }
            else
            {
                var hasConflicts = HasConflict(bookings, model);

                if (!hasConflicts) return await UpdateRentalData(id, model);
                else return ConvertToErrorModel<RentalServiceModel>.GetErrorModel("Conflict with existing booking(s). ");
            }
        }

        private async Task<RentalServiceModel> UpdateRentalData(int id, RentalBindingModel model)
        {
            var updatedModel = await _rentalRepo.Update(id, new RentalDataModel()
            {
                NumberOfUnits = model.Units,
                PreparationTimeInDays = model.PreparationTimeInDays
            }).ConfigureAwait(false);

            return updatedModel == null
                ? null
                : TransformModels.GetRentalServiceModel(updatedModel);
        }

        private static bool HasConflict(IEnumerable<BookingDataModel> bookings, RentalBindingModel model)
        {
            if (bookings.Any())
            {
                var bookingsByDateRange = bookings
                    .Select(b => new { StartDt = b.StartDate, EndDt = b.StartDate.AddDays(b.NumberOfNights + model.PreparationTimeInDays) })
                    .ToList();

                var dateToProcess = bookingsByDateRange.Min(b => b.StartDt);  // earliest booking date from all bookings
                var lastDt = bookingsByDateRange.Max(b => b.EndDt); // latest booking date from all bookings            

                // Go through each date. If number of booking on any day exceeds number of unit, there is conflict
                while (dateToProcess < lastDt)
                {
                    if (bookingsByDateRange.Where(b => dateToProcess >= b.StartDt && dateToProcess < b.EndDt).Count() > model.Units)
                        return true;
                    dateToProcess = dateToProcess.AddDays(1);
                }
            }

            return false;
        }
    }
}
