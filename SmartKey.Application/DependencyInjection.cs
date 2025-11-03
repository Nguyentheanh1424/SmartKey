using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SmartKey.Application.Common.Behaviors;
using System.Reflection;

namespace SmartKey.Application
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Đăng ký toàn bộ dịch vụ của Application layer vào DI container
        /// Gọi một lần trong Program.cs của API: build.Services.AddApplication();
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Đăng ký các dịch vụ của Application ở đây nếu cần thiết
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()
            ));

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

            return services;
        }
    }
}
