using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models.ViewModels;
using VacationRental.Api.Utilities;
using VacationRental.Logic.Interfaces;
using VacationRental.Logic.Models.BindingModels;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/bookings")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        [Route("{bookingId:int}")]
        public async Task<ActionResult<BookingViewModel>> Get(int bookingId)
        {
            if (bookingId <= 0) return BadRequest($"Invalid booking id. ");

            try
            {
                var booking = await _bookingService.GetById(bookingId);
                
                if (booking.HasError)
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"{booking.ErrorMessage} ");
                
                return Ok(TransformModels.GetViewModel(booking));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Could not get booking. {ex.Message}. ");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ResourceIdViewModel>> Post(BookingBindingModel model)
        {
            (var isValid, string errorMessage) = ValidateInput.Validate(model);

            if (!isValid)  return BadRequest($"Invalid input. {errorMessage} ");

            try
            {
                var addedBooking = await _bookingService.AddBooking(model);
                
                if (addedBooking.HasError)
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"{addedBooking.ErrorMessage} ");

                return Ok(TransformModels.GetViewModel(addedBooking));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Could not add booking. {ex.Message}. ");
            }
        }
    }
}
