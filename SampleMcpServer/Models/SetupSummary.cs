namespace SampleMcpServer.Models;

/// <summary>
/// Aggregates results from the entire setup process.
/// </summary>
/// <param name="ProjectPath">Path to project that was analyzed</param>
/// <param name="DetectedTechnologies">All technologies found/selected</param>
/// <param name="FileResults">All file operations performed</param>
/// <param name="Warnings">Non-fatal issues encountered</param>
/// <param name="ExecutionTime">Total time taken for setup</param>
/// <param name="CompletedAt">When setup finished</param>
public record SetupSummary(
    string ProjectPath,
    List<TechnologyInfo> DetectedTechnologies,
    List<InstructionFileResult> FileResults,
    List<string> Warnings,
    TimeSpan ExecutionTime,
    DateTimeOffset CompletedAt
)
{
    /// <summary>Number of files created</summary>
    public int FilesCreated => FileResults.Count(r => r.Status == FileOperationStatus.Created);
    
    /// <summary>Number of files updated</summary>
    public int FilesUpdated => FileResults.Count(r => r.Status == FileOperationStatus.Updated);
    
    /// <summary>Number of files skipped</summary>
    public int FilesSkipped => FileResults.Count(r => r.Status == FileOperationStatus.Skipped);
    
    /// <summary>Number of files failed</summary>
    public int FilesFailed => FileResults.Count(r => r.Status == FileOperationStatus.Failed);
};
