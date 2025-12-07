# Implementation Plan: In-Memory Notes Store

**Branch**: `001-notes-store` | **Date**: December 7, 2025 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-notes-store/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement an in-memory note storage system for the SampleMcpServer that allows users to create, retrieve, list, search, update, and delete notes through MCP tools and prompts. Notes contain auto-generated IDs, titles, content, and timestamps. The system uses .NET 10 with the ModelContextProtocol SDK, stores notes in a thread-safe Dictionary, and exposes 6 tools and 5 prompts for AI assistant interaction.

## Technical Context

**Language/Version**: C# 13 / .NET 10 SDK  
**Primary Dependencies**: ModelContextProtocol 0.5.0-preview.1, ModelContextProtocol.AspNetCore 0.5.0-preview.1, Microsoft.Extensions.Hosting 10.0.0  
**Storage**: In-memory Dictionary<string, NoteItem> with thread-safe access patterns  
**Testing**: Manual testing with MCP clients (GitHub Copilot, Claude Desktop)  
**Target Platform**: Cross-platform (Linux, Windows, macOS) via .NET 10 runtime
**Project Type**: Single project - MCP server with ASP.NET Core minimal API hosting  
**Performance Goals**: <1 second for list/search operations with 10,000 notes, <2 seconds for single CRUD operations  
**Constraints**: In-memory only (no persistence), thread-safe concurrent access, <100MB memory for 10,000 notes  
**Scale/Scope**: Educational workshop feature demonstrating MCP tools, prompts, and DI patterns. ~500 lines of code across 3 new files.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Principle I: ModelContextProtocol SDK Integration ✅
- **Status**: COMPLIANT
- **Evidence**: Using `[McpServerTool]` for 6 tools, `[McpServerPrompt]` for 5 prompts
- **Evidence**: Server configured via `AddMcpServer()` builder in Program.cs
- **Evidence**: Tools/prompts registered via `WithTools<NotesTools>()` and `WithPrompts<NotesPrompts>()`
- **Evidence**: Supports both stdio and HTTP transports (already configured in existing server)

### Principle II: AI Discoverability Through Attributes ✅
- **Status**: COMPLIANT
- **Evidence**: All 6 tools will have `[Description]` attributes explaining purpose
- **Evidence**: All tool parameters will have `[Description]` attributes with expected input details
- **Evidence**: All 5 prompts will have `[Description]` attributes explaining their guided interaction purpose
- **Evidence**: Following pattern from existing RandomNumberTools.cs

### Principle III: Dependency Injection for State Management ✅
- **Status**: COMPLIANT
- **Evidence**: NotesService registered as Singleton in Program.cs (in-memory state requires singleton lifetime)
- **Evidence**: NotesService injected into NotesTools and NotesPrompts via parameter injection
- **Evidence**: Using Microsoft.Extensions.DependencyInjection patterns (already configured in Program.cs)
- **Evidence**: CancellationToken support in async operations

### Principle IV: User-Friendly Tool Responses ✅
- **Status**: COMPLIANT
- **Evidence**: Tool responses will use emojis (✅ success, ❌ error, 📝 note data, 🔍 search results)
- **Evidence**: Error messages will be clear and actionable ("Note with ID 'abc123' not found ❌")
- **Evidence**: Multi-item responses formatted with bullets or numbering
- **Evidence**: Responses include context ("Added note 'Meeting Ideas' with ID abc12345 ✅")

### Principle V: Workshop-Focused Simplicity ✅
- **Status**: COMPLIANT
- **Evidence**: Using simple Dictionary<string, NoteItem> for storage (no database, no complex abstractions)
- **Evidence**: Single service class (NotesService) with straightforward CRUD methods
- **Evidence**: No additional external dependencies beyond MCP SDK
- **Evidence**: Clean separation: Services/ for data, Tools/ for MCP tools, Prompts/ for MCP prompts
- **Evidence**: Educational focus with clear, commented code demonstrating MCP concepts

### Principle VI: Modern .NET Patterns ✅
- **Status**: COMPLIANT
- **Evidence**: Using C# 13 record type for NoteItem: `record NoteItem(string Id, string Title, string Content, DateTime CreatedAt, DateTime? UpdatedAt)`
- **Evidence**: Nullable reference types enabled (project setting)
- **Evidence**: Async/await with CancellationToken support where appropriate
- **Evidence**: Following .NET naming conventions (PascalCase for public, camelCase for parameters)
- **Evidence**: Proper error handling with McpProtocolException for protocol errors

**GATE RESULT**: ✅ PASS - All 6 constitution principles satisfied. No violations requiring justification.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
SampleMcpServer/
├── Program.cs                      # Updated: Register NotesService, WithTools<NotesTools>, WithPrompts<NotesPrompts>
├── SampleMcpServer.csproj         # Existing: No changes needed
├── appsettings.json               # Existing: No changes needed
├── appsettings.Development.json   # Existing: No changes needed
├── Properties/
│   └── launchSettings.json        # Existing: No changes needed
├── Services/                      # NEW directory
│   └── NotesService.cs            # NEW: In-memory note store with CRUD operations
├── Tools/
│   ├── RandomNumberTools.cs       # Existing: No changes
│   └── NotesTools.cs              # NEW: 6 MCP tools for note operations
└── Prompts/                       # NEW directory
    └── NotesPrompts.cs            # NEW: 5 MCP prompts for guided interactions
```

**Structure Decision**: Single project structure. Extending existing SampleMcpServer with new Services/ and Prompts/ directories. This follows the workshop's single-project pattern and keeps all MCP-related code co-located. No tests directory created (manual testing with MCP clients per workshop simplicity principle).

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

**No violations** - All constitution principles satisfied. No complexity justification required.

---

## Phase 0: Research Complete ✅

**Output**: [research.md](./research.md)

**Decisions Made**:
1. ID Generation: 8-character GUID prefixes
2. Thread Safety: ConcurrentDictionary<string, NoteItem>
3. Data Model: C# 13 record type with positional parameters
4. Search: LINQ with case-insensitive Contains
5. Tool Responses: Formatted strings with emojis
6. Prompt Design: Template-based guidance with auto-generation
7. Error Handling: McpProtocolException with appropriate codes
8. DI Configuration: Singleton NotesService registration

**All NEEDS CLARIFICATION items resolved** ✅

---

## Phase 1: Design Complete ✅

### Data Model

**Output**: [data-model.md](./data-model.md)

**Entities**:
- **NoteItem**: id, title, content, createdAt, updatedAt (nullable)

**Validation Rules**:
- Title and content must not be empty
- IDs must be unique and exist for operations
- CreatedAt immutable, UpdatedAt set on updates only

**State Transitions**:
- [Not Created] → AddNote → [Created]
- [Created] → UpdateNote → [Updated]
- [Created/Updated] → DeleteNote → [Deleted]

### Contracts

**Output**: 
- [contracts/tools.md](./contracts/tools.md) - 6 MCP Tools
- [contracts/prompts.md](./contracts/prompts.md) - 5 MCP Prompts

**MCP Tools** (6):
1. **AddNote**: Create with auto-generated ID and timestamp
2. **GetNote**: Retrieve by ID with formatted display
3. **ListNotes**: All notes ordered newest first
4. **SearchNotes**: Case-insensitive title/content search
5. **UpdateNote**: Partial updates preserving timestamps
6. **DeleteNote**: Permanent removal by ID

**MCP Prompts** (5):
1. **QuickNote**: Auto-generate title from content
2. **SummarizeNotes**: Statistics and recent notes overview
3. **FindTopicNotes**: Topic search with relevance context
4. **MeetingNotes**: Structured meeting template
5. **CodeReviewNote**: Structured code review template

### Quickstart

**Output**: [quickstart.md](./quickstart.md)

**Implementation Steps**:
1. Create NoteItem record model
2. Implement NotesService with CRUD operations
3. Implement NotesTools with 6 MCP tools
4. Implement NotesPrompts with 5 MCP prompts
5. Register services in Program.cs
6. Build, run, and test

**Files to Create**: 4 new files
**Files to Modify**: 1 file (Program.cs)
**Estimated LOC**: ~500 lines

### Agent Context

**Output**: [.github/agents/copilot-instructions.md](../../.github/agents/copilot-instructions.md)

**Technology Stack Added**:
- C# 13 / .NET 10 SDK
- ModelContextProtocol 0.5.0-preview.1
- ModelContextProtocol.AspNetCore 0.5.0-preview.1
- In-memory Dictionary storage

---

## Constitution Re-Check ✅

**Post-Design Validation**: All 6 principles remain satisfied

No violations introduced during design phase. Implementation plan maintains:
- MCP SDK integration with attributes
- Comprehensive descriptions for AI discoverability  
- Singleton DI for state management
- User-friendly formatted responses
- Workshop-focused simplicity (no complex patterns)
- Modern .NET conventions and patterns

---

## Next Steps

**Phase 2: Tasks** (Use `/speckit.tasks` command)
- Break down implementation into concrete, sequenced tasks
- Create [tasks.md](./tasks.md) with step-by-step checklist
- Track progress through implementation

**Implementation Ready**:
- All technical decisions documented
- All contracts defined
- All patterns established
- Quickstart guide complete
- Constitution validated

**Estimated Implementation Time**: 2-3 hours for experienced .NET developer

---

## Plan Summary

✅ **Technical Context**: All platform details specified  
✅ **Constitution Check**: All 6 principles satisfied (PASS)  
✅ **Phase 0 Research**: 8 technical decisions documented  
✅ **Phase 1 Design**: Data model + 2 contracts + quickstart complete  
✅ **Agent Context**: Technology stack registered  
✅ **Constitution Re-Check**: No violations introduced  

**Status**: Ready for Phase 2 (Task Breakdown) or direct implementation

**Branch**: `001-notes-store`  
**Spec**: [spec.md](./spec.md)  
**Plan**: This file
