using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacationRental.Logic.Models.BindingModels;

namespace VacationRental.Api.Utilities
{
    internal static class ValidateInput
    {
        internal static (bool, string) Validate(RentalBindingModel model)
        {
            var sb = new StringBuilder(string.Empty);

            if (model.Units <= 0)
            {
                sb.Append("Number of units must be positive. ");
            }

            if (model.PreparationTimeInDays < 0)
            {
                sb.Append("Preparation days cannot be negative. ");
            }

            if (sb.Length > 0) return (false, sb.ToString());
            return (true, null);
        }

        internal static (bool, string) Validate(BookingBindingModel model)
        {
            var sb = new StringBuilder(string.Empty);

            if (model.RentalId <= 0)
            {
                sb.Append("Rental id must be positive. ");
            }
            if (model.Nights <= 0)
            {
                sb.Append("Nigts must be positive. ");
            }
            if (model.Start < DateTime.Today.Date)
            {
                sb.Append("Booking cannot be in the past. ");
            }

            if (sb.Length > 0) return (false, sb.ToString());
            return (true, null);
        }

        internal static (bool, string) ValidateCalendarViewInput(int rentalId, DateTime start, int nights)
        {
            var sb = new StringBuilder(string.Empty);

            if (rentalId <= 0)
                sb.Append("invalid rental id");
            if (nights <= 0)
                sb.Append("Nights must be positive");

            if (sb.Length > 0) return (false, sb.ToString());
            return (true, null);
        }
    }
}
