# Work Item Management System — Documentación Técnica

---

## Tabla de Contenidos

1. [Descripción General](#descripción-general)
2. [Arquitectura](#arquitectura)
3. [Clean Architecture](#clean-architecture)
4. [Principios SOLID](#principios-solid)
5. [Interfaces del Sistema](#interfaces-del-sistema)
6. [Flujo de Asignación](#flujo-de-asignación)
7. [Algoritmo de Distribución](#algoritmo-de-distribución)
8. [Ejemplos Reales](#ejemplos-reales)
9. [Resultados de Prueba](#resultados-de-prueba)

---

## Descripción General

Sistema web desarrollado en **.NET 9.0** para la gestión y distribución automática de ítems de trabajo entre usuarios. El sistema aplica un algoritmo de distribución inteligente basado en urgencia, relevancia y carga de trabajo por usuario.

---

## Arquitectura

El sistema sigue el patrón de **Microservicios**, separado en dos servicios independientes:

```
┌─────────────────────────────────────────────────────────┐
│                    API Gateway / Client                  │
└────────────────────┬────────────────────┬───────────────┘
                     │                    │
          ┌──────────▼──────────┐ ┌───────▼──────────────┐
          │  WorkItems Service  │ │   Users Service       │
          │                     │ │                       │
          │  - Gestión de items │ │  - Gestión usuarios   │
          │  - Asignación       │ │  - Estado de carga    │
          │  - Priorización     │ │  - UserWork tracking  │
          └──────────┬──────────┘ └───────┬───────────────┘
                     │                    │
          ┌──────────▼────────────────────▼───────────────┐
          │              Base de Datos                     │
          │   tbl_work_items | tbl_user | tbl_user_work    │
          └────────────────────────────────────────────────┘
```

### Estructura de Capas por Microservicio

```
WorkItemService/
├── Api/                        # Controllers, Program.cs
├── Application/                # Commands, Handlers, Validators, DTOs
│   ├── WorkItems/
│   │   └── Commands/
│   │       └── AssignWorkItem/
│   │           ├── AssignWorkItemCommand.cs
│   │           ├── AssignWorkItemCommandHandler.cs
│   │           └── AssignWorkItemCommandValidator.cs
├── Domain/                     # Entities, Enums, Interfaces
│   ├── Entities/
│   │   ├── WorkItem.cs
│   │   ├── User.cs
│   │   └── UserWork.cs
│   ├── Enums/
│   │   ├── WorkItemStatus.cs
│   │   ├── UserWorkStatus.cs
│   │   └── Relevance.cs
│   └── Interfaces/
│       ├── IWorkItemRepository.cs
│       └── IUserWorkRepository.cs
└── Infrastructure/             # DbContext, Repositories
    ├── Persistence/
    │   └── ApplicationDbContext.cs
    └── Repositories/
        ├── WorkItemRepository.cs
        └── UserWorkRepository.cs
```

---

## Clean Architecture

El sistema respeta estrictamente las capas de Clean Architecture, donde **las dependencias apuntan siempre hacia adentro**:

```
┌─────────────────────────────────────────┐
│               API Layer                 │  ← Controllers
│  ┌───────────────────────────────────┐  │
│  │         Application Layer         │  │  ← Commands, Handlers, Validators
│  │  ┌─────────────────────────────┐  │  │
│  │  │       Domain Layer          │  │  │  ← Entities, Enums, Interfaces
│  │  └─────────────────────────────┘  │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
          ↑
Infrastructure Layer                          ← Repositories, DbContext
(implementa interfaces del Domain)
```

| Capa | Responsabilidad | Dependencias |
|---|---|---|
| **Domain** | Entidades, Enums, contratos (interfaces) | Ninguna |
| **Application** | Lógica de negocio, CQRS | Solo Domain |
| **Infrastructure** | Implementación de repositorios, EF Core | Domain |
| **Api** | Exposición HTTP, Controllers | Application |

### Regla clave
> La capa **Domain** no conoce ni EF Core, ni HTTP, ni ningún framework externo. Es código C# puro.

---

## Principios SOLID

### S — Single Responsibility Principle

Cada clase tiene una única razón para cambiar:

| Clase | Única responsabilidad |
|---|---|
| `AssignWorkItemCommandHandler` | Orquestar la lógica de asignación |
| `AssignWorkItemCommandValidator` | Validar el comando de entrada |
| `WorkItemRepository` | Acceso a datos de `WorkItem` |
| `UserWorkRepository` | Acceso a datos de `UserWork` y `User` |
| `WorkItemsController` | Recibir peticiones HTTP y delegar al Mediator |

### O — Open/Closed Principle

El sistema es **abierto para extensión, cerrado para modificación**:

- Nuevos commands se agregan implementando `IRequest<T>` sin modificar código existente.
- Nuevas reglas de distribución se pueden inyectar sin tocar el handler actual.

```csharp
// Extender sin modificar: nuevo command independiente
public record CompleteWorkItemCommand(int UserWorkId) : IRequest<bool>;
```

### L — Liskov Substitution Principle

Las implementaciones concretas son **sustituibles** por sus interfaces:

```csharp
// El handler solo conoce la interfaz, no la implementación
public AssignWorkItemCommandHandler(
    IWorkItemRepository workItemRepository,     // No WorkItemRepository
    IUserWorkRepository userWorkRepository)     // No UserWorkRepository
```

### I — Interface Segregation Principle

Las interfaces están **segregadas por responsabilidad**:

```csharp
// IWorkItemRepository — solo lo que WorkItem necesita
public interface IWorkItemRepository
{
    Task<WorkItem> CreateAsync(...);
    Task<WorkItem?> GetByIdAsync(...);
    Task<WorkItem?> GetNextPendingAsync(...);
    Task UpdateAsync(...);
    Task<bool> ExistsByCodeAsync(...);
}

// IUserWorkRepository — solo lo que UserWork necesita
public interface IUserWorkRepository
{
    Task<List<UserWork>> GetPendingByUserAsync(...);
    Task<List<User>> GetAllActiveUsersAsync(...);
    Task AddAsync(...);
    Task UpdateRangeAsync(...);
}
```

> No existe una interfaz `IRepository` genérica que mezcle responsabilidades de ambas entidades.

### D — Dependency Inversion Principle

Las capas de alto nivel dependen de **abstracciones**, no de implementaciones concretas:

```csharp
// ✅ Correcto — depende de la abstracción
private readonly IWorkItemRepository _workItemRepository;
private readonly IUserWorkRepository _userWorkRepository;

// ❌ Incorrecto — dependería de la implementación
private readonly WorkItemRepository _workItemRepository;
```

El registro en DI container desacopla la implementación:

```csharp
builder.Services.AddScoped<IWorkItemRepository, WorkItemRepository>();
builder.Services.AddScoped<IUserWorkRepository, UserWorkRepository>();
```

---

## Interfaces del Sistema

### IWorkItemRepository

```csharp
public interface IWorkItemRepository
{
    Task<WorkItem> CreateAsync(WorkItem workItem, CancellationToken cancellationToken = default);
    Task<WorkItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<WorkItem?> GetNextPendingAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkItem workItem, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
}
```

| Método | Propósito |
|---|---|
| `CreateAsync` | Persiste un nuevo WorkItem |
| `GetByIdAsync` | Recupera un WorkItem por ID |
| `GetNextPendingAsync` | Obtiene el siguiente ítem a asignar según prioridad |
| `UpdateAsync` | Actualiza el estado de un WorkItem |
| `ExistsByCodeAsync` | Verifica unicidad del código |

---

### IUserWorkRepository

```csharp
public interface IUserWorkRepository
{
    Task<List<UserWork>> GetPendingByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<User>> GetAllActiveUsersAsync(CancellationToken cancellationToken = default);
    Task AddAsync(UserWork userWork, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(List<UserWork> userWorks, CancellationToken cancellationToken = default);
}
```

| Método | Propósito |
|---|---|
| `GetPendingByUserAsync` | Lista pendientes de un usuario (para calcular carga) |
| `GetAllActiveUsersAsync` | Obtiene todos los usuarios disponibles |
| `AddAsync` | Registra una nueva asignación |
| `UpdateRangeAsync` | Reordena prioridades tras cada asignación |

---

## Flujo de Asignación

```
POST /api/work-items/assign
           │
           ▼
┌─────────────────────┐
│  WorkItemsController │
│  .Assign()          │
└────────┬────────────┘
         │ ISender.Send(AssignWorkItemCommand)
         ▼
┌─────────────────────────────┐
│ AssignWorkItemCommandHandler │
│                             │
│ 1. GetNextPendingAsync()    │◄── Selecciona ítem más prioritario
│    ┌─────────────────────┐  │
│    │ Urgente (< 3 días)? │  │
│    └────────┬────────────┘  │
│             │               │
│         SÍ ▼          NO ▼  │
│    Todos los      Filtrar   │
│    usuarios    saturados    │
│                             │
│ 2. SelectUserWithLeast      │◄── Usuario con menos pendientes
│    PendingAsync()           │
│                             │
│ 3. AddAsync(UserWork)       │◄── Registra asignación
│                             │
│ 4. UpdateAsync(WorkItem)    │◄── StatusWi = Assigned
│                             │
│ 5. ReorderUserPending       │◄── Reordena prioridades del usuario
│    ItemsAsync()             │
└─────────────────────────────┘
         │
         ▼
┌─────────────────────┐
│  AssignWorkItemResult│
│  - userWorkId       │
│  - assignedUsername │
│  - workItemId       │
│  - workItemCode     │
│  - orderPriority    │
└─────────────────────┘
```

---

## Algoritmo de Distribución

### Reglas en orden de precedencia

```
┌──────────────────────────────────────────────────────────┐
│               ALGORITMO DE DISTRIBUCIÓN                  │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  1. SELECCIÓN DE ÍTEM                                    │
│     ├── Urgentes primero (ExpirationDate < 3 días)       │
│     ├── Luego High relevance                             │
│     └── Luego por fecha de expiración más próxima        │
│                                                          │
│  2. SELECCIÓN DE USUARIO                                 │
│     ├── Si ítem es URGENTE:                              │
│     │   └── Todos los usuarios activos son candidatos    │
│     └── Si ítem NO es urgente:                           │
│         └── Excluir saturados                            │
│             (usuario con > 3 ítems High pendientes)      │
│                                                          │
│  3. DE LOS CANDIDATOS                                    │
│     └── Elegir el usuario con MENOS ítems pendientes     │
│                                                          │
│  4. POST-ASIGNACIÓN                                      │
│     └── Reordenar pendientes del usuario asignado        │
│         ├── High relevance primero                       │
│         └── Luego por fecha de expiración                │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

### Valores en base de datos

| Concepto | Campo | Valor |
|---|---|---|
| WorkItem Pendiente | `StatusWi` | `"0"` |
| WorkItem Asignado | `StatusWi` | `"1"` |
| WorkItem Completado | `StatusWi` | `"2"` |
| UserWork Pendiente | `Status` | `"0"` |
| UserWork Completado | `Status` | `"1"` |
| Relevancia Alta | `Relevance` | `"H"` |
| Relevancia Baja | `Relevance` | `"L"` |
| Usuario Activo | `StatusUs` | `"A"` |

---

## Ejemplos Reales

### Datos de prueba creados vía `POST /api/work-items`

#### 🔴 Urgentes — vencen en menos de 3 días

```json
{ "code": "WI-001", "description": "Fix producción crítico", "relevance": "H", "expirationDate": "2026-06-03T08:00:00.000Z" }
```
```json
{ "code": "WI-002", "description": "Parche base de datos", "relevance": "L", "expirationDate": "2026-06-04T08:00:00.000Z" }
```
```json
{ "code": "WI-003", "description": "Reporte regulatorio", "relevance": "H", "expirationDate": "2026-06-03T23:59:00.000Z" }
```

#### 🟡 Alta relevancia — no urgentes

```json
{ "code": "WI-004", "description": "Migración de servicios", "relevance": "H", "expirationDate": "2026-06-15T08:00:00.000Z" }
```
```json
{ "code": "WI-005", "description": "Auditoría de seguridad", "relevance": "H", "expirationDate": "2026-06-20T08:00:00.000Z" }
```
```json
{ "code": "WI-006", "description": "Optimización de queries", "relevance": "H", "expirationDate": "2026-06-18T08:00:00.000Z" }
```
```json
{ "code": "WI-007", "description": "Revisión de arquitectura", "relevance": "H", "expirationDate": "2026-06-25T08:00:00.000Z" }
```

#### 🟢 Baja relevancia — no urgentes

```json
{ "code": "WI-008", "description": "Actualizar documentación", "relevance": "L", "expirationDate": "2026-07-01T08:00:00.000Z" }
```
```json
{ "code": "WI-009", "description": "Refactor módulo de reportes", "relevance": "L", "expirationDate": "2026-07-10T08:00:00.000Z" }
```
```json
{ "code": "WI-010", "description": "Limpieza de logs antiguos", "relevance": "L", "expirationDate": "2026-07-15T08:00:00.000Z" }
```

---

## Resultados de Prueba

Llamadas consecutivas a `POST /api/work-items/assign` con usuarios: **Mateo, Luis, Paul, Sofia, Michelle**

### Asignaciones obtenidas

| # | userWorkId | assignedUsername | workItemCode | orderPriority | Regla aplicada |
|---|---|---|---|---|---|
| 1 | 3 | Mateo | WI-003 | 1 | Urgente + High, usuario con menos carga |
| 2 | 4 | Luis | WI-002 | 1 | Urgente + Low, siguiente usuario libre |
| 3 | 5 | Paul | WI-004 | 1 | High relevance, no saturado, menos pendientes |
| 4 | 6 | Sofia | WI-006 | 1 | High relevance, expira antes que WI-005 |
| 5 | 7 | Michelle | WI-005 | 2 | High relevance, Michelle ya tenía 1 pendiente |

### Análisis de resultados

**Llamadas 1 y 2 — Regla de urgencia:**
> WI-003 y WI-002 fueron tomados primero por tener fecha de expiración menor a 3 días. La relevancia no fue determinante — WI-002 es `Low` pero igualmente fue priorizado por urgencia. La distribución rotó entre Mateo y Luis correctamente.

**Llamadas 3, 4 y 5 — Regla de relevancia:**
> Con los urgentes agotados, el sistema tomó los ítems `High relevance` ordenados por fecha de expiración. WI-006 (18 jun) fue antes que WI-005 (20 jun). Michelle recibió `orderPriority: 2` porque ya tenía un ítem pendiente, confirmando que el reordenamiento post-asignación funciona.

### Orden esperado de asignación completo

| Llamada | Ítem esperado | Razón |
|---|---|---|
| 1° | WI-001 o WI-003 | Urgente + High |
| 2° | WI-001 o WI-003 | Urgente + High |
| 3° | WI-002 | Urgente + Low |
| 4° | WI-004 | High, expira 15 jun |
| 5° | WI-006 | High, expira 18 jun |
| 6° | WI-005 | High, expira 20 jun |
| 7° | WI-007 | High, expira 25 jun |
| 8° | WI-008 | Low, expira 01 jul |
| 9° | WI-009 | Low, expira 10 jul |
| 10° | WI-010 | Low, expira 15 jul |

---

*Documentación generada para WorkItem Management System — .NET 9.0 / Clean Architecture / DDD*
