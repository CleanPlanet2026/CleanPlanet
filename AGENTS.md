# AGENTS.md

# CleanPlanet

Shared instructions for AI coding agents working on this project.

---

## Project

- Engine: Unity 6
- Render Pipeline: Universal Render Pipeline (URP)
- Genre: 2D Top-Down Idle / Incremental Game
- Theme: Collect trash, recycle it, and upgrade the world.

---

## Goals

This is a hackathon project.

Prioritize:

1. Working code
2. Readability
3. Maintainability
4. Simplicity

Avoid over-engineering.

---

## Folder Structure

Assets/
  Art/
    Sprites/
    Animations/
    Materials/

  Audio/
    BGM/
    SFX/

  Prefabs/
    Player/
    Trash/
    UI/
    Effects/

  Scenes/

  Scripts/
    Core/
    Player/
    Trash/
    Upgrade/
    UI/
    Utils/

  ScriptableObjects/
    Upgrades/
    GameBalance/

  Settings/

Place new files in the appropriate folder.

Examples:

- Player scripts -> Assets/Scripts/Player/
- Trash prefabs -> Assets/Prefabs/Trash/
- Upgrade data -> Assets/ScriptableObjects/Upgrades/
- Sound effects -> Assets/Audio/SFX/
- Shared materials -> Assets/Art/Materials/

---

## Coding Style

- Language: C#
- Use PascalCase for classes, methods, properties, and public members.
- Use _camelCase for private fields.
- Prefer [SerializeField] instead of public fields.
- Keep methods small and focused.
- One MonoBehaviour should have one responsibility.
- Follow the existing coding style.

---

## Unity Guidelines

Prefer:

- Serialized fields
- Prefabs
- ScriptableObjects when appropriate
- Events instead of polling
- Object Pooling for frequently spawned objects
- Inspector references instead of runtime lookups
- Cache component references

Avoid:

- FindObjectOfType
- GameObject.Find
- Resources.Load
- Large Update() methods
- God Objects
- Unnecessary allocations every frame

---

## Architecture

Keep systems loosely coupled.

Typical flow:

Player
-> Collect Trash
-> Inventory
-> Currency
-> Upgrade System

Do not introduce complex design patterns unless explicitly requested.

---

## Before Implementing

Before writing code:

1. Read all related scripts first.
2. Understand the current architecture.
3. Explain the implementation plan.
4. Reuse existing code whenever possible.
5. Ask for confirmation if major architectural changes are required.

---

## Code Changes

- Make the smallest change necessary.
- Do not rewrite unrelated code.
- Do not reformat unrelated files.
- Preserve naming conventions.
- Preserve the existing architecture.
- Only modify files related to the requested feature.

---

## Response Style

When implementing a feature:

1. Briefly explain the plan.
2. Implement the code.
3. Summarize what changed.
4. Mention any assumptions made.

---

## Git Workflow

Follow the repository rules described in GIT_WORKFLOW.md.

Do not commit directly to main.

Always:
- Create a new branch
- Commit using Conventional Commits
- Push the branch
- Open a Pull Request

---

## AI Collaboration Documentation

Every Pull Request created with AI assistance must include exactly one AI collaboration record.

- Store records in `docs/ai-prompts/`.
- Name records `pr-{number}-{feature-name}.md`.
- Record the main requests, discussion, decisions, AI contribution, and verification.
- Summarize relevant context instead of copying the entire conversation or tool log.
- Never include credentials, tokens, personal information, or other sensitive data.
- Follow the detailed rules and template in `docs/ai-prompts/README.md`.

---

## Comments

Write comments only when explaining **why** something is implemented a certain way or documenting non-obvious behavior.

Do not write comments that simply describe what the code already says.

---

## Performance

Optimize only when there is a demonstrated need.

Do not sacrifice readability for premature optimization.
