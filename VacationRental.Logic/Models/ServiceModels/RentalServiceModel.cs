namespace VacationRental.Logic.Models.ServiceModels
{
    public class RentalServiceModel : GenericError
    {
        public int Id { get; internal set; }
        public int Units { get; internal set; }
        public int PreparationTimeInDays { get; internal set; }
    }
}
