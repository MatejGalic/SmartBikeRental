namespace SmartBikeRental.Models.DTOs
{
    public class DeviceDto
    {
        public string DeviceName { get; set; } = String.Empty;
        public bool IsLocked { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
