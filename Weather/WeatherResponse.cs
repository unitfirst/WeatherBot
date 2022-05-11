namespace WeatherBot
{
    public class WeatherResponse
    {
        public TemperatureInfo Main { get; set; }
        public Coordinates Coord { get; set; }
        public string Name { get; set; }
    }
}