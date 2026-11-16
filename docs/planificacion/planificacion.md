# Planificación del Proyecto y Diagrama de Gantt

## 1. Hitos del Proyecto Semestral

| Hito | Descripción | Fechas | Estado | Responsable |
| :--- | :--- | :---: | :---: | :--- |
| **H1: Análisis y Definición** | Definición del alcance del Pasanaku Digital y selección del stack tecnológico. | 03/ago - 08/ago | Completado | Equipo |
| **H2: Base de Datos 3FN** | Diseño DDL/DML en SQL Server, modelo relacional en 3FN y restricciones de unicidad. | 31/ago - 12/sep | Completado | Sebastián |
| **H3: Backend API y Frontend** | Desarrollo de `PasanakuRepository` (ADO.NET puro) y Dashboard estático Vanilla JS. | 12/oct - 24/oct | Completado | Sebastián |
| **H4: Seguridad y Estandarización** | Autenticación JWT, modales interactivos y estandarización del repositorio GitHub. | 16/nov - 24/nov | En progreso | Equipo |
| **H5: Despliegue en la Nube** | Publicación en producción: SQL Server, API (SmarterASP.NET/Azure) y Frontend (Vercel). | 11/dic - 19/dic | Pendiente | Equipo |

---

## 2. Diagrama de Gantt Actualizado

```mermaid
gantt
    title Cronograma de Desarrollo - Pasanaku Digital (Semestre 02/2026)
    dateFormat  YYYY-MM-DD
    
    section Hito 1
    Definición y Arquitectura           :done,    h1, 2026-08-03, 2026-08-08
    
    section Hito 2
    Script SQL Server y Modelo 3FN      :done,    h2, 2026-08-31, 2026-09-12
    
    section Hito 3
    Backend C# (ADO.NET) y API REST     :done,    h3a, 2026-10-12, 2026-10-18
    Frontend Dashboard (HTML/CSS/JS)    :done,    h3b, 2026-10-18, 2026-10-24
    
    section Hito 4
    Seguridad JWT y Modales             :active,  h4a, 2026-11-16, 2026-11-20
    Estandarización GitHub UNIFRANZ     :active,  h4b, 2026-11-20, 2026-11-24
    
    section Hito 5
    Hosting SQL, API y Frontend         :         h5, 2026-12-11, 2026-12-19
```