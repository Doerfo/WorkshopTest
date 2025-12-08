using ModelContextProtocol.Server;
using System.ComponentModel;

namespace SampleMcpServer.Prompts;

/// <summary>
/// Agent-mode prompt for orchestrating the complete Copilot instructions setup workflow.
/// </summary>
public class SetupCopilotInstructionsPrompt
{
    [McpServerPrompt]
    [Description("Orchestrates the complete workflow to analyze a project and set up GitHub Copilot instruction files with baselines and company guidelines")]
    public async Task<string> SetupCopilotInstructions(
        [Description("Absolute path to the project directory to set up")]
        string projectPath,
        [Description("Whether to update existing instruction files (true) or skip if they exist (false)")]
        bool updateExisting = false,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For async consistency

        return $"""
You are setting up GitHub Copilot instruction files for a project. Follow these steps:

## STEP 1: DISCOVER TECHNOLOGIES

1. Invoke the **ListAvailableTechnologies** tool to see what technologies the MCP server supports
2. Invoke the **DetectProjectTechnologies** tool with:
   - projectPath: "{projectPath}"

If NO technologies are detected:
   - Show the user the list of available technologies
   - Ask the user to select technologies manually
   - Proceed with user-selected technologies

If technologies ARE detected:
   - Review the detected technologies with the user
   - Note any with "Medium" or "Low" confidence
   - Ask if the user wants to add/remove any technologies
   - Proceed with confirmed technology list

## STEP 2: RETRIEVE CONTENT

For EACH confirmed technology:
   1. Invoke **GetBaselineInstruction** with the technology name
      - If baseline not found, note this as a warning
      - Continue with company guidelines only
   
   2. Invoke **GetCompanyGuideline** with the technology name
      - If no guidelines found, note this (not an error)
      - Continue with baseline only

## STEP 3: CHECK FOR EXISTING FILES

Check if the following files exist:
   - {projectPath}/.github/copilot-instructions.md
   - {projectPath}/.github/instructions/[technology].instructions.md (for each technology)

If updateExisting is FALSE and files exist:
   - Warn the user which files will be SKIPPED
   - Ask if they want to proceed or cancel
   
If updateExisting is TRUE:
   - Inform user that BACKUPS will be created for existing files
   - Proceed with updates

## STEP 4: CREATE INSTRUCTION FILES

1. Invoke **CreateRepositoryInstructionFile** with:
   - projectPath: "{projectPath}"
   - technologies: [list of confirmed technology names]
   - updateExisting: {updateExisting.ToString().ToLower()}

2. For EACH technology, invoke **CreateTechnologyInstructionFile** with:
   - projectPath: "{projectPath}"
   - technology: [technology name]
   - baseline: [result from GetBaselineInstruction or null]
   - guidelines: [result from GetCompanyGuideline or empty list]
   - updateExisting: {updateExisting.ToString().ToLower()}

## STEP 5: REPORT RESULTS

Provide a comprehensive summary:

### ✅ Files Created/Updated:
   - List each file with its status (Created, Updated, Skipped, Failed)
   - Show file paths
   - Note backup paths if any files were updated

### 📊 Technologies Covered:
   - List each technology with:
     - ✅ Has baseline from awesome-copilot
     - ✅ Has company guidelines
     - ⚠️  Missing baseline (if applicable)
     - ⚠️  Missing guidelines (if applicable)

### ⚠️  Warnings (if any):
   - Technologies without baselines
   - Technologies without guidelines  
   - Files that were skipped
   - Any errors encountered

### 📝 Next Steps:
   - Verify that GitHub Copilot respects the new instructions
   - Review and customize instruction files if needed
   - Add company-specific guidelines to improve future setups

## IMPORTANT NOTES:

- Always confirm with the user before creating/updating files
- Provide clear progress updates after each step
- Handle errors gracefully - partial success is acceptable
- If GitHub API fails, note that cached baselines may be used
- Encourage the user to review generated files

Begin the setup process now.
""";
    }
}
