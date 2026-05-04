# Unity Project Test

## Descripción
Proyecto de Unity para pruebas y desarrollo.

## Requisitos

- **Unity Version**: Unity 6000.2.6f2 (Unity 6.2 Tech Stream)
- **LTS Recomendada**: Unity 6.3 LTS (Soporte hasta diciembre 2027)

## Configuración del Proyecto

Este proyecto utiliza:
- Git para control de versiones
- .gitignore configurado para Unity (basado en estándares de la industria)

## Estructura de Carpetas

```
├── Assets/          # Recursos del proyecto
├── Packages/        # Dependencias de paquetes
├── ProjectSettings/ # Configuración del proyecto
└── .gitignore       # Archivos ignorados por git
```

## Buenas Prácticas

- No subir la carpeta `Library/` al repositorio (se regenera localmente)
- Commitear siempre los archivos `.meta` junto con sus assets
- Usar Unity LTS para proyectos en producción
- Hacer commits atómicos y descriptivos

## Instalación

1. Clonar el repositorio
2. Abrir con Unity Hub usando la versión 6000.2.6f2 o superior
3. La carpeta `Library/` se regenerará automáticamente

## Notas

Actualmente usando Unity 6.2 (Tech Stream). Para producción, considera migrar a **Unity 6.3 LTS** cuando sea estable.
