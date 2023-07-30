using System;

namespace HomeApi.Contracts.Models.Rooms
{
    public class EditRoomRequest
    {
        public int NewArea { get; set; }
        public int NewVoltage { get; set; }
        public bool ChangeGasConnection { get; set; }
        /// <summary>
        /// Изменение вольтажа и подключение/отключение газа
        /// могут вызвать проблемы с уже подключенными дева-
        /// сами, поэтому апдейт комнаты может не произойти,
        /// если пользователь захочет сохранить устройства, 
        /// чьи характеристики будут не подходить для изме-
        /// нённой комнаты. 
        /// </summary>
        public bool KeepDevices { get; set; }
    }
}