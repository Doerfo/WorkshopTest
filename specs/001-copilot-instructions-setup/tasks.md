---
description: "Implementation tasks for Copilot Instructions Setup MCP Server"
---

# Tasks: Copilot Instructions Setup MCP Server

**Input**: Design documents from `/specs/001-copilot-instructions-setup/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Tests are NOT requested in the specification, so test tasks are excluded.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Project uses .NET 10 structure:
- Main project: `SampleMcpServer/`
- Tools: `SampleMcpServer/Tools/`
- Services: `SampleMcpServer/Services/`
- Models: `SampleMcpServer/Models/`
- Prompts: `SampleMcpServer/Prompts/`
- Guidelines: `SampleMcpServer/Guidelines/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T01 Verify .NET 10 SDK and ModelContextProtocol packages per SampleMcpServer/SampleMcpServer.csproj
- [X] T02 [P] Create directory structure: SampleMcpServer/Models/, SampleMcpServer/Services/, SampleMcpServer/Prompts/, SampleMcpServer/Guidelines/
- [X] T03 [P] Configure logging to stderr in SampleMcpServer/Program.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T04 [P] Create TechnologyInfo record in SampleMcpServer/Models/TechnologyInfo.cs
- [X] T05 [P] Create GuidelineContent record in SampleMcpServer/Models/GuidelineContent.cs
- [X] T06 [P] Create BaselineInstruction record in SampleMcpServer/Models/BaselineInstruction.cs
- [X] T07 [P] Create InstructionFileResult record with FileOperationStatus enum in SampleMcpServer/Models/InstructionFileResult.cs
- [X] T08 [P] Create TechnologyDetectionResult record in SampleMcpServer/Models/TechnologyDetectionResult.cs
- [X] T09 [P] Create MergedInstructionContent and InstructionSection records in SampleMcpServer/Models/MergedInstructionContent.cs
- [X] T10 [P] Create SetupSummary record in SampleMcpServer/Models/SetupSummary.cs
- [X] T11 Create IAwesomeCopilotCacheService interface in SampleMcpServer/Services/IAwesomeCopilotCacheService.cs
- [X] T12 Implement AwesomeCopilotCacheService with in-memory cache and 24-hour TTL in SampleMcpServer/Services/AwesomeCopilotCacheService.cs
- [X] T13 Create IGitHubService interface in SampleMcpServer/Services/IGitHubService.cs
- [X] T14 Implement GitHubService for fetching files from awesome-copilot repository in SampleMcpServer/Services/GitHubService.cs
- [X] T15 Register cache and GitHub services as Singleton in SampleMcpServer/Program.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Initial Project Setup (Priority: P1) 🎯 MVP

**Goal**: Developer can invoke a setup prompt that analyzes project technology stack and automatically creates instruction files with baseline from awesome-copilot

**Independent Test**: Run setup prompt on a sample .NET project and verify that .github/copilot-instructions.md and .github/instructions/csharp.instructions.md are created with C# baseline content

### Implementation for User Story 1

- [X] T16 Create ITechnologyDetectionService interface in SampleMcpServer/Services/ITechnologyDetectionService.cs
- [X] T17 Implement TechnologyDetectionService with file-based pattern matching for C#, TypeScript, JavaScript, Python, Java, React, Angular, Vue, Go, Rust in SampleMcpServer/Services/TechnologyDetectionService.cs
- [X] T18 Register TechnologyDetectionService as Singleton in SampleMcpServer/Program.cs
- [X] T19 [P] [US1] Create TechnologyDiscoveryTools class in SampleMcpServer/Tools/TechnologyDiscoveryTools.cs
- [X] T20 [P] [US1] Implement list-available-technologies tool in SampleMcpServer/Tools/TechnologyDiscoveryTools.cs
- [X] T21 [US1] Implement detect-project-technologies tool in SampleMcpServer/Tools/TechnologyDiscoveryTools.cs (depends on T020)
- [X] T22 [P] [US1] Create BaselineInstructionTools class in SampleMcpServer/Tools/BaselineInstructionTools.cs
- [X] T23 [US1] Implement get-baseline-instruction tool with GitHub API integration in SampleMcpServer/Tools/BaselineInstructionTools.cs
- [X] T24 [US1] Implement refresh-cache tool in SampleMcpServer/Tools/BaselineInstructionTools.cs
- [X] T25 Create IInstructionMergeService interface in SampleMcpServer/Services/IInstructionMergeService.cs
- [X] T26 Implement InstructionMergeService with section-based merging and content hashing in SampleMcpServer/Services/InstructionMergeService.cs
- [X] T27 Register InstructionMergeService as Singleton in SampleMcpServer/Program.cs
- [X] T28 [P] [US1] Create InstructionFileTools class in SampleMcpServer/Tools/InstructionFileTools.cs
- [X] T29 [US1] Implement create-repository-instruction-file tool with file writing and directory creation in SampleMcpServer/Tools/InstructionFileTools.cs
- [X] T30 [US1] Implement create-technology-instruction-file tool with merge integration and backup creation in SampleMcpServer/Tools/InstructionFileTools.cs
- [X] T31 [US1] Create SetupCopilotInstructionsPrompt class in SampleMcpServer/Prompts/SetupCopilotInstructionsPrompt.cs
- [X] T32 [US1] Implement setup-copilot-instructions prompt with multi-turn workflow instructions in SampleMcpServer/Prompts/SetupCopilotInstructionsPrompt.cs
- [X] T33 [US1] Register all tools and prompts in SampleMcpServer/Program.cs using WithTools<> and WithPrompts<>
- [ ] T034 [US1] Add error handling with McpProtocolException for InvalidParams and InternalError cases in all US1 tools
- [ ] T035 [US1] Add logging for technology detection, baseline retrieval, and file operations in all US1 components

**Checkpoint**: At this point, User Story 1 should be fully functional - developer can run setup prompt and get baseline instruction files

---

## Phase 4: User Story 2 - Apply Company-Specific Guidelines (Priority: P2)

**Goal**: Developer can enhance auto-generated instructions with company-specific guidelines that are automatically merged with baselines

**Independent Test**: Place company guideline files (e.g., csharp-testing.md) in Guidelines/ directory, run setup prompt, and verify guidelines are merged into generated instruction files with company content overriding baseline when overlapping

### Implementation for User Story 2

- [X] T36 [P] [US2] Create IGuidelineService interface in SampleMcpServer/Services/IGuidelineService.cs
- [X] T37 [US2] Implement GuidelineService with directory scanning, frontmatter parsing, and technology mapping in SampleMcpServer/Services/GuidelineService.cs
- [X] T38 [US2] Register GuidelineService as Singleton in SampleMcpServer/Program.cs
- [X] T39 [P] [US2] Create GuidelineTools class in SampleMcpServer/Tools/GuidelineTools.cs
- [X] T40 [US2] Implement get-company-guideline tool with guideline discovery and loading in SampleMcpServer/Tools/GuidelineTools.cs
- [X] T41 [US2] Register GuidelineTools in SampleMcpServer/Program.cs using WithTools<GuidelineTools>()
- [X] T42 [US2] Update SetupCopilotInstructionsPrompt to include guideline retrieval step in SampleMcpServer/Prompts/SetupCopilotInstructionsPrompt.cs
- [X] T43 [P] [US2] Create sample company guidelines: SampleMcpServer/Guidelines/csharp-testing.md
- [X] T44 [P] [US2] Create sample company guidelines: SampleMcpServer/Guidelines/typescript.md
- [X] T45 [P] [US2] Create sample company guidelines: SampleMcpServer/Guidelines/react.md
- [ ] T046 [US2] Add validation for guideline file format and frontmatter in GuidelineService in SampleMcpServer/Services/GuidelineService.cs
- [ ] T047 [US2] Update InstructionMergeService to apply company-first override rule in SampleMcpServer/Services/InstructionMergeService.cs
- [ ] T048 [US2] Add logging for guideline discovery, loading, and merging in US2 components

**Checkpoint**: At this point, User Stories 1 AND 2 should both work - setup generates files with both baselines and company guidelines merged

---

## Phase 5: User Story 3 - Update Existing Instructions (Priority: P2)

**Goal**: Developer can refresh existing instruction files with updated baselines or guidelines, with automatic backup creation and merge/replace options

**Independent Test**: Run setup prompt on project that already has instruction files, verify existing files are detected, user is prompted for update decision, backups are created, and files are updated with warnings shown

### Implementation for User Story 3

- [ ] T049 [US3] Update create-repository-instruction-file tool to detect existing files and handle updateExisting flag in SampleMcpServer/Tools/InstructionFileTools.cs
- [ ] T050 [US3] Update create-technology-instruction-file tool to create timestamped backups when updateExisting=true in SampleMcpServer/Tools/InstructionFileTools.cs
- [ ] T051 [US3] Add file existence checking and skip logic when updateExisting=false in SampleMcpServer/Tools/InstructionFileTools.cs
- [ ] T052 [US3] Update SetupCopilotInstructionsPrompt to check for existing files and prompt user about update options in SampleMcpServer/Prompts/SetupCopilotInstructionsPrompt.cs
- [ ] T053 [US3] Add warning messages to InstructionFileResult for skipped files and removed content in SampleMcpServer/Models/InstructionFileResult.cs
- [ ] T054 [US3] Update SetupSummary to collect and display warnings about existing files and backups in workflow
- [ ] T055 [US3] Add logging for backup creation, file skipping, and update operations in US3 components

**Checkpoint**: All User Stories 1, 2, and 3 should now work independently - can create new instructions, merge guidelines, and update existing files

---

## Phase 6: User Story 4 - Maintain and Extend Guidelines (Priority: P3)

**Goal**: Developer or organization can add new company guidelines or modify existing ones, with automatic discovery requiring no code changes

**Independent Test**: Add a new guideline file (e.g., angular.md) to Guidelines/ directory, restart MCP server, run setup prompt on Angular project, and verify new guideline is discovered and incorporated

### Implementation for User Story 4

- [ ] T056 [US4] Implement hot-reload or startup scan for guideline directory in GuidelineService in SampleMcpServer/Services/GuidelineService.cs
- [ ] T057 [US4] Add automatic guideline discovery based on file naming convention in GuidelineService in SampleMcpServer/Services/GuidelineService.cs
- [ ] T058 [US4] Update list-available-technologies tool to show which guidelines are available in SampleMcpServer/Tools/TechnologyDiscoveryTools.cs
- [ ] T059 [P] [US4] Create additional sample guidelines: SampleMcpServer/Guidelines/angular.md
- [ ] T060 [P] [US4] Create additional sample guidelines: SampleMcpServer/Guidelines/python-type-hints.md
- [ ] T061 [US4] Add validation for guideline naming convention (technology[-aspect].md) in GuidelineService
- [ ] T062 [US4] Update README or documentation to explain guideline file format and naming in SampleMcpServer/Guidelines/README.md
- [ ] T063 [US4] Add logging for guideline directory changes and new file discovery

**Checkpoint**: All user stories should now be independently functional - complete guideline lifecycle supported

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T64 [P] Add GitHub API configuration options to SampleMcpServer/appsettings.json (repository, cache duration, user agent)
- [X] T65 [P] Add cache configuration options to SampleMcpServer/appsettings.json (TTL, enable persistence)
- [X] T66 [P] Update logging configuration in SampleMcpServer/appsettings.Development.json for development debugging
- [ ] T067 Add comprehensive error handling for network failures with graceful degradation across all services
- [ ] T068 Add GitHub API rate limit handling with cache fallback in GitHubService
- [ ] T069 Implement content size validation (max 100KB per file) in InstructionMergeService
- [ ] T070 Add markdown validation for guideline files and merged content across services
- [X] T71 [P] Update README.md with setup instructions, usage examples, and troubleshooting
- [ ] T072 Code cleanup: ensure all public APIs have XML documentation comments
- [ ] T073 Code cleanup: verify nullable reference types are correctly applied throughout
- [ ] T074 Verify all MCP tool descriptions are comprehensive and follow best practices
- [ ] T075 Run quickstart.md validation scenarios to verify all examples work
- [ ] T076 Performance optimization: profile GitHub API calls and file I/O operations
- [ ] T077 Security review: validate file path handling prevents directory traversal attacks

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - User Story 1 (P1): Can start after Foundational - No dependencies on other stories
  - User Story 2 (P2): Can start after Foundational - Depends on US1 tools but independently testable
  - User Story 3 (P2): Can start after US1 - Enhances existing file tools
  - User Story 4 (P3): Can start after US2 - Extends guideline management
- **Polish (Phase 7)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Foundation only - Core technology detection, baseline retrieval, and file creation
- **User Story 2 (P2)**: US1 tools - Adds guideline merging to existing workflow
- **User Story 3 (P2)**: US1 completion - Updates existing file tools with backup and merge options
- **User Story 4 (P3)**: US2 completion - Extends guideline management with auto-discovery

### Within Each User Story

- Models before services
- Services before tools
- Tools before prompts
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

Within Foundation (Phase 2):
- T004-T010: All model records can be created in parallel
- T011/T013: Service interfaces can be created in parallel

Within User Story 1:
- T019/T022/T028: All tool classes can be created in parallel (different files)
- T020/T023/T029: Tool implementations can be written in parallel after their classes exist

Within User Story 2:
- T043/T044/T045: All sample guideline files can be created in parallel
- T036/T039: Service interface and tools class can be created in parallel

Within User Story 4:
- T059/T060: Additional guideline files can be created in parallel

Within Polish (Phase 7):
- T064/T065/T066: All configuration updates can be done in parallel
- T071/T072/T073/T074: Documentation and code cleanup can be parallelized

---

## Parallel Example: User Story 1 Core Tools

```bash
# After foundation is complete, launch all tool classes in parallel:
Task: "T019 [P] [US1] Create TechnologyDiscoveryTools class"
Task: "T022 [P] [US1] Create BaselineInstructionTools class"
Task: "T028 [P] [US1] Create InstructionFileTools class"

# After tool classes exist, implement core tool methods:
Task: "T020 [P] [US1] Implement list-available-technologies tool"
Task: "T023 [US1] Implement get-baseline-instruction tool"
Task: "T029 [US1] Implement create-repository-instruction-file tool"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: Foundational (T004-T015) - CRITICAL
3. Complete Phase 3: User Story 1 (T016-T035)
4. **STOP and VALIDATE**: Test setup prompt on sample .NET project
5. Demo: Show automated instruction file creation with baselines

**MVP Delivers**: Automated setup of Copilot instruction files with awesome-copilot baselines

### Incremental Delivery

1. Foundation (Phases 1-2) → Core infrastructure ready (T001-T015)
2. MVP (Phase 3) → User Story 1 complete → Test independently → Deploy/Demo
3. Guidelines (Phase 4) → User Story 2 complete → Test independently → Deploy/Demo
4. Updates (Phase 5) → User Story 3 complete → Test independently → Deploy/Demo
5. Maintenance (Phase 6) → User Story 4 complete → Test independently → Deploy/Demo
6. Polish (Phase 7) → Production-ready release

Each phase adds value without breaking previous functionality.

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together (T001-T015)
2. Once Foundational is done:
   - Developer A: User Story 1 (T016-T035) - Core workflow
   - Developer B: Start US2 models/services (T036-T038) in parallel
   - Developer C: Create sample guidelines (T043-T045) in parallel
3. After US1 completes:
   - Developer A: User Story 3 (T049-T055)
   - Developer B: Complete US2 tools (T039-T048)
   - Developer C: User Story 4 (T056-T063)
4. All converge on Phase 7: Polish (T064-T077)

---

## Summary

**Total Tasks**: 77 tasks
**Task Distribution**:
- Phase 1 (Setup): 3 tasks
- Phase 2 (Foundational): 12 tasks
- Phase 3 (User Story 1 - P1): 20 tasks
- Phase 4 (User Story 2 - P2): 13 tasks
- Phase 5 (User Story 3 - P2): 7 tasks
- Phase 6 (User Story 4 - P3): 8 tasks
- Phase 7 (Polish): 14 tasks

**Parallel Opportunities**: 30+ tasks can be executed in parallel across different phases

**MVP Scope**: Phases 1-3 (35 tasks) - Core automated setup with baselines

**Independent Test Criteria**:
- US1: Setup prompt creates instruction files with baselines from awesome-copilot
- US2: Setup merges company guidelines with baselines (company overrides on conflict)
- US3: Setup updates existing files with backups and warnings
- US4: New guidelines auto-discovered without code changes

**Implementation Approach**: 
- TDD not requested - tests excluded
- User story organization enables independent delivery
- Clear file paths for each task
- Foundational phase blocks all stories (by design)
- Stories can proceed in parallel after foundation complete

---

## Notes

- [P] tasks = different files, no dependencies within same phase
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- File paths follow .NET project structure from plan.md
- All MCP tools use [McpServerTool] attribute and [Description] attributes
- Constitution compliance verified in plan.md
