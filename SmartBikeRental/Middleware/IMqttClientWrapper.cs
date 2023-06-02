using SmartBikeRental.Models.DTOs;

namespace SmartBikeRental.Middleware;
public interface IMqttClientWrapper
{
    public void Connect(string clientId);
    public void SubscribeToTopic(string topic);
    public void PublishToTopic(string id);
    public void Disconnect();
    public List<DeviceDto> getDevices();
    public UnlockResponseDto ConfigureResponse(string deviceId);
}

