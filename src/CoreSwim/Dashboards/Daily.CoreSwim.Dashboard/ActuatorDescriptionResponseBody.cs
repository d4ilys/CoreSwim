using Daily.CoreSwim.Actuators;

namespace Daily.CoreSwim.Dashboard
{
    public class ActuatorDescriptionResponseBody : ActuatorDescription
    {
        public string? JobStatusText { get; set; }
    }
}