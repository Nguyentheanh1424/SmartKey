using FluentValidation;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.UserFeatures.Commands
{
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public record UpdateMyAvatarCommand(IFormFile? File, bool IsRandom = false) : IRequest<Result>;


    public class UpdateMyAvatarCommandValidator : AbstractValidator<UpdateMyAvatarCommand>
    {
        private static readonly string[] AllowedContentTypes =
            { "image/jpeg", "image/png", "image/webp" };

        private const long MaxBytes = 36 * 1024 * 1024;

        public UpdateMyAvatarCommandValidator()
        {
            When(x => !x.IsRandom, () =>
            {
                RuleFor(x => x.File)
                    .NotNull().WithMessage("Vui lòng chọn file ảnh avatar.");

                RuleFor(x => x.File!)
                    .Must(f => f.Length > 0)
                    .WithMessage("File ảnh rỗng.");

                RuleFor(x => x.File!)
                    .Must(f => f.Length <= MaxBytes)
                    .WithMessage($"File ảnh quá lớn (tối đa {MaxBytes / 1024 / 1024}MB).");

                RuleFor(x => x.File!)
                    .Must(f => AllowedContentTypes.Contains(f.ContentType))
                    .WithMessage("Định dạng ảnh không hỗ trợ (chỉ JPG/PNG/WEBP).");
            });

            When(x => x.IsRandom, () =>
            {
                RuleFor(x => x.File)
                    .Null().WithMessage("Không cần upload file khi chọn avatar random.");
            });
        }
    }

    public class UpdateMyAvatarCommandHandler
    : IRequestHandler<UpdateMyAvatarCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IAvatarGenerator _avatarGenerator;
        private readonly IFileStorageService _fileStorage;

        public UpdateMyAvatarCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUser,
            IAvatarGenerator avatarGenerator,
            IFileStorageService fileStorage)
        {
            _uow = uow;
            _currentUser = currentUser;
            _avatarGenerator = avatarGenerator;
            _fileStorage = fileStorage;
            _userRepository = _uow.GetRepository<User, Guid>();
        }

        public async Task<Result> Handle(
            UpdateMyAvatarCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Result.Failure("Không tìm thấy người dùng.");

            var oldAvatarUrl = user.AvatarUrl;

            if (request.IsRandom)
            {
                var avatarUrl = await _avatarGenerator.GenerateSvgAsync(user.Id.ToString());
                user.UpdateAvatar(avatarUrl);

                if (!string.IsNullOrWhiteSpace(oldAvatarUrl))
                {
                    try { await _fileStorage.DeleteAsync(oldAvatarUrl!, cancellationToken); } catch { }
                }
            }
            else
            {
                if (request.File == null || request.File.Length == 0)
                    return Result.Failure("Vui lòng chọn file ảnh avatar.");

                var ext = Path.GetExtension(request.File.FileName);
                if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

                var fileName = $"avatars/{user.Id}/{Path.GetFileNameWithoutExtension(request.File.FileName)}{ext}";

                await using var stream = request.File.OpenReadStream();
                var avatarUrl = await _fileStorage.UploadAsync(
                    stream,
                    fileName,
                    request.File.ContentType,
                    cancellationToken);

                user.UpdateAvatar(avatarUrl);

                if (!string.IsNullOrWhiteSpace(oldAvatarUrl))
                {
                    try { await _fileStorage.DeleteAsync(oldAvatarUrl!, cancellationToken); } catch { }
                }
            }

            await _uow.SaveChangesAsync(cancellationToken);
            return Result.Success("Cập nhật avatar thành công.");
        }
    }
}
