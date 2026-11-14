using UnityEngine;

namespace BlastGame.View
{
    /// <summary>Makes the camera orthographic and keeps the whole grid visible, re-fitting on resize.</summary>
    public sealed class CameraFitter : MonoBehaviour
    {
        private const float Margin = 0.5f; // empty space around the grid, in cells

        private Camera _camera;
        private int _gridWidth;
        private int _gridHeight;
        private int _lastScreenWidth;
        private int _lastScreenHeight;

        public void Setup(Camera cam, int gridWidth, int gridHeight)
        {
            _camera = cam;
            _gridWidth = gridWidth;
            _gridHeight = gridHeight;
            _camera.orthographic = true;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = TileColors.Background;
            Fit();
        }

        private void Update()
        {
            if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
                Fit();
        }

        private void Fit()
        {
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;

            // Cells are centered on integer coordinates, so the grid spans [-0.5, W-0.5] x [-0.5, H-0.5].
            float halfW = _gridWidth * 0.5f + Margin;
            float halfH = _gridHeight * 0.5f + Margin;
            float aspect = Mathf.Max(1, Screen.width) / (float)Mathf.Max(1, Screen.height);

            // Orthographic size is the half-height; take the larger of what height and width need.
            _camera.orthographicSize = Mathf.Max(halfH, halfW / aspect);
            _camera.transform.position = new Vector3((_gridWidth - 1) * 0.5f, (_gridHeight - 1) * 0.5f, -10f);
        }
    }
}
