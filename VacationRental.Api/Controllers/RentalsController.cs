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
    [Route("api/v1/rentals")]
    [ApiController]
    public class RentalsController : ControllerBase
    {
        private readonly IRentalService _rentalService;

        public RentalsController(IRentalService rentalService)
        {            
            _rentalService = rentalService;
        }

        [HttpGet]
        [Route("{rentalId:int}")]
        public async Task<ActionResult<RentalViewModel>> Get(int rentalId)
        {
            try
            {
                var rental = await _rentalService.GetById(rentalId);

                if (rental.HasError) 
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"{rental.ErrorMessage}");

                return Ok(TransformModels.GetViewModel(rental));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Could not retrieve rental. {ex.Message}. ");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ResourceIdViewModel>> Post(RentalBindingModel model)
        {
            (var isValid, string errorMessage) = ValidateInput.Validate(model); 

            if (!isValid) return BadRequest($"Invalid input. {errorMessage} ");            
            
            try
            {
                var addedRental = await _rentalService.AddRental(model);

                if (addedRental.HasError)
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"Could not add rental. {addedRental.ErrorMessage} ");
                
                return Ok(TransformModels.GetViewModel(addedRental));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Could not add rental. {ex.Message}. ");
            }
        }

        [HttpPut]
        [Route("/api/v1/vacationrental/rentals/{id}")]
        public async Task<ActionResult<RentalViewModel>> Put(int id, RentalBindingModel model)
        {
            (var isValid, string errorMessage) = ValidateInput.Validate(model);

            if (!isValid) return BadRequest($"Invalid input. {errorMessage} ");

            try
            {
                var updatedRental = await _rentalService.UpdateRental(id, model);

                if (updatedRental == null)
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"Could not update rental. ");

                if (updatedRental.HasError) 
                    return StatusCode((int)HttpStatusCode.InternalServerError, $"Could not update rental. {updatedRental.ErrorMessage} ");
                
                return Ok(TransformModels.GetViewModel(updatedRental));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError , $"Could not update rental. Operation failed. {ex.Message}. ");
            }
        }
    }
}
