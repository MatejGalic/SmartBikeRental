namespace SmartBikeRental.Models.DTOs;

public class UnlockResponseDto
{
    public string DeviceId { get; set; }
    public Header Header { get; set; }
    public Body Body { get; set; }
}

public class Header
{
    public long TimeStamp { get; set; }
}

public class Body
{
    public BikeRentalActuator BikeRentalActuator { get; set; }
}

public class BikeRentalActuator
{
    public int BikeRentalLed { get; set; }
}


