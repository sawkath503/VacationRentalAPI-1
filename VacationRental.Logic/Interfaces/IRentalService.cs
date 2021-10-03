using System.Threading.Tasks;
using VacationRental.Logic.Models.BindingModels;
using VacationRental.Logic.Models.ServiceModels;

namespace VacationRental.Logic.Interfaces
{
    public interface IRentalService
    {
        Task<RentalServiceModel> GetById(int rentalId);
        Task<ResourceIdServiceModel> AddRental(RentalBindingModel model);
        Task<RentalServiceModel> UpdateRental(int id, RentalBindingModel model);
    }
}