namespace SampleMcpServer.Services;

/// <summary>
/// In-memory cache for awesome-copilot repository structure with 24-hour TTL.
/// </summary>
public class AwesomeCopilotCacheService : IAwesomeCopilotCacheService
{
    private readonly record struct CacheEntry(
        Dictionary<string, string> TechnologyFiles,
        DateTimeOffset CachedAt
    );

    private CacheEntry? _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IGitHubService _gitHubService;
    private readonly ILogger<AwesomeCopilotCacheService> _logger;

    public AwesomeCopilotCacheService(
        IGitHubService gitHubService,
        ILogger<AwesomeCopilotCacheService> logger)
    {
        _gitHubService = gitHubService;
        _logger = logger;
    }

    public async Task<Dictionary<string, string>> GetTechnologyFilesAsync(
        CancellationToken cancellationToken = default)
    {
        // Check cache validity
        if (_cache is { } cache && 
            DateTimeOffset.UtcNow - cache.CachedAt < _cacheDuration)
        {
            _logger.LogDebug("Returning cached technology files (age: {Age})", 
                DateTimeOffset.UtcNow - cache.CachedAt);
            return cache.TechnologyFiles;
        }

        // Cache miss or expired - refresh
        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_cache is { } cache2 && 
                DateTimeOffset.UtcNow - cache2.CachedAt < _cacheDuration)
            {
                return cache2.TechnologyFiles;
            }

            _logger.LogInformation("Cache miss or expired, fetching from GitHub");
            var files = await FetchFromGitHubAsync(cancellationToken);
            _cache = new CacheEntry(files, DateTimeOffset.UtcNow);
            return files;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Manually refreshing cache");
            var files = await FetchFromGitHubAsync(cancellationToken);
            _cache = new CacheEntry(files, DateTimeOffset.UtcNow);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<Dictionary<string, string>> FetchFromGitHubAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var files = await _gitHubService.GetDirectoryContentsAsync(
                "github", 
                "awesome-copilot", 
                "instructions",
                cancellationToken);

            var technologyFiles = files
                .Where(f => f.Name.EndsWith(".instructions.md", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(
                    f => f.Name.Replace(".instructions.md", "", StringComparison.OrdinalIgnoreCase),
                    f => f.Path
                );

            _logger.LogInformation("Fetched {Count} technology instruction files from awesome-copilot", 
                technologyFiles.Count);

            return technologyFiles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch from GitHub");
            
            // If we have stale cache, use it
            if (_cache is { } staleCache)
            {
                _logger.LogWarning("Using stale cache due to GitHub error");
                return staleCache.TechnologyFiles;
            }

            throw;
        }
    }
}
