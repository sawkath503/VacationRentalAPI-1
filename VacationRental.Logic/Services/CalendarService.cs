using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using VacationRental.Data.Interfaces;
using VacationRental.Logic.Interfaces;
using VacationRental.Logic.Models.ServiceModels;
using VacationRental.Logic.Utilities;

namespace VacationRental.Logic.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly IBookingRepo _bookingRepo;
        private readonly IRentalRepo _rentalRepo;

        public CalendarService(IRentalRepo rentalRepository, IBookingRepo bookingRepository)
        {
            _bookingRepo = bookingRepository;
            _rentalRepo = rentalRepository;
        }

        public async Task<CalendarServiceModel> Get(int rentalId, DateTime start, int nights)
        {
            var rental = await _rentalRepo.GetById(rentalId).ConfigureAwait(false);

            if (rental == null)
                return ConvertToErrorModel<CalendarServiceModel>.GetErrorModel("Rentel not found. ");
            
            try
            {
                var bookings = await _bookingRepo.GetByRentalId(rentalId).ConfigureAwait(false);
                var unitMappedToBookingId = bookings.Where(b => b.StartDate >= start.Date && b.StartDate <= start.Date.AddDays(nights))
                    .Select((b, i) => new
                    {
                        Id = b.Id,
                        Unit = ++i
                    });

                var result = new CalendarServiceModel
                {
                    RentalId = rentalId,
                    Dates = new List<CalendarDateServiceModel>()
                };

                var newMappings = new List<(int, int)>();

                for (var i = 0; i < nights; i++)
                {
                    var date = new CalendarDateServiceModel
                    {
                        Date = start.Date.AddDays(i),
                        Bookings = new List<CalendarBookingServiceModel>()
                    };                    

                    foreach (var booking in bookings)
                    {
                        if (booking.StartDate <= date.Date && booking.StartDate.AddDays(booking.NumberOfNights + rental.PreparationTimeInDays) > date.Date)
                        {
                            int unit = (int)newMappings.Where(c => c.Item1 == booking.Id)?.FirstOrDefault().Item2;

                            if (unit <= 0)
                            {
                                unit = unitMappedToBookingId.Where(b => b.Id == booking.Id).FirstOrDefault().Unit;

                                // if unit id is larger than number of units, find an unit currently not occupied, keep it in a new mapping
                                // and use it in subsequent iterations. This way a booking will always map to a single unit
                                if (unit > rental.NumberOfUnits)
                                {
                                    unit %= rental.NumberOfUnits;
                                    while (date.Bookings.Where(b => b.Unit == unit).Any())
                                    {
                                        ++unit;
                                    }
                                    newMappings.Add((booking.Id, unit));
                                }
                            }

                            date.Bookings.Add(new CalendarBookingServiceModel
                            {
                                Id = booking.Id,
                                Unit = unit
                            });
                        }
                    }

                    result.Dates.Add(date);
                }

                return result;
            }
            catch (Exception ex)
            {
                return ConvertToErrorModel<CalendarServiceModel>.GetErrorModel($"An error occured trying to get the data. {ex}");                
            }
        }   
    }
}
