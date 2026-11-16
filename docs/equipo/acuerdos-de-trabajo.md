# Acuerdos de Trabajo del Equipo

1. **Gestión del Código en GitHub:**
   * La rama `main` contiene únicamente versiones estables que compilan sin errores en Visual Studio.
   * Ningún archivo con credenciales privadas, contraseñas de producción ni carpetas `bin/` o `obj/` debe subirse al repositorio.
2. **Convención de Commits:**
   * Se utilizan mensajes de commit descriptivos en español con prefijos semánticos:
     * `feat:` para nuevas funcionalidades (ej. `feat: agregar autenticación JWT y modal de creación de grupos`).
     * `fix:` para corrección de errores (ej. `fix: corregir puerto de conexión en app.js a 7056`).
     * `docs:` para cambios en la documentación y estandarización del repositorio.
3. **Revisión y Pruebas:**
   * Antes de cerrar una tarea, se verifica que el script SQL corra limpio en SSMS y que el frontend se conecte sin errores de CORS.