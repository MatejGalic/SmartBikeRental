

export class DeviceDto
{
    deviceName: string;
    isLocked: boolean;
    latitude: number;
    longitude: number;

    constructor(deviceName: string = null,isLocked: boolean = null,latitude: number = null,longitude: number = null,)
    {
    
        this.deviceName = deviceName;
        this.isLocked = isLocked;
        this.latitude = latitude;
        this.longitude = longitude;
    }
}