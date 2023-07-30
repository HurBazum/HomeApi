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
        /// Изменение комнаты
        /// </summary>
        [HttpPatch]
        [Route("Update/{name}")]
        public async Task<IActionResult> Update([FromRoute] string name, [FromBody] EditRoomRequest editRoomRequest)
        {
            var room = _roomRepository.GetRoomByName(name).Result;
            if (room == null) return NotFound($"Ошибка: Комната с именем \"{name}\" не подключена");

            // получаем все устройства, подключенные в данной комнате,
            // чтобы либо отключить их, если новые параметры комнаты
            // будут для них неподходящими, либо отказать в обновлении
            // самой комнаты, если пользователь в реквесте указал,
            // что хочет сохранить все устройства
            var roomsDevices = _deviceRepository.GetDevicesByRoom(room).Result;

            // для вывода имём удалённых устройств и их подсчёта, если такие имеются
            List<DeviceView> deletedDevicesNames = new();

            if (roomsDevices.Length != 0)
            {
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

                    await _roomRepository.Update(room, new(editRoomRequest.NewArea, editRoomRequest.NewVoltage, editRoomRequest.ChangeGasConnection));

                    string result = (deletedDevicesNames.Count != 0)
                        ? $"Комната \"{room.Name}\" успешно обновлена! Из неё было удалено {deletedDevicesNames.Count} устройств:\n {deletedDevicesNames.PrintElements()}"
                        : $"Комната \"{room.Name}\" успешно обновлена! Все устройства сохранились.";

                    return StatusCode(200, result);
                }
                else
                {
                    // проверяем наличие устройств, которым не подходят новые хар-ки комнаты
                    bool existTroublesWithChar = roomsDevices.Any(device =>
                            (device.CurrentVolts > editRoomRequest.NewVoltage) || (device.GasUsage != default && editRoomRequest.ChangeGasConnection != default));

                    if (existTroublesWithChar) return StatusCode(400, $"Ошибка: нельзя обновить комнату, сохранив устройства!");
                    else
                    {
                        await _roomRepository.Update(room, new(editRoomRequest.NewArea, editRoomRequest.NewVoltage, editRoomRequest.ChangeGasConnection));
                        return StatusCode(200, $"Комната \"{room.Name}\" успешно обновлена!");
                    }
                }
            }
            else
            {
                await _roomRepository.Update(room, new(editRoomRequest.NewArea, editRoomRequest.NewVoltage, editRoomRequest.ChangeGasConnection));
                return StatusCode(200, $"Комната \"{room.Name}\" успешно обновлена!");
            }
        }
    }
}