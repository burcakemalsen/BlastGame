using BlastGame.Core;
using UnityEngine;
using Grid = BlastGame.Core.Grid; // UnityEngine.Grid also exists

namespace BlastGame.View
{
    /// <summary>
    /// Creates itself after the scene loads, builds a seeded grid and draws it.
    /// Draws only; it owns no game rules.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const int Width = 8;
        private const int Height = 10;
        private const int ColorCount = 5;
        private const int Seed = 42;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateOnLoad()
        {
            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            Grid grid = GridFactory.CreateRandom(Width, Height, ColorCount, new SeededRandom(Seed));

            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
            }

            var view = new GameObject("GridView").AddComponent<GridView>();
            view.Build(grid);
            cam.gameObject.AddComponent<CameraFitter>().Setup(cam, grid.Width, grid.Height);
        }
    }
}
