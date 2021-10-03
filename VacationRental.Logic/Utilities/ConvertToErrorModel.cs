using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using VacationRental.Logic.Models.ServiceModels;

namespace VacationRental.Logic.Utilities
{
    public static class ConvertToErrorModel<T> where T : GenericError, new()
    {
        public static T GetErrorModel(string errorMessage)
        {
            return new T()
            {
                HasError = true,
                ErrorMessage = errorMessage
            };
        }
    }
}
