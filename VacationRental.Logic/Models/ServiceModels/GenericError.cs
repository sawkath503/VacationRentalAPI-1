namespace VacationRental.Logic.Models.ServiceModels
{
    public abstract class GenericError
    {
        public bool HasError { get; internal set; }
        public string ErrorMessage { get; internal set; }        
    }
}
