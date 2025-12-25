namespace SmartKey.Application.Features.ICCardFeatures.Dtos
{
    public class ICCardDto
    {
        public Guid Id { get; set; }
        public string CardUid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
