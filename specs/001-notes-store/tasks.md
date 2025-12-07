# Tasks: In-Memory Notes Store

**Input**: Design documents from `/specs/001-notes-store/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: Not included - feature specification does not request TDD approach. Manual testing with MCP clients per workshop pattern.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

Single project structure at `/workspaces/WorkshopTest/SampleMcpServer/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create directory structure and prepare for implementation

- [X] T001 Create Models/ directory in SampleMcpServer/
- [X] T002 Create Services/ directory in SampleMcpServer/
- [X] T003 Create Prompts/ directory in SampleMcpServer/

**Checkpoint**: Directory structure ready for code files

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 [P] Create NoteItem record in SampleMcpServer/Models/NoteItem.cs with Id, Title, Content, CreatedAt, UpdatedAt properties
- [X] T005 Create NotesService class in SampleMcpServer/Services/NotesService.cs with ConcurrentDictionary storage
- [X] T006 Implement GenerateId() method in NotesService using 8-character GUID prefix
- [X] T007 Implement ValidateNotEmpty() helper method in NotesService for input validation
- [X] T008 Register NotesService as Singleton in SampleMcpServer/Program.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Quick Note Capture (Priority: P1) 🎯 MVP

**Goal**: Enable users to create notes with auto-generated IDs/timestamps and retrieve them by ID

**Independent Test**: Create a note with AddNote tool, then retrieve it with GetNote tool. Verify ID, title, content, and timestamps are correct.

### Implementation for User Story 1

- [X] T009 [US1] Implement AddNote() method in NotesService with title/content validation, ID generation, timestamp creation
- [X] T010 [US1] Implement GetNote() method in NotesService with ID validation and dictionary lookup
- [X] T011 [P] [US1] Create NotesTools class in SampleMcpServer/Tools/NotesTools.cs with namespace and class structure
- [X] T012 [US1] Implement AddNote tool in NotesTools with [McpServerTool] attribute, [Description] attributes, NotesService injection, error handling
- [X] T013 [US1] Implement GetNote tool in NotesTools with [McpServerTool] attribute, [Description] attributes, formatted note display
- [X] T014 [US1] Implement FormatNote() helper method in NotesTools for consistent note formatting with emojis
- [X] T015 [US1] Register NotesTools with WithTools<NotesTools>() in SampleMcpServer/Program.cs

**Checkpoint**: At this point, User Story 1 should be fully functional - can create and retrieve notes independently

---

## Phase 4: User Story 2 - Browse and Search Notes (Priority: P2)

**Goal**: Enable users to list all notes (newest first) and search by keywords in title/content

**Independent Test**: Create multiple notes with different timestamps and content. Use ListNotes to verify newest-first ordering. Use SearchNotes to find notes by keyword (case-insensitive).

### Implementation for User Story 2

- [X] T016 [P] [US2] Implement ListNotes() method in NotesService with OrderByDescending on CreatedAt
- [X] T017 [P] [US2] Implement SearchNotes() method in NotesService with case-insensitive LINQ Where on Title/Content
- [X] T018 [US2] Implement ListNotes tool in NotesTools with [McpServerTool] attribute, formatted numbered list output, empty state handling
- [X] T019 [US2] Implement SearchNotes tool in NotesTools with [McpServerTool] attribute, formatted search results, match count display

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - can create, retrieve, list, and search notes

---

## Phase 5: User Story 3 - Update and Manage Notes (Priority: P3)

**Goal**: Enable users to update existing notes (partial updates supported) and delete notes permanently

**Independent Test**: Create a note, update its title only (verify content preserved), update content only (verify title preserved), verify UpdatedAt changes while CreatedAt stays same. Delete a note and verify it no longer appears in lists/searches.

### Implementation for User Story 3

- [X] T020 [P] [US3] Implement UpdateNote() method in NotesService with partial update support using 'with' expressions, timestamp preservation
- [X] T021 [P] [US3] Implement DeleteNote() method in NotesService with ID validation and dictionary removal
- [X] T022 [US3] Implement UpdateNote tool in NotesTools with [McpServerTool] attribute, optional title/content parameters, error handling
- [X] T023 [US3] Implement DeleteNote tool in NotesTools with [McpServerTool] attribute, confirmation message with note title

**Checkpoint**: All core CRUD operations work - User Stories 1, 2, AND 3 are independently functional

---

## Phase 6: User Story 4 - Guided Note Creation (Priority: P4)

**Goal**: Provide AI-assisted structured prompts for common note-taking scenarios

**Independent Test**: Invoke each of the 5 prompts and verify they return appropriate templates/guidance. Test QuickNote's auto-title generation with various content lengths.

### Implementation for User Story 4

- [X] T024 [P] [US4] Create NotesPrompts class in SampleMcpServer/Prompts/NotesPrompts.cs with namespace and class structure
- [X] T025 [P] [US4] Implement GenerateTitle() helper method in NotesPrompts for auto-title generation from content
- [X] T026 [P] [US4] Implement QuickNote prompt in NotesPrompts with [McpServerPrompt] attribute, auto-title generation, confirmation template
- [X] T027 [P] [US4] Implement SummarizeNotes prompt in NotesPrompts with [McpServerPrompt] attribute, statistics calculation, recent notes preview
- [X] T028 [P] [US4] Implement FindTopicNotes prompt in NotesPrompts with [McpServerPrompt] attribute, relevance indicators (title vs content match)
- [X] T029 [P] [US4] Implement MeetingNotes prompt in NotesPrompts with [McpServerPrompt] attribute, structured template with date/attendees/agenda/decisions/actions
- [X] T030 [P] [US4] Implement CodeReviewNote prompt in NotesPrompts with [McpServerPrompt] attribute, structured template with reviewer/findings/recommendations
- [X] T031 [US4] Register NotesPrompts with WithPrompts<NotesPrompts>() in SampleMcpServer/Program.cs

**Checkpoint**: All user stories (1-4) are complete and independently functional

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Build verification, testing, and final quality checks

- [X] T032 Build project with 'dotnet build' and verify no compilation errors
- [X] T033 Run project with 'dotnet run' and verify server starts successfully
- [ ] T034 Manual test: Use AddNote to create a test note, verify success message with ID
- [ ] T035 Manual test: Use GetNote with the ID from T034, verify formatted note display
- [ ] T036 Manual test: Use ListNotes, verify test note appears in numbered list
- [ ] T037 Manual test: Use SearchNotes with a keyword from the test note, verify it's found
- [ ] T038 Manual test: Use UpdateNote to change the test note's title, verify success and preserved timestamps
- [ ] T039 Manual test: Use DeleteNote to remove the test note, verify success message
- [ ] T040 Manual test: Use QuickNote prompt with short content, verify auto-generated title
- [ ] T041 Manual test: Use SummarizeNotes prompt after creating multiple notes, verify statistics
- [ ] T042 Manual test: Verify error handling - try GetNote with invalid ID, verify error message with ❌
- [ ] T043 Manual test: Verify error handling - try AddNote with empty title, verify validation error
- [X] T044 Validate implementation against quickstart.md steps
- [X] T045 Verify all 6 MCP tools are discoverable by MCP clients
- [X] T046 Verify all 5 MCP prompts are discoverable by MCP clients

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3 → P4)
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Independent, builds on NotesService
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Independent, builds on NotesService
- **User Story 4 (P4)**: Can start after Foundational (Phase 2) - May reference NotesService for data access but independently testable

### Within Each User Story

**User Story 1**:
- T009, T010 (service methods) can run in parallel [P]
- T011 (create class) before T012-T014
- T012-T014 (tool implementations) after T011
- T015 (registration) after all tools implemented

**User Story 2**:
- T016, T017 (service methods) can run in parallel [P]
- T018, T019 (tools) can run in parallel after service methods exist

**User Story 3**:
- T020, T021 (service methods) can run in parallel [P]
- T022, T023 (tools) can be implemented in parallel after service methods exist

**User Story 4**:
- T024, T025 (class + helper) can run in parallel [P]
- T026-T030 (all prompts) can run in parallel [P] after T024-T025
- T031 (registration) after all prompts implemented

### Parallel Opportunities

**Setup Phase**: All 3 directory creation tasks can run simultaneously

**Foundational Phase**: 
- T004 (model) can run independently
- T005-T007 (service implementation) after T004
- T008 (registration) after T005

**User Story Phase** (if team capacity allows):
- After Foundational completes, all 4 user stories can start in parallel by different developers
- Within US1: T009 and T010 in parallel, T012-T014 in parallel
- Within US2: T016 and T017 in parallel, T018 and T019 in parallel
- Within US3: T020 and T021 in parallel, T022 and T023 in parallel
- Within US4: T026-T030 all in parallel

---

## Parallel Example: User Story 1

```bash
# After Foundational Phase completes:

# Launch service methods together:
Task T009: "Implement AddNote() method in NotesService"
Task T010: "Implement GetNote() method in NotesService"

# After NotesTools class created (T011):
# Launch tool implementations together (all work on same file but different methods):
Task T012: "Implement AddNote tool in NotesTools"
Task T013: "Implement GetNote tool in NotesTools"  
Task T014: "Implement FormatNote() helper in NotesTools"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (3 tasks)
2. Complete Phase 2: Foundational (5 tasks) - CRITICAL
3. Complete Phase 3: User Story 1 (7 tasks)
4. **STOP and VALIDATE**: Run manual tests T034-T035
5. **RESULT**: Working MVP - can create and retrieve notes!

**Time Estimate**: 1-1.5 hours for MVP

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready (8 tasks)
2. Add User Story 1 → Test independently → **MVP Release** (7 tasks)
3. Add User Story 2 → Test independently → **v1.1 Release** (4 tasks)
4. Add User Story 3 → Test independently → **v1.2 Release** (4 tasks)
5. Add User Story 4 → Test independently → **v1.3 Release** (8 tasks)
6. Polish & validate → **v1.0 Release** (15 tasks)

**Total Tasks**: 46 tasks
**Time Estimate**: 2-3 hours for complete feature

### Parallel Team Strategy

With 4 developers after Foundational phase completes:

- **Developer A**: User Story 1 (7 tasks) - Core CRUD
- **Developer B**: User Story 2 (4 tasks) - List and Search
- **Developer C**: User Story 3 (4 tasks) - Update and Delete
- **Developer D**: User Story 4 (8 tasks) - Prompts

Stories complete independently and integrate via shared NotesService.

---

## Task Breakdown by User Story

### Summary

| Phase | User Story | Priority | Tasks | Can Parallelize |
|-------|------------|----------|-------|-----------------|
| 1 | Setup | N/A | 3 | Yes (all 3) |
| 2 | Foundational | CRITICAL | 5 | Partial (T004) |
| 3 | US1 - Quick Note Capture | P1 🎯 MVP | 7 | Partial |
| 4 | US2 - Browse and Search | P2 | 4 | Yes (within story) |
| 5 | US3 - Update and Manage | P3 | 4 | Yes (within story) |
| 6 | US4 - Guided Creation | P4 | 8 | Yes (most tasks) |
| 7 | Polish | N/A | 15 | Sequential |
| **Total** | | | **46** | |

### Critical Path

```
Setup (3) → Foundational (5) → US1 (7) → Polish (15) = 30 tasks for MVP
                            ↓
                            ├→ US2 (4) → Polish subset
                            ├→ US3 (4) → Polish subset  
                            └→ US4 (8) → Polish subset
```

---

## Validation Checklist

After completing all tasks, verify:

- [ ] All 6 MCP tools work correctly (AddNote, GetNote, ListNotes, SearchNotes, UpdateNote, DeleteNote)
- [ ] All 5 MCP prompts work correctly (QuickNote, SummarizeNotes, FindTopicNotes, MeetingNotes, CodeReviewNote)
- [ ] Notes have unique IDs (8-character GUID prefixes)
- [ ] Timestamps are in UTC and work correctly (CreatedAt immutable, UpdatedAt changes on update)
- [ ] List ordering is newest-first
- [ ] Search is case-insensitive
- [ ] Partial updates work (can update title only or content only)
- [ ] Error messages use ❌ emoji and are clear
- [ ] Success messages use ✅ emoji and include context
- [ ] Empty states are handled gracefully
- [ ] Thread-safe concurrent operations (ConcurrentDictionary)
- [ ] All tools/prompts have [Description] attributes for AI discoverability
- [ ] NotesService is registered as Singleton
- [ ] Project builds without errors
- [ ] Server starts and listens on stdio and HTTP transports

---

## Notes

- **[P] markers**: Tasks marked [P] use different files or independent code sections - safe to parallelize
- **[Story] labels**: Map tasks to user stories for traceability and independent testing
- **No automated tests**: Feature follows workshop pattern of manual testing with MCP clients
- **Commit strategy**: Commit after completing each user story phase (natural checkpoints)
- **Constitution compliance**: All tasks follow workshop simplicity and modern .NET patterns
- **MVP scope**: Phase 1-3 only (Setup + Foundational + User Story 1) delivers working note-taking system
- **File count**: 4 new files + 1 modified file = 5 files total
- **Line count**: Approximately 500 lines of code across all files
