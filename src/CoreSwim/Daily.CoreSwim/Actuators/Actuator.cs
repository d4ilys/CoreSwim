using Daily.CoreSwim.Retaining;

namespace Daily.CoreSwim.Actuators
{
    public class Actuator : ActuatorDescription
    {
        /// <summary>
        /// 获取下一次触发时间
        /// </summary>
        /// <param name="startAt"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual DateTime? GetNextOccurrence(DateTime startAt)
        {
            return null;
        }
    }
}