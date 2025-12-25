using MediatR;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.ICCardFeatures.Dtos;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.ICCardFeatures.Queries
{
    public record GetICCardsQuery(Guid DoorId)
        : IRequest<List<ICCardDto>>;

    public class GetICCardsQueryHandler
        : IRequestHandler<GetICCardsQuery, List<ICCardDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetICCardsQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<ICCardDto>> Handle(
            GetICCardsQuery request,
            CancellationToken ct)
        {
            var repo = _uow.GetRepository<ICCard, Guid>();

            var cards = await repo.FindAsync(
                c => c.DoorId == request.DoorId);

            return cards.Select(c => new ICCardDto
            {
                Id = c.Id,
                CardUid = c.CardUid,
                Name = c.Name,
                IsActive = c.IsActive
            }).ToList();
        }
    }
}
