using System;
using System.Collections.Generic;
using BlastGame.Core;
using UnityEngine;
using Grid = BlastGame.Core.Grid; // UnityEngine.Grid also exists

namespace BlastGame.View
{
    /// <summary>
    /// Creates itself after the scene loads, starts a game from a clock seed and draws it.
    /// Turns taps into GameSession.Tap calls; it owns no game rules.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        // Level settings (a file takes over in step 5).
        private const int Width = 9;
        private const int Height = 9;
        private const int ColorCount = 4;
        private const int Moves = 20;
        private const TileColor FirstGoalColor = TileColor.Red;
        private const int FirstGoalCount = 20;
        private const TileColor SecondGoalColor = TileColor.Blue;
        private const int SecondGoalCount = 20;

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
            _hud.Build(StartGame);

            StartGame();
        }

        /// <summary>Starts a fresh game: new clock seed, new grid, new session; the old board is destroyed.</summary>
        private void StartGame()
        {
            int seed = unchecked((int)DateTime.UtcNow.Ticks);
            Debug.Log("BlastGame seed: " + seed);

            _random = new SeededRandom(seed);
            Grid grid = GridFactory.CreateRandom(Width, Height, ColorCount, _random);
            var goals = new List<ColorGoal>
            {
                new ColorGoal(FirstGoalColor, FirstGoalCount),
                new ColorGoal(SecondGoalColor, SecondGoalCount)
            };
            _session = new GameSession(grid, ColorCount, Moves, goals, _random);

            if (_view != null) Destroy(_view.gameObject);
            _view = new GameObject("GridView").AddComponent<GridView>();
            _view.Build(grid);
            _fitter.Setup(_camera, grid.Width, grid.Height);

            _hud.SetSeed(seed);
            _hud.BuildGoals(_session);
            _hud.Refresh(_session);
            _hud.HideResult();
        }

        private void Update()
        {
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

            // The result panel opens once the last turn has been shown.
            if (_session.Status != GameStatus.Playing && !_view.IsAnimating && !_hud.IsResultVisible)
                _hud.ShowResult(_session.Status);
        }

        private void HandleTap(Vector2 screenPos)
        {
            if (_view.IsAnimating) return;  // taps are ignored until the turn has been shown
            if (_hud.IsResultVisible) return; // the panel is open; only its button works
            if (!_camera.pixelRect.Contains(screenPos)) return; // the UI bar is not the board

            // z = distance from the camera to the z=0 plane where the tiles sit.
            Vector3 world = _camera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, -_camera.transform.position.z));
            int x = Mathf.RoundToInt(world.x);
            int y = Mathf.RoundToInt(world.y);
            if (!_session.Grid.IsInside(x, y)) return;

            TurnResult result = _session.Tap(x, y);
            _hud.Refresh(_session);
            _view.AnimateTurn(result);
        }
    }
}
