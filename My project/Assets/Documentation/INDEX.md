# Survival FPS Game - Índice de Documentación

## ¿De qué se trata este proyecto?

Un juego **First Person Survival** en Unity, donde el jugador explora, recolecta recursos, combate, craftea y sobrevive en distintos entornos. El desarrollo sigue una arquitectura en tres capas reutilizables, principios SOLID y patrones inspirados en la industria AAA (Unreal Engine, DOTS, juegos como V Rising y The Last of Us).

---

## Si sos nuevo en el proyecto, empezá por acá

### 1. Entender qué hace el juego
Leé primero la especificación funcional. Describe los 7 sistemas principales (movimiento, cámara, interacciones, injuries, daño, entorno, stamina) y qué debe hacer cada uno.

> **[PlayerController-Specification.md](Systems/PlayerController-Specification.md)**  
> Requisitos funcionales y no funcionales del player controller. Incluye acceptance criteria para cada feature.

### 2. Entender cómo está construido
El documento de diseño explica la arquitectura completa: separación Controller/Pawn, input system, state machine, cámara, combat, environment y data flow.

> **[PlayerController-Design.md](Architecture/PlayerController-Design.md)**  
> Documento de diseño detallado. Contiene la arquitectura de componentes, el diagrama de flujo de datos y todos los puntos de extensibilidad.

### 3. Entender la estructura de código
El proyecto se organiza en tres capas (SurvGame → FPSGame → SurvivalProject) con namespaces flat y Clean Architecture interna.

> **[09-Three-Layer-Architecture.md](Architecture/09-Three-Layer-Architecture.md)**  
> Cómo se dividen las responsabilidades entre las tres capas, reglas de dependencia, Assembly Definitions y ejemplos de código.

### 4. Empezar a codear
Los Sprint Backlogs tienen las tareas concretas en formato SCRUM (stories + tasks con acceptance criteria).

> **[Phase1-Sprint1-Backlog.md](Development/Phase1-Sprint1-Backlog.md)**  
> Sprint 1: Estructura Three-Layer, DI, interfaces, input system, Controller/Pawn. Empezá por acá.

---

## Mapa completo de documentos

### Architecture/ — Cómo está construido

| Documento | Qué contiene | Cuándo leerlo |
|-----------|-------------|---------------|
| [00-Overview.md](Architecture/00-Overview.md) | Overview del player controller, estado de fases | Para ver el roadmap general y el estado de cada fase |
| [PlayerController-Design.md](Architecture/PlayerController-Design.md) | Diseño completo: Controller/Pawn, input pipeline, 3Cs, state machine, camera, interactions, environment, data flow | Es el doc central de arquitectura, leerlo para entender cómo encaja todo |
| [PlayerController-Extensibility.md](Architecture/PlayerController-Extensibility.md) | Extensibility points, domain events, performance considerations, roadmap de fases, dependencies | Cuando necesites agregar features o entender el roadmap |
| [09-Three-Layer-Architecture.md](Architecture/09-Three-Layer-Architecture.md) | Arquitectura en 3 capas (SurvGame/FPSGame/SurvivalProject), assembly definitions, reglas de dependencia, namespace convention | Antes de crear cualquier archivo nuevo, para saber en qué capa va |
| [08-Phase1-Development-Plan.md](Architecture/08-Phase1-Development-Plan.md) | ~~Deprecated~~ — redirige a los Sprint Backlogs en Development/ | Solo para contexto histórico |

### Standards/ — Cómo escribir código

| Documento | Qué contiene | Cuándo leerlo |
|-----------|-------------|---------------|
| [Coding-Standards.md](Standards/Coding-Standards.md) | Naming conventions flat, code style, region order, SOLID examples, git commits, branch naming | Antes de escribir tu primer archivo .cs |
| [01-Testable-Code-Guidelines.md](Standards/01-Testable-Code-Guidelines.md) | Cómo escribir código testeable: DI, pure functions, interface segregation, no statics, checklist | Antes de escribir cualquier clase de domain |
| [02-DI-Guidelines.md](Standards/02-DI-Guidelines.md) | API del injector (`com.torque-games.injector`): Register, Get, Create, TryGet, Reset, `[Inject]`. Reglas por capa | Cuando necesites registrar o resolver dependencias |
| [03-Patterns-And-Examples.md](Standards/03-Patterns-And-Examples.md) | ScriptableObject patterns, testing patterns por capa, XML docs, layer communication, performance | Para ver ejemplos concretos de código por capa |
| [04-Code-Templates.md](Standards/04-Code-Templates.md) | Templates: Domain Entity, Infrastructure MonoBehaviour, State Machine testing | Para copiar como base al crear nuevas clases |

### Development/ — Plan de ejecución (SCRUM)

| Documento | Qué contiene | Cuándo leerlo |
|-----------|-------------|---------------|
| [README.md](Development/README.md) | Cómo usar los backlogs (PM, AI Agent, Developer) | Para entender el formato |
| [Phase1-Sprint1-Backlog.md](Development/Phase1-Sprint1-Backlog.md) | Sprint 1: Project Setup, DI, Interfaces, Input System, Controller/Pawn. 4 épicas, 6 stories, ~20 tasks | **Empezar acá para implementar** |
| [Phase1-Sprint2-Backlog.md](Development/Phase1-Sprint2-Backlog.md) | Sprint 2: State Machine, Movement (6 estados), Coyote Time, Polish. 2 épicas, 5 stories, ~20 tasks | Después de completar Sprint 1 |

### Systems/ — Qué hace el juego

| Documento | Qué contiene | Cuándo leerlo |
|-----------|-------------|---------------|
| [PlayerController-Specification.md](Systems/PlayerController-Specification.md) | Requisitos funcionales (FR-001 a FR-007): movimiento, cámara, interacciones, injuries, daño, entorno, stamina. Interfaces públicas, save data, acceptance scenarios | Para entender qué debe hacer el juego desde el punto de vista del jugador |
| [01-Phase1-Testing-Strategy.md](Systems/01-Phase1-Testing-Strategy.md) | Pirámide de testing, unit tests (state machine, movement, input), integration tests (controller/pawn, ground detection), play mode tests (E2E), performance benchmarks, playtest checklist | Cuando necesites escribir o correr tests |

---

## Guía rápida por rol

### Soy programador y acabo de llegar
1. [PlayerController-Specification.md](Systems/PlayerController-Specification.md) — qué hace el juego
2. [09-Three-Layer-Architecture.md](Architecture/09-Three-Layer-Architecture.md) — dónde va cada cosa
3. [Standards/Coding-Standards.md](Standards/Coding-Standards.md) — cómo nombrar y estructurar código
4. [Phase1-Sprint1-Backlog.md](Development/Phase1-Sprint1-Backlog.md) — qué tarea hacer hoy

### Soy game designer
1. [PlayerController-Specification.md](Systems/PlayerController-Specification.md) — los requisitos
2. [PlayerController-Design.md](Architecture/PlayerController-Design.md) — las decisiones de diseño
3. [01-Phase1-Testing-Strategy.md](Systems/01-Phase1-Testing-Strategy.md) — la playtest checklist (sección 5)

### Soy lead / reviewer
1. [PlayerController-Design.md](Architecture/PlayerController-Design.md) — la arquitectura completa
2. [09-Three-Layer-Architecture.md](Architecture/09-Three-Layer-Architecture.md) — las capas y dependencias
3. [00-Overview.md](Architecture/00-Overview.md) — el roadmap y estado de fases
4. [01-Phase1-Testing-Strategy.md](Systems/01-Phase1-Testing-Strategy.md) — criterios de sign-off

---

## Roadmap de Fases

| Fase | Nombre | Duración | Estado | Doc principal |
|------|--------|----------|--------|--------------|
| 1 | Core Architecture + Movement | 2 semanas | **Ready to start** | [08-Phase1-Development-Plan.md](Architecture/08-Phase1-Development-Plan.md) |
| 2 | Camera System | 1 semana | Pending | — |
| 3 | Interaction Systems | 2 semanas | Pending | — |
| 4 | Survival Systems | 2 semanas | Pending | — |
| 5 | Polish + Feel | 1 semana | Pending | — |

---

## Stack técnico

- **Motor**: Unity 6000.2.6f2+
- **Input**: Input System 1.7+
- **Cámara**: Cinemachine 3.0+
- **DI**: [alvmivan/injector](https://github.com/alvmivan/injector) — UPM package propio (`com.torque-games.injector`), MIT. Constructor injection con resolución recursiva, `[Inject]` attribute, `TryGet<T>()`, `Reset()`. Se instala como Git dependency en el manifest.
- **Arquitectura**: Three-Layer (SurvGame → FPSGame → SurvivalProject)
- **Patrones**: Controller/Pawn, State Machine, Strategy (view modes), Clean Architecture, DDD
- **Testing**: Unity Test Framework (Edit Mode + Play Mode)
- **Opcional**: DOTS (Entities, Jobs, Burst) para NPCs masivos

---

## Reglas para agentes de IA

Ver [AGENTS.md](../../AGENTS.md) en la raíz del repo. Define reglas de documentación, código, y convenciones que cualquier IA debe seguir.

---

**Última actualización**: 2026-05-04  
**Última revisión de stack**: 2026-05-04 (agregado Injector DI)
