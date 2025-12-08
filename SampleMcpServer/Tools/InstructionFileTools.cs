using ModelContextProtocol.Server;
using SampleMcpServer.Models;
using SampleMcpServer.Services;
using System.ComponentModel;

namespace SampleMcpServer.Tools;

/// <summary>
/// MCP tools for creating and managing instruction files.
/// </summary>
public class InstructionFileTools
{
    [McpServerTool]
    [Description("Creates or updates the repository-wide .github/copilot-instructions.md file with general coding standards and tech stack overview")]
    public async Task<InstructionFileResult> CreateRepositoryInstructionFile(
        [Description("Absolute path to the project directory")]
        string projectPath,
        [Description("List of technologies detected in the project")]
        List<string> technologies,
        [Description("Whether to update the file if it already exists (creates backup if true)")]
        bool updateExisting,
        ILogger<InstructionFileTools> logger,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
        {
            throw new ArgumentException("projectPath is required", nameof(projectPath));
        }

        if (technologies == null || technologies.Count == 0)
        {
            throw new ArgumentException("technologies list cannot be empty", nameof(technologies));
        }

        var githubDir = Path.Combine(projectPath, ".github");
        var filePath = Path.Combine(githubDir, "copilot-instructions.md");

        logger.LogInformation("Creating repository instruction file at {FilePath}", filePath);

        // Check if file exists
        if (File.Exists(filePath) && !updateExisting)
        {
            logger.LogWarning("File already exists and updateExisting=false, skipping");
            return new InstructionFileResult(
                FilePath: filePath,
                Technology: null,
                Status: FileOperationStatus.Skipped,
                Message: "File already exists. Use updateExisting=true to update."
            );
        }

        try
        {
            // Create .github directory if needed
            Directory.CreateDirectory(githubDir);

            // Create backup if updating
            string? backupPath = null;
            if (File.Exists(filePath) && updateExisting)
            {
                backupPath = CreateBackup(filePath, logger);
            }

            // Generate content
            var content = GenerateRepositoryInstructionContent(technologies);

            // Write file
            await File.WriteAllTextAsync(filePath, content, cancellationToken);

            var status = backupPath != null ? FileOperationStatus.Updated : FileOperationStatus.Created;
            var bytesWritten = new FileInfo(filePath).Length;

            logger.LogInformation("Repository instruction file {Status}: {BytesWritten} bytes", status, bytesWritten);

            return new InstructionFileResult(
                FilePath: filePath,
                Technology: null,
                Status: status,
                BackupPath: backupPath,
                BytesWritten: bytesWritten,
                Message: $"Repository-wide instruction file {status.ToString().ToLower()} successfully"
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create repository instruction file");
            return new InstructionFileResult(
                FilePath: filePath,
                Technology: null,
                Status: FileOperationStatus.Failed,
                Message: $"Failed to create file: {ex.Message}"
            );
        }
    }

    [McpServerTool]
    [Description("Creates or updates a technology-specific instruction file in .github/instructions/ directory with merged baseline and company guidelines")]
    public async Task<InstructionFileResult> CreateTechnologyInstructionFile(
        [Description("Absolute path to the project directory")]
        string projectPath,
        [Description("Technology name (lowercase)")]
        string technology,
        [Description("Baseline instruction content from awesome-copilot (null if not available)")]
        BaselineInstruction? baseline,
        [Description("List of company guideline contents for this technology (empty if none)")]
        List<GuidelineContent> guidelines,
        [Description("Whether to update the file if it already exists (creates backup if true)")]
        bool updateExisting,
        IInstructionMergeService mergeService,
        ILogger<InstructionFileTools> logger,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
        {
            throw new ArgumentException("projectPath is required", nameof(projectPath));
        }

        if (string.IsNullOrWhiteSpace(technology))
        {
            throw new ArgumentException("technology is required", nameof(technology));
        }

        if (baseline == null && (guidelines == null || guidelines.Count == 0))
        {
            throw new ArgumentException("Either baseline or guidelines must be provided");
        }

        var instructionsDir = Path.Combine(projectPath, ".github", "instructions");
        var fileName = $"{technology.ToLowerInvariant()}.instructions.md";
        var filePath = Path.Combine(instructionsDir, fileName);

        logger.LogInformation("Creating technology instruction file at {FilePath}", filePath);

        // Check if file exists
        if (File.Exists(filePath) && !updateExisting)
        {
            logger.LogWarning("File already exists and updateExisting=false, skipping");
            return new InstructionFileResult(
                FilePath: filePath,
                Technology: technology,
                Status: FileOperationStatus.Skipped,
                Message: "File already exists. Use updateExisting=true to update."
            );
        }

        try
        {
            // Create instructions directory if needed
            Directory.CreateDirectory(instructionsDir);

            // Create backup if updating
            string? backupPath = null;
            if (File.Exists(filePath) && updateExisting)
            {
                backupPath = CreateBackup(filePath, logger);
            }

            // Merge baseline and guidelines
            var merged = await mergeService.MergeAsync(baseline, guidelines ?? [], cancellationToken);

            // Write file
            var content = merged.ToMarkdown();
            await File.WriteAllTextAsync(filePath, content, cancellationToken);

            var status = backupPath != null ? FileOperationStatus.Updated : FileOperationStatus.Created;
            var bytesWritten = new FileInfo(filePath).Length;

            var message = $"Merged {merged.SourceSummary}";
            if (backupPath != null)
            {
                message += ", created backup";
            }

            logger.LogInformation("Technology instruction file {Status}: {BytesWritten} bytes, {Message}", 
                status, bytesWritten, message);

            return new InstructionFileResult(
                FilePath: filePath,
                Technology: technology,
                Status: status,
                BackupPath: backupPath,
                BytesWritten: bytesWritten,
                Message: message
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create technology instruction file for {Technology}", technology);
            return new InstructionFileResult(
                FilePath: filePath,
                Technology: technology,
                Status: FileOperationStatus.Failed,
                Message: $"Failed to create file: {ex.Message}"
            );
        }
    }

    private string CreateBackup(string filePath, ILogger logger)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var backupPath = $"{filePath}.{timestamp}.backup";
        
        File.Copy(filePath, backupPath, overwrite: true);
        logger.LogInformation("Created backup at {BackupPath}", backupPath);
        
        return backupPath;
    }

    private string GenerateRepositoryInstructionContent(List<string> technologies)
    {
        var techList = string.Join(", ", technologies.Select(FormatTechnologyName));
        
        return $"""
---
description: Repository-wide coding standards and GitHub Copilot instructions
applyTo: '**'
---

# GitHub Copilot Instructions

This repository uses the following technologies: {techList}

Technology-specific instructions can be found in `.github/instructions/` directory.

## General Guidelines

- Follow the coding standards defined for each technology
- Write clear, maintainable code
- Include appropriate documentation and comments
- Follow established patterns and conventions

## Technology Stack

{string.Join("\n", technologies.Select(t => $"- {FormatTechnologyName(t)}: See `.github/instructions/{t}.instructions.md`"))}

""";
    }

    private string FormatTechnologyName(string technology)
    {
        return technology switch
        {
            "csharp" => "C#",
            "dotnet" => ".NET",
            "typescript" => "TypeScript",
            "javascript" => "JavaScript",
            "golang" => "Go",
            _ => char.ToUpper(technology[0]) + technology[1..]
        };
    }
}
