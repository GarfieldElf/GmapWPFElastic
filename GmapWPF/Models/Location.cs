using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GmapWPF.Models
{
    public class Location
    {
        [JsonPropertyName("text")]
        public string? text { get; set; }

        [JsonPropertyName("location")]
        public GeoPoint? geoPoint { get; set; }

    }

    public class GeoPoint
    {
        public double Lat { get; set; } // Enlem (Latitude)
        public double Lon { get; set; } // Boylam (Longitude)
    }
}
