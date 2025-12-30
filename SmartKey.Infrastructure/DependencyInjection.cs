using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Application.Features.MQTTFeatures;
using SmartKey.Infrastructure.Auth;
using SmartKey.Infrastructure.MQTT;
using SmartKey.Infrastructure.Persistence;
using SmartKey.Infrastructure.Repositories;
using SmartKey.Infrastructure.Services;
using StackExchange.Redis;
using Supabase;

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
            services.AddScoped<IAvatarGenerator, DiceBearAvatarGenerator>();

            // Đăng ký Server Storage
            var supabaseOptions = new SupabaseStorageOptions();
            configuration.GetSection("Supabase").Bind(supabaseOptions);

            services.AddSingleton(supabaseOptions);

            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<SupabaseStorageOptions>();

                var client = new Client(
                    options.Url,
                    options.ServiceRoleKey,
                    new SupabaseOptions
                    {
                        AutoConnectRealtime = false,
                        AutoRefreshToken = false
                    }
                );

                return client;
            });

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

            // Đăng ký MQTT
            services.Configure<MqttOptions>(
                configuration.GetSection("Mqtt"));

            services.AddSingleton<IMqttClient>(_ =>
            {
                return new MqttClientFactory().CreateMqttClient();
            });


            services.AddSingleton<IMqttClientOptionsFactory, MqttClientOptionsFactory>();
            services.AddHostedService<MqttHostedService>();

            services.AddSingleton<IMqttPublisher, MqttPublisher>();
            services.AddSingleton<IDoorMqttService, DoorMqttService>();
            services.AddSingleton<IMqttMessageDispatcher, MqttMessageDispatcher>();

            services.AddScoped<DoorStateMessageHandler>();
            services.AddScoped<DoorBatteryMessageHandler>();
            services.AddScoped<DoorLogMessageHandler>();

            services.AddScoped<DoorPasscodesListHandler>();
            services.AddScoped<DoorICCardsListHandler>();


            return services;
        }
    }
}
