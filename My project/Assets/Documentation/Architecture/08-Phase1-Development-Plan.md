# Phase 1: Core Architecture + Movement - Development Plan

> **⚠️ Este documento fue reemplazado por los Sprint Backlogs en `Development/`.**
> El plan original tenía inconsistencias con la Three-Layer Architecture.
> Usá los nuevos documentos para ejecutar:

## Plan actualizado

| Sprint | Contenido | Link |
|--------|-----------|------|
| Sprint 1 | Project Setup, DI, Interfaces, Input, Controller/Pawn | [Phase1-Sprint1-Backlog.md](../Development/Phase1-Sprint1-Backlog.md) |
| Sprint 2 | State Machine, Movement, Coyote Time, Polish | [Phase1-Sprint2-Backlog.md](../Development/Phase1-Sprint2-Backlog.md) |

## ¿Por qué se reemplazó?

El plan original:
1. Usaba `Assets/Domain/`, `Assets/Infrastructure/` — no respetaba la Three-Layer Architecture (`SurvGame/`, `FPSGame/`, `SurvivalProject/`)
2. No incluía el injector (`com.torque-games.injector`)
3. Usaba `Time.deltaTime` directo en domain logic
4. No especificaba Assembly Definitions reales
5. No estaba en formato ticketeable (SCRUM stories/tasks)

Los Sprint Backlogs corrigen todo esto y están diseñados para ser usados por un PM (crear tickets) y por AI Agents (ejecutar secuencialmente).

---

**Last Updated**: 2026-05-04  
**Status**: Deprecated — ver `Development/`
