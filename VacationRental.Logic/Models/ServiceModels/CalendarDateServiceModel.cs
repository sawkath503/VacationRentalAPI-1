using System;
using System.Collections.Generic;

namespace VacationRental.Logic.Models.ServiceModels
{
    public class CalendarDateServiceModel
    {
        public DateTime Date { get; internal set; }
        public List<CalendarBookingServiceModel> Bookings { get; internal set; }
    }
}
