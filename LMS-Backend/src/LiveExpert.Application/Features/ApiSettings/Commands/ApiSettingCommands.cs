using MediatR;
using LiveExpert.Application.Common;

namespace LiveExpert.Application.Features.ApiSettings.Commands;

public class UpdateApiSettingCommand : IRequest<Result<bool>>
{
    public string Provider { get; set; } = string.Empty;
    public string KeyName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool UpdateEnvFile { get; set; } = true;
}

public class GetApiSettingsCommand : IRequest<Result<Dictionary<string, string>>>
{
    public string Provider { get; set; } = string.Empty;
}

public class DeleteApiSettingCommand : IRequest<Result<bool>>
{
    public string Provider { get; set; } = string.Empty;
    public string KeyName { get; set; } = string.Empty;
}
