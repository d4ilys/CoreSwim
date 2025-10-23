using Daily.CoreSwim.Actuators;

namespace Daily.CoreSwim.Dashboard
{
    public class ActuatorDescriptionResponseBody : ActuatorDescription
    {
        public IEnumerable<ActuatorExecutionRecord> ExecutionRecords { get; set; }
    }
}