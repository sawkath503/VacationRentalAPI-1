using VacationRental.Data.Models;
using VacationRental.Logic.Models.BindingModels;
using VacationRental.Logic.Models.ServiceModels;

namespace VacationRental.Logic.Utilities
{
    internal static class TransformModels
    {
        #region ToDataModels
        internal static BookingDataModel GetDataModel(BookingBindingModel model)
        {
            return new BookingDataModel
            {
                RentalId = model.RentalId,
                NumberOfNights = model.Nights,
                StartDate = model.Start
            };
        }

        internal static RentalDataModel GetDataModel(RentalBindingModel model)
        {
            return new RentalDataModel
            {
                NumberOfUnits = model.Units,
                PreparationTimeInDays = model.PreparationTimeInDays
            };
        }
        #endregion ToDataModels

        #region ToServiceModels
        internal static BookingServiceModel GetServiceModel(BookingDataModel model)
        {
            return new BookingServiceModel()
            {
                Id = model.Id,
                RentalId = model.RentalId,
                Nights = model.NumberOfNights,
                Start = model.StartDate
            };
        }

        internal static RentalServiceModel GetRentalServiceModel(RentalDataModel model)
        {
            return new RentalServiceModel
            {
                Id = model.Id,
                Units = model.NumberOfUnits,
                PreparationTimeInDays = model.PreparationTimeInDays
            };
        }
        #endregion ToServiceModels
    }
}
