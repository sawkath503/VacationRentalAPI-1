using System;
using VacationRental.Logic.Models.ServiceModels;

namespace VacationRental.Logic.Models.ServiceModels
{
    public class BookingServiceModel : GenericError
    {
        public int Id { get; internal set; }
        public int RentalId { get; internal set; }
        public int Unit { get; internal set; }
        public DateTime Start { get; internal set; }
        public int Nights { get; internal set; }  
    }
}
