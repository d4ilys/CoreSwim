using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daily.CoreSwim.Retaining
{
    internal static class Penetrates
    {
        /// <summary>
        /// 获取标准计算时间
        /// </summary>
        /// <remarks>忽略毫秒之后的部分</remarks>
        /// <param name="dateTime"><see cref="DateTime"/></param>
        /// <returns><see cref="DateTime"/></returns>
        internal static DateTime GetStandardDateTime(DateTime dateTime)
        {
            // 忽略毫秒之后部分并重新返回新的时间，主要用于减少时间计算误差
            return new DateTime(dateTime.Year
                , dateTime.Month
                , dateTime.Day
                , dateTime.Hour
                , dateTime.Minute
                , dateTime.Second);
        }

       internal static string GetPeriod(long interval)
       {
          //根据interval获取时时间间隔，适配 毫秒 秒 分钟 小时
          return interval switch
          {
              < 1000 => $"{interval}ms",
              < 60000 => $"{interval / 1000}s",
              < 3600000 => $"{interval / 60000}m",
              < 86400000 => $"{interval / 3600000}h",
              _ => $"{interval / 86400000}d"
          };
       }
    }
}
