namespace SampleMcpServer.Models;

/// <summary>
/// Status of a file operation.
/// </summary>
public enum FileOperationStatus
{
    /// <summary>File was newly created</summary>
    Created,
    
    /// <summary>Existing file was updated</summary>
    Updated,
    
    /// <summary>Operation was skipped (file exists and updateExisting=false)</summary>
    Skipped,
    
    /// <summary>Operation failed</summary>
    Failed
}

/// <summary>
/// Represents the result of creating or updating an instruction file.
/// </summary>
/// <param name="FilePath">Absolute path where instruction file was written</param>
/// <param name="Technology">Technology name (null for repository-wide file)</param>
/// <param name="Status">Status of the operation</param>
/// <param name="BackupPath">Path to backup file if existing file was updated</param>
/// <param name="BytesWritten">Size of written file in bytes</param>
/// <param name="Message">Human-readable status message or warning</param>
public record InstructionFileResult(
    string FilePath,
    string? Technology,
    FileOperationStatus Status,
    string? BackupPath = null,
    long BytesWritten = 0,
    string? Message = null
);
