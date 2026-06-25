# EventosVivos API - Backend

## Descripcion

API RESTful para el sistema de gestion de eventos y reservas de EventosVivos. Desarrollado con .NET 8 y Clean Architecture.

## Arquitectura

Se implemento **Clean Architecture** con las siguientes capas:

```
src/
  EventosVivos.Domain/        -> Entidades, Enums, Interfaces, Excepciones
  EventosVivos.Application/   -> DTOs, Servicios, Interfaces de servicios
  EventosVivos.Infrastructure/-> DbContext, Repositorios (EF Core InMemory)
  EventosVivos.API/           -> Controllers, Middleware, Program.cs
tests/
  EventosVivos.Tests/         -> Pruebas unitarias (xUnit + Moq)
```

### Justificacion

- **Separacion de responsabilidades**: Cada capa tiene un proposito unico.
- **Independencia de infraestructura**: El dominio no depende de EF Core ni de ASP.NET.
- **Testabilidad**: Los servicios se prueban con mocks, sin base de datos real.
- **Escalabilidad**: Facil de migrar a PostgreSQL/SQL Server cambiando solo Infrastructure.

## Tecnologias

| Tecnologia | Version | Proposito |
|---|---|---|
| .NET | 8.0 | Runtime y SDK |
| ASP.NET Core | 8.0 | Framework Web API |
| Entity Framework Core | 8.0.11 | ORM (InMemory para dev) |
| JWT Bearer | 8.0.11 | Autenticacion |
| BCrypt.Net | 4.0.3 | Hash de passwords |
| xUnit | 2.5+ | Framework de testing |
| Moq | 4.20.72 | Mocking para tests |
| Swagger/OpenAPI | Built-in | Documentacion de API |

## Ejecutar Localmente

### Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Pasos

```bash
# Clonar repositorio
git clone <repo-url>
cd backPruebaBrayan

# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run --project src/EventosVivos.API

# La API estara disponible en:
# https://localhost:5001
# http://localhost:5000
# Swagger UI: https://localhost:5001/swagger
```

### Ejecutar Tests

```bash
dotnet test --verbosity normal
```

## Endpoints

### Auth
| Metodo | Ruta | Descripcion | Auth |
|---|---|---|---|
| POST | /api/v1/auth/register | Registrar usuario | No |
| POST | /api/v1/auth/login | Login (obtener JWT) | No |

### Eventos
| Metodo | Ruta | Descripcion | Auth |
|---|---|---|---|
| POST | /api/v1/eventos | Crear evento | Si (JWT) |
| GET | /api/v1/eventos | Listar eventos con filtros | No |
| GET | /api/v1/eventos/{id}/reporte | Reporte de ocupacion | No |

### Reservas
| Metodo | Ruta | Descripcion | Auth |
|---|---|---|---|
| POST | /api/v1/reservas | Crear reserva | No |
| PUT | /api/v1/reservas/{id}/confirmar | Confirmar pago | Si (Admin) |
| PUT | /api/v1/reservas/{id}/cancelar | Cancelar reserva | Si (JWT) |

### Venues
| Metodo | Ruta | Descripcion | Auth |
|---|---|---|---|
| GET | /api/v1/venues | Listar venues | No |

## Reglas de Negocio Implementadas

- **RN-01**: Capacidad del evento no puede exceder la del venue.
- **RN-02**: No se permite superposicion de eventos activos en el mismo venue.
- **RN-03**: Eventos en fines de semana no pueden iniciar despues de las 22:00.
- **RN-04**: No se permiten reservas para eventos que inicien en menos de 1 hora.
- **RN-05**: Eventos con precio > $100 limitan a 10 entradas por transaccion.
- **RN-06**: Evento se marca completado automaticamente al superar su fecha fin.
- **RN-07**: Cancelacion con menos de 48h marca entradas como "perdidas".

## Seguridad

- Autenticacion JWT con tokens de corta duracion (2 horas).
- Passwords hasheados con BCrypt.
- Validacion de modelo con Data Annotations.
- Middleware centralizado de excepciones (no expone stack traces).
- CORS configurado.
- Swagger con soporte para Authorization Bearer.

## Datos de Referencia (Seed)

| ID | Venue | Capacidad | Ciudad |
|---|---|---|---|
| 1 | Auditorio Central | 200 | Bogota |
| 2 | Sala Norte | 50 | Bogota |
| 3 | Arena Sur | 500 | Medellin |

## Autor

Brayan Munoz

