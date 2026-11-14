using System;
using BlastGame.Core;
using UnityEngine;
using Grid = BlastGame.Core.Grid; // UnityEngine.Grid also exists

namespace BlastGame.View
{
    /// <summary>
    /// Creates itself after the scene loads, starts a game from a clock seed and draws it.
    /// Turns taps into rule calls (TurnRules.Tap in endless mode, GameSession.Tap otherwise); it owns no game rules.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const string LevelsFolder = "Levels";

        /// <summary>
        /// Endless play: moves, goals and win/lose are hidden and not applied; the board plays forever and
        /// "Next board" opens the next level file. Off: the level game (moves, goals, win/lose panel).
        /// Set true to bring endless play back (its code stays).
        /// static readonly, not const, so the unused branch does not warn.
        /// </summary>
        private static readonly bool EndlessMode = false;

        private LevelData[] _levels; // null when loading failed; the error panel is up then
        private int _levelIndex;
        private GameSession _session;
        private SeededRandom _random; // one source for the grid and every refill of this game
        private GridView _view;
        private HudView _hud;
        private Camera _camera;
        private CameraFitter _fitter;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateOnLoad()
        {
            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
            }
            _camera = cam;
            // The board uses the screen below the UI bar.
            _camera.rect = new Rect(0f, 0f, 1f, 1f - HudView.BarFraction);
            _fitter = cam.gameObject.AddComponent<CameraFitter>();

            _hud = new GameObject("Hud").AddComponent<HudView>();
            _hud.Build(OnResultButton, OnNextBoard, OnLevels);
            _hud.SetEndless(EndlessMode);

            // The game opens on the level select; a level starts when its button is pressed.
            if (LoadLevels())
            {
                _hud.BuildLevelSelect(_levels.Length, OnPickLevel);
                _hud.ShowLevelSelect();
            }
        }

        /// <summary>Reads every level file up front. On any problem shows the error panel and returns false.</summary>
        private bool LoadLevels()
        {
            TextAsset[] files = Resources.LoadAll<TextAsset>(LevelsFolder);
            if (files.Length == 0)
            {
                ShowLoadError("No level files found.");
                return false;
            }

            Array.Sort(files, (a, b) => string.CompareOrdinal(a.name, b.name));
            var levels = new LevelData[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    levels[i] = LevelParser.Parse(files[i].text);
                }
                catch (LevelFormatException ex)
                {
                    ShowLoadError(files[i].name + ".json: " + ex.Message);
                    return false;
                }
            }
            _levels = levels;
            return true;
        }

        private void ShowLoadError(string message)
        {
            Debug.LogError(message);
            _hud.ShowError(message);
        }

        /// <summary>The panel button: next level after a win (or level 1 after the last), same level after a loss.</summary>
        private void OnResultButton()
        {
            if (_session == null) return;
            if (_session.Status == GameStatus.Won)
                _levelIndex = (_levelIndex + 1) % _levels.Length;
            StartLevel(_levelIndex);
        }

        /// <summary>The bar's "Levels" button: back to the level select (the board stays behind it until a pick).</summary>
        private void OnLevels()
        {
            if (_levels == null) return;
            _hud.HideResult();
            _hud.ShowLevelSelect();
        }

        /// <summary>A level select button: that level starts, whatever was being played.</summary>
        private void OnPickLevel(int index)
        {
            _hud.HideLevelSelect();
            StartLevel(index);
        }

        /// <summary>Endless mode: the next level file (the first after the last) with a new seed; not during an animation.</summary>
        private void OnNextBoard()
        {
            if (_levels == null || _view == null || _view.IsAnimating) return;
            StartLevel((_levelIndex + 1) % _levels.Length);
        }

        /// <summary>Starts a level with a fresh clock seed, grid and session; the old board is destroyed.</summary>
        private void StartLevel(int index)
        {
            _levelIndex = index;
            int seed = unchecked((int)DateTime.UtcNow.Ticks);
            Debug.Log("BlastGame seed: " + seed);

            _random = new SeededRandom(seed);
            _session = _levels[index].CreateSession(_random);
            Grid grid = _session.Grid;

            if (_view != null) Destroy(_view.gameObject);
            _view = new GameObject("GridView").AddComponent<GridView>();
            _view.Build(grid);
            _view.SetGoalTarget(GoalWorldPosition, ShowFlyer, _hud.HideFlyers);
            _fitter.Setup(_camera, grid.Width, grid.Height);

            _hud.SetSeed(seed);
            _hud.SetLevel(index + 1);
            _hud.BuildGoals(_session);
            _hud.Refresh(_session);
            _hud.HideResult();
        }

        /// <summary>The HUD icon of an obstacle goal in board space (the board sits at the world origin), or null.</summary>
        private Vector3? GoalWorldPosition(ObstacleKind kind)
        {
            Vector2 screen;
            if (!_hud.TryGetGoalScreenPoint(kind, out screen)) return null;
            Vector3 world = _camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -_camera.transform.position.z));
            world.z = 0f;
            return world;
        }

        /// <summary>Draws a flying item on the HUD at a board position, sized in board units.</summary>
        private void ShowFlyer(int index, Sprite sprite, Color color, Vector3 boardPos, float size)
        {
            Vector3 screen = _camera.WorldToScreenPoint(boardPos);
            float pixelsPerUnit = _camera.pixelHeight / (2f * _camera.orthographicSize);
            _hud.ShowFlyer(index, sprite, color, new Vector2(screen.x, screen.y), size * pixelsPerUnit);
        }

        private void Update()
        {
            if (_session == null) return; // loading failed: only the error panel shows

            // Touch first: Unity also emulates a mouse click from a touch, so the mouse is read only without touches.
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began) HandleTap(touch.position);
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                HandleTap(Input.mousePosition);
            }

            // The result panel opens once the last turn has been shown (never in endless mode).
            if (!EndlessMode && _session.Status != GameStatus.Playing && !_view.IsAnimating && !_hud.IsResultVisible
                && !_hud.IsLevelSelectVisible)
                ShowResult();
        }

        private void ShowResult()
        {
            if (_session.Status == GameStatus.Won)
            {
                bool last = _levelIndex == _levels.Length - 1;
                _hud.ShowResult(last ? "All levels done!" : "You win!", last ? "Play again" : "Next level");
            }
            else
            {
                _hud.ShowResult("Out of moves", "Try again");
            }
        }

        private void HandleTap(Vector2 screenPos)
        {
            if (_hud.IsLevelSelectVisible) return; // the level select covers the board
            if (_view.IsAnimating) return;  // taps are ignored until the turn has been shown
            if (_hud.IsResultVisible) return; // the panel is open; only its button works
            if (!_camera.pixelRect.Contains(screenPos)) return; // the UI bar is not the board

            // z = distance from the camera to the z=0 plane where the tiles sit.
            Vector3 world = _camera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, -_camera.transform.position.z));
            int x = Mathf.RoundToInt(world.x);
            int y = Mathf.RoundToInt(world.y);
            if (!_session.Grid.IsInside(x, y)) return;

            TurnResult result;
            if (EndlessMode)
            {
                // The session would stop taps when moves run out, so the rules are called directly;
                // the level's moves and goals are read but not used.
                result = TurnRules.Tap(_session.Grid, x, y, _session.ColorCount, _random);
            }
            else
            {
                result = _session.Tap(x, y);
                _hud.Refresh(_session);
            }
            _view.AnimateTurn(result);
        }
    }
}
