using System;
using System.Collections;
using System.Collections.Generic;
using BlastGame.Core;
using UnityEngine;
using Grid = BlastGame.Core.Grid; // UnityEngine.Grid also exists

namespace BlastGame.View
{
    /// <summary>
    /// Draws a grid: colored squares, special items made of primitive shapes and obstacles (ObstacleSprites).
    /// Cell (0, 0) is at the world origin; spacing is 1 unit. Animates a turn from its TurnResult: every
    /// effect plays on one timeline (a chained item starts when the effect that triggers it reaches it),
    /// with obstacle hits and balloon pops on the same timeline; then tiles fall and collected Anchors
    /// slide down and fly to their goal.
    /// </summary>
    public sealed class GridView : MonoBehaviour
    {
        private const float TileSize = 0.9f;

        // Seconds. Targets from the spec; tuned by eye.
        private const float BlastDuration = 0.12f;     // a normal group shrinks away
        private const float CollectDuration = 0.2f;    // a merging group slides into the tapped cell
        private const float PopDuration = 0.15f;       // the new item pops in
        private const float RocketCellTime = 0.025f;   // a rocket half crosses one cell
        private const float RocketTileVanish = 0.06f;  // a tile under a rocket half disappears
        private const float TrailLength = 1.6f;        // cells
        private const float TrailFade = 0.1f;
        private const float BombWave = 0.15f;
        private const float BombFlash = 0.1f;
        private const float BombTileVanish = 0.1f;
        private const float BombShake = 0.12f;
        private const float BombShakeAmount = 0.07f;   // world units
        private const float OrbPulse = 0.1f;
        private const float OrbBeam = 0.12f;
        private const float OrbMaxStep = 0.035f;       // gap between beams; shorter when there are many
        private const float OrbTotal = 0.5f;
        private const float FallDuration = 0.35f;      // every falling tile, whatever the distance
        private const float MergeDuration = 0.15f;     // combo participants slide onto the tapped cell
        private const float MergeFlash = 0.12f;
        private const float BigBombCellTime = 0.03f;   // the big wave crosses one ring of cells
        private const float BigBombWave = 0.3f;
        private const float BigBombShake = 0.25f;
        private const float BigBombShakeAmount = 0.16f;
        private const float ClearAllSweep = 0.28f;     // the whole-board wave, center to corner
        private const float ClearAllWave = 0.4f;
        private const float ConvertPop = 0.1f;         // a converted tile pops into its new item
        private const float ConvertStagger = 0.02f;    // converted items go off almost together
        private const float HitShake = 0.14f;          // a Crate shakes when it loses a layer
        private const float HitShakeAmount = 0.08f;
        private const float BreakVanish = 0.1f;        // a broken Pinata or Crate shrinks away
        private const float ParticleLife = 0.45f;      // confetti, planks, drops
        private const float ParticleGravity = -14f;    // world units / s^2
        private const float SplashWave = 0.2f;
        private const float AnchorFly = 0.35f;         // a collected Anchor flies to its goal icon
        private const int ConfettiCount = 10;
        private const int DropCount = 7;

        // Sorting order: tiles under their decoration, effects above both.
        private const int TileOrder = 0;
        private const int DecoOrder = 1;
        private const int TrailOrder = 2;
        private const int BeamOrder = 3;
        private const int HalfOrder = 4;
        private const int WaveOrder = 5;
        private const int FlashOrder = 6;

        private enum TileAnim { None, Shrink, Collect, FlashOut, Hide, OrbSelf, MergeCenter, Shake }

        private enum FxKind { RocketHalf, RocketTrail, BombFlash, BombWave, Beam, Shake, Particle }

        // Bright confetti and candy colors for a broken Pinata.
        private static readonly Color[] Confetti =
        {
            new Color(1f, 0.25f, 0.75f, 1f), new Color(1f, 0.85f, 0.1f, 1f), new Color(0.1f, 0.85f, 0.85f, 1f),
            new Color(1f, 0.55f, 0.1f, 1f), new Color(0.6f, 0.4f, 1f, 1f)
        };

        private struct TileStep
        {
            public TileAnim Anim;
            public float Start;
            public float Duration;
            public Vector3 Target;  // Collect: where the tile slides to
            public Color BaseColor; // FlashOut: the color it flashes from
        }

        private struct Fx
        {
            public FxKind Kind;
            public float Start;
            public float Duration;
            public Vector3 From;
            public Vector3 To;
            public Color Color;
            public SpriteRenderer Renderer; // null for Shake
            public float Size;              // flash/wave: end scale; shake: amount; particle: scale
            public float Spin;              // particle: degrees per second
            public float Stretch;           // particle: width / height
        }

        // A tile that takes a new look mid-timeline (a colored cell turned into an item, the Orb of a combo).
        private struct Look
        {
            public CellPos Pos;
            public Cell Cell;
            public float Start;
        }

        private Grid _grid;
        private SpriteRenderer[,] _tiles;
        private SpriteRenderer[,] _decos;   // child of each tile: rocket arrows, bomb ring, orb ring
        private Cell[,] _shown;             // what each tile currently draws
        private Vector3[,] _fallStart;      // reused by every turn
        private TileStep[,] _steps;         // reused by every turn
        private int[] _newInColumn;         // reused by every turn
        private float[] _effectStart = new float[8]; // grows when a chain is longer
        private readonly List<Fx> _fx = new List<Fx>();
        private readonly List<Look> _looks = new List<Look>();
        private readonly List<SpriteRenderer> _pool = new List<SpriteRenderer>();
        private int _poolUsed;
        private bool _hasPop;
        private CellPos _popPos;
        private Cell _popCell;
        private float _popStart;
        private bool _isAnimating;
        private Func<ObstacleKind, Vector3?> _goalTarget;
        private readonly List<SpriteRenderer> _flyers = new List<SpriteRenderer>(); // collected Anchors, reused

        /// <summary>True while a turn is being animated; taps should be ignored meanwhile.</summary>
        public bool IsAnimating
        {
            get { return _isAnimating; }
        }

        /// <summary>Where the goal icon of an obstacle kind is, in board space (null: no icon, the item just fades).</summary>
        public void SetGoalTarget(Func<ObstacleKind, Vector3?> goalTarget)
        {
            _goalTarget = goalTarget;
        }

        public void Build(Grid grid)
        {
            _grid = grid;
            _tiles = new SpriteRenderer[grid.Width, grid.Height];
            _decos = new SpriteRenderer[grid.Width, grid.Height];
            _shown = new Cell[grid.Width, grid.Height];
            _fallStart = new Vector3[grid.Width, grid.Height];
            _steps = new TileStep[grid.Width, grid.Height];
            _newInColumn = new int[grid.Width];
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    var tile = new GameObject("Tile " + x + "," + y);
                    tile.transform.SetParent(transform, false);
                    tile.transform.localPosition = CellPosition(x, y);
                    tile.transform.localScale = new Vector3(TileSize, TileSize, 1f);
                    _tiles[x, y] = NewRenderer(tile, TileOrder);

                    var deco = new GameObject("Deco");
                    deco.transform.SetParent(tile.transform, false);
                    _decos[x, y] = NewRenderer(deco, DecoOrder);
                }
            }
            BuildMask(grid.Width, grid.Height);
            Refresh();
        }

        /// <summary>Syncs every tile with the grid.</summary>
        public void Refresh()
        {
            for (int y = 0; y < _grid.Height; y++)
                for (int x = 0; x < _grid.Width; x++)
                    ShowCell(x, y, _grid.Get(x, y));
        }

        /// <summary>
        /// Shows how the grid reached its current state. The grid is already final; the tiles still show
        /// the state before the tap, so the effects play first, then every tile takes its final look and
        /// falls into its cell.
        /// </summary>
        public void AnimateTurn(TurnResult result)
        {
            if (_isAnimating || !result.HasBlasted) return;
            StartCoroutine(AnimateTurnRoutine(result));
        }

        // Light "alive" motion of special items; only decorations move, so turn animations are untouched.
        private void Update()
        {
            if (_grid == null) return;
            float time = Time.time;
            float breathe = Mathf.Sin(time * 3.2f);
            for (int y = 0; y < _grid.Height; y++)
            {
                for (int x = 0; x < _grid.Width; x++)
                {
                    Cell cell = _shown[x, y];
                    if (!cell.IsSpecial) continue;
                    Transform deco = _decos[x, y].transform;
                    switch (cell.Special)
                    {
                        case SpecialKind.RocketHorizontal:
                            deco.localScale = new Vector3(1f + 0.05f * breathe, 1f, 1f);
                            break;
                        case SpecialKind.RocketVertical:
                            deco.localScale = new Vector3(1f, 1f + 0.05f * breathe, 1f);
                            break;
                        case SpecialKind.Bomb:
                            float s = 1f + 0.035f * breathe;
                            deco.localScale = new Vector3(s, s, 1f);
                            break;
                        default:
                            deco.localRotation = Quaternion.Euler(0f, 0f, -time * 45f);
                            break;
                    }
                }
            }
        }

        private IEnumerator AnimateTurnRoutine(TurnResult result)
        {
            _isAnimating = true;

            // 1. Every effect, on one timeline.
            float total = PlanEffects(result);
            for (float t = 0f; t < total; t += Time.deltaTime)
            {
                ApplyTimeline(t);
                yield return null;
            }
            ApplyTimeline(total);
            ReleaseFx();
            transform.localPosition = Vector3.zero;

            // 2. Every tile shows its final cell; moved and new tiles start where they come from.
            for (int y = 0; y < _grid.Height; y++)
                for (int x = 0; x < _grid.Width; x++)
                {
                    _fallStart[x, y] = CellPosition(x, y);
                    _tiles[x, y].transform.localScale = new Vector3(TileSize, TileSize, 1f);
                }
            foreach (CellMove m in result.Moves)
                _fallStart[m.To.X, m.To.Y] = CellPosition(m.From.X, m.From.Y);
            // New tiles in a column fill its top cells; they start just above the grid, in the same order,
            // so each one is lifted by the number of new tiles in its column.
            for (int x = 0; x < _grid.Width; x++) _newInColumn[x] = 0;
            foreach (CellPos p in result.Filled) _newInColumn[p.X]++;
            foreach (CellPos p in result.Filled)
                _fallStart[p.X, p.Y] = CellPosition(p.X, p.Y + _newInColumn[p.X]);
            Refresh();
            ApplyFall(0f);
            StartCollected(result.Collected);

            // 3. Everything falls together, slow start and fast end (cubic ease-in); collected Anchors slide to y = 0.
            for (float t = 0f; t < FallDuration; t += Time.deltaTime)
            {
                float k = t / FallDuration;
                ApplyFall(k * k * k);
                ApplyCollectedFall(result.Collected, k * k * k);
                yield return null;
            }
            ApplyCollectedFall(result.Collected, 1f);
            ApplyFall(1f);

            // 4. Collected Anchors: they slid down with the fall (see AnimateCollected); now they fly to the goal.
            if (result.Collected.Count > 0)
            {
                Vector3? target = _goalTarget != null ? _goalTarget(ObstacleKind.Anchor) : null;
                for (float t = 0f; t < AnchorFly; t += Time.deltaTime)
                {
                    ApplyFly(result.Collected, target, t / AnchorFly);
                    yield return null;
                }
                for (int i = 0; i < _flyers.Count; i++) _flyers[i].enabled = false;
            }

            _isAnimating = false;
        }

        // ---------- Planning: when each tile and effect plays. Returns the total time. ----------

        private float PlanEffects(TurnResult result)
        {
            for (int y = 0; y < _grid.Height; y++)
                for (int x = 0; x < _grid.Width; x++)
                    _steps[x, y].Anim = TileAnim.None;
            _fx.Clear();
            _looks.Clear();
            _hasPop = false;

            IReadOnlyList<TurnEffect> effects = result.Effects;
            if (effects.Count == 0)
            {
                // A result without effects: shrink what blasted.
                foreach (CellPos p in result.Blasted)
                    SetStep(p, TileAnim.Shrink, 0f, BlastDuration);
                return BlastDuration;
            }

            if (_effectStart.Length < effects.Count)
                _effectStart = new float[Mathf.Max(effects.Count, _effectStart.Length * 2)];

            float end = 0f;
            for (int i = 0; i < effects.Count; i++)
            {
                TurnEffect e = effects[i];
                float start = 0f;
                if (e.TriggeredBy >= 0)
                    start = _effectStart[e.TriggeredBy] + ReachTime(effects[e.TriggeredBy], e.Origin);
                _effectStart[i] = start;

                float done;
                switch (e.Kind)
                {
                    case EffectKind.Rocket: done = PlanRocket(e, start); break;
                    case EffectKind.Bomb: done = PlanBomb(e, start); break;
                    case EffectKind.Orb: done = PlanOrb(e, start); break;
                    case EffectKind.RocketCross:
                    case EffectKind.RocketBomb: done = PlanRocketCombo(e, start); break;
                    case EffectKind.BigBomb: done = PlanBigBomb(e, start); break;
                    case EffectKind.OrbRocket:
                    case EffectKind.OrbBomb: done = PlanConvert(e, start, effects); break;
                    case EffectKind.OrbOrb: done = PlanClearAll(e, start); break;
                    default: done = PlanGroup(e, start); break;
                }
                end = Mathf.Max(end, done);
            }

            // Obstacle hits and balloon pops, on the same timeline as the effect that made them.
            for (int i = 0; i < effects.Count; i++)
            {
                end = Mathf.Max(end, PlanHits(effects[i], _effectStart[i]));
                end = Mathf.Max(end, PlanBalloons(effects[i], _effectStart[i]));
            }
            return end;
        }

        // ---------- Obstacles ----------

        /// <summary>When an effect reaches a cell it hits (rockets travel; a group, Bomb or Orb hits at once).</summary>
        private static float HitTime(TurnEffect e, CellPos p)
        {
            switch (e.Kind)
            {
                case EffectKind.Group:
                    return e.Created.IsEmpty ? BlastDuration * 0.5f : CollectDuration;
                case EffectKind.Orb:
                    return OrbPulse + OrbBeam;
                case EffectKind.Bomb:
                    return 0.02f;
                case EffectKind.OrbOrb:
                    return MergeDuration + 0.1f;
                default:
                    return ReachTime(e, p);
            }
        }

        private float PlanHits(TurnEffect e, float start)
        {
            float end = start;
            foreach (ObstacleHit hit in e.Hits)
            {
                CellPos p = hit.Pos;
                float t = start + HitTime(e, p);
                Vector3 at = CellPosition(p.X, p.Y);
                bool crate = hit.Before.Obstacle == ObstacleKind.Crate;
                if (hit.After.IsEmpty)
                {
                    SetStep(p, TileAnim.Shrink, t, BreakVanish);
                    if (crate)
                        AddPlanks(at, t, 3);
                    else
                        AddBurst(at, t, ConfettiCount, Confetti, ShapeSprites.Square, 0.13f, 4.5f, p.X * 7 + p.Y * 13);
                }
                else
                {
                    // The Crate loses a layer: it shows the next look, shakes, and one plank falls off.
                    _looks.Add(new Look { Pos = p, Cell = hit.After, Start = t });
                    SetStep(p, TileAnim.Shake, t, HitShake);
                    AddPlanks(at, t, 1);
                }
                end = Mathf.Max(end, t + ParticleLife);
            }
            return end;
        }

        /// <summary>Every balloon on a cleared or converted cell pops with a splash when its cell goes.</summary>
        private float PlanBalloons(TurnEffect e, float start)
        {
            float end = start;
            foreach (ClearedCell c in e.Cleared)
            {
                if (!c.Cell.HasBalloon) continue;
                TileStep step = _steps[c.Pos.X, c.Pos.Y];
                float t = step.Anim != TileAnim.None ? step.Start : start;
                end = Mathf.Max(end, AddSplash(c.Pos, t));
            }
            foreach (ClearedCell c in e.Converted)
            {
                if (!c.Cell.HasBalloon) continue;
                float t = start;
                for (int i = 0; i < _looks.Count; i++)
                    if (_looks[i].Pos == c.Pos) t = _looks[i].Start;
                end = Mathf.Max(end, AddSplash(c.Pos, t));
            }
            return end;
        }

        private float AddSplash(CellPos p, float t)
        {
            Vector3 at = CellPosition(p.X, p.Y);
            Color water = ObstacleSprites.Water;
            AddBurst(at, t, DropCount, new[] { water }, ShapeSprites.Disc, 0.16f, 3.5f, p.X * 5 + p.Y * 11);
            AddFx(FxKind.BombWave, t, SplashWave, at, at, water, WaveOrder, ShapeSprites.Ring, 1.4f);
            return t + ParticleLife;
        }

        /// <summary>Pieces flying out of a cell and falling; directions are spread evenly, turned by a seed.</summary>
        private void AddBurst(Vector3 at, float t, int count, Color[] colors, Sprite sprite, float size, float speed, int seed)
        {
            float turn = (seed * 37) % 360;
            for (int i = 0; i < count; i++)
            {
                float angle = (turn + i * 360f / count) * Mathf.Deg2Rad;
                float v = speed * (0.7f + 0.3f * ((i * 7 + seed) % 5) / 4f);
                var velocity = new Vector3(Mathf.Cos(angle) * v, Mathf.Sin(angle) * v + 2f, 0f);
                AddParticle(at, t, velocity, colors[i % colors.Length], sprite, size, 1f, (i % 2 == 0 ? 1 : -1) * 540f);
            }
        }

        /// <summary>Broken wooden planks dropping off a Crate.</summary>
        private void AddPlanks(Vector3 at, float t, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float side = count == 1 ? 0.6f : (i - (count - 1) * 0.5f) * 1.2f;
                var velocity = new Vector3(side, 2.2f + 0.4f * i, 0f);
                AddParticle(at + new Vector3(0f, 0.1f * i, 0f), t, velocity, ObstacleSprites.Wood, ShapeSprites.Square,
                            0.12f, 5f, (i % 2 == 0 ? 1 : -1) * 260f);
            }
        }

        private void AddParticle(Vector3 at, float t, Vector3 velocity, Color color, Sprite sprite, float size,
                                 float stretch, float spin)
        {
            SpriteRenderer sr = TakeFromPool();
            sr.sprite = sprite;
            sr.sortingOrder = FlashOrder;
            sr.enabled = false;
            _fx.Add(new Fx
            {
                Kind = FxKind.Particle, Start = t, Duration = ParticleLife, From = at, To = velocity, Color = color,
                Renderer = sr, Size = size, Spin = spin, Stretch = stretch
            });
        }

        // ---------- Collected Anchors ----------

        private void StartCollected(IReadOnlyList<CellMove> collected)
        {
            Color tint;
            Sprite sprite = ObstacleSprites.For(ObstacleKind.Anchor, 1, out tint);
            while (_flyers.Count < collected.Count)
            {
                var go = new GameObject("Anchor");
                go.transform.SetParent(transform, false);
                SpriteRenderer sr = NewRenderer(go, HalfOrder);
                sr.maskInteraction = SpriteMaskInteraction.None; // it flies over the HUD bar
                _flyers.Add(sr);
            }
            for (int i = 0; i < _flyers.Count; i++)
            {
                SpriteRenderer sr = _flyers[i];
                sr.enabled = i < collected.Count;
                if (!sr.enabled) continue;
                sr.sprite = sprite;
                sr.color = tint;
                SetScale(sr.transform, TileSize);
            }
        }

        private void ApplyCollectedFall(IReadOnlyList<CellMove> collected, float amount)
        {
            for (int i = 0; i < collected.Count; i++)
            {
                CellMove m = collected[i];
                // Slides to the bottom row, then a little further down, as if it sinks out of the board.
                Vector3 from = CellPosition(m.From.X, m.From.Y);
                Vector3 to = CellPosition(m.To.X, m.To.Y) + new Vector3(0f, -0.35f, 0f);
                _flyers[i].transform.localPosition = Vector3.LerpUnclamped(from, to, amount);
            }
        }

        private void ApplyFly(IReadOnlyList<CellMove> collected, Vector3? target, float k)
        {
            float e = EaseInOut(Mathf.Clamp01(k));
            for (int i = 0; i < collected.Count; i++)
            {
                CellMove m = collected[i];
                Vector3 from = CellPosition(m.To.X, m.To.Y) + new Vector3(0f, -0.35f, 0f);
                SpriteRenderer sr = _flyers[i];
                Color c = sr.color;
                if (target.HasValue)
                {
                    // An arc up to the icon, shrinking to icon size.
                    Vector3 pos = Vector3.Lerp(from, target.Value, e);
                    pos.x += Mathf.Sin(Mathf.PI * e) * 0.6f;
                    sr.transform.localPosition = pos;
                    SetScale(sr.transform, TileSize * Mathf.Lerp(1f, 0.55f, e));
                    c.a = k < 0.85f ? 1f : 1f - (k - 0.85f) / 0.15f;
                }
                else
                {
                    sr.transform.localPosition = from + new Vector3(0f, -0.5f * e, 0f);
                    c.a = 1f - e;
                }
                sr.color = c;
            }
        }

        /// <summary>Time from the start of an effect until it reaches a cell (when a chained item there fires).</summary>
        private static float ReachTime(TurnEffect effect, CellPos cell)
        {
            CellPos o = effect.Origin;
            switch (effect.Kind)
            {
                case EffectKind.Rocket:
                    return (Mathf.Abs(cell.X - o.X) + Mathf.Abs(cell.Y - o.Y)) * RocketCellTime;
                case EffectKind.RocketCross:
                    return MergeDuration + CrossDistance(o, cell, 0) * RocketCellTime;
                case EffectKind.RocketBomb:
                    return MergeDuration + CrossDistance(o, cell, 1) * RocketCellTime;
                case EffectKind.BigBomb:
                    return MergeDuration + Ring(o, cell) * BigBombCellTime;
                case EffectKind.OrbRocket:
                case EffectKind.OrbBomb:
                {
                    // Converted items go off, in queue order, once every conversion is done.
                    IReadOnlyList<ClearedCell> conv = effect.Converted;
                    int index = 0;
                    for (int i = 0; i < conv.Count; i++)
                        if (conv[i].Pos == cell) { index = i; break; }
                    return MergeDuration + ConvertTime(conv.Count) + index * ConvertStagger;
                }
                default:
                    return 0f; // a Bomb sets things off the moment it explodes; Orbs and groups never chain
            }
        }

        private float PlanGroup(TurnEffect e, float start)
        {
            if (e.Created.IsEmpty)
            {
                foreach (ClearedCell c in e.Cleared)
                    SetStep(c.Pos, TileAnim.Shrink, start, BlastDuration);
                return start + BlastDuration;
            }

            // Merge: the group gathers in the tapped cell, then the new item pops in there.
            Vector3 target = CellPosition(e.Origin.X, e.Origin.Y);
            foreach (ClearedCell c in e.Cleared)
            {
                SetStep(c.Pos, TileAnim.Collect, start, CollectDuration);
                _steps[c.Pos.X, c.Pos.Y].Target = target;
            }
            _hasPop = true;
            _popPos = e.Origin;
            _popCell = e.Created;
            _popStart = start + CollectDuration;
            return _popStart + PopDuration;
        }

        private float PlanRocket(TurnEffect e, float start)
        {
            CellPos o = e.Origin;
            bool vertical = e.Cleared[0].Cell.Special == SpecialKind.RocketVertical;
            SetStep(o, TileAnim.Hide, start, 0f);

            foreach (ClearedCell c in e.Cleared)
            {
                if (c.Pos == o) continue;
                int d = Mathf.Abs(c.Pos.X - o.X) + Mathf.Abs(c.Pos.Y - o.Y);
                SetStep(c.Pos, TileAnim.Shrink, start + d * RocketCellTime, RocketTileVanish);
            }

            return AddRocketPair(o.X, o.Y, vertical, start);
        }

        /// <summary>Two rocket halves fly apart from (x, y) until they leave the board (the mask hides them there).</summary>
        private float AddRocketPair(int x, int y, bool vertical, float start)
        {
            int span = vertical ? _grid.Height : _grid.Width;
            int pos = vertical ? y : x;
            float end = start;
            for (int side = -1; side <= 1; side += 2)
            {
                int toEdge = side < 0 ? pos + 1 : span - pos; // cells until just past the edge
                float flight = toEdge * RocketCellTime;
                Vector3 from = CellPosition(x, y);
                Vector3 dir = vertical ? new Vector3(0f, side, 0f) : new Vector3(side, 0f, 0f);
                Vector3 to = from + dir * (toEdge + TrailLength);
                float flightWithTrail = flight + TrailLength * RocketCellTime; // the trail also leaves
                AddFx(FxKind.RocketHalf, start, flightWithTrail, from, to, TileColors.RocketBody, HalfOrder,
                      ShapeSprites.ArrowHead);
                AddFx(FxKind.RocketTrail, start, flightWithTrail + TrailFade, from, to, TileColors.RocketBody,
                      TrailOrder, ShapeSprites.Trail);
                end = Mathf.Max(end, start + flightWithTrail + TrailFade);
            }
            return end;
        }

        private float PlanBomb(TurnEffect e, float start)
        {
            CellPos o = e.Origin;
            Vector3 at = CellPosition(o.X, o.Y);
            foreach (ClearedCell c in e.Cleared)
            {
                bool self = c.Pos == o;
                SetStep(c.Pos, TileAnim.Shrink, start + (self ? 0f : 0.02f), self ? 0.05f : BombTileVanish);
            }
            AddFx(FxKind.BombFlash, start, BombFlash, at, at, TileColors.Flash, FlashOrder, ShapeSprites.Disc, 1.7f);
            AddFx(FxKind.BombWave, start, BombWave, at, at, TileColors.Wave, WaveOrder, ShapeSprites.Ring, 3.4f);
            _fx.Add(new Fx { Kind = FxKind.Shake, Start = start, Duration = BombShake, Size = BombShakeAmount });
            return start + Mathf.Max(BombWave, BombShake);
        }

        private float PlanOrb(TurnEffect e, float start)
        {
            CellPos o = e.Origin;
            Vector3 from = CellPosition(o.X, o.Y);
            int targets = e.Cleared.Count - 1;
            float step = targets > 1
                ? Mathf.Min(OrbMaxStep, (OrbTotal - OrbPulse - OrbBeam) / (targets - 1))
                : 0f;
            Color beam = Color.Lerp(TileColors.ToColor(e.Cleared[0].Cell.OrbColor), Color.white, 0.55f);

            for (int i = 1; i < e.Cleared.Count; i++)
            {
                CellPos p = e.Cleared[i].Pos;
                float t = start + OrbPulse + (i - 1) * step;
                SetStep(p, TileAnim.FlashOut, t, OrbBeam);
                AddFx(FxKind.Beam, t, OrbBeam, from, CellPosition(p.X, p.Y), beam, BeamOrder, ShapeSprites.Square);
            }
            float end = start + OrbPulse + Mathf.Max(0, targets - 1) * step + OrbBeam;
            SetStep(o, TileAnim.OrbSelf, start, end - start);
            return end;
        }

        // ---------- Combos ----------

        /// <summary>
        /// The participants (the first MergedCount cleared cells) slide onto the tapped cell, then a short flash.
        /// Returns when the combo itself fires.
        /// </summary>
        private float PlanMerge(TurnEffect e, float start, bool keepOrigin)
        {
            CellPos o = e.Origin;
            Vector3 at = CellPosition(o.X, o.Y);
            for (int i = 1; i < e.MergedCount; i++)
            {
                CellPos p = e.Cleared[i].Pos;
                SetStep(p, TileAnim.Collect, start, MergeDuration);
                _steps[p.X, p.Y].Target = at;
            }
            if (!keepOrigin)
                SetStep(o, TileAnim.MergeCenter, start, MergeDuration);
            float fire = start + MergeDuration;
            AddFx(FxKind.BombFlash, fire, MergeFlash, at, at, TileColors.Flash, FlashOrder, ShapeSprites.Disc, 1.4f);
            return fire;
        }

        private float PlanRocketCombo(TurnEffect e, float start)
        {
            float fire = PlanMerge(e, start, false);
            CellPos o = e.Origin;
            int half = e.Kind == EffectKind.RocketBomb ? 1 : 0;
            for (int i = e.MergedCount; i < e.Cleared.Count; i++)
            {
                CellPos p = e.Cleared[i].Pos;
                SetStep(p, TileAnim.Shrink, fire + CrossDistance(o, p, half) * RocketCellTime, RocketTileVanish);
            }

            // One horizontal pair per row of the band and one vertical pair per column, all at once.
            float end = fire;
            for (int d = -half; d <= half; d++)
            {
                if (o.Y + d >= 0 && o.Y + d < _grid.Height)
                    end = Mathf.Max(end, AddRocketPair(o.X, o.Y + d, false, fire));
                if (o.X + d >= 0 && o.X + d < _grid.Width)
                    end = Mathf.Max(end, AddRocketPair(o.X + d, o.Y, true, fire));
            }
            return end;
        }

        private float PlanBigBomb(TurnEffect e, float start)
        {
            float fire = PlanMerge(e, start, false);
            CellPos o = e.Origin;
            Vector3 at = CellPosition(o.X, o.Y);
            float end = fire;
            for (int i = e.MergedCount; i < e.Cleared.Count; i++)
            {
                CellPos p = e.Cleared[i].Pos;
                float t = fire + 0.02f + Ring(o, p) * BigBombCellTime;
                SetStep(p, TileAnim.Shrink, t, BombTileVanish);
                end = Mathf.Max(end, t + BombTileVanish);
            }
            AddFx(FxKind.BombFlash, fire, BombFlash * 1.5f, at, at, TileColors.Flash, FlashOrder, ShapeSprites.Disc, 4f);
            AddFx(FxKind.BombWave, fire, BigBombWave, at, at, TileColors.Wave, WaveOrder, ShapeSprites.Ring, 8.5f);
            _fx.Add(new Fx { Kind = FxKind.Shake, Start = fire, Duration = BigBombShake, Size = BigBombShakeAmount });
            return Mathf.Max(end, fire + Mathf.Max(BigBombWave, BigBombShake));
        }

        private float PlanClearAll(TurnEffect e, float start)
        {
            float fire = PlanMerge(e, start, false);
            CellPos o = e.Origin;
            Vector3 at = CellPosition(o.X, o.Y);
            Vector3 center = new Vector3((_grid.Width - 1) * 0.5f, (_grid.Height - 1) * 0.5f, 0f);
            float far = Mathf.Max(0.001f, center.magnitude); // center to a corner
            float end = fire;
            for (int i = e.MergedCount; i < e.Cleared.Count; i++)
            {
                CellPos p = e.Cleared[i].Pos;
                float d = Vector3.Distance(CellPosition(p.X, p.Y), center) / far;
                float t = fire + d * ClearAllSweep;
                SetStep(p, TileAnim.Shrink, t, BombTileVanish);
                end = Mathf.Max(end, t + BombTileVanish);
            }
            float size = Mathf.Max(_grid.Width, _grid.Height) * 2.2f;
            AddFx(FxKind.BombFlash, fire, BombFlash * 2f, at, at, TileColors.Flash, FlashOrder, ShapeSprites.Disc, 5f);
            AddFx(FxKind.BombWave, fire, ClearAllWave, center, center, TileColors.Wave, WaveOrder, ShapeSprites.Ring, size);
            _fx.Add(new Fx { Kind = FxKind.Shake, Start = fire, Duration = BigBombShake, Size = BombShakeAmount });
            return Mathf.Max(end, fire + ClearAllWave);
        }

        /// <summary>OrbRocket / OrbBomb: beams from the Orb turn each target into its new item; the items' own effects follow.</summary>
        private float PlanConvert(TurnEffect e, float start, IReadOnlyList<TurnEffect> effects)
        {
            float fire = PlanMerge(e, start, true);
            CellPos o = e.Origin;
            Vector3 from = CellPosition(o.X, o.Y);

            // The tapped cell shows the Orb (it may have been the Rocket or Bomb) and pulses while the beams go out.
            Cell orb = e.Cleared[0].Cell;
            for (int i = 0; i < e.MergedCount; i++)
                if (e.Cleared[i].Cell.Special == SpecialKind.Orb) orb = e.Cleared[i].Cell;
            _looks.Add(new Look { Pos = o, Cell = orb, Start = fire }); // after the merge: the tapped item keeps its look until then

            IReadOnlyList<ClearedCell> conv = e.Converted;
            float step = BeamStep(conv.Count);
            Color beam = Color.Lerp(TileColors.ToColor(orb.OrbColor), Color.white, 0.55f);
            for (int i = 0; i < conv.Count; i++)
            {
                CellPos p = conv[i].Pos;
                float t = fire + OrbPulse + i * step;
                AddFx(FxKind.Beam, t, OrbBeam, from, CellPosition(p.X, p.Y), beam, BeamOrder, ShapeSprites.Square);
                _looks.Add(new Look { Pos = p, Cell = ConvertedItem(e, p, effects), Start = t + OrbBeam * 0.6f });
            }

            float end = fire + ConvertTime(conv.Count);
            SetStep(o, TileAnim.OrbSelf, fire, end - fire);
            return end;
        }

        /// <summary>The item a converted cell became: the first cell its own (queued) effect cleared.</summary>
        private static Cell ConvertedItem(TurnEffect combo, CellPos p, IReadOnlyList<TurnEffect> effects)
        {
            for (int i = 1; i < effects.Count; i++)
                if (effects[i].TriggeredBy == 0 && effects[i].Origin == p)
                    return effects[i].Cleared[0].Cell;
            return combo.Kind == EffectKind.OrbBomb
                ? Cell.OfSpecial(SpecialKind.Bomb)
                : Cell.OfSpecial(SpecialKind.RocketHorizontal);
        }

        private static float BeamStep(int targets)
        {
            return targets > 1 ? Mathf.Min(OrbMaxStep, (OrbTotal - OrbPulse - OrbBeam) / (targets - 1)) : 0f;
        }

        /// <summary>From the combo firing until the last beam has hit.</summary>
        private static float ConvertTime(int targets)
        {
            return OrbPulse + Mathf.Max(0, targets - 1) * BeamStep(targets) + OrbBeam;
        }

        /// <summary>Cells a band rocket travels to reach p: rows o.Y +- half fly sideways, columns o.X +- half fly up/down.</summary>
        private static int CrossDistance(CellPos o, CellPos p, int half)
        {
            int dx = Mathf.Abs(p.X - o.X), dy = Mathf.Abs(p.Y - o.Y);
            int best = int.MaxValue;
            if (dy <= half) best = dx;
            if (dx <= half) best = Mathf.Min(best, dy);
            return best == int.MaxValue ? dx + dy : best;
        }

        private static int Ring(CellPos o, CellPos p)
        {
            return Mathf.Max(Mathf.Abs(p.X - o.X), Mathf.Abs(p.Y - o.Y));
        }

        private void SetStep(CellPos p, TileAnim anim, float start, float duration)
        {
            _steps[p.X, p.Y] = new TileStep
            {
                Anim = anim,
                Start = start,
                Duration = duration,
                Target = CellPosition(p.X, p.Y),
                BaseColor = _tiles[p.X, p.Y].color
            };
        }

        private void AddFx(FxKind kind, float start, float duration, Vector3 from, Vector3 to, Color color,
                           int order, Sprite sprite, float size = 1f)
        {
            SpriteRenderer sr = TakeFromPool();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            sr.enabled = false; // shown when its start time comes
            _fx.Add(new Fx { Kind = kind, Start = start, Duration = duration, From = from, To = to, Color = color, Renderer = sr, Size = size });
        }

        // ---------- Playing: everything at time t. ----------

        private void ApplyTimeline(float t)
        {
            // New looks first; a tile's own step (which starts later) then takes over its scale.
            for (int i = 0; i < _looks.Count; i++)
            {
                Look look = _looks[i];
                if (t < look.Start) continue;
                if (_shown[look.Pos.X, look.Pos.Y] != look.Cell)
                    ShowCell(look.Pos.X, look.Pos.Y, look.Cell);
                float k = Mathf.Clamp01((t - look.Start) / ConvertPop);
                SetScale(_tiles[look.Pos.X, look.Pos.Y].transform, TileSize * (1f + 0.2f * Mathf.Sin(Mathf.PI * k)));
            }

            for (int y = 0; y < _grid.Height; y++)
                for (int x = 0; x < _grid.Width; x++)
                    if (_steps[x, y].Anim != TileAnim.None)
                        ApplyTile(x, y, _steps[x, y], t);

            if (_hasPop && t >= _popStart)
            {
                Transform tile = _tiles[_popPos.X, _popPos.Y].transform;
                if (_shown[_popPos.X, _popPos.Y] != _popCell)
                    ShowCell(_popPos.X, _popPos.Y, _popCell);
                tile.localPosition = CellPosition(_popPos.X, _popPos.Y);
                float k = Mathf.Clamp01((t - _popStart) / PopDuration);
                // 0 to a bit bigger, then back to normal.
                float s = k < 0.6f ? EaseOut(k / 0.6f) * 1.18f : Mathf.Lerp(1.18f, 1f, EaseInOut((k - 0.6f) / 0.4f));
                SetScale(tile, TileSize * s);
            }

            Vector3 shake = Vector3.zero;
            for (int i = 0; i < _fx.Count; i++)
            {
                Fx fx = _fx[i];
                float k = (t - fx.Start) / fx.Duration;
                if (fx.Kind == FxKind.Shake)
                {
                    if (k >= 0f && k < 1f)
                    {
                        float a = fx.Size * (1f - k);
                        shake += new Vector3(Mathf.Sin(t * 95f) * a, Mathf.Cos(t * 83f) * a, 0f);
                    }
                    continue;
                }
                fx.Renderer.enabled = k >= 0f && k < 1f;
                if (fx.Renderer.enabled) ApplyFx(fx, k, t);
            }
            transform.localPosition = shake;
        }

        private void ApplyTile(int x, int y, TileStep step, float t)
        {
            if (t < step.Start) return;
            Transform tile = _tiles[x, y].transform;
            float k = step.Duration > 0f ? Mathf.Clamp01((t - step.Start) / step.Duration) : 1f;
            switch (step.Anim)
            {
                case TileAnim.Hide:
                    SetScale(tile, 0f);
                    break;
                case TileAnim.Shrink:
                    // A small swell, then gone.
                    float s = k < 0.25f ? 1f + 0.12f * (k / 0.25f) : 1.12f * (1f - EaseIn((k - 0.25f) / 0.75f));
                    SetScale(tile, TileSize * s);
                    break;
                case TileAnim.Collect:
                    float e = EaseIn(k);
                    tile.localPosition = Vector3.Lerp(CellPosition(x, y), step.Target, e);
                    SetScale(tile, k >= 1f ? 0f : TileSize * Mathf.Lerp(1f, 0.35f, e));
                    break;
                case TileAnim.FlashOut:
                    _tiles[x, y].color = Color.Lerp(step.BaseColor, TileColors.Flash, Mathf.Clamp01(k / 0.35f));
                    SetScale(tile, k < 0.35f ? TileSize * (1f + 0.15f * k / 0.35f)
                                             : TileSize * 1.15f * (1f - EaseIn((k - 0.35f) / 0.65f)));
                    break;
                case TileAnim.OrbSelf:
                    // Pulse, hold while the beams go out, then vanish at the end.
                    float pulseEnd = OrbPulse / step.Duration;
                    float size;
                    if (k < pulseEnd) size = 1f + 0.22f * Mathf.Sin(Mathf.PI * k / pulseEnd);
                    else if (k < 0.8f) size = 1f;
                    else size = 1f - EaseIn((k - 0.8f) / 0.2f);
                    SetScale(tile, TileSize * size);
                    break;
                case TileAnim.Shake:
                    // Side to side around its cell, dying out; the scale is left to its look.
                    float wobble = k >= 1f ? 0f : Mathf.Sin(k * Mathf.PI * 6f) * HitShakeAmount * (1f - k);
                    tile.localPosition = CellPosition(x, y) + new Vector3(wobble, 0f, 0f);
                    break;
                case TileAnim.MergeCenter:
                    // Swells while the others land on it, then gives way to the combo.
                    SetScale(tile, k >= 1f ? 0f : TileSize * (1f + 0.25f * Mathf.Sin(Mathf.PI * k)));
                    break;
            }
        }

        private void ApplyFx(Fx fx, float k, float t)
        {
            Transform tr = fx.Renderer.transform;
            Color c = fx.Color;
            switch (fx.Kind)
            {
                case FxKind.RocketHalf:
                {
                    // Constant speed.
                    Vector3 dir = (fx.To - fx.From).normalized;
                    tr.localPosition = Vector3.Lerp(fx.From, fx.To, k);
                    tr.localRotation = Facing(dir);
                    SetScale(tr, 0.55f);
                    fx.Renderer.color = c;
                    break;
                }
                case FxKind.RocketTrail:
                {
                    // The head moves like its half; the trail stops growing at TrailLength and fades at the end.
                    float flight = fx.Duration - TrailFade;
                    float travelled = Vector3.Distance(fx.From, fx.To) * Mathf.Clamp01((t - fx.Start) / flight);
                    float length = Mathf.Min(travelled, TrailLength);
                    Vector3 dir = (fx.To - fx.From).normalized;
                    Vector3 head = fx.From + dir * travelled;
                    tr.localPosition = head - dir * (length * 0.5f);
                    tr.localRotation = Facing(dir);
                    tr.localScale = new Vector3(Mathf.Max(0.001f, length), 0.9f, 1f);
                    float fade = t - fx.Start > flight ? 1f - (t - fx.Start - flight) / TrailFade : 1f;
                    c.a = 0.55f * fade;
                    fx.Renderer.color = c;
                    break;
                }
                case FxKind.BombFlash:
                    tr.localPosition = fx.From;
                    tr.localRotation = Quaternion.identity;
                    SetScale(tr, Mathf.Lerp(0.5f, fx.Size, EaseOut(k)));
                    c.a = 0.9f * (1f - k);
                    fx.Renderer.color = c;
                    break;
                case FxKind.BombWave:
                    tr.localPosition = fx.From;
                    tr.localRotation = Quaternion.identity;
                    SetScale(tr, Mathf.Lerp(0.6f, fx.Size, EaseOut(k)));
                    c.a = 1f - EaseIn(k);
                    fx.Renderer.color = c;
                    break;
                case FxKind.Particle:
                {
                    // Thrown with its velocity (To), pulled down, spinning, fading out at the end.
                    float s = t - fx.Start;
                    tr.localPosition = fx.From + fx.To * s + new Vector3(0f, 0.5f * ParticleGravity * s * s, 0f);
                    tr.localRotation = Quaternion.Euler(0f, 0f, fx.Spin * s);
                    tr.localScale = new Vector3(fx.Size * fx.Stretch, fx.Size, 1f);
                    c.a *= k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;
                    fx.Renderer.color = c;
                    break;
                }
                case FxKind.Beam:
                {
                    Vector3 d = fx.To - fx.From;
                    tr.localPosition = (fx.From + fx.To) * 0.5f;
                    tr.localRotation = Facing(d);
                    tr.localScale = new Vector3(d.magnitude, Mathf.Lerp(0.14f, 0.03f, k), 1f);
                    c.a = 1f - EaseIn(k);
                    fx.Renderer.color = c;
                    break;
                }
            }
        }

        private void ApplyFall(float amount)
        {
            for (int y = 0; y < _grid.Height; y++)
                for (int x = 0; x < _grid.Width; x++)
                    _tiles[x, y].transform.localPosition =
                        Vector3.LerpUnclamped(_fallStart[x, y], CellPosition(x, y), amount);
        }

        // ---------- Tile looks. ----------

        private void ShowCell(int x, int y, Cell cell)
        {
            _shown[x, y] = cell;
            SpriteRenderer sr = _tiles[x, y];
            SpriteRenderer deco = _decos[x, y];
            deco.transform.localRotation = Quaternion.identity;
            deco.transform.localScale = Vector3.one;

            if (cell.IsEmpty)
            {
                sr.enabled = false;
                deco.enabled = false;
                return;
            }
            sr.enabled = true;
            if (cell.Kind == CellKind.Color)
            {
                sr.sprite = ShapeSprites.Square;
                sr.color = TileColors.ToColor(cell.Color);
                // A water balloon is a see-through layer on top of the square.
                deco.enabled = cell.HasBalloon;
                if (cell.HasBalloon)
                {
                    Color balloonTint;
                    deco.sprite = ObstacleSprites.For(ObstacleKind.WaterBalloon, 1, out balloonTint);
                    deco.color = balloonTint;
                    SetScale(deco.transform, 1.08f);
                }
                return;
            }
            if (cell.IsObstacle)
            {
                Color tint;
                sr.sprite = ObstacleSprites.ForCell(cell, out tint);
                sr.color = tint;
                deco.enabled = false;
                return;
            }

            deco.enabled = true;
            switch (cell.Special)
            {
                case SpecialKind.RocketHorizontal:
                case SpecialKind.RocketVertical:
                    bool vertical = cell.Special == SpecialKind.RocketVertical;
                    sr.sprite = ShapeSprites.Diamond(vertical);
                    sr.color = TileColors.RocketBody;
                    deco.sprite = ShapeSprites.Arrows(vertical);
                    deco.color = TileColors.RocketArrows;
                    break;
                case SpecialKind.Bomb:
                    sr.sprite = ShapeSprites.Disc;
                    sr.color = TileColors.BombBody;
                    deco.sprite = ShapeSprites.Ring;
                    deco.color = TileColors.BombRing;
                    break;
                default:
                    sr.sprite = ShapeSprites.Sphere;
                    sr.color = TileColors.ToColor(cell.OrbColor);
                    deco.sprite = ShapeSprites.DashedRing;
                    deco.color = TileColors.OrbRing;
                    break;
            }
        }

        // ---------- Pool of effect sprites: made on demand, reused every turn, gone with the board. ----------

        private SpriteRenderer TakeFromPool()
        {
            if (_poolUsed == _pool.Count)
            {
                var go = new GameObject("Fx");
                go.transform.SetParent(transform, false);
                _pool.Add(NewRenderer(go, 0));
            }
            return _pool[_poolUsed++];
        }

        private void ReleaseFx()
        {
            for (int i = 0; i < _poolUsed; i++) _pool[i].enabled = false;
            _poolUsed = 0;
            _fx.Clear();
        }

        private static SpriteRenderer NewRenderer(GameObject go, int order)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ShapeSprites.Square;
            sr.sortingOrder = order;
            sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            return sr;
        }

        // Rotation about z that turns +x toward dir.
        private static Quaternion Facing(Vector3 dir)
        {
            return Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }

        private static void SetScale(Transform t, float s)
        {
            t.localScale = new Vector3(s, s, 1f);
        }

        private static float EaseIn(float k) { return k * k; }

        private static float EaseOut(float k) { return 1f - (1f - k) * (1f - k); }

        private static float EaseInOut(float k) { return k * k * (3f - 2f * k); }

        private static Vector3 CellPosition(int x, int y)
        {
            return new Vector3(x, y, 0f);
        }

        // Tiles and effects are visible only inside this rectangle, so new tiles entering from above
        // and rocket halves leaving the board stay hidden.
        private void BuildMask(int width, int height)
        {
            var maskGo = new GameObject("GridMask");
            maskGo.transform.SetParent(transform, false);
            maskGo.transform.localPosition = new Vector3((width - 1) * 0.5f, (height - 1) * 0.5f, 0f);
            maskGo.transform.localScale = new Vector3(width, height, 1f);
            maskGo.AddComponent<SpriteMask>().sprite = ShapeSprites.Square;
        }
    }
}
