using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.UserAuthFeatures.Commands
{
    public class LogoutCommand : IRequest<Result>
    {
        public string RefreshToken { get; set; } = default!;
    }

    public class LogoutCommandHandler
        : IRequestHandler<LogoutCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<UserAuth, Guid> _userAuthRepository;

        public LogoutCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userAuthRepository =
                _unitOfWork.GetRepository<UserAuth, Guid>();
        }

        public async Task<Result> Handle(
            LogoutCommand request,
            CancellationToken cancellationToken)
        {
            var auth = await _userAuthRepository.FirstOrDefaultAsync(a =>
                a.RefreshToken == request.RefreshToken)
                ?? throw new UnauthorizedException("Token không hợp lệ.");

            auth.RevokeRefreshToken();

            await _userAuthRepository.UpdateAsync(auth);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success("Đăng xuất thành công.");
        }
    }
}
