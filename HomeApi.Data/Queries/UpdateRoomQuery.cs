using System;

namespace HomeApi.Data.Queries
{
    /// <summary>
    /// Класс для передачи дополнительных параметров для изменения комнаты
    /// </summary>
    public class UpdateRoomQuery
    {
        public int? NewArea { get; init; }
        public int? NewVoltage { get; init; }
        public bool ChangeGasConnection { get; init; }
        public DateTime? UpdateDate { get; init; }
        public string NewName { get; init; }


        public UpdateRoomQuery(int? area, int? voltage, bool gasConnection, bool changeDate, string name)
        {
            NewArea = area;
            NewVoltage = voltage;
            ChangeGasConnection = gasConnection;
            NewName = name;
            if(changeDate) UpdateDate = DateTime.Now;
        }
    }
}