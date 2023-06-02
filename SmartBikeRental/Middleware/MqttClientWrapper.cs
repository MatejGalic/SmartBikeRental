using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SmartBikeRental.Hubs;
using SmartBikeRental.Models.DTOs;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace SmartBikeRental.Middleware;
public class MqttClientWrapper : IMqttClientWrapper
{
    private readonly MqttClient mqttClient;
    private readonly IHubContext<BikeRentHub> _hub;
    private List<ContentNodeDto> contentNodes;


    public MqttClientWrapper(IHubContext<BikeRentHub> hub)
    {
        mqttClient = new MqttClient("161.53.19.19", 56883, false, null, null, MqttSslProtocols.None);
        contentNodes = new List<ContentNodeDto>();
        _hub = hub;
    }



    public List<DeviceDto> getDevices()
    {
        return GetDevices();
    }


    public void Connect(string clientId)
    {
        mqttClient.Connect(clientId);
        mqttClient.MqttMsgPublishReceived += OnMessageReceived;
    }

    public void Disconnect()
    {
        mqttClient.MqttMsgPublishReceived -= OnMessageReceived;
        mqttClient.Disconnect();
    }

    public async void PublishToTopic(string id)
    {
        DefaultContractResolver contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };

        string json = JsonConvert.SerializeObject(ConfigureResponse(id), new JsonSerializerSettings
        {
            ContractResolver = contractResolver,
            Formatting = Formatting.Indented
        }).Replace("bikeRentalActuator", "BikeRentalActuator").Replace("bikeRentalLed", "BikeRentalLed");


        var httpClientHandler = new HttpClientHandler();
        httpClientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

        using var httpClient = new HttpClient(httpClientHandler);
        string username = "IoTGrupa10";
        string password = "IoTProject123";
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

        var authenticationValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
        httpClient.DefaultRequestHeaders.Authorization = authenticationValue;

        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ericsson.simple.input.hierarchical+json");

        string url = "https://161.53.19.19:56443/m2m/data";
        HttpResponseMessage response = await httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"HTTP request succeded with status code: {response.StatusCode}");
        }
        else
        {
            Console.WriteLine($"HTTP request failed with status code: {response.StatusCode}");
        }

    }

    public void SubscribeToTopic(string topic)
    {
        mqttClient.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    public UnlockResponseDto ConfigureResponse(string deviceId)
    {
        return new UnlockResponseDto
        {
            DeviceId = deviceId,
            Header = new Header
            {
                TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            },
            Body = new Body
            {
                BikeRentalActuator = new BikeRentalActuator
                {
                    BikeRentalLed = 1
                }
            }
        };

    }

    private void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        var message = Encoding.Default.GetString(e.Message);
        Console.WriteLine($"Received message on topic: {e.Topic}, Message: {message}");

        if (message != null)
        {
            MessageDto data = JsonConvert.DeserializeObject<MessageDto>(message)!;
            List<ContentNodeDto> newNodes = data.ContentNodes;

            UpdateNewNodes(newNodes);
            var devices = GetDevices();

            _hub.Clients.All.SendAsync("DeviceData", devices);
        }


    }

    private void UpdateNewNodes(List<ContentNodeDto> newNodes)
    {
        foreach (var updatedNode in newNodes)
        {
            var existingNode = contentNodes.FirstOrDefault(n => n.Source.GatewayGroup == updatedNode.Source.GatewayGroup &&
            n.Source.ResourceSpec == updatedNode.Source.ResourceSpec);

            if (existingNode != null)
            {
                //existingNode = updatedNode;

                var idx = contentNodes.FindIndex(n => n == existingNode);
                if (idx != -1)
                {
                    contentNodes.Remove(existingNode);
                    contentNodes.Add(updatedNode);
                }
            }
        }
    }

    private List<DeviceDto> GetDevices()
    {
        if (!contentNodes.Any())
        {
            fillNodes();
        }


        var deviceNames = contentNodes.Select(n => n.Source.GatewayGroup).Distinct().ToList();

        var devices = new List<DeviceDto>();

        foreach (var deviceName in deviceNames)
        {
            var device = new DeviceDto
            {
                DeviceName = deviceName,
                BikeRentalLED = contentNodes.FirstOrDefault(n => n.Source.GatewayGroup == deviceName && n.Source.ResourceSpec == "BikeRentalLed")?.Value == 1,
                BikeRentalLatitude = contentNodes.FirstOrDefault(n => n.Source.GatewayGroup == deviceName && n.Source.ResourceSpec == "BikeRentalLatitude")?.Value ?? 0.0,
                BikeRentalLongitude = contentNodes.FirstOrDefault(n => n.Source.GatewayGroup == deviceName && n.Source.ResourceSpec == "BikeRentalLongitude")?.Value ?? 0.0
            };

            devices.Add(device);
        }

        return devices.OrderBy(e => e.DeviceName).ToList();
    }


    private async void fillNodes()
    {
        var httpClientHandler = new HttpClientHandler();
        httpClientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

        using var httpClient = new HttpClient(httpClientHandler);
        string username = "IoTGrupa10";
        string password = "IoTProject123";
        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

        var authenticationValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
        httpClient.DefaultRequestHeaders.Authorization = authenticationValue;

        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.ericsson.m2m.output+json"));

        string url = "https://161.53.19.19:56443/m2m/data?gatewaySpec=BikeRentalMQTT&maxPayloadsPerResource=1&orderPayloadsBy=timestamp,DESC";
        HttpResponseMessage response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();

            MessageDto data = JsonConvert.DeserializeObject<MessageDto>(responseBody)!;

            List<ContentNodeDto> newNodes = data.ContentNodes.Where(obj => obj.Source.GatewayGroup.StartsWith("BR_")).ToList();

            contentNodes = newNodes;
        }
        else
        {
            Console.WriteLine($"HTTP request failed with status code: {response.StatusCode}");
        }
    }
}

