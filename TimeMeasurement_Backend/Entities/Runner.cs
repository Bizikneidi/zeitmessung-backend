namespace TimeMeasurement_Backend.Entities.Entities
{
    public class Runner
    {
        public int Starter { get; set; }
        public Time Time { get; set; }
        public Participant Participant { get; set; }
        public Race Race { get; set; }
    }
}