using System;

namespace VacationRental.Data.Models
{
    public class BookingDataModel
    {
        public int Id { get; internal set; }
        public int RentalId { get; set; }
        public int NumberOfNights { get; set; }
        public DateTime StartDate { get; set; }
    }
}
