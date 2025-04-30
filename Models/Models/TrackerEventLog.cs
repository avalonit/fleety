namespace Listener.Models
{
    public class TrackerEventLog
    {
        public TrackerEvent? destinationEvent { get; set; }
        public string? sourceEvent { get; set; }

        public string? error { get; set; }

    }
}
