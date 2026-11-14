# Backlog

Ideas that are out of scope for the current step. Add new ideas here instead of building them (see `docs/RULES.md`, rule 3).

- Native iOS build (needs macOS/Xcode or a macOS CI runner and Apple signing)
- Test that different `Cell` values usually get different hash codes (a constant `GetHashCode` is not caught today). Source: step 2 review.
- `BlastRules.Blast` returns the `List<CellPos>` from `GroupFinder` as `IReadOnlyList`; a caller could cast and mutate it. Wrap it read-only if results start being shared. Source: step 2 review.
- Tapping while tiles are still falling (common in the genre): needs gravity to advance step by step in the rules layer instead of one-shot `TurnRules.Tap`. Source: step 3 animation addendum.
- Small landing bounce for falling tiles. Source: step 3 animation addendum.
- New board layout on every launch: pick the seed at start (e.g. from the clock) instead of a fixed 42, and log or show it so a bug can be replayed with the same board. Rules layer already takes the seed from outside. Step 4 or 5 (may become a level setting). Source: product owner, step 3 play test.
