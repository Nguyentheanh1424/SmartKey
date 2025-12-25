namespace SmartKey.Application.Features.AdminFeatures.Dtos
{
    public class SystemStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalDoors { get; set; }
        public int TotalDoorRecords { get; set; }

        public int TotalMqttInbox { get; set; }
        public int PendingMqttInbox { get; set; }
    }

}
