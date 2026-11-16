# Modelo de Datos (SQL Server en 3FN)

## 1. Justificación de Normalización (Tercera Forma Normal - 3FN)
* **1FN:** Todos los atributos son atómicos y cada tabla posee una clave primaria definida.
* **2FN:** En la tabla intermedia `Miembros_Grupo` (cuya clave primaria compuesta es `(ID_Grupo, ID_Usuario)`), los atributos `TurnoCobro` y `EstadoCobro` dependen funcionalmente de la combinación completa del grupo y del usuario, no de uno solo.
* **3FN:** No existen dependencias transitivas. Por ejemplo, el `PozoTotal` o el `MontoRecaudadoCiclo` no se almacenan como columnas redundantes en la tabla `Grupos`, sino que se calculan a partir de `MontoCuota` y los registros reales de `Pagos`.

---

## 2. Diccionario de Tablas

### 2.1. Tabla `Usuarios`
Almacena la identidad y credenciales de acceso de cada participante.
* `ID` (`INT IDENTITY PRIMARY KEY`): Identificador único.
* `Nombre`, `Apellido` (`NVARCHAR(50) NOT NULL`): Datos personales.
* `Telefono` (`VARCHAR(20) NOT NULL`): Número de contacto.
* `Correo` (`VARCHAR(100) NOT NULL UNIQUE`): Correo único para inicio de sesión.
* `PasswordHash` (`VARCHAR(256) NOT NULL`): Clave de acceso.

### 2.2. Tabla `Grupos`
Define los parámetros financieros y el ciclo vigente de cada Pasanaku.
* `ID` (`INT IDENTITY PRIMARY KEY`): Identificador del grupo.
* `Nombre` (`NVARCHAR(100) NOT NULL`): Nombre descriptivo del Pasanaku.
* `MontoCuota` (`DECIMAL(10,2) NOT NULL CHECK > 0`): Valor en Bs. de cada aporte individual.
* `Frecuencia` (`VARCHAR(20) CHECK IN ('Semanal', 'Quincenal', 'Mensual')`).
* `FechaInicio` (`DATE NOT NULL`).
* `CicloActual` (`INT NOT NULL DEFAULT 1`): Indica el número de turno que se está jugando actualmente.
* `Estado` (`VARCHAR(20) CHECK IN ('Activo', 'Finalizado', 'Pendiente')`).

### 2.3. Tabla `Miembros_Grupo`
Resuelve la relación muchos a muchos ($N:M$) entre `Grupos` y `Usuarios` y asigna el turno de cobro.
* `ID_Grupo` (`INT FK`), `ID_Usuario` (`INT FK`): Clave primaria compuesta.
* `TurnoCobro` (`INT NOT NULL CHECK > 0`): Orden en que el miembro recibe el pozo. Posee restricción `UNIQUE (ID_Grupo, TurnoCobro)`.
* `EstadoCobro` (`VARCHAR(20) CHECK IN ('Pendiente', 'Cobrado')`).

### 2.4. Tabla `Pagos`
Registra cada aporte realizado por un miembro en un ciclo determinado.
* `ID` (`INT IDENTITY PRIMARY KEY`).
* `ID_Grupo`, `ID_Usuario` (`FK` compuesta hacia `Miembros_Grupo`).
* `Monto` (`DECIMAL(10,2) NOT NULL`).
* `FechaPago` (`DATETIME DEFAULT GETDATE()`).
* `CicloCorrespondiente` (`INT NOT NULL`): Turno al que corresponde el pago. Posee restricción `UNIQUE (ID_Grupo, ID_Usuario, CicloCorrespondiente)`.