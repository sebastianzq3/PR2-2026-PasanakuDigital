# Bitácora de Seguimiento del Proyecto

## Resumen de Avance Actual: **85% Completado**

### Tareas Completadas
* [x] Diseño y normalización en 3FN de la base de datos `PasanakuDB` (`Usuarios`, `Grupos`, `Miembros_Grupo`, `Pagos`).
* [x] Implementación de la capa de acceso a datos con `Microsoft.Data.SqlClient` sin usar ORMs.
* [x] Resolución de incompatibilidad de paquetes en `.NET 10.0` y levantamiento exitoso en `https://localhost:7056`.
* [x] Diseño UI/UX responsivo con paleta verde esmeralda y cinta superior estilo aguayo.
* [x] Integración de inicio de sesión con `JwtBearer` y modales para crear nuevos participantes y grupos de Pasanaku.
* [x] Documentación técnica y manual de usuario bajo el estándar de la materia `PRO-212`.

### Próximos Pasos (Etapa 2)
* [ ] Crear instancia de base de datos SQL Server en la nube (SmarterASP.NET o Azure SQL).
* [ ] Publicar el proyecto `PasanakuAPI` desde Visual Studio mediante Web Deploy.
* [ ] Desplegar el frontend estático en Vercel / GitHub Pages apuntando a la URL de producción.