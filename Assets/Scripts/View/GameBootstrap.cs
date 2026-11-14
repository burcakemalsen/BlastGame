using BlastGame.Core;
using UnityEngine;
using Grid = BlastGame.Core.Grid; // UnityEngine.Grid also exists

namespace BlastGame.View
{
    /// <summary>
    /// Creates itself after the scene loads, builds a seeded grid and draws it.
    /// Turns taps into BlastRules.Blast calls; it owns no game rules.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const int Width = 8;
        private const int Height = 10;
        private const int ColorCount = 5;
        private const int Seed = 42;

        private Grid _grid;
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
            _grid = GridFactory.CreateRandom(Width, Height, ColorCount, new SeededRandom(Seed));

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
            // z = distance from the camera to the z=0 plane where the tiles sit.
            Vector3 world = _camera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, -_camera.transform.position.z));
            int x = Mathf.RoundToInt(world.x);
            int y = Mathf.RoundToInt(world.y);
            if (!_grid.IsInside(x, y)) return;

            BlastRules.Blast(_grid, x, y);
            _view.Refresh();
        }
    }
}
