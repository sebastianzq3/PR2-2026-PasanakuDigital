# Visión y Alcance del Producto — Pasanaku Digital

## 1. Visión del Producto
Convertirse en una herramienta web ligera, confiable y accesible desde cualquier navegador móvil o de escritorio que permita a grupos de ahorro en Bolivia administrar sus ciclos de **Pasanaku** con total claridad financiera, eliminando disputas por cobros confusos o turnos mal registrados.

## 2. Objetivos del Sistema
* **Objetivo General:** Desarrollar un sistema web Full-Stack con arquitectura desacoplada (Frontend HTML/CSS/JS Vanilla + Backend C# ASP.NET Core Web API + SQL Server en 3FN) para gestionar el ciclo de vida completo de grupos de ahorro rotativo.
* **Objetivos Específicos:**
  1. Garantizar la integridad de los turnos y pagos mediante restricciones relacionales en SQL Server y transacciones en ADO.NET (`Microsoft.Data.SqlClient`).
  2. Ofrecer una interfaz intuitiva basada en tarjetas (*Cards*) con identidad visual inspirada en colores de confianza financiera (esmeralda) y acentos cálidos andinos (aguayo).
  3. Proteger las operaciones del sistema mediante autenticación basada en JSON Web Tokens (JWT).

## 3. Funcionalidades dentro del Alcance (In-Scope)
* Inicio y cierre de sesión con JWT (`POST /api/Auth/login`).
* Registro de nuevos usuarios/participantes (`POST /api/Auth/registrar`).
* Creación de grupos de Pasanaku con asignación ordenada de turnos (`POST /api/Pasanaku/grupos`).
* Consulta de listado de grupos y cambio dinámico de grupo activo en el Dashboard (`GET /api/Pasanaku/grupos`).
* Visualización detallada del estado financiero del ciclo actual, receptor de turno y estado individual de cada miembro (`GET /api/Pasanaku/grupo/{id}`).
* Registro de aportes individuales por ciclo (`POST /api/Pasanaku/pagar`).
* Cierre transaccional de turno, entrega de pozo y avance automático de ciclo o finalización de grupo (`POST /api/Pasanaku/grupo/{id}/avanzar-turno`).

## 4. Límites y Exclusiones del Alcance (Out-of-Scope)
Para mantener el enfoque académico y la arquitectura de mínimas dependencias del semestre actual, quedan fuera del alcance:
* Pasarelas de cobro bancario directo o generación automática de códigos QR interbancarios (Simple QR). El sistema registra la confirmación del aporte, no mueve fondos bancarios reales.
* Subastas de turnos o pasanakus con cuotas variables por miembro.
* Notificaciones automatizadas por WhatsApp o SMS.