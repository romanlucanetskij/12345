using MvcXmlConfigApp.Models;

namespace MvcXmlConfigApp.Services;

public interface ISettingsService
{
    Task<AppSettings> GetSettingsAsync(CancellationToken cancellationToken = default);

    Task UpdateSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
