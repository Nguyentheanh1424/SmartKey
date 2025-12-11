using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Infrastructure.Auth;
using SmartKey.Infrastructure.Persistence;
using SmartKey.Infrastructure.Repositories;
using SmartKey.Infrastructure.Services;
using StackExchange.Redis;

namespace SmartKey.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Đăng ký DbContext 
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection")
                ));

            // Đăng ký UnitOfWork
            services.AddScoped<DbContext, ApplicationDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Đăng ký các dịch vụ khác
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddSingleton<IDateTimeService, DateTimeService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddHttpClient<IOAuthVerifier, OAuthVerifier>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            //Đăng ký Cache
            var useRedis = bool.TryParse(configuration["Cache:UseRedis"], out var val) && val;

            if (useRedis)
            {
                var redisConnectionString = configuration.GetConnectionString("Redis")
                    ?? throw new InvalidOperationException("Redis connection string is not configured.");

                var redisConfig = new ConfigurationOptions
                {
                    EndPoints = { redisConnectionString },
                    User = configuration["Cache:RedisUser"],
                    Password = configuration["Cache:RedisPassword"],
                    AbortOnConnectFail = false,
                    Ssl = false,
                };

                var muxer = ConnectionMultiplexer.Connect(redisConfig);
                services.AddSingleton<IConnectionMultiplexer>(muxer);
                //Console.WriteLine($"Redis connected: {muxer.IsConnected} | Endpoint: {redisConnectionString}");


                services.AddScoped<ICacheService>(sp =>
                    new RedisCacheService(
                        sp.GetRequiredService<IConnectionMultiplexer>(),
                        configuration
                    ));
            }
            else
            {
                // Sử dụng Memory Cache fallback
                services.AddMemoryCache();
                services.AddScoped<ICacheService>(sp =>
                    new MemoryCacheService(
                        sp.GetRequiredService<IMemoryCache>(),
                        sp.GetRequiredService<IConfiguration>()
                    ));
            }

            return services;
        }
    }
}
