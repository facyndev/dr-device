---
name: smart-commit
description: >-
  Use this skill whenever the user wants to create, group, or prepare git commits,
  review git changes, or organize commits with Conventional Commits. It inspects
  git status, groups related files into atomic commits, writes commit messages in Spanish,
  and outputs a proposal table in the chat for user confirmation BEFORE executing any git commands.
---

# Smart Git Commit Skill

Esta skill guía el flujo para inspeccionar, agrupar y proponer commits de Git de forma atómica, siguiendo **Conventional Commits** en **español**, presentando una **tabla Markdown** y **esperando la confirmación explícita del usuario antes de ejecutar cualquier comando de escritura (`git add`, `git commit`, `git push`)**.

---

## Regla de Oro (Sin Ejecución Automática)
> [!IMPORTANT]
> **NO ejecutes `git add`, `git commit` ni `git push` automáticamente.**
> La primera interacción debe ser estrictamente de **análisis y propuesta**. El usuario debe revisar la tabla y dar su aprobación explícita antes de que se ejecute cualquier comando que modifique el estado de Git.

---

## Flujo de Trabajo en 2 Fases

### Fase 1: Análisis y Propuesta (Solo Lectura)

1. **Inspeccionar Cambios:**
   Ejecuta comandos de solo lectura para conocer el estado exacto:
   ```powershell
   git status -s
   git diff --stat
   ```

2. **Agrupar Archivos por Relación (Commits Atómicos):**
   Analiza las diferencias y define grupos lógicos:
   * **Por funcionalidad / Caso de uso:** Si modificaste una entidad, su DTO y su servicio, van juntos.
   * **Por arquitectura / Capa:** Middlewares, configuración en `Program.cs` o inyección de dependencias van por separado.
   * **Por tipo de cambio:** Separa refactorizaciones de nuevas funcionalidades o tareas de mantenimiento (`chore`).

3. **Redactar Mensajes con Conventional Commits en Español:**
   * Estructura: `<tipo>(<alcance opcional>): <descripción en español en imperativo>`
   * Tipos: `feat`, `fix`, `refactor`, `chore`, `docs`, `style`, `test`.
   * Verbo en presente o imperativo (*"agrega..."*, *"corrige..."*, *"configura..."*), sin punto final.

4. **Presentar la Tabla de Propuesta en el Chat:**
   Muestra la propuesta al usuario con este formato:

   | Commit / Mensaje Propuesto | Tipo | Archivos Incluidos | Resumen del Cambio |
   | :--- | :--- | :--- | :--- |
   | `feat(auth): permitir selección de rol al registrarse` | `feat` | `src/Domain/Entities/User.cs`<br>`src/Application/Models/Auth/Requests/RegisterRequest.cs`<br>`src/Application/Services/AuthService.cs` | Modifica constructor y DTO para admitir rol opcional y valida rol permitido. |
   | `feat(presentation): agregar middleware para manejo global de excepciones` | `feat` | `src/Presentation/Middlewares/ExceptionHandlingMiddleware.cs`<br>`src/Presentation/Program.cs` | Captura excepciones de dominio y devuelve respuestas JSON con códigos HTTP adecuados. |

5. **Solicitar Confirmación:**
   Pregunta al usuario al final del mensaje si está de acuerdo con los commits propuestos para proceder a crearlos (y si además desea que se suban con `git push`).

---

### Fase 2: Ejecución (Únicamente tras Aprobación del Usuario)

Una vez que el usuario responde confirmando la propuesta (ej. *"sí"*, *"procede"*, *"confirmado"*, *"dale"*):

1. **Ejecutar los Commits Grupo por Grupo:**
   ```powershell
   # Por cada grupo acordado:
   git add <ruta/al/archivo1> <ruta/al/archivo2>
   git commit -m "<tipo>(<alcance>): <mensaje en español>"
   ```

2. **Subir los Cambios (si fue solicitado o confirmado):**
   ```powershell
   git push
   ```

3. **Verificar y Notificar:**
   Ejecuta `git status` y `git log -n <cantidad> --oneline` para confirmar que los commits se aplicaron correctamente y notifica al usuario en el chat.
