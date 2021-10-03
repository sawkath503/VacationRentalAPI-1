using System.Threading.Tasks;
using VacationRental.Data.Models;

namespace VacationRental.Data.Interfaces
{
    public interface IRentalRepo
    {
        Task<RentalDataModel> GetById(int id);
        Task<int> Add(RentalDataModel model);
        Task<RentalDataModel> Update(int id, RentalDataModel model);
    }
}