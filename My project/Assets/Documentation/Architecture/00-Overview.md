# Player Controller Documentation - Overview

> **Nota**: El índice maestro de documentación está en [INDEX.md](../INDEX.md). Este archivo es un overview del player controller.

## Documentos del Player Controller

### Architecture (How it's built)
- [PlayerController-Design.md](PlayerController-Design.md) - Diseño completo: Controller/Pawn, input, cámara, state machine, data flow
- [09-Three-Layer-Architecture.md](09-Three-Layer-Architecture.md) - Arquitectura en 3 capas, assembly definitions, reglas de dependencia
- [08-Phase1-Development-Plan.md](08-Phase1-Development-Plan.md) - ~~Deprecated~~ — ver Sprint Backlogs en [Development/](../Development/README.md)

### Systems (What it does)
- [PlayerController-Specification.md](../Systems/PlayerController-Specification.md) - Requisitos funcionales y no funcionales
- [01-Phase1-Testing-Strategy.md](../Systems/01-Phase1-Testing-Strategy.md) - Pirámide de testing, benchmarks

### Standards (How to code)
- [Coding-Standards.md](../Standards/Coding-Standards.md) - Naming conventions, code style, SOLID
- [01-Testable-Code-Guidelines.md](../Standards/01-Testable-Code-Guidelines.md) - Cómo escribir código testeable

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

## File Size Policy
- **Max 400 lines** per file
- **One topic** per file
- **Clear naming**: `NN-Topic-Description.md`
- **Cross-links**: Use relative paths

---

**Last Updated**: 2026-05-04  
**Version**: 2.0 (Cleaned phantom links, added INDEX reference)  
**Status**: Ready for Phase 1 Implementation
