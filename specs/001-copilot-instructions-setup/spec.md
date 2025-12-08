# Feature Specification: Copilot Instructions Setup MCP Server

**Feature Branch**: `001-copilot-instructions-setup`  
**Created**: December 7, 2025  
**Status**: Draft  
**Input**: User description: "The mcp server should help projects to setup good github copilot and vscode focused instruction files."

## Clarifications

### Session 2025-12-07

- Q: When baseline instructions and company guidelines both provide rules for the same aspect (e.g., both define naming conventions for C# methods), which content should take precedence in the final generated instruction file? → A: Company guidelines override conflicting baseline content (company-first approach)
- Q: When the system cannot retrieve baseline instructions from the awesome-copilot repository (network error, repository unavailable, or technology not found), what should happen? → A: Continue with company guidelines only and warn user about missing baseline
- Q: When merging content from multiple sources (baseline + company guidelines) into a single instruction file, how should duplicate or overlapping guidance be handled? → A: Automatically deduplicate, keeping company guideline version when overlap detected
- Q: When a project has no detectable technology stack (empty project, ambiguous files, or unsupported languages), what should the setup process do? → A: Prompt user to select technologies from a predefined list
- Q: When updating existing instruction files that contain custom user modifications (content not from baseline or guidelines), how should the system preserve user customizations? → A: Create a backup file before overwriting, allow restoration if needed
- Q: When multiple technologies are detected, how should instruction files be organized? → A: Create a separate instruction file in `.github/instructions/` for each technology, retrieving corresponding baseline from awesome-copilot for each
- Q: When the awesome-copilot repository structure changes (e.g., baseline files are renamed, moved, or reorganized), how should the system handle this breaking change? → A: Attempt pattern-based discovery of new file locations in the repository

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Initial Project Setup (Priority: P1)

A developer wants to quickly set up GitHub Copilot instructions for a new project. They invoke a setup prompt that analyzes their project's technology stack and automatically creates repository-wide and path-specific instruction files based on industry best practices and official guidance from the awesome-copilot repository.

**Why this priority**: This is the core value proposition - getting from zero to properly configured Copilot instructions with minimal manual effort. Without this, users have no way to leverage the MCP server.

**Independent Test**: Can be fully tested by running the setup prompt on a sample project (e.g., a .NET MCP server) and verifying that appropriate instruction files are created with relevant technology-specific guidance.

**Acceptance Scenarios**:

1. **Given** a project with a clear technology stack (e.g., C# .NET project), **When** user invokes the setup prompt, **Then** the system analyzes the project structure and identifies the primary technologies
2. **Given** identified technologies match available baseline instructions, **When** baseline instructions are retrieved, **Then** the system fetches the correct instruction files from the awesome-copilot repository
3. **Given** baseline instructions are retrieved, **When** the system generates instruction files, **Then** a repository-wide `.github/copilot-instructions.md` file is created with general coding standards
4. **Given** multiple technologies are detected (e.g., C#, TypeScript, React), **When** path-specific instructions are generated, **Then** a separate instruction file is created in `.github/instructions/` directory for each technology (e.g., `csharp.instructions.md`, `typescript.instructions.md`, `react.instructions.md`), each populated with its corresponding baseline from awesome-copilot

---

### User Story 2 - Apply Company-Specific Guidelines (Priority: P2)

A developer working in an organization with specific coding standards wants to enhance auto-generated instructions with company-specific guidelines. The MCP server provides tools to incorporate pre-defined company guideline files (e.g., `csharp-testing.md`, `angular.md`) into the generated instructions based on detected technologies.

**Why this priority**: Enables organizations to enforce consistent coding practices across projects. This builds on the basic setup (P1) by adding customization capabilities.

**Independent Test**: Can be tested by placing company guideline files in the MCP server's guideline directory, running the setup prompt, and verifying that relevant guidelines are merged into the generated instruction files.

**Acceptance Scenarios**:

1. **Given** company guideline files exist in the MCP server project (e.g., `guidelines/csharp-testing.md`), **When** the setup prompt detects C# in the project, **Then** the C# testing guidelines are incorporated into the `csharp.instructions.md` file along with the C# baseline from awesome-copilot
2. **Given** multiple applicable guidelines exist for detected technologies, **When** instructions are generated, **Then** all relevant guidelines are merged into appropriate instruction files with automatic deduplication (company guidelines override baseline when overlap occurs)
3. **Given** a guideline file specifies rules for a particular technology, **When** that technology is not detected in the project, **Then** that guideline is not included in the generated instructions
4. **Given** a project uses multiple technologies (e.g., C#, TypeScript, React, Angular), **When** instructions are generated, **Then** each technology gets its own instruction file with its specific baseline from awesome-copilot and relevant company guidelines merged together

---

### User Story 3 - Update Existing Instructions (Priority: P2)

A developer with existing Copilot instruction files wants to refresh them with updated best practices or newly added company guidelines. The setup prompt detects existing instructions and provides options to merge new content or replace outdated sections, with warnings about removed content.

**Why this priority**: Projects evolve, and instructions need to stay current. This enables maintenance of existing setups without starting from scratch.

**Independent Test**: Can be tested by running the setup prompt on a project that already has instruction files, making choices about merging or replacing, and verifying the final state matches expectations with appropriate warnings shown.

**Acceptance Scenarios**:

1. **Given** existing instruction files are present, **When** the setup prompt is invoked, **Then** the system detects existing files and prompts the user about merge or replace options
2. **Given** user chooses to merge instructions, **When** new content overlaps with existing content, **Then** the system automatically deduplicates by keeping company guideline versions and removing overlapping baseline content
3. **Given** user chooses to replace specific sections, **When** content is removed, **Then** the system creates a timestamped backup file and warns the user about what content will be deleted before proceeding
4. **Given** the setup completes with modifications to existing files, **When** the process finishes, **Then** a summary is shown listing all changes, additions, and removals

---

### User Story 4 - Maintain and Extend Guidelines (Priority: P3)

A developer or organization wants to add new company-specific guidelines or modify existing ones. They can add, modify, or remove guideline files in the MCP server project, update the server configuration if needed, and re-run the setup prompt to regenerate instructions with the updated guidelines.

**Why this priority**: Ensures the solution remains flexible and can adapt to changing organizational standards. This is maintenance/extensibility rather than core functionality.

**Independent Test**: Can be tested by adding a new guideline file to the MCP server, restarting or updating the server, running the setup prompt, and verifying the new guideline is incorporated.

**Acceptance Scenarios**:

1. **Given** a new guideline file is added to the MCP server's guidelines directory, **When** the MCP server is updated/restarted, **Then** the new guideline becomes available for inclusion in project setups
2. **Given** an existing guideline file is modified, **When** the setup prompt is re-run on a project, **Then** the updated guideline content is reflected in the regenerated instructions
3. **Given** a guideline file is removed from the MCP server, **When** the setup prompt is run, **Then** that guideline is no longer incorporated into any project instructions
4. **Given** guideline files follow a consistent naming convention, **When** new guidelines are added, **Then** they are automatically discovered and mapped to appropriate technologies without code changes

---

### Edge Cases

- What happens when the project uses a technology not covered by awesome-copilot baseline instructions or company guidelines? *(Resolved: Prompt user for manual selection, create instruction file with available content)*
- How does the system handle projects with mixed or ambiguous technology stacks (e.g., multiple backend frameworks)? *(Resolved: Each technology gets separate instruction file with its baseline)*
- What happens when baseline instructions from awesome-copilot are unavailable (network error, repository changes)? *(Resolved: Continue with guidelines, warn user)*
- How does the system handle conflicts between baseline instructions and company guidelines (which takes precedence)? *(Resolved: Company guidelines override baseline)*
- What happens when existing instruction files have custom user modifications that don't match any baseline or guideline pattern? *(Resolved: Backup created before overwrite)*
- How does the system handle very large projects with many different technologies? *(Each technology gets its own instruction file, scaled appropriately)*
- What happens if the awesome-copilot repository structure changes? *(Resolved: Use GitHub MCP tool for pattern-based discovery of new locations)*

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an MCP prompt/tool that can be invoked to set up Copilot instructions for a project
- **FR-002**: System MUST analyze the project structure to identify primary technologies and frameworks used (e.g., C#, .NET, TypeScript, Angular, React)
- **FR-003**: System MUST retrieve appropriate baseline instruction files from the awesome-copilot repository (https://github.com/github/awesome-copilot/tree/main/instructions) for each detected technology individually
- **FR-004**: System MUST support storing company-specific coding guideline files within the MCP server project (e.g., in a `guidelines/` directory)
- **FR-005**: System MUST apply relevant company guidelines to generated instructions based on detected technologies, with company guidelines taking precedence over baseline instructions when both provide conflicting rules for the same aspect
- **FR-006**: System MUST generate a repository-wide instruction file at `.github/copilot-instructions.md` with general coding standards and tech stack information
- **FR-007**: System MUST generate separate instruction files in `.github/instructions/` directory for each detected technology using naming pattern `<technology>.instructions.md` (e.g., `csharp.instructions.md`, `typescript.instructions.md`, `react.instructions.md`, `angular.instructions.md`), with each file containing the baseline from awesome-copilot plus relevant company guidelines for that technology
- **FR-008**: System MUST follow the additive instruction file pattern where repository-wide instructions are always applied and path-specific instructions stack with others
- **FR-009**: System MUST detect existing instruction files and provide options to merge or replace content
- **FR-010**: System MUST warn users when existing content will be removed or replaced
- **FR-011**: System MUST support adding, modifying, or removing company guideline files with the ability to regenerate instructions after changes
- **FR-012**: System MUST provide a summary report after setup completion showing what files were created, modified, or would be removed
- **FR-013**: System MUST handle cases where baseline instructions are unavailable (network error, repository issues, or technology not found) by continuing with company guidelines only and warning the user about missing baseline content
- **FR-014**: System MUST use a consistent naming convention for guideline files that maps to technology names (e.g., `csharp-testing.md`, `angular.md`, `typescript.md`)
- **FR-015**: System MUST prompt users to manually select technologies from a predefined list when automatic detection finds no recognizable technology stack
- **FR-016**: System MUST create backup files (with timestamp suffix) of existing instruction files before overwriting them during updates, allowing users to restore previous versions if needed
- **FR-017**: System MUST use GitHub repository inspection capabilities (e.g., GitHub MCP tool) to discover baseline instruction file locations when the awesome-copilot repository structure changes, attempting pattern-based discovery before falling back to error handling

### Key Entities

- **Baseline Instruction**: Official instruction templates from the awesome-copilot repository, organized by technology/framework, containing industry best practices for that technology
- **Company Guideline**: Organization-specific coding standards and practices stored as markdown files in the MCP server, mapped to specific technologies
- **Project Analysis**: Results of scanning a project structure, including detected technologies, frameworks, project structure (backend/frontend/etc.), and existing instruction files
- **Instruction File**: Generated markdown file following GitHub Copilot's instruction file format, with metadata (description, applyTo pattern) and content sections
- **Setup Configuration**: Parameters and options for the setup process, including merge/replace preferences, selected guidelines, and output paths

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can generate complete Copilot instruction files for a new project in under 2 minutes from invocation to completion
- **SC-002**: The setup prompt correctly identifies at least 90% of mainstream technologies and frameworks in typical projects (C#, JavaScript/TypeScript, Python, Java, React, Angular, Vue, .NET, etc.)
- **SC-003**: Generated instruction files contain relevant, technology-specific guidance that improves Copilot suggestion quality for that technology
- **SC-004**: Developers can add a new company guideline file and regenerate instructions without modifying MCP server code
- **SC-005**: The system successfully retrieves baseline instructions from the awesome-copilot repository when available, and gracefully continues with company guidelines while warning users when baseline instructions are unavailable
- **SC-006**: Users receive clear warnings before any existing instruction content is removed, with the ability to review changes before committing
- **SC-007**: Generated instruction files follow the correct format and structure that GitHub Copilot recognizes and applies

## Assumptions

- The awesome-copilot repository structure remains relatively stable, or the MCP server can adapt to changes using GitHub repository inspection (via GitHub MCP tool)
- Company guideline files are written in markdown format compatible with Copilot instruction files
- Project technologies can be detected through standard indicators (file extensions, package.json, .csproj files, etc.)
- Users have network access to retrieve baseline instructions from GitHub
- The MCP server has file system access to read project structure and write instruction files
- Instruction file format follows the applyTo pattern with YAML frontmatter (description, applyTo glob patterns)
- Technology names map directly to instruction filenames (e.g., `csharp` → `csharp.instructions.md`, `typescript` → `typescript.instructions.md`)
- The awesome-copilot repository structure allows lookup of baselines by technology name
- Each technology's instruction file uses `applyTo` patterns to target relevant file paths for that technology
