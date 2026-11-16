# ¿Qué es el Pasanaku y Cómo se Integra en Pasanaku Digital? (Guía de Uso)

Este documento explica el funcionamiento cultural y financiero del **Pasanaku** en Bolivia y detalla cómo cada una de sus reglas tradicionales ha sido traducida a funcionalidades concretas dentro de **Pasanaku Digital**.

---

## 1. ¿Qué es el Pasanaku? (Contexto Cultural y Financiero)

El **Pasanaku** (del quechua *pasanakuy*, que significa «pasarse de mano en mano» o «rotar entre varios») es una práctica tradicional de **ahorro y microcrédito rotativo grupal** profundamente arraigada en Bolivia. Funciona como un mecanismo financiero basado en la confianza mutua entre amigos, familiares, compañeros de trabajo o gremios comerciales, sin cobro de intereses bancarios.

### Reglas Tradicionales del Pasanaku:
1. **Conformación del Grupo:** Se reúne un número fijo de participantes $N$ (por ejemplo, $N = 5$ personas).
2. **Definición de la Cuota y Frecuencia:** Se acuerda un monto de aporte fijo $M$ (por ejemplo, $\text{Bs. } 500$) y una periodicidad (semanal, quincenal o mensual).
3. **Asignación de Turnos (Números):** Cada participante recibe un número de turno único del $1$ al $N$. El número de turnos totales es exactamente igual al número de participantes.
4. **Aporte y Formación del Pozo:** En cada ciclo (por ejemplo, cada semana), **todos** los $N$ participantes aportan su cuota $M$, formando un pozo total $P$:
   $$P = N \times M$$
   En un grupo de $5$ personas con cuota de $\text{Bs. } 500$, el pozo de cada semana es de $\text{Bs. } 2500$.
5. **Rotación del Pozo:**
   * En el **Ciclo 1**, el participante con el **Turno #1** recibe los $\text{Bs. } 2500$ íntegros.
   * En el **Ciclo 2**, todos vuelven a aportar $\text{Bs. } 500$ y el participante con el **Turno #2** recibe los $\text{Bs. } 2500$.
   * El ciclo continúa hasta que el participante del **Turno #$N$** cobra su pozo, momento en el que el Pasanaku concluye.

---

## 2. ¿Cómo se Integra el Pasanaku en Nuestra Aplicación Web?

Cada regla tradicional del Pasanaku tiene una correspondencia directa en la base de datos SQL Server, en la lógica C# y en la interfaz gráfica:

| Concepto del Pasanaku Tradicional | Integración en **Pasanaku Digital** |
| :--- | :--- |
| **Cuaderno de anotaciones del organizador** | Panel principal (**Dashboard**) con tarjetas financieras y estado de aportes en tiempo real. |
| **Sorteo o asignación de números de turno** | Modal **Crear Pasanaku** que asigna secuencialmente `Turno #1`, `Turno #2`... y aplica la restricción SQL `UQ_Grupo_Turno` para que dos personas jamás tengan el mismo turno. |
| **A quién le toca recibir el dinero hoy** | **Banner de Receptor del Turno Actual** (con borde cálido de aguayo) y estrella `★` en la tarjeta del participante cuyo `TurnoCobro == CicloActual`. |
| **Anotar quién ya pagó su cuota de la semana** | Botón **Registrar Pago** en cada tarjeta individual. Inserta el registro en la tabla `Pagos` y actualiza la barra de progreso y el monto recaudado. |
| **Evitar que alguien pague dos veces el mismo turno** | Restricción SQL `UQ_Pago_Por_Ciclo` e inhabilitación automática del botón a `✓ Aporte Registrado`. |
| **Entregar el pozo solo cuando todos cumplieron** | El botón **Entregar Pozo y Cerrar Turno** permanece bloqueado (`disabled`) hasta que el contador llegue al 100% (`N/N Pagados`). |
| **Pasar a la siguiente semana / turno** | Transacción SQL en C# que cambia el estado del receptor actual a `Cobrado` e incrementa `CicloActual + 1` (o cambia el grupo a `Finalizado` si era el último turno). |

---

## 3. Guía Paso a Paso de Uso de la Página

### Paso 1: Inicio de Sesión (Autenticación JWT)
1. Al abrir la aplicación, se muestra la tarjeta de **Inicio de Sesión**.
2. Ingresa tu correo electrónico y contraseña (para pruebas locales puedes usar `valeria@pasanaku.bo` y contraseña `123456`).
3. Al presionar **Ingresar al Pasanaku**, la aplicación valida tus credenciales en SQL Server, guarda el token JWT de sesión y despliega el Dashboard principal.

### Paso 2: Explorar el Dashboard del Pasanaku Activo
En la pantalla principal observarás tres zonas clave:
* **Barra Superior Oscura:** Muestra tu nombre de usuario, un selector desplegable para cambiar entre distintos grupos de Pasanaku y los botones de acción rápida.
* **Tarjetas de Indicadores Financieros:**
  * **Turno / Ciclo Actual:** Indica en qué ronda va el grupo (ej. `Turno #2 de 5`).
  * **Cuota por Participante:** El monto fijo que aporta cada miembro (ej. `Bs. 500.00`).
  * **Pozo Total a Entregar:** El monto total que recibirá el ganador del turno (`Bs. 2500.00`) y cuánto dinero ya se ha recaudado en el ciclo actual.
* **Banner del Receptor del Pozo:** Destaca con nombre y teléfono a la persona que debe cobrar el pozo en el turno vigente, junto con una barra de progreso visual.

### Paso 3: Registrar el Pago de un Participante
1. En la sección inferior **Estado de Aportes del Turno Actual**, ubica la tarjeta del participante que acaba de entregar su cuota.
2. Verifica que su insignia diga `Pago Pendiente` en color ámbar.
3. Haz clic en el botón verde **`Registrar Pago (Bs. 500)`**.
4. El sistema registrará la fecha y hora exacta en SQL Server, cambiará la insignia a `Cuota Pagada`, sumará el monto al total recaudado y avanzará la barra de progreso.

### Paso 4: Entregar el Pozo y Avanzar al Siguiente Turno
1. Una vez que **todos** los miembros del grupo tengan su estado en `Cuota Pagada` (por ejemplo, `5/5 Pagados`), el botón **`Entregar Pozo y Cerrar Turno`** se habilitará automáticamente.
2. Haz clic en **`Entregar Pozo y Cerrar Turno`**.
3. El sistema ejecutará el cierre del ciclo:
   * El receptor actual quedará marcado con `Estado de pozo: Cobrado`.
   * El grupo avanzará automáticamente al siguiente turno (ej. `Turno #3`) y reiniciará las tarjetas de aportes del nuevo ciclo a `Pago Pendiente`.
   * Si se trataba del último turno del grupo, el estado del Pasanaku cambiará a **`Finalizado`**.

### Paso 5: Registrar Nuevos Participantes y Crear un Nuevo Pasanaku
1. **Para agregar una persona nueva al sistema:** Haz clic en **`+ Nuevo Participante`** en la barra superior, completa su nombre, apellido, celular, correo y contraseña, y presiona **Guardar Participante**.
2. **Para abrir un nuevo juego de Pasanaku:**
   * Haz clic en **`+ Crear Pasanaku`**.
   * Escribe el nombre del grupo (ej. *Pasanaku Navideño*), el monto de la cuota en Bolivianos, la frecuencia y la fecha de inicio.
   * En el selector de participantes, elige a cada miembro y presiona **`+ Asignar Turno`** en el orden exacto en que cobrarán (`Turno #1`, `Turno #2`, `Turno #3`...).
   * Haz clic en **`Crear Pasanaku Oficial`**. El nuevo grupo se guardará en SQL Server y se cargará inmediatamente en pantalla.