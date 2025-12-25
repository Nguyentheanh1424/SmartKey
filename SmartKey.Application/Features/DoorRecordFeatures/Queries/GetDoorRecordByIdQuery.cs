using MediatR;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.DoorRecordFeatures.Dtos;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.DoorRecordFeatures.Queries
{
    public record GetDoorRecordByIdQuery(
        Guid DoorId,
        Guid RecordId
    ) : IRequest<DoorRecordDetailDto?>;

    public class GetDoorRecordByIdQueryHandler
        : IRequestHandler<GetDoorRecordByIdQuery, DoorRecordDetailDto?>
    {
        private readonly IUnitOfWork _uow;

        public GetDoorRecordByIdQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<DoorRecordDetailDto?> Handle(
            GetDoorRecordByIdQuery request,
            CancellationToken ct)
        {
            var repo = _uow.GetRepository<DoorRecord, Guid>();
            var record = await repo.GetByIdAsync(request.RecordId);

            if (record == null || record.DoorId != request.DoorId)
                return null;

            return new DoorRecordDetailDto
            {
                Id = record.Id,
                Event = record.Event,
                Method = record.Method,
                RawPayload = record.RawPayload,
                OccurredAt = record.OccurredAt
            };
        }
    }
}
