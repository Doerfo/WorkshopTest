namespace SampleMcpServer.Services;

/// <summary>
/// Service for caching awesome-copilot repository structure.
/// </summary>
public interface IAwesomeCopilotCacheService
{
    /// <summary>
    /// Gets the mapping of technology names to instruction file paths.
    /// Returns cached value if available and not expired, otherwise fetches from GitHub.
    /// </summary>
    Task<Dictionary<string, string>> GetTechnologyFilesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Forces a refresh of the cache from GitHub.
    /// </summary>
    Task RefreshCacheAsync(CancellationToken cancellationToken = default);
}
