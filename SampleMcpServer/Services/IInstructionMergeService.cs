using SampleMcpServer.Models;

namespace SampleMcpServer.Services;

/// <summary>
/// Service for merging baseline instructions with company guidelines.
/// </summary>
public interface IInstructionMergeService
{
    /// <summary>
    /// Merges baseline instruction and company guidelines into a single instruction file content.
    /// Company guidelines override baseline content when sections overlap.
    /// </summary>
    Task<MergedInstructionContent> MergeAsync(
        BaselineInstruction? baseline,
        List<GuidelineContent> guidelines,
        CancellationToken cancellationToken = default);
}
