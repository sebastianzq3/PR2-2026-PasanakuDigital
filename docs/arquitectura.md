# Arquitectura del Sistema — Pasanaku Digital

## 1. Estilo Arquitectónico
El proyecto implementa una arquitectura **Cliente-Servidor desacoplada basada en una API REST** con separación en tres capas lógicas:

1. **Capa de Presentación (Cliente Web Estático):**
   * **Tecnologías:** HTML5 (`index.html`), CSS3 (`style.css`) y JavaScript nativo (`app.js`).
   * **Responsabilidad:** Renderizar dinámicamente las tarjetas del dashboard, gestionar los modales de creación, almacenar el token JWT en `localStorage` y consumir los endpoints del servidor mediante peticiones HTTP asíncronas (`fetch`).

2. **Capa de Lógica de Negocio y Servicios (Backend API):**
   * **Tecnologías:** C# en ASP.NET Core Web API (`.NET 10.0`).
   * **Componentes:**
     * `AuthController`: Gestiona `/api/Auth/login` y `/api/Auth/registrar`, firmando tokens JWT con `HmacSha256`.
     * `PasanakuController`: Expone los endpoints REST protegidos por `[Authorize]` para consultar grupos, registrar pagos y avanzar turnos.
     * `PasanakuRepository`: Implementa el **Patrón Repositorio** encapsulando todas las consultas parametrizadas T-SQL y transacciones mediante `Microsoft.Data.SqlClient`.
     * `PasanakuModels`: DTOs (*Data Transfer Objects*) para serialización JSON limpia.

3. **Capa de Persistencia de Datos:**
   * **Tecnología:** Microsoft SQL Server (`PasanakuDB`).
   * **Responsabilidad:** Almacenamiento relacional en 3FN y aplicación de reglas de integridad referencial y de dominio.

---

## 2. Decisiones Técnicas Clave
* **Uso de `Microsoft.Data.SqlClient` (ADO.NET) en lugar de ORMs:** Permite un control exacto sobre las consultas `JOIN`, el uso de `SqlTransaction` al avanzar turnos y un menor consumo de memoria y dependencias externas.
* **Consultas Parametrizadas (`@Parametro`):** Todas las instrucciones `SqlCommand` utilizan parámetros tipados para prevenir ataques de Inyección SQL (*SQL Injection*).
* **Autenticación Stateless con JWT:** Evita mantener sesiones en memoria del servidor web, facilitando el despliegue independiente del frontend (en Vercel/GitHub Pages) y del backend (en SmarterASP.NET o Azure).