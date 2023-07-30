namespace HomeApi.Data.Queries
{
    public class UpdateRoomQuery
    {
        public int NewArea { get; set; }
        public int NewVoltage { get; set; }
        public bool ChangeGasConnection { get; set; }

        public UpdateRoomQuery(int area, int voltage, bool gasConnection)
        {
            NewArea = area;
            NewVoltage = voltage;
            ChangeGasConnection = gasConnection;
        }
    }
}