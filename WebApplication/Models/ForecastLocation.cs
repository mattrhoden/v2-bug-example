using System;

namespace WebApplication.Models
{
    /// <summary>
    /// Where you would like to see forecast info.
    /// </summary>
    [Serializable]
    public class ForecastLocation
    {
        /// <summary>
        /// The city you would like to see the forecast for.
        /// </summary>
        public string City { get; set; }
        
        /// <summary>
        /// The state you would like to see the forecast for.
        /// </summary>
        public string State { get; set; }
        
        /// <summary>
        /// The zip code you would like to see the forecast for.
        /// </summary>
        public string ZipCode { get; set; }
    }
}