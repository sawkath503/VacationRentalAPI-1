using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VacationRental.Data.Interfaces;
using VacationRental.Data.Models;

namespace VacationRental.Data.Repos
{
    public class RentalRepo : IRentalRepo
    {
        private readonly IDictionary<int, RentalDataModel> _rentals;
        public RentalRepo(IDictionary<int, RentalDataModel> rentals)
        {
            _rentals = rentals;
        }

        public async Task<RentalDataModel> GetById(int id)
        {
            return await Task.Run(() =>
            {
                if (_rentals.ContainsKey(id)) return _rentals[id];                
                return null;
            });

        }

        public async Task<int> Add(RentalDataModel model)
        {
            try
            {
                return await Task.Run(() =>
                {
                    model.Id = _rentals.Keys.Count + 1;
                    _rentals.Add(model.Id, model);
                    return model.Id;
                });
            }
            catch (Exception e)
            {
                return -1;
            }

        }

        public async Task<RentalDataModel> Update(int id, RentalDataModel model)
        {
            RentalDataModel existingDataModel = await GetById(id);
            
            // off loading to a task for demo purpose. In actual scenario it will be an async operation to the external datasource
            await Task.Run(() =>
            {                
                if (existingDataModel != null)
                {
                    existingDataModel.NumberOfUnits = model.NumberOfUnits;
                    existingDataModel.PreparationTimeInDays = model.PreparationTimeInDays;
                }                
            });
            
            return existingDataModel;
        }
    }
}
