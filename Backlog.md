# Backlog

Ideas that are out of scope for the current step. Add new ideas here instead of building them (see `docs/RULES.md`, rule 3).

- Native iOS build (needs macOS/Xcode or a macOS CI runner and Apple signing)
- Test that different `Cell` values usually get different hash codes (a constant `GetHashCode` is not caught today). Source: step 2 review.
- `BlastRules.Blast` returns the `List<CellPos>` from `GroupFinder` as `IReadOnlyList`; a caller could cast and mutate it. Wrap it read-only if results start being shared. Source: step 2 review.
- Tapping while tiles are still falling (common in the genre): needs gravity to advance step by step in the rules layer instead of one-shot `TurnRules.Tap`. Source: step 3 animation addendum.
- Small landing bounce for falling tiles. Source: step 3 animation addendum.
