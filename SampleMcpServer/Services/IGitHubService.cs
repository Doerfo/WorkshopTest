namespace SampleMcpServer.Services;

/// <summary>
/// Service for interacting with GitHub API.
/// </summary>
public interface IGitHubService
{
    /// <summary>
    /// Fetches directory listing from a GitHub repository.
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repo">Repository name</param>
    /// <param name="path">Path within repository</param>
    Task<List<GitHubFile>> GetDirectoryContentsAsync(
        string owner, 
        string repo, 
        string path, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Fetches raw file content from a GitHub repository.
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repo">Repository name</param>
    /// <param name="path">Path to file within repository</param>
    Task<string> GetFileContentAsync(
        string owner, 
        string repo, 
        string path, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a file in a GitHub repository.
/// </summary>
public record GitHubFile(
    string Name,
    string Path,
    string Type,
    string? DownloadUrl,
    string? Sha
);
