using System.Collections.Generic;

namespace VacationRental.Logic.Models.ServiceModels
{
    public class CalendarServiceModel : GenericError
    {
        public int RentalId { get; internal set; }
        public List<CalendarDateServiceModel> Dates { get; internal set; }
    }
}
