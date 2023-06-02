namespace SmartBikeRental.Models.DTOs
{
    public class DeviceDto
    {
        public string DeviceName { get; set; } = String.Empty;
        public bool BikeRentalLED { get; set; }
        public double BikeRentalLatitude { get; set; }
        public double BikeRentalLongitude { get; set; }
    }
}
