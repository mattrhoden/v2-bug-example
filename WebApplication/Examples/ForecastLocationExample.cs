using Swashbuckle.AspNetCore.Filters;
using WebApplication.Models;

namespace WebApplication.Examples
{
    public class ForecastLocationExample : IExamplesProvider<ForecastLocation>
    {
        public ForecastLocation GetExamples()
        {
            return new ForecastLocation()
            {
                City = "Los Angeles",
                State = "CA",
                ZipCode = "90001"
            };
        }
    }
}