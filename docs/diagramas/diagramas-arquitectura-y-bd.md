# Diagramas del Sistema — Pasanaku Digital

GitHub renderiza automáticamente los siguientes diagramas en formato **Mermaid**.

## 1. Diagrama Entidad-Relación (Modelo de Datos 3FN)

```mermaid
erDiagram
    USUARIOS {
        int ID PK
        nvarchar Nombre
        nvarchar Apellido
        varchar Telefono
        varchar Correo UK
        varchar PasswordHash
    }

    GRUPOS {
        int ID PK
        nvarchar Nombre
        decimal MontoCuota
        varchar Frecuencia
        date FechaInicio
        int CicloActual
        varchar Estado
    }

    MIEMBROS_GRUPO {
        int ID_Grupo PK, FK
        int ID_Usuario PK, FK
        int TurnoCobro UK
        varchar EstadoCobro
    }

    PAGOS {
        int ID PK
        int ID_Grupo FK
        int ID_Usuario FK
        decimal Monto
        datetime FechaPago
        int CicloCorrespondiente
    }

    USUARIOS ||--o{ MIEMBROS_GRUPO : "participa en"
    GRUPOS ||--|{ MIEMBROS_GRUPO : "contiene"
    MIEMBROS_GRUPO ||--o{ PAGOS : "realiza"
```

---

## 2. Diagrama de Componentes y Arquitectura

```mermaid
flowchart LR
    subgraph Cliente ["Frontend (Navegador Web)"]
        UI["index.html + style.css"]
        JS["app.js (Fetch API + JWT)"]
        UI --> JS
    end

    subgraph Servidor ["Backend (ASP.NET Core Web API - .NET 10)"]
        AUTH["AuthController (/api/Auth)"]
        CTRL["PasanakuController (/api/Pasanaku)"]
        REPO["PasanakuRepository (ADO.NET Microsoft.Data.SqlClient)"]
        CTRL --> REPO
        AUTH --> REPO
    end

    subgraph BaseDeDatos ["Persistencia (SQL Server)"]
        DB[("PasanakuDB (3FN)")]
    end

    JS -- "HTTP/JSON + Bearer Token" --> AUTH
    JS -- "HTTP/JSON + Bearer Token" --> CTRL
    REPO -- "T-SQL / SqlTransaction" --> DB
```

---

## 3. Diagrama de Secuencia: Entrega de Pozo y Avance de Turno

```mermaid
sequenceDiagram
    actor Usuario as Organizador (Navegador)
    participant JS as app.js
    participant API as PasanakuController
    participant Repo as PasanakuRepository
    participant SQL as SQL Server (PasanakuDB)

    Usuario->>JS: Clic en "Entregar Pozo y Cerrar Turno"
    JS->>API: POST /api/Pasanaku/grupo/1/avanzar-turno (Bearer JWT)
    API->>Repo: AvanzarSiguienteTurnoAsync(1)
    Repo->>SQL: BEGIN TRANSACTION
    Repo->>SQL: Verificar PagosRealizados == TotalMiembros
    SQL-->>Repo: 5 de 5 pagados
    Repo->>SQL: UPDATE Miembros_Grupo SET EstadoCobro = 'Cobrado'
    Repo->>SQL: UPDATE Grupos SET CicloActual = CicloActual + 1
    Repo->>SQL: COMMIT TRANSACTION
    Repo-->>API: (true, "¡Pozo entregado! Turno avanzado")
    API-->>JS: 200 OK (JSON)
    JS->>Usuario: Actualiza Dashboard al nuevo turno
```