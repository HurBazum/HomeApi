using System;
using System.Threading.Tasks;
using AutoMapper;
using HomeApi.Contracts.Models.Devices;
using HomeApi.Data.Models;
using HomeApi.Data.Queries;
using HomeApi.Data.Repos;
using Microsoft.AspNetCore.Mvc;

namespace HomeApi.Controllers
{
    /// <summary>
    /// Контроллер устройсив
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DevicesController : ControllerBase
    {
        readonly IDeviceRepository _devices;
        readonly IRoomRepository _rooms;
        readonly IMapper _mapper;
        
        public DevicesController(IDeviceRepository devices, IRoomRepository rooms, IMapper mapper)
        {
            _devices = devices;
            _rooms = rooms;
            _mapper = mapper;
        }

        /// <summary>
        /// Просмотр списка подключенных устройств
        /// </summary>
        [HttpGet] 
        [Route("")] 
        public async Task<IActionResult> GetDevices()
        {
            var devices = await _devices.GetDevices();

            var resp = new GetDevicesResponse
            {
                DeviceAmount = devices.Length,
                Devices = _mapper.Map<Device[], DeviceView[]>(devices)
            };
            
            return StatusCode(200, resp);
        }

        /// <summary>
        /// Удаление устройства
        /// </summary>
        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> Delete([FromBody]DeleteDeviceRequest deleteDeviceRequest)
        {
            var room = _rooms.GetRoomByName(deleteDeviceRequest.RoomLocation).Result;

            if (room == null) return StatusCode(400, $"Ошибка: Комната \"{deleteDeviceRequest.RoomLocation}\" не подключена!");

            var device = _devices.GetDeviceByName(deleteDeviceRequest.Name).Result;

            if (device == null) return StatusCode(400, $"Ошибка: Устройство с именем \"{deleteDeviceRequest.Name}\" не существует!");

            if (device.Location != room.Name) return StatusCode(400, $"Ошибка: устройство \"{device.Name}\" не подключено в комнате \"{room.Name}\"!");

            await _devices.DeleteDevice(device);

            return StatusCode(200, $"Устройство \"{deleteDeviceRequest.Name}\" успешно удалено из комнаты \"{deleteDeviceRequest.RoomLocation}\"!");
        }

        /// <summary>
        /// Добавление нового устройства
        /// </summary>
        [HttpPost] 
        [Route("")] 
        public async Task<IActionResult> Add(AddDeviceRequest request)
        {
            var room = await _rooms.GetRoomByName(request.RoomLocation);
            if(room == null)
                return StatusCode(400, $"Ошибка: Комната \"{request.RoomLocation}\" не подключена. Сначала подключите комнату!");
            
            var device = await _devices.GetDeviceByName(request.Name);
            if(device != null)
                return StatusCode(400, $"Ошибка: Устройство \"{request.Name}\" уже существует.");


            var newDevice = _mapper.Map<AddDeviceRequest, Device>(request);
            // проверка на возможность подключения устройства в данной комнате:
            // по вольтажу и наличию газового подключения
            if (room.Voltage < newDevice.CurrentVolts)
                return StatusCode(400, $"Ошибка: Устройство \"{newDevice.Name}\" требует большее напряжение, чем есть в комнате \"{room.Name}\"!");

            if(room.GasConnected != newDevice.GasUsage && newDevice.GasUsage == true)
                return StatusCode(400, $"Ошибка: Устройство \"{newDevice.Name}\" требует газ, а в комнате \"{room.Name}\" он не подключен!");

            await _devices.SaveDevice(newDevice, room);
            
            return StatusCode(201, $"Устройство \"{request.Name}\" добавлено. Идентификатор: {newDevice.Id}");
        }
        
        /// <summary>
        /// Обновление существующего устройства
        /// </summary>
        [HttpPatch] 
        [Route("{id}")] 
        public async Task<IActionResult> Edit(
            [FromRoute] Guid id,
            [FromBody]  EditDeviceRequest request)
        {
            var room = await _rooms.GetRoomByName(request.NewRoom);
            if(room == null)
                return StatusCode(400, $"Ошибка: Комната \"{request.NewRoom}\" не подключена. Сначала подключите комнату!");
            
            var device = await _devices.GetDeviceById(id);
            if(device == null)
                return StatusCode(400, $"Ошибка: Устройство с идентификатором \"{id}\" не существует.");
            
            var withSameName = await _devices.GetDeviceByName(request.NewName);
            if(withSameName != null)
                return StatusCode(400, $"Ошибка: Устройство с именем \"{request.NewName}\" уже подключено. Выберите другое имя!");
            // проверка на возможность подключения устройства в данной комнате:
            // по вольтажу и наличию газового подключения
            if (room.Voltage < device.CurrentVolts)
                return StatusCode(400, $"Ошибка: Устройство \"{device.Name}\" требует большее напряжение, чем есть в комнате \"{room.Name}\"!");

            if (room.GasConnected != device.GasUsage && device.GasUsage == true)
                return StatusCode(400, $"Ошибка: Устройство \"{device.Name}\" требует газ, а в комнате \"{room.Name}\" он не подключен!");

            await _devices.UpdateDevice(
                device,
                room,
                new UpdateDeviceQuery(request.NewName, request.NewSerial)
            );

            return StatusCode(200, $"Устройство обновлено! Имя - \"{device.Name}\", Серийный номер - \"{device.SerialNumber}\",  Комната подключения - \"{device.Room.Name}\"");
        }
    }
}