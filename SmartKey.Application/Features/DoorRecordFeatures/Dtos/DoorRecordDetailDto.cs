namespace SmartKey.Application.Features.DoorRecordFeatures.Dtos
{
    public class DoorRecordDetailDto
    {
        public Guid Id { get; set; }
        public string Event { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string RawPayload { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
    }
}
