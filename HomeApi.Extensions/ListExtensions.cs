using HomeApi.Contracts.Models.Devices;
using System.Collections.Generic;
using System.Text;

namespace HomeApi
{
    /// <summary>
    /// Класс-расширение для вывода устройств из списка
    /// </summary>
    public static class ListExtensions
    {
        public static string PrintElements(this List<DeviceView> listDevice)
        {
            StringBuilder sb = new();
            foreach(var view in listDevice)
            {
                if(listDevice.IndexOf(view) != listDevice.Count - 1) sb.AppendLine(view.ToString() + ",");
                else sb.AppendLine(view.ToString() + ";");
            }

            return sb.ToString();
        }
    }
}