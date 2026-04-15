using Hangfire;
using Hangfire.MemoryStorage;
using LiveExpert.Application.Interfaces;
using LiveExpert.Infrastructure.Data;
using LiveExpert.Infrastructure.Repositories;
using LiveExpert.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LiveExpert.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database - Using SQLite for cross-platform compatibility
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Core Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddScoped<ISystemSettingsService, SystemSettingsService>();
        services.AddScoped<IAPIKeyService, APIKeyService>();
        
        // Communication Services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISMSService, SMSService>();
        services.AddHttpClient<IWhatsAppService, WhatsAppService>();

        // External Services
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddHttpClient<PaymentService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
        services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
        services.AddScoped<ICalendarConnectionService, CalendarConnectionService>();
        services.AddScoped<IApiSettingService, ApiSettingService>();
        services.AddSingleton<EnvFileService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddHttpClient<GoogleOAuthService>();
        services.AddHttpClient<CalendarConnectionService>();
        
        // AI Services
        services.AddHttpClient<IResumeParserService, AffindaResumeParserService>();

        // Background Services
        services.AddHostedService<SessionReminderBackgroundService>();
        services.AddHostedService<TutorVerificationReminderService>();

        // Hangfire — background job queue for notifications (email, WhatsApp)
        // MemoryStorage: jobs survive SMTP blips but are lost on process restart (acceptable for notifications)
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseMemoryStorage());
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 2; // Low concurrency — notifications don't need many parallel workers
            options.Queues = new[] { "default" };
        });
        services.AddScoped<NotificationDispatchJob>();

        return services;
    }
}
