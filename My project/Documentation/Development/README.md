# Development Plans

Esta carpeta contiene los planes de desarrollo orientados a ejecución.

## Propósito

Cada documento de esta carpeta está diseñado para ser consumido por:
1. **Project Manager**: Para crear tickets SCRUM (épicas, historias, tareas) en Jira, Linear, etc.
2. **AI Agents**: Para saber qué hacer cuando reciben "seguí trabajando" — leen el sprint actual, buscan la próxima tarea `pending`, y la ejecutan.
3. **Desarrolladores**: Para entender el scope, las dependencias, y el Definition of Done de cada tarea.

## Estructura

```
Development/
├── README.md                           ← Este archivo
├── Phase1-Sprint1-Backlog.md           ← Sprint 1: Arquitectura base + Input
├── Phase1-Sprint2-Backlog.md           ← Sprint 2: Movement + Polish
└── (futuros sprints por fase)
```

## Formato de cada Sprint Backlog

Cada backlog contiene:
- **Sprint Goal**: Objetivo del sprint en una oración
- **Épicas**: Agrupación funcional (ej: "Project Setup", "Input System")
- **User Stories**: En formato "Como [rol] quiero [qué] para [beneficio]"
- **Tasks**: Tareas concretas con:
  - `[ ]` / `[x]` para estado
  - Archivos a crear/modificar
  - Acceptance criteria
  - Dependencias entre tareas
  - Estimación en puntos (1=trivial, 2=simple, 3=medio, 5=complejo, 8=grande)

## Cómo usar como PM

1. Cada **User Story** es un ticket de historia
2. Cada **Task** es un subtask
3. Las **Acceptance Criteria** van en la descripción del ticket
4. Los **Story Points** están estimados para sprints de 1 semana

## Cómo usar como AI Agent

1. Abrir el sprint backlog actual
2. Buscar la primera tarea con `[ ]` (pending)
3. Verificar que sus dependencias estén `[x]` (completed)
4. Ejecutar la tarea siguiendo los acceptance criteria
5. Marcar como `[x]` al completar
6. Pasar a la siguiente tarea

---

**Relacionado**: [INDEX.md](../INDEX.md) | [09-Three-Layer-Architecture.md](../Architecture/09-Three-Layer-Architecture.md)
