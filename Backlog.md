# Backlog

Ideas that are out of scope for the current step. Add new ideas here instead of building them (see `docs/RULES.md`, rule 3).

- Native iOS build (needs macOS/Xcode or a macOS CI runner and Apple signing)
- Sprite cache in the view layer: `GridView.Build` creates a new texture on every call; cache it once redraws start (step 3). Source: step 1 review.
- Test that different `Cell` values usually get different hash codes (a constant `GetHashCode` is not caught today). Source: step 2 review.
- `BlastRules.Blast` returns the `List<CellPos>` from `GroupFinder` as `IReadOnlyList`; a caller could cast and mutate it. Wrap it read-only if results start being shared. Source: step 2 review.
