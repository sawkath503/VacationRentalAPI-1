using System.Linq;
using VacationRental.Api.Models.ViewModels;
using VacationRental.Logic.Models.ServiceModels;

namespace VacationRental.Api.Utilities
{
    internal static class TransformModels
    {
        internal static ResourceIdViewModel GetViewModel(ResourceIdServiceModel model)
        {            
            return new ResourceIdViewModel
            {
                Id = model.Id
            };
        }

        internal static BookingViewModel GetViewModel(BookingServiceModel model)
        {
            return new BookingViewModel
            {
                Id = model.Id,
                RentalId = model.RentalId,
                Nights = model.Nights,
                Start = model.Start
            };
        }

        internal static RentalViewModel GetViewModel(RentalServiceModel model)
        {
            return new RentalViewModel
            {
                Id = model.Id,
                Units = model.Units,
                PreparationTimeInDays = model.PreparationTimeInDays                
            };
        }

        internal static CalendarViewModel GetViewModel(CalendarServiceModel model)
        {
            return new CalendarViewModel
            {
                RentalId = model.RentalId,
                Dates = model.Dates.Select(d => GetViewModel(d)).ToList()
            };
        }

        private static CalendarDateViewModel GetViewModel(CalendarDateServiceModel model)
        {
            return new CalendarDateViewModel
            {
                Date = model.Date,
                Bookings = model.Bookings.Select(b => GetViewModel(b)).ToList()
            };
        }

        private static CalendarBookingViewModel GetViewModel(CalendarBookingServiceModel model)
        {
            return new CalendarBookingViewModel
            {
                Id = model.Id,
                Unit = model.Unit
            };
        }
    }
}
