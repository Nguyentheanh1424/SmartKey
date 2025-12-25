using MediatR;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.PasscodeFeatures.Dtos;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.PasscodeFeatures.Queries
{
    public record GetPasscodesQuery(Guid DoorId)
        : IRequest<List<PasscodeDto>>;

    public class GetPasscodesQueryHandler
        : IRequestHandler<GetPasscodesQuery, List<PasscodeDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetPasscodesQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<PasscodeDto>> Handle(
            GetPasscodesQuery request,
            CancellationToken ct)
        {
            var repo = _uow.GetRepository<Passcode, Guid>();

            var passcodes = await repo.FindAsync(
                p => p.DoorId == request.DoorId);

            return passcodes.Select(p => new PasscodeDto
            {
                Id = p.Id,
                Code = p.Code,
                Type = p.Type.ToString(),
                ValidFrom = p.ValidFrom,
                ValidTo = p.ValidTo,
                IsActive = p.IsActive
            }).ToList();
        }
    }
}
