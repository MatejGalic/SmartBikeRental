using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartBikeRental.Helpers;
using SmartBikeRental.Hubs;
using SmartBikeRental.Middleware;
using SmartBikeRental.Models.DTOs;
using System.Text.Json;

namespace SmartBikeRental.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BikeRentController : ControllerBase
    {
        private readonly ILogger<BikeRentController> _logger;
        private readonly IHubContext<BikeRentHub> _hub;
        private readonly IMqttClientWrapper _mqttClient;
        private readonly IConfiguration _configuration;

        public BikeRentController(ILogger<BikeRentController> logger, IHubContext<BikeRentHub> hub, IMqttClientWrapper mqttClient, IConfiguration configuration)
        {
            _logger = logger;
            _hub = hub;
            _mqttClient = mqttClient;
            _configuration = configuration;
        }

        // used for testing SignalR through Swagger, refactor later
        [HttpGet("mock-stream-with-timer")]
        public IActionResult SendDeviceDataToAll()
        {
            var mockDevice = new DeviceDto();
            var mockData = new List<DeviceDto>
            {
                mockDevice,
                mockDevice,
                mockDevice,
                mockDevice
            };
            var timerManager = new TimerManager(() => _hub.Clients.All.SendAsync("DeviceData", mockData));
            //_hub.Clients.All.SendAsync("DeviceData", new { newDate = DateTime.Now });

            return Ok(new { Message = "Request completed" });
        }

        [HttpPost("unlock/{id}")]
        public IActionResult UnlockBike(string id)
        {            
            _mqttClient.PublishToTopic(id);

            return Ok(new { Message = "Request completed" });
        }


        [HttpGet("devices")]
        public IActionResult GetDevices()
        {
            var devices = _mqttClient.getDevices();
            return Ok(new { Devices = devices });
        }
    }
}
