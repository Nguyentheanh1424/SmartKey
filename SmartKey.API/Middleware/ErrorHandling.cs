using SmartKey.Application.Common.Exceptions;
using SmartKey.Domain.Common;

namespace SmartKey.API.Middleware
{
    public class ErrorHandling
    {
        private readonly RequestDelegate _next;

        public ErrorHandling(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                // 404 Dữ liệu không tìm thấy
                await HandleNotFoundExceptionAsync(context, ex);
            }
            catch (AppException ex)
            {
                // 400 Lỗi ứng dụng chung
                await HandleAppExceptionAsync(context, ex);
            }
            catch (SmartKey.Application.Common.Exceptions.ValidationException ex)
            {
                // 400 Lỗi xác thực dữ liệu
                await HandleValidationExceptionAsync(context, ex);
            }
            catch (UnauthorizedException ex)
            {
                // 401 Lỗi xác thực
                await HandleUnauthorizedExceptionAsync(context, ex);
            }
            catch (ForbiddenAccessException ex)
            {
                // 403 Lỗi phân quyền
                await HandleForbiddenAccessExceptionAsync(context, ex);
            }
            catch (BusinessException ex)
            {
                // 414 Lỗi nghiệp vụ
                await HandleBusinessExceptionAsync(context, ex);
            }
            catch (DomainException ex)
            {
                await HandleDomainExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                // 500 Lỗi hệ thống
                await HandleInternalExceptionAsync(context, ex);
            }
        }

        private async Task HandleDomainExceptionAsync(HttpContext context, DomainException ex)
        {
            context.Response.StatusCode = 415;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                message = ex.Message
            });
        }

        private async Task HandleBusinessExceptionAsync(HttpContext context, BusinessException ex)
        {
            context.Response.StatusCode = 414;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                message = ex.Message
            });
        }

        private async Task HandleForbiddenAccessExceptionAsync(HttpContext context, ForbiddenAccessException ex)
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                message = ex.Message
            });
        }

        private async Task HandleUnauthorizedExceptionAsync(HttpContext context, UnauthorizedException ex)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                message = ex.Message
            });
        }

        private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                message = ex.Message,
                errors = ex.Errors
            });

            return;
        }

        private async Task HandleNotFoundExceptionAsync(HttpContext context, NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                message = ex.Message
            });
        }

        private async Task HandleAppExceptionAsync(HttpContext context, AppException ex)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                message = ex.Message
            });
        }

        private async Task HandleInternalExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Internal server error",
                detail = ex.Message
            });
        }
    }
}
