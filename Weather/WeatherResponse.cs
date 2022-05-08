using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace WeatherBot
{
    public class WeatherResponse
    {
        public TemperatureInfo Main { get; set; }
        public Coordinates Coord { get; set; }
        public string Name { get; set; }
    }
}