using System.ComponentModel.DataAnnotations;

namespace HomeApi.Contracts.Models.Devices
{
    /// <summary>
    /// Запрос на удаление девайса из комнаты
    /// </summary>
    public class DeleteDeviceRequest
    {
        public string Name { get; set; }
        public string RoomLocation { get; set; }
    }
}