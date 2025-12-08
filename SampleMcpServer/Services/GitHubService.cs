using System.Text.Json;

namespace SampleMcpServer.Services;

/// <summary>
/// Implementation of GitHub API service for fetching awesome-copilot content.
/// </summary>
public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubService> _logger;

    public GitHubService(HttpClient httpClient, ILogger<GitHubService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Configure HttpClient for GitHub API
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SampleMcpServer/1.0");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    }

    public async Task<List<GitHubFile>> GetDirectoryContentsAsync(
        string owner, 
        string repo, 
        string path, 
        CancellationToken cancellationToken = default)
    {
        var url = $"https://api.github.com/repos/{owner}/{repo}/contents/{path}";
        _logger.LogDebug("Fetching directory contents from {Url}", url);

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var files = JsonSerializer.Deserialize<List<GitHubFileDto>>(json);

            return files?.Select(f => new GitHubFile(
                f.name ?? "",
                f.path ?? "",
                f.type ?? "",
                f.download_url,
                f.sha
            )).ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch directory contents from GitHub: {Url}", url);
            throw;
        }
    }

    public async Task<string> GetFileContentAsync(
        string owner, 
        string repo, 
        string path, 
        CancellationToken cancellationToken = default)
    {
        var url = $"https://raw.githubusercontent.com/{owner}/{repo}/main/{path}";
        _logger.LogDebug("Fetching file content from {Url}", url);

        try
        {
            var content = await _httpClient.GetStringAsync(url, cancellationToken);
            _logger.LogInformation("Successfully fetched {Path} from {Owner}/{Repo}", path, owner, repo);
            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch file content from GitHub: {Url}", url);
            throw;
        }
    }

    // DTO for GitHub API response
    private record GitHubFileDto(
        string? name,
        string? path,
        string? type,
        string? download_url,
        string? sha
    );
}
