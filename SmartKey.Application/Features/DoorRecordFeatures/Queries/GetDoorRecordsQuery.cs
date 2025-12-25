using MediatR;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.DoorRecordFeatures.Dtos;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.DoorRecordFeatures.Queries
{
    public record GetDoorRecordsQuery(Guid DoorId)
        : IRequest<List<DoorRecordDto>>;

    public class GetDoorRecordsQueryHandler
        : IRequestHandler<GetDoorRecordsQuery, List<DoorRecordDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetDoorRecordsQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<DoorRecordDto>> Handle(
            GetDoorRecordsQuery request,
            CancellationToken ct)
        {
            var repo = _uow.GetRepository<DoorRecord, Guid>();

            var records = await repo.FindAsync(
                r => r.DoorId == request.DoorId);

            return records
                .OrderByDescending(r => r.OccurredAt)
                .Select(r => new DoorRecordDto
                {
                    Id = r.Id,
                    Event = r.Event,
                    Method = r.Method,
                    OccurredAt = r.OccurredAt
                })
                .ToList();
        }
    }
}
