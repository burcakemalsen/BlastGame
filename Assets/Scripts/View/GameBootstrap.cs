using BlastGame.Core;
using UnityEngine;
using Grid = BlastGame.Core.Grid; // UnityEngine.Grid also exists

namespace BlastGame.View
{
    /// <summary>
    /// Creates itself after the scene loads, builds a seeded grid and draws it.
    /// Turns taps into TurnRules.Tap calls; it owns no game rules.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const int Width = 9;
        private const int Height = 9;
        private const int ColorCount = 5;
        private const int Seed = 42;

        private Grid _grid;
        private SeededRandom _random; // one source for the first grid and every refill
        private GridView _view;
        private Camera _camera;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateOnLoad()
        {
            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            _random = new SeededRandom(Seed);
            _grid = GridFactory.CreateRandom(Width, Height, ColorCount, _random);

            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
            }
            _camera = cam;

            _view = new GameObject("GridView").AddComponent<GridView>();
            _view.Build(_grid);
            cam.gameObject.AddComponent<CameraFitter>().Setup(cam, _grid.Width, _grid.Height);
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
        }

        private void HandleTap(Vector2 screenPos)
        {
            if (_view.IsAnimating) return; // taps are ignored until the turn has been shown

            // z = distance from the camera to the z=0 plane where the tiles sit.
            Vector3 world = _camera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, -_camera.transform.position.z));
            int x = Mathf.RoundToInt(world.x);
            int y = Mathf.RoundToInt(world.y);
            if (!_grid.IsInside(x, y)) return;

            TurnResult result = TurnRules.Tap(_grid, x, y, ColorCount, _random);
            _view.AnimateTurn(result);
        }
    }
}
