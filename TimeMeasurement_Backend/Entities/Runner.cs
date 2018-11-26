namespace TimeMeasurement_Backend.Entities
{
    public class Runner
    {
        public int Id { get; set; }
        public Participant Participant { get; set; }
        public Race Race { get; set; }
        public int Starter { get; set; }
        public long Time { get; set; }
    }
}