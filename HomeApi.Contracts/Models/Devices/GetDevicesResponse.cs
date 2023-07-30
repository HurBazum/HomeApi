using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace HomeApi.Contracts.Models.Devices
{
    public class GetDevicesResponse
    {
        public int DeviceAmount { get; set; }
        public DeviceView [] Devices { get; set; }
    }

    public class DeviceView
    {
        public DateTime AddDate { get; set; } 
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public int CurrentVolts { get; set; }
        public bool GasUsage { get; set; }


        public string Location  { get; set; }

        // конвертация в json
        public override string ToString()
        {
            // опции для сериализации 
            var options = new JsonSerializerOptions
            {
                // переводит /u.., если не используется кириллица - можно опустить
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                // перенос строки
                WriteIndented = true
            };

            return JsonSerializer.Serialize(this, options);
        }
    }
}