using LiveExpert.Application.Interfaces;
using LiveExpert.Domain.Entities;
using LiveExpert.Domain.Enums;
using NotificationChannelEnum = LiveExpert.Domain.Enums.NotificationChannel;
using System.Linq;

namespace LiveExpert.Infrastructure.Services;

public class NotificationPreferenceService : INotificationPreferenceService
{
    private readonly IRepository<UserNotificationPreference> _preferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationPreferenceService(
        IRepository<UserNotificationPreference> preferenceRepository,
        IUnitOfWork unitOfWork)
    {
        _preferenceRepository = preferenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<UserNotificationPreference>> GetOrCreateDefaultsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var existing = (await _preferenceRepository.FindAsync(p => p.UserId == userId, cancellationToken)).ToList();
        if (existing.Any())
        {
            return existing;
        }

        var defaults = Enum.GetValues<NotificationCategory>()
            .Select(category => new UserNotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Category = category,
                EmailEnabled = true,
                WhatsAppEnabled = true,
                InAppEnabled = true
            })
            .ToList();

        await _preferenceRepository.AddRangeAsync(defaults, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return defaults;
    }

    public async Task<bool> IsChannelEnabledAsync(
        Guid userId,
        NotificationCategory category,
        NotificationChannelEnum channel,
        bool isTransactional,
        CancellationToken cancellationToken = default)
    {
        if (isTransactional)
        {
            return true;
        }

        var preferences = await GetOrCreateDefaultsAsync(userId, cancellationToken);
        var preference = preferences.FirstOrDefault(p => p.Category == category);
        if (preference == null)
        {
            return true;
        }

        return channel switch
        {
            NotificationChannelEnum.Email => preference.EmailEnabled,
            NotificationChannelEnum.WhatsApp => preference.WhatsAppEnabled,
            NotificationChannelEnum.InApp => preference.InAppEnabled,
            _ => true
        };
    }

    public async Task UpdatePreferencesAsync(Guid userId, List<UserNotificationPreference> preferences, CancellationToken cancellationToken = default)
    {
        var existing = (await _preferenceRepository.FindAsync(p => p.UserId == userId, cancellationToken)).ToList();
        foreach (var preference in preferences)
        {
            var current = existing.FirstOrDefault(p => p.Category == preference.Category);
            if (current == null)
            {
                preference.Id = Guid.NewGuid();
                preference.UserId = userId;
                await _preferenceRepository.AddAsync(preference, cancellationToken);
                continue;
            }

            current.EmailEnabled = preference.EmailEnabled;
            current.WhatsAppEnabled = preference.WhatsAppEnabled;
            current.InAppEnabled = preference.InAppEnabled;
            await _preferenceRepository.UpdateAsync(current, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
