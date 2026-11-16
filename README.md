# Pasanaku Digital — Sistema Web de Ahorro y Crédito Rotativo

> **Asignatura:** Programación II (`PRO-212`) — Universidad Franz Tamayo (UNIFRANZ), Sede Cochabamba  
> **Semestre:** 02/2026  
> **Estado del Proyecto:** Funcional (MVP Completo — Etapa 1 Finalizada)

---

## 1. Descripción y Propósito del Sistema
**Pasanaku Digital** es una aplicación web Full-Stack diseñada para digitalizar, transparentar y automatizar la gestión del **Pasanaku**, el sistema tradicional boliviano de ahorro y crédito rotativo grupal. 

El propósito del sistema es reemplazar los registros manuales en cuadernos o grupos de mensajería por una plataforma centralizada donde el organizador y los participantes puedan verificar en tiempo real quién ya aportó su cuota en el ciclo actual, cuánto dinero se ha recaudado para el pozo y a qué participante le corresponde recibir el monto total según su turno asignado.

---

## 2. Problema que Resuelve y Usuarios Objetivo
### Problema Identificado
En los pasanakus tradicionales gestionados de forma informal suelen presentarse:
1. **Falta de transparencia:** Los miembros no siempre conocen quiénes están al día con sus aportes semanales, quincenales o mensuales.
2. **Errores de control en cambios de turno:** Confusiones sobre qué participante ya cobró su pozo en ciclos pasados y a quién le corresponde el turno vigente.
3. **Riesgo de cobros duplicados o cierres incompletos:** Entregar el pozo antes de que el 100% de los participantes haya completado su cuota del ciclo.

### Usuarios Objetivo
* **Organizadores / Líderes de Pasanaku:** Personas encargadas de crear el grupo, definir el monto de la cuota, la frecuencia, el orden de los turnos y registrar los pagos recibidos.
* **Participantes (Ahorristas):** Grupos de amigos, familias, compañeros de universidad o comerciantes que participan en juegos de ahorro rotativo en Bolivia.

---

## 3. Integrantes y Responsabilidades

| Integrante | Rol Principal | Responsabilidades |
| :--- | :--- | :--- |
| **Sebastián Zubieta Quiroga** | Full-Stack Developer & Arquitecto de BD | Diseño del modelo 3FN en SQL Server, desarrollo de la Web API en C# con ADO.NET (`Microsoft.Data.SqlClient`), frontend Vanilla JS e integración JWT. |
| **Kristofer Lara Solares** | Especialista en Documentación y QA | Redacción de manuales de uso (concepto del Pasanaku), estructuración de los documentos Markdown y pruebas funcionales de la interfaz. |
| **Fabiana Darlene Zurita Delgado** | Gestora de Repositorio (GitHub) | Estandarización de carpetas en GitHub, revisión de cumplimiento de rúbrica UNIFRANZ y configuración de archivos raíz (gitignore, plantillas). |

*(Ver detalle completo en [`docs/equipo/integrantes-y-roles.md`](docs/equipo/integrantes-y-roles.md) y [`docs/equipo/acuerdos-de-trabajo.md`](docs/equipo/acuerdos-de-trabajo.md)).*

---

## 4. Funcionalidades Principales
* **Autenticación Segura con JWT:** Inicio y cierre de sesión validado contra SQL Server mediante JSON Web Tokens (`Bearer Token`).
* **Gestión de Participantes:** Registro de nuevos usuarios con validación de correo único en base de datos.
* **Creación Transaccional de Grupos de Pasanaku:** Definición de nombre, cuota en Bolivianos (`Bs.`), frecuencia (`Semanal`, `Quincenal`, `Mensual`), fecha de inicio y asignación ordenada de turnos (`Turno #1`, `Turno #2`, etc.).
* **Dashboard Financiero en Tiempo Real:** Visualización del turno actual, cuota individual, pozo total (`MontoCuota * TotalMiembros`), barra de progreso de recaudación y tarjeta destacada del receptor del turno.
* **Control de Pagos por Ciclo:** Registro de aportes individuales protegido por restricciones SQL para impedir pagos duplicados en el mismo ciclo.
* **Entrega de Pozo y Avance Automático de Turno:** Bloqueo preventivo que solo habilita el cierre de turno cuando el 100% de los miembros ha pagado su cuota, avanzando al siguiente ciclo o finalizando el Pasanaku mediante transacciones ACID.

---

## 5. Tecnologías y Requisitos del Entorno

| Capa | Tecnología Utilizada |
| :--- | :--- |
| **Frontend** | HTML5, CSS3 (Flexbox / Custom Properties / Responsive Design), Vanilla JavaScript (`fetch` API) |
| **Backend** | C# — ASP.NET Core Web API (`.NET 10.0`) |
| **Acceso a Datos** | ADO.NET puro mediante NuGet `Microsoft.Data.SqlClient` (Sin Entity Framework) |
| **Seguridad** | `Microsoft.AspNetCore.Authentication.JwtBearer` (Tokens JWT HMAC-SHA256) |
| **Base de Datos** | Microsoft SQL Server (Modelo Relacional en 3FN) |

### Requisitos Previos
1. **Visual Studio 2022 / 2026 Community** con la carga de trabajo *ASP.NET y desarrollo web*.
2. **.NET SDK 8.0 / 9.0 / 10.0** instalado.
3. **Microsoft SQL Server** (Express o Developer Edition) y **SQL Server Management Studio (SSMS)**.
4. Navegador web moderno (Chrome, Edge, Brave o Firefox).

---

## 6. Instrucciones de Instalación, Configuración y Ejecución

### Paso 1: Clonar el repositorio
```bash
git clone https://github.com/TU_USUARIO/PasanakuDigital.git
cd PasanakuDigital
```

### Paso 2: Inicializar la Base de Datos en SQL Server
1. Abre **SQL Server Management Studio (SSMS)** y conéctate a tu instancia local (`localhost` o `localhost\SQLEXPRESS`).
2. Abre el script ubicado en `database/01_schema_y_seed.sql` (o ejecuta el script provisto en [`docs/modelo-de-datos.md`](docs/modelo-de-datos.md)).
3. Ejecuta el script completo (`F5`). Esto creará la base de datos `PasanakuDB`, las 4 tablas normalizadas en 3FN y los 5 usuarios de prueba con contraseña `123456`.

### Paso 3: Configurar y Ejecutar el Backend (C# Web API)
1. Abre la solución `PasanakuAPI.sln` en **Visual Studio**.
2. Revisa el archivo `src/PasanakuAPI/appsettings.json` y asegúrate de que el `Server` en `SqlServerConnection` apunte a tu instancia local:
   ```json
   "ConnectionStrings": {
     "SqlServerConnection": "Server=localhost;Database=PasanakuDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```
   *(Nota: Por seguridad, nunca subas contraseñas reales de producción al repositorio).*
3. Restaura los paquetes NuGet (`Microsoft.Data.SqlClient` y `Microsoft.AspNetCore.Authentication.JwtBearer`).
4. Presiona **F5** para iniciar el servidor. Verifica en la consola de salida que la API esté escuchando en `https://localhost:7056`.

### Paso 4: Ejecutar el Frontend
1. Ve a la carpeta `src/frontend/` y verifica en la línea 1 de `app.js` que la constante `API_ROOT` coincida con el puerto de tu API:
   ```javascript
   const API_ROOT = "https://localhost:7056/api";
   ```
2. Abre el archivo `src/frontend/index.html` en tu navegador (o utiliza la extensión *Live Server* de VS Code).
3. Inicia sesión con la cuenta de prueba precargada:
   * **Correo:** `valeria@pasanaku.bo`
   * **Contraseña:** `123456`

---

## 7. Estructura del Repositorio y Enlaces a la Documentación

```text
PasanakuDigital/
├── README.md
├── .gitignore
├── LICENSE
├── CONTRIBUTING.md
├── CHANGELOG.md
├── SECURITY.md
├── docs/
│   ├── vision-y-alcance.md
│   ├── requisitos.md
│   ├── arquitectura.md
│   ├── modelo-de-datos.md
│   ├── manuales/
│   │   └── guia-de-uso-y-pasanaku.md
│   ├── planificacion/
│   │   ├── planificacion.md
│   │   └── seguimiento.md
│   ├── diagramas/
│   │   └── diagramas-arquitectura-y-bd.md
│   └── equipo/
│       ├── integrantes-y-roles.md
│       └── acuerdos-de-trabajo.md
├── database/
│   └── 01_schema_y_seed.sql
├── src/
│   ├── PasanakuAPI/         # Proyecto C# ASP.NET Core Web API
│   └── frontend/            # Cliente Web (index.html, style.css, app.js)
├── .github/
│   ├── ISSUE_TEMPLATE/
│   │   └── reporte-de-tarea-o-bug.md
│   └── pull_request_template.md
└── PasanakuAPI.sln
```

### Índice de Documentación Oficial (`docs/`)
* [Visión y Alcance del Producto](docs/vision-y-alcance.md)
* [Qué es el Pasanaku y Manual de Uso de la Plataforma](docs/manuales/guia-de-uso-y-pasanaku.md)
* [Especificación de Requisitos (RF y RNF)](docs/requisitos.md)
* [Arquitectura del Sistema y Decisiones Técnicas](docs/arquitectura.md)
* [Modelo de Datos en 3FN y Diccionario SQL](docs/modelo-de-datos.md)
* [Diagramas (Mermaid: Entidad-Relación, Secuencia y Componentes)](docs/diagramas/diagramas-arquitectura-y-bd.md)
* [Planificación, Hitos y Diagrama de Gantt](docs/planificacion/planificacion.md)
* [Seguimiento del Proyecto](docs/planificacion/seguimiento.md)