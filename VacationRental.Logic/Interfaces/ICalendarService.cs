using System;
using System.Threading.Tasks;
using VacationRental.Logic.Models.ServiceModels;

namespace VacationRental.Logic.Interfaces
{
    public interface ICalendarService
    {
        Task<CalendarServiceModel> Get(int rentalId, DateTime start, int nights);
    }
}