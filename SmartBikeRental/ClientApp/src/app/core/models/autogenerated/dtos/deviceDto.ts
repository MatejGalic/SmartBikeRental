

export class DeviceDto
{
    deviceName: string;
    bikeRentalLED: boolean;
    bikeRentalLatitude: number;
    bikeRentalLongitude: number;
    bikeRentalTaken: boolean;

    constructor(deviceName: string = null,bikeRentalLED: boolean = null,bikeRentalLatitude: number = null,bikeRentalLongitude: number = null,bikeRentalTaken: boolean = null,)
    {
    
        this.deviceName = deviceName;
        this.bikeRentalLED = bikeRentalLED;
        this.bikeRentalLatitude = bikeRentalLatitude;
        this.bikeRentalLongitude = bikeRentalLongitude;
        this.bikeRentalTaken = bikeRentalTaken;
    }
}