using Microsoft.AspNetCore.SignalR;

namespace SmartBikeRental.Hubs
{
    public class BikeRentHub : Hub
    {
        public async void SendDeviceDataToAll() //trenutno useless
        {
            await Clients.All.SendAsync("DeviceData", new { newDate = DateTime.Now });
        }
    }
}
