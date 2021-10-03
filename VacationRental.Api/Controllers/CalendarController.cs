using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models.ViewModels;
using VacationRental.Api.Utilities;
using VacationRental.Logic.Interfaces;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/calendar")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _calendarService;
        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        [HttpGet]
        public async Task<ActionResult<CalendarViewModel>> Get(int rentalId, DateTime start, int nights)
        {
            (var isValid, string errorMessage) = ValidateInput.ValidateCalendarViewInput(rentalId, start, nights);

            if (!isValid) 
                return BadRequest($"Invalid input. {errorMessage} ");
                        
            try
            {
                var calendar = await _calendarService.Get(rentalId, start, nights);
                
                if (calendar.HasError)
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"Could not get calendar view. {calendar.ErrorMessage}. ");
                
                return Ok(TransformModels.GetViewModel(calendar));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Could not get calendar view. {ex.Message}. ");
            }
        }
    }
}
