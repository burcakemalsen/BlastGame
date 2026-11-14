using System;
using System.Collections.Generic;
using BlastGame.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlastGame.View
{
    /// <summary>
    /// Builds the on-screen UI in code (the scene file stays untouched): a top bar with moves left and
    /// the goals, a small seed label in a corner, and a result panel with one button (the caller says what it reads and does), and an error panel.
    /// It only shows numbers it is given; it owns no game rules.
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        /// <summary>Share of the screen height (from the top) the bar takes; the board camera uses the rest.</summary>
        public const float BarFraction = 0.12f;

        private const int ResultTitleSize = 100;
        private const int ErrorTextSize = 44;
        private const float DoneAlpha = 0.35f;
        private static readonly Color TextColor = new Color(0.93f, 0.93f, 0.96f, 1f);
        private static readonly Color BarColor = new Color(0.12f, 0.12f, 0.18f, 1f);

        private sealed class GoalItem
        {
            public TileColor Color;
            public Image Swatch;
            public Text Label;
        }

        private Font _font;
        private Text _levelText;
        private Text _movesText;
        private RectTransform _goalsRoot;
        private Text _seedText;
        private GameObject _resultPanel;
        private Text _resultTitle;
        private Text _buttonLabel;
        private GameObject _button;
        private readonly List<GoalItem> _goalItems = new List<GoalItem>();
        private Action _onButton;

        public bool IsResultVisible
        {
            get { return _resultPanel.activeSelf; }
        }

        public void Build(Action onButton)
        {
            _onButton = onButton;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (EventSystem.current == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            BuildBar();
            BuildSeedLabel();
            BuildResultPanel();
        }

        /// <summary>Shows the seed in the corner (the caller also logs it).</summary>
        public void SetSeed(int seed)
        {
            _seedText.text = "Seed " + seed;
        }

        /// <summary>Shows the level number (1-based) in the bar.</summary>
        public void SetLevel(int number)
        {
            _levelText.text = "Level " + number;
        }

        /// <summary>Replaces the goal items; call once per new session, not per tap.</summary>
        public void BuildGoals(GameSession session)
        {
            for (int i = 0; i < _goalItems.Count; i++)
                Destroy(_goalItems[i].Swatch.transform.parent.gameObject);
            _goalItems.Clear();

            foreach (ColorGoal goal in session.Goals)
            {
                var item = new GameObject("Goal " + goal.Color, typeof(RectTransform));
                item.transform.SetParent(_goalsRoot, false);
                ((RectTransform)item.transform).sizeDelta = new Vector2(190f, 90f);

                var swatch = NewImage("Swatch", item.transform, TileColors.ToColor(goal.Color));
                swatch.rectTransform.anchorMin = new Vector2(0f, 0.5f);
                swatch.rectTransform.anchorMax = new Vector2(0f, 0.5f);
                swatch.rectTransform.pivot = new Vector2(0f, 0.5f);
                swatch.rectTransform.sizeDelta = new Vector2(64f, 64f);

                var label = NewText("Count", item.transform, 52, TextAnchor.MiddleLeft);
                label.rectTransform.anchorMin = new Vector2(0f, 0f);
                label.rectTransform.anchorMax = new Vector2(1f, 1f);
                label.rectTransform.offsetMin = new Vector2(80f, 0f);
                label.rectTransform.offsetMax = Vector2.zero;

                _goalItems.Add(new GoalItem { Color = goal.Color, Swatch = swatch, Label = label });
            }
        }

        /// <summary>Updates the numbers from the session. Allocates no objects (only the label strings).</summary>
        public void Refresh(GameSession session)
        {
            _movesText.text = "Moves " + session.MovesLeft;
            for (int i = 0; i < _goalItems.Count; i++)
            {
                GoalItem item = _goalItems[i];
                int left = session.Remaining(item.Color);
                bool done = left == 0;
                item.Label.text = done ? "Done" : left.ToString();
                float alpha = done ? DoneAlpha : 1f;
                Color c = item.Swatch.color;
                c.a = alpha;
                item.Swatch.color = c;
                Color t = item.Label.color;
                t.a = alpha;
                item.Label.color = t;
            }
        }

        /// <summary>Opens the panel with a title and a button that runs the Build callback.</summary>
        public void ShowResult(string title, string buttonText)
        {
            _resultTitle.text = title;
            _resultTitle.fontSize = ResultTitleSize;
            _resultTitle.horizontalOverflow = HorizontalWrapMode.Overflow;
            _buttonLabel.text = buttonText;
            _button.SetActive(true);
            _resultPanel.SetActive(true);
        }

        /// <summary>Opens the panel with a readable message and no button (the game cannot go on).</summary>
        public void ShowError(string message)
        {
            _resultTitle.text = message;
            _resultTitle.fontSize = ErrorTextSize;
            _resultTitle.horizontalOverflow = HorizontalWrapMode.Wrap;
            _button.SetActive(false);
            _resultPanel.SetActive(true);
        }

        public void HideResult()
        {
            _resultPanel.SetActive(false);
        }

        private void BuildBar()
        {
            Image bar = NewImage("TopBar", transform, BarColor);
            bar.rectTransform.anchorMin = new Vector2(0f, 1f - BarFraction);
            bar.rectTransform.anchorMax = new Vector2(1f, 1f);
            bar.rectTransform.offsetMin = Vector2.zero;
            bar.rectTransform.offsetMax = Vector2.zero;

            _levelText = NewText("Level", bar.transform, 40, TextAnchor.LowerLeft);
            _levelText.rectTransform.anchorMin = new Vector2(0.03f, 0.55f);
            _levelText.rectTransform.anchorMax = new Vector2(0.42f, 1f);
            _levelText.rectTransform.offsetMin = Vector2.zero;
            _levelText.rectTransform.offsetMax = Vector2.zero;

            _movesText = NewText("Moves", bar.transform, 56, TextAnchor.UpperLeft);
            _movesText.rectTransform.anchorMin = new Vector2(0.03f, 0f);
            _movesText.rectTransform.anchorMax = new Vector2(0.42f, 0.55f);
            _movesText.rectTransform.offsetMin = Vector2.zero;
            _movesText.rectTransform.offsetMax = Vector2.zero;

            var goals = new GameObject("Goals", typeof(RectTransform));
            goals.transform.SetParent(bar.transform, false);
            _goalsRoot = (RectTransform)goals.transform;
            _goalsRoot.anchorMin = new Vector2(0.42f, 0f);
            _goalsRoot.anchorMax = new Vector2(0.97f, 1f);
            _goalsRoot.offsetMin = Vector2.zero;
            _goalsRoot.offsetMax = Vector2.zero;
            var layout = goals.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.spacing = 10f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private void BuildSeedLabel()
        {
            _seedText = NewText("Seed", transform, 28, TextAnchor.LowerLeft);
            _seedText.color = new Color(TextColor.r, TextColor.g, TextColor.b, 0.6f);
            _seedText.rectTransform.anchorMin = new Vector2(0f, 0f);
            _seedText.rectTransform.anchorMax = new Vector2(1f, 0.04f);
            _seedText.rectTransform.offsetMin = new Vector2(16f, 8f);
            _seedText.rectTransform.offsetMax = Vector2.zero;
        }

        private void BuildResultPanel()
        {
            Image dim = NewImage("ResultPanel", transform, new Color(0f, 0f, 0f, 0.65f));
            dim.raycastTarget = true; // swallows UI clicks behind the panel
            dim.rectTransform.anchorMin = Vector2.zero;
            dim.rectTransform.anchorMax = Vector2.one;
            dim.rectTransform.offsetMin = Vector2.zero;
            dim.rectTransform.offsetMax = Vector2.zero;
            _resultPanel = dim.gameObject;

            _resultTitle = NewText("Title", dim.transform, ResultTitleSize, TextAnchor.MiddleCenter);
            _resultTitle.rectTransform.anchorMin = new Vector2(0.05f, 0.5f);
            _resultTitle.rectTransform.anchorMax = new Vector2(0.95f, 0.5f);
            _resultTitle.rectTransform.pivot = new Vector2(0.5f, 0f);
            _resultTitle.rectTransform.sizeDelta = new Vector2(0f, 160f);
            _resultTitle.rectTransform.anchoredPosition = new Vector2(0f, 40f);

            Image buttonImage = NewImage("ResultButton", dim.transform, new Color(0.20f, 0.60f, 0.86f, 1f));
            buttonImage.raycastTarget = true;
            buttonImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            buttonImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            buttonImage.rectTransform.pivot = new Vector2(0.5f, 1f);
            buttonImage.rectTransform.sizeDelta = new Vector2(520f, 140f);
            buttonImage.rectTransform.anchoredPosition = new Vector2(0f, -20f);
            var button = buttonImage.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(OnButtonClicked);
            _button = buttonImage.gameObject;

            Text label = NewText("Label", buttonImage.transform, 64, TextAnchor.MiddleCenter);
            _buttonLabel = label;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;

            _resultPanel.SetActive(false);
        }

        private void OnButtonClicked()
        {
            if (_onButton != null) _onButton();
        }

        private Image NewImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Text NewText(string name, Transform parent, int size, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = _font;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = TextColor;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }
    }
}
