# Especificación de Requisitos — Pasanaku Digital

## 1. Requisitos Funcionales (RF)

| Código | Nombre | Descripción | Prioridad | Estado |
| :--- | :--- | :--- | :---: | :---: |
| **RF-01** | Autenticación de Usuarios | El sistema debe permitir a un usuario registrado iniciar sesión mediante su correo y contraseña, devolviendo un token JWT válido por 8 horas. | Alta | Completado |
| **RF-02** | Registro de Participantes | El sistema debe permitir registrar nuevos participantes con Nombre, Apellido, Teléfono, Correo único y Contraseña. | Alta | Completado |
| **RF-03** | Creación de Grupo y Turnos | El sistema debe permitir crear un grupo de Pasanaku definiendo nombre, cuota (`> 0`), frecuencia (`Semanal`, `Quincenal`, `Mensual`), fecha de inicio y el orden de turnos de al menos 2 participantes. | Alta | Completado |
| **RF-04** | Dashboard del Pasanaku | El sistema debe mostrar el ciclo actual, monto de la cuota, pozo total a entregar, monto recaudado en el ciclo y los datos del receptor del turno vigente. | Alta | Completado |
| **RF-05** | Registro de Pago por Ciclo | El sistema debe permitir marcar el pago de la cuota de un participante en el ciclo actual, impidiendo que un mismo usuario pague dos veces en el mismo ciclo. | Alta | Completado |
| **RF-06** | Entrega de Pozo y Avance de Turno | El sistema debe validar que el 100% de los miembros haya pagado el ciclo actual antes de permitir marcar al receptor como `Cobrado` y avanzar `CicloActual + 1`. | Alta | Completado |
| **RF-07** | Finalización Automática | Cuando se entregue el pozo correspondiente al último turno (`CicloActual == TotalMiembros`), el sistema debe cambiar el estado del grupo a `Finalizado`. | Media | Completado |

---

## 2. Requisitos No Funcionales (RNF)

| Código | Categoría | Descripción y Criterio de Verificación |
| :--- | :--- | :--- |
| **RNF-01** | Acceso a Datos (Restricción Técnica) | La comunicación entre C# y SQL Server debe implementarse estrictamente con el paquete NuGet `Microsoft.Data.SqlClient` (ADO.NET puro), sin utilizar Entity Framework ni otros ORMs. |
| **RNF-02** | Frontend sin Frameworks | El cliente web debe construirse únicamente con HTML5, CSS3 y Vanilla JavaScript (`fetch()`), sin dependencias de React, Angular o Vue. |
| **RNF-03** | Integridad de Base de Datos | El esquema en SQL Server debe estar normalizado en Tercera Forma Normal (3FN) y contar con restricciones `PRIMARY KEY`, `FOREIGN KEY`, `UNIQUE` y `CHECK`. |
| **RNF-04** | Atomicidad (Transacciones) | La creación de grupos con miembros y el avance de turno deben ejecutarse dentro de transacciones SQL (`SqlTransaction`) con `Commit` y `Rollback`. |
| **RNF-05** | Diseño Responsivo (UI/UX) | La interfaz debe adaptarse a pantallas móviles (`max-width: 640px`) y de escritorio empleando Flexbox y el modelo `box-sizing: border-box`. |
| **RNF-06** | Seguridad de API | Todos los endpoints de `PasanakuController` deben exigir autorización mediante el atributo `[Authorize]` y validación de firma JWT. |