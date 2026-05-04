# Player Controller Documentation - Index

## Quick Navigation

### Architecture (How it's built)
- [00-Overview.md](00-Overview.md) - **You are here**
- [01-Controller-Pawn-Pattern.md](01-Controller-Pawn-Pattern.md) - Unreal-style separation
- [02-Input-System.md](02-Input-System.md) - AAA input architecture
- [03-State-Machine.md](03-State-Machine.md) - Movement states (15-30Hz)
- [04-Camera-System.md](04-Camera-System.md) - View modes, feel systems
- [05-Domain-Model.md](05-Domain-Model.md) - Entities, ScriptableObjects
- [06-Data-Flow.md](06-Data-Flow.md) - Clean Architecture + DOTS
- [07-Extensibility.md](07-Extensibility.md) - Adding new features
- [08-Phase1-Development-Plan.md](08-Phase1-Development-Plan.md) - **Week-by-week plan**

### Systems (What it does)
- [PlayerController-Specification.md](../Systems/PlayerController-Specification.md) - Functional requirements
- [01-Phase1-Testing-Strategy.md](../Systems/01-Phase1-Testing-Strategy.md) - **How to test**

### Standards (How to code)
- [Coding-Standards.md](../Standards/Coding-Standards.md) - Naming, structure, patterns

---

## Project Status

### Current Phase: Planning
- [x] Architecture designed
- [x] AAA patterns researched
- [x] Documentation structured
- [ ] **Phase 1 starting** (Core Architecture + Movement)

### Phase Progress
| Phase | Name | Status | Duration |
|-------|------|---------|----------|
| 1 | Core Architecture + Movement | **Ready to start** | 2 weeks |
| 2 | Camera System | Pending | 1 week |
| 3 | Interaction Systems | Pending | 2 weeks |
| 4 | Survival Systems | Pending | 2 weeks |
| 5 | Polish + Feel | Pending | 1 week |

---

## Quick Start

### For Developers
1. Read [08-Phase1-Development-Plan.md](08-Phase1-Development-Plan.md)
2. Follow day-by-day tasks
3. Use [01-Phase1-Testing-Strategy.md](../Systems/01-Phase1-Testing-Strategy.md) to verify

### For Designers
1. Read [PlayerController-Specification.md](../Systems/PlayerController-Specification.md)
2. Check acceptance criteria for each feature
3. Use playtest checklist in testing doc

### For Leads
1. Review architecture docs (01-07)
2. Check development plan timeline
3. Verify test coverage goals met

---

## File Size Policy
- **Max 400 lines** per file
- **One topic** per file
- **Clear naming**: `NN-Topic-Description.md`
- **Cross-links**: Use relative paths

---

**Last Updated**: 2026-05-04  
**Version**: 1.1 (Refactored for small files)  
**Status**: Ready for Phase 1 Implementation
