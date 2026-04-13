using LiveExpert.Domain.Common;
using LiveExpert.Domain.Enums;

namespace LiveExpert.Domain.Entities;

/// <summary>
/// Stores encrypted API keys and credentials for external services
/// Managed only by admins, never exposed to frontend
/// </summary>
public class ApiSetting : BaseEntity
{
    public ApiProvider Provider { get; set; }
    public string KeyName { get; set; } = string.Empty;
    
    /// <summary>
    /// Encrypted key value
    /// </summary>
    public string KeyValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this API key is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Additional metadata (JSON string)
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Description of what this key is used for
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Last time this key was used/validated
    /// </summary>
    public DateTime? LastValidatedAt { get; set; }
}
