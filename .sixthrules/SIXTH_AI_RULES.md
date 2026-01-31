# Sixth AI Project Rules for Mask Effect

## Task Context Loading

When performing a new task within the 'Mask Effect' project, Sixth AI should automatically load and consider the contents of `CLAUDE.md` files located in the relevant `Assets/` subdirectories. These directory-level `CLAUDE.md` files contain specific sub-tasks and their current status for each game component, providing essential context for task execution.

**Relevant Directories to Check for CLAUDE.md:**

- `Assets/Audio/`
- `Assets/Data/`
- `Assets/Gameplay/`
- `Assets/Input/`
- `Assets/Mechs/`
- `Assets/Masks/`
- `Assets/Networking/`
- `Assets/Scenes/`
- `Assets/Scripts/Core/`
- `Assets/Scripts/UI/`
- `Assets/UI/`
- `Assets/VFX/`

Sixth AI should use the information in these `CLAUDE.md` files to understand the current state of each component's development, prioritize sub-tasks, and ensure consistency with the overall project plan outlined in the root `CLAUDE.md`.
