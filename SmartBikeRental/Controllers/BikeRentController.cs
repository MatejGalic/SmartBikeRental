﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartBikeRental.Helpers;
using SmartBikeRental.Hubs;

namespace SmartBikeRental.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BikeRentController : ControllerBase
    {
        private readonly ILogger<BikeRentController> _logger;
        private readonly IHubContext<BikeRentHub> _hub;

        public BikeRentController(ILogger<BikeRentController> logger, IHubContext<BikeRentHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        // used for testing SignalR through Swagger, refactor later
        [HttpGet("mock-stream-with-timer")]
        public IActionResult SendDeviceDataToAll()
        {
            var timerManager = new TimerManager(() => _hub.Clients.All.SendAsync("DeviceData", new { newDate = DateTime.Now }));
            //_hub.Clients.All.SendAsync("DeviceData", new { newDate = DateTime.Now });

            return Ok(new { Message = "Request completed" });
        }
    }
}