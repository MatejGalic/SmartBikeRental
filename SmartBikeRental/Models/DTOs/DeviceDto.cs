namespace SmartBikeRental.Models.DTOs
{
    public class DeviceDto
    {
        public string DeviceName { get; set; } = String.Empty;
        // False = locked, True = unlocked
        public bool BikeRentalLED { get; set; }
        public double BikeRentalLatitude { get; set; }
        public double BikeRentalLongitude { get; set; }
        public bool BikeRentalTaken { get; set; }
    }
}
