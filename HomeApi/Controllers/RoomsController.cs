using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HomeApi.Contracts.Models.Devices;
using HomeApi.Contracts.Models.Rooms;
using HomeApi.Data.Models;
using HomeApi.Data.Repos;
using Microsoft.AspNetCore.Mvc;

namespace HomeApi.Controllers
{
    /// <summary>
    /// Контроллер комнат
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class RoomsController : ControllerBase
    {
        readonly IRoomRepository _roomRepository;
        readonly IDeviceRepository _deviceRepository;
        readonly IMapper _mapper;

        public RoomsController(IRoomRepository roomRepository, IDeviceRepository deviceRepository, IMapper mapper)
        {
            _roomRepository = roomRepository;
            _deviceRepository = deviceRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Получить все комнаты
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await _roomRepository.GetAll();

            var resp = new GetRoomsResponse
            {
                RoomAmount = rooms.Length,
                Rooms = _mapper.Map<Room[], RoomView[]>(rooms)
            };

            return (resp.RoomAmount == 0) ? NotFound("Ошибка: Пока не подключено ни одной комнаты!") : Ok(resp);
        }

        /// <summary>
        /// Добавление комнаты
        /// </summary>
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Add([FromBody] AddRoomRequest request)
        {
            var existingRoom = await _roomRepository.GetRoomByName(request.Name);

            if (existingRoom == null)
            {
                var newRoom = _mapper.Map<AddRoomRequest, Room>(request);
                await _roomRepository.AddRoom(newRoom);
                return StatusCode(201, $"Комната \"{request.Name}\" добавлена!");
            }

            return StatusCode(409, $"Ошибка: Комната \"{request.Name}\" уже существует.");
        }

        /// <summary>
        /// Изменение комнаты:
        /// для не булевых значений в реквесте, если не нужно их изменять, необходимо ввести null.
        /// т.о. можно изменить все свойства комнаты, кроме Id(вроде для этого нужно удалить комнату, а потом добавить новую),
        /// при этом так же может происходить апдейт устройств, изменяеться их свойство Location, или они вовсе могут быть удалены
        /// </summary>
        [HttpPut]
        [Route("Update/{name}")]
        public async Task<IActionResult> Update([FromRoute] string name, [FromBody] EditRoomRequest editRoomRequest)
        {
            var room = _roomRepository.GetRoomByName(name).Result;
            if (room == null) 
                return NotFound($"Ошибка: Комната с именем \"{name}\" не подключена");

            // проверка на существование комнаты с именем из реквеста
            var exsistRoom = _roomRepository.GetRoomByName(editRoomRequest.NewName).Result;
            if (exsistRoom != null)
                return StatusCode(409, $"Ошибка: Комната с именем \"{editRoomRequest.NewName}\" уже подключена.");

            // получаем все устройства, подключенные в данной комнате,
            // чтобы либо отключить их, если новые параметры комнаты
            // будут для них неподходящими, либо отказать в обновлении
            // самой комнаты, если пользователь в реквесте указал,
            // что хочет сохранить все устройства
            var roomsDevices = _deviceRepository.GetDevicesByRoom(room).Result;

            // для вывода имём удалённых устройств и их подсчёта, если такие имеются
            List<DeviceView> deletedDevicesNames = new();

            // для работы с условиями, при проверке удовлетворения требований устройств изменёнными характеристиками
            if(editRoomRequest.NewVoltage == null) editRoomRequest.NewVoltage = room.Voltage;

            // проверка на наличие подключенных устройств в комнате
            if(roomsDevices.Length != 0)
            {
                // обновление комнаты, если устройства можно отключать
                if (editRoomRequest.KeepDevices == false)
                {
                    foreach (var device in roomsDevices)
                    {
                        if (device.CurrentVolts > editRoomRequest.NewVoltage || (device.GasUsage != default && editRoomRequest.ChangeGasConnection != default))
                        {
                            // отключаем устройство, которому не подходят новые характеристики комнаты
                            deletedDevicesNames.Add(_mapper.Map<Device, DeviceView>(device));
                            await _deviceRepository.DeleteDevice(device);
                        }
                    }

                    await _roomRepository.Update(room, new(editRoomRequest.NewArea, editRoomRequest.NewVoltage, editRoomRequest.ChangeGasConnection, editRoomRequest.ChangeAddDate, editRoomRequest.NewName));
                    
                    // если призошло изменение имени комнаты, изменяем значения у соответствующих устройств
                    if (!string.IsNullOrEmpty(editRoomRequest.NewName) && roomsDevices != null)
                    {
                        // получаем обновлённую комнату
                        var newRoom = _roomRepository.GetRoomByName(editRoomRequest.NewName).Result;
                        // изменяем локацию для устройств, которые были подключены в изменённой комнате
                        foreach (var device in roomsDevices)
                        {
                            // т.к. происходило удаление устройств, не удовлетворяющих новым хар-кам, необходима проверка на null
                            if(device != null)
                                await _deviceRepository.UpdateDevice(device, newRoom, new(null, null, newRoom.Name));
                        }
                    }

                    // если есть устройства, которые были отключены - выводим их JSON формате, с помощью расширенения для List<DeviceView>, лежащего в HomeApi.Extensions
                    // возможно, лучше просто создать папку в этом проекте, т.к. другие проекты не могут ссылаться на HomeApi.Extensions. . .
                    var result = (deletedDevicesNames.Count != 0)
                        ? $"Комната \"{room.Name}\" успешно обновлена! Из неё было удалено {deletedDevicesNames.Count} устройств:\n {deletedDevicesNames.PrintElements()}"
                        : $"Комната \"{room.Name}\" успешно обновлена! Все устройства сохранились.";

                    return StatusCode(200, result);
                }
                // попытка обновления комнаты, если устройства отключать нельзя
                else
                {
                    // проверяем наличие устройств, которым не подходят новые хар-ки комнаты
                    bool existTroublesWithChar = roomsDevices.Any(device =>
                            (device.CurrentVolts > editRoomRequest.NewVoltage) || (device.GasUsage != default && editRoomRequest.ChangeGasConnection != default));

                    if (existTroublesWithChar) return StatusCode(400, $"Ошибка: нельзя обновить комнату, сохранив устройства!");
                    else
                    {
                        await _roomRepository.Update(room, new(editRoomRequest.NewArea, editRoomRequest.NewVoltage, editRoomRequest.ChangeGasConnection, editRoomRequest.ChangeAddDate, editRoomRequest.NewName));

                        // . . .
                        if (!string.IsNullOrEmpty(editRoomRequest.NewName) && roomsDevices != null)
                        {
                            // получаем обновлённую комнату
                            var newRoom = _roomRepository.GetRoomByName(editRoomRequest.NewName).Result;
                            // изменяем локацию для устройств, которые были подключены в изменённой комнате
                            foreach (var device in roomsDevices)
                            {
                                // т.к. происходило удаление устройств, не удовлетворяющих новым хар-кам, необходима проверка на null
                                if (device != null)
                                    await _deviceRepository.UpdateDevice(device, newRoom, new(null, null, newRoom.Name));
                            }
                        }

                        return StatusCode(200, $"Комната \"{room.Name}\" успешно обновлена!");
                    }
                }
            }
            // обновление комнаты, к которой не подключено ни одного устройства
            else
            {
                await _roomRepository.Update(room, new(editRoomRequest.NewArea, editRoomRequest.NewVoltage, editRoomRequest.ChangeGasConnection, editRoomRequest.ChangeAddDate, editRoomRequest.NewName));
                return StatusCode(200, $"Комната \"{room.Name}\" успешно обновлена!");
            }
        }
    }
}