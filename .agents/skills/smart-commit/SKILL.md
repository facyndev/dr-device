---
name: smart-commit
description: >-
  Use this skill whenever the user wants to create, group, or push git commits,
  review git changes, or prepare commits with Conventional Commits. It groups
  related files into atomic commits, writes commit messages in Spanish, and outputs
  a summary table in the chat indicating the commit message and affected files.
---

# Smart Git Commit Skill

Esta skill guía el flujo completo para preparar, agrupar y realizar commits de Git de forma atómica, siguiendo **Conventional Commits** en **español** y presentando un reporte final en formato de **tabla Markdown**.

---

## Procedimiento Paso a Paso

### 1. Inspeccionar Cambios Pendientes
Ejecuta los siguientes comandos para conocer el estado exacto del repositorio:
```powershell
git status -s
git diff --stat
```

---

### 2. Agrupar Archivos por Relación (Commits Atómicos)
**Nunca** hagas un `git add .` masivo si hay cambios de distinta naturaleza. Analiza las diferencias y agrupa los archivos según su objetivo y dependencia lógica:

* **Por funcionalidad / Caso de uso:** Si modificaste una entidad, su DTO y el servicio correspondiente, van juntos en un solo commit.
* **Por arquitectura / Capa:** Si creaste un middleware o configuraste `Program.cs`, agrúpalos como un cambio de infraestructura/presentación separado.
* **Por tipo de cambio:** Separa refactorizaciones de nuevas características o correcciones de bugs.

---

### 3. Redactar el Mensaje con Conventional Commits en Español
Cada commit debe seguir la estructura:
```text
<tipo>(<alcance opcional>): <descripción en español en imperativo>
```

#### Tipos permitidos:
* **`feat`**: Nueva funcionalidad (ej. nuevo endpoint, soporte para roles, etc.).
* **`fix`**: Corrección de un error o bug.
* **`refactor`**: Refactorización de código sin alterar su comportamiento externo.
* **`chore`**: Tareas de mantenimiento, dependencias, paquetes o configuración básica.
* **`docs`**: Documentación, README o comentarios.
* **`style`**: Ajustes de formato, indentación o limpieza visual.
* **`test`**: Creación o modificación de pruebas.

#### Reglas de redacción:
* Mensajes redactados en **español**.
* Usar verbo en tiempo presente o imperativo (ej. *"agrega middleware de excepciones"* en lugar de *"agregado"* o *"agregando"*).
* Breve, claro y sin punto final.

---

### 4. Ejecutar los Commits en Git
Para cada grupo de archivos:
```powershell
# 1. Agregar los archivos específicos del grupo
git add <ruta/al/archivo1> <ruta/al/archivo2>

# 2. Realizar el commit
git commit -m "<tipo>(<alcance>): <mensaje en español>"
```

*(Si el usuario solicitó explícitamente subir los cambios a un repositorio remoto, ejecuta `git push` tras completar todos los commits).*

---

### 5. Respuesta Final Obligatoria (Formato Tabla)
Siempre que finalices o presentes la propuesta de commits en el chat, **debes responder utilizando una tabla Markdown** con las siguientes columnas:

| Commit / Mensaje | Tipo | Archivos Incluidos | Resumen del Cambio |
| :--- | :--- | :--- | :--- |
| `feat(auth): permitir selección de rol al registrarse` | `feat` | `src/Domain/Entities/User.cs`<br>`src/Application/Models/Auth/Requests/RegisterRequest.cs`<br>`src/Application/Services/AuthService.cs` | Modifica constructor y DTO para admitir rol opcional y valida rol permitido. |
| `feat(presentation): agregar middleware para manejo global de excepciones` | `feat` | `src/Presentation/Middlewares/ExceptionHandlingMiddleware.cs`<br>`src/Presentation/Program.cs` | Captura excepciones de dominio y devuelve respuestas JSON con códigos HTTP adecuados. |

---

## Verificación
Tras realizar los commits, verifica que el working tree haya quedado en el estado deseado:
```powershell
git status
git log -n <cantidad_de_commits> --oneline
```
