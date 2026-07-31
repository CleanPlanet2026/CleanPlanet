# Git Workflow

## Branches

- main (protected)
- feature/<name>
- fix/<name>
- refactor/<name>
- docs/<name>

## Commit

Use Conventional Commits.

Examples

feat: add trash spawning
fix: resolve null reference
refactor: simplify game manager

## Pull Request

- One feature per PR
- Make sure the project builds
- Keep PRs small
- Write a short description
- When AI is used, add exactly one collaboration record under `docs/ai-prompts/`
- Follow the format in `docs/ai-prompts/README.md`

## Merge

- Squash Merge
- Delete branch after merging
