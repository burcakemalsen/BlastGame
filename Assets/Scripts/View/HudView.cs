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
    /// the goals (color swatches, then obstacle icons), a small seed label in a corner, and a result panel with one button (the caller says what it reads and does), and an error panel.
    /// In endless mode the moves and goals are hidden (their code stays) and a "Next board" button shows instead.
    /// A level select panel lists every level (no locks); a "Levels" button in the bar opens it during play.
    /// It only shows numbers it is given; it owns no game rules.
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        /// <summary>Share of the screen height (from the top) the bar takes; the board camera uses the rest.</summary>
        public const float BarFraction = 0.12f;

        private const int ResultTitleSize = 100;
        private const int ErrorTextSize = 44;
        private const float DoneAlpha = 0.35f;
        private const float IconPop = 0.15f;        // seconds: a goal icon swells when a flying item lands on it
        private const float IconPopAmount = 0.3f;   // extra size at the top of the swell
        private static readonly Color TextColor = new Color(0.93f, 0.93f, 0.96f, 1f);
        private static readonly Color BarColor = new Color(0.12f, 0.12f, 0.18f, 1f);

        private sealed class GoalItem
        {
            public bool IsObstacle;
            public TileColor Color;     // color goals
            public ObstacleKind Kind;   // obstacle goals
            public Color SwatchColor;   // full-alpha swatch or icon tint
            public Image Swatch;
            public Text Label;
            public int Shown;           // the number on screen; lags the session while items are flying
            public int Claimed;         // items flying to it this turn
            public float PopStart = -1f;
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
        private GameObject _nextButton;
        private Action _onNextBoard;
        private readonly List<GoalItem> _goalItems = new List<GoalItem>();
        private Action _onButton;
        private Action _onLevels;
        private GameObject _levelSelect;
        private Canvas _canvas;
        private GameSession _session;
        private readonly List<Image> _flyers = new List<Image>(); // items flying to a goal icon, reused

        public bool IsResultVisible
        {
            get { return _resultPanel.activeSelf; }
        }

        public bool IsLevelSelectVisible
        {
            get { return _levelSelect != null && _levelSelect.activeSelf; }
        }

        /// <summary>onLevels runs when the bar's "Levels" button is pressed.</summary>
        public void Build(Action onButton, Action onNextBoard, Action onLevels)
        {
            _onButton = onButton;
            _onNextBoard = onNextBoard;
            _onLevels = onLevels;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (EventSystem.current == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            var canvas = gameObject.AddComponent<Canvas>();
            _canvas = canvas;
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

        /// <summary>Endless mode: hides moves and goals, shows the "Next board" button.</summary>
        public void SetEndless(bool endless)
        {
            _movesText.gameObject.SetActive(!endless);
            _goalsRoot.gameObject.SetActive(!endless);
            _nextButton.SetActive(endless);
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
                Color color = TileColors.ToColor(goal.Color);
                GoalItem item = NewGoalItem("Goal " + goal.Color, null, color);
                item.Color = goal.Color;
                _goalItems.Add(item);
            }
            foreach (ObstacleGoal goal in session.ObstacleGoals)
            {
                Color tint;
                Sprite icon = ObstacleSprites.For(goal.Kind, 1, out tint);
                GoalItem item = NewGoalItem("Goal " + goal.Kind, icon, tint);
                item.IsObstacle = true;
                item.Kind = goal.Kind;
                _goalItems.Add(item);
            }
        }

        /// <summary>
        /// One more item wants to fly to a goal this turn: true, with the icon's screen position, while the goal
        /// still shows more than the items already flying to it. False when that goal does not exist, is already
        /// covered, or the goals are hidden (endless mode).
        /// </summary>
        public bool TryClaimGoal(GoalRef goal, out Vector2 screen)
        {
            screen = Vector2.zero;
            if (!_goalsRoot.gameObject.activeInHierarchy) return false;
            GoalItem item = Find(goal);
            if (item == null || item.Shown - item.Claimed <= 0) return false;
            item.Claimed++;
            // Screen-space overlay canvas: the world position is the screen position.
            RectTransform rt = item.Swatch.rectTransform;
            Vector3 p = rt.TransformPoint(rt.rect.center);
            screen = new Vector2(p.x, p.y);
            return true;
        }

        /// <summary>A flying item landed on its goal: the number drops by one (never below the session's) and the icon pops.</summary>
        public void GoalArrived(GoalRef goal)
        {
            GoalItem item = Find(goal);
            if (item == null || _session == null) return;
            item.Shown = Mathf.Max(Remaining(item), item.Shown - 1);
            ShowGoal(item);
            item.PopStart = Time.time;
        }

        private GoalItem Find(GoalRef goal)
        {
            for (int i = 0; i < _goalItems.Count; i++)
            {
                GoalItem item = _goalItems[i];
                if (item.IsObstacle != goal.IsObstacle) continue;
                if (item.IsObstacle ? item.Kind == goal.Kind : item.Color == goal.Color) return item;
            }
            return null;
        }

        private int Remaining(GoalItem item)
        {
            return item.IsObstacle ? _session.Remaining(item.Kind) : _session.Remaining(item.Color);
        }

        // The icon pop of goals that just got an item.
        private void Update()
        {
            for (int i = 0; i < _goalItems.Count; i++)
            {
                GoalItem item = _goalItems[i];
                if (item.PopStart < 0f) continue;
                float k = (Time.time - item.PopStart) / IconPop;
                float s = 1f;
                if (k >= 1f) item.PopStart = -1f;
                else s = 1f + IconPopAmount * Mathf.Sin(Mathf.PI * k);
                item.Swatch.rectTransform.localScale = new Vector3(s, s, 1f);
            }
        }

        private GoalItem NewGoalItem(string name, Sprite icon, Color color)
        {
            var item = new GameObject(name, typeof(RectTransform));
            item.transform.SetParent(_goalsRoot, false);
            ((RectTransform)item.transform).sizeDelta = new Vector2(150f, 90f);

            var swatch = NewImage("Swatch", item.transform, color);
            if (icon != null)
            {
                swatch.sprite = icon;
                swatch.preserveAspect = true;
            }
            swatch.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            swatch.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            // Pivot at the center, so the arrival pop swells in place.
            swatch.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            swatch.rectTransform.sizeDelta = icon != null ? new Vector2(64f, 64f) : new Vector2(56f, 56f);
            swatch.rectTransform.anchoredPosition = new Vector2(swatch.rectTransform.sizeDelta.x * 0.5f, 0f);

            var label = NewText("Count", item.transform, 44, TextAnchor.MiddleLeft);
            label.rectTransform.anchorMin = new Vector2(0f, 0f);
            label.rectTransform.anchorMax = new Vector2(1f, 1f);
            label.rectTransform.offsetMin = new Vector2(70f, 0f);
            label.rectTransform.offsetMax = Vector2.zero;

            return new GoalItem { SwatchColor = color, Swatch = swatch, Label = label };
        }

        /// <summary>
        /// Shows every number as the session has it (start of a level, end of a shown turn) and ends the turn's
        /// claims. Allocates no objects (only the label strings).
        /// </summary>
        public void Refresh(GameSession session)
        {
            _session = session;
            _movesText.text = "Moves " + session.MovesLeft;
            for (int i = 0; i < _goalItems.Count; i++)
            {
                GoalItem item = _goalItems[i];
                item.Shown = Remaining(item);
                item.Claimed = 0;
                ShowGoal(item);
            }
        }

        /// <summary>
        /// After a tap: only the moves follow the session at once; goal numbers drop as flying items land
        /// (GoalArrived) and catch up with the session in the next Refresh.
        /// </summary>
        public void RefreshMoves(GameSession session)
        {
            _session = session;
            _movesText.text = "Moves " + session.MovesLeft;
        }

        private void ShowGoal(GoalItem item)
        {
            bool done = item.Shown == 0;
            item.Label.text = done ? "Done" : item.Shown.ToString();
            float alpha = done ? DoneAlpha : 1f;
            Color c = item.SwatchColor;
            c.a *= alpha;
            item.Swatch.color = c;
            Color t = item.Label.color;
            t.a = alpha;
            item.Label.color = t;
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

        /// <summary>
        /// Shows flying item number index (made on first use) at a screen point, sizePixels wide, over everything.
        /// The board camera cannot draw over the bar, so the board hands such flights to the HUD.
        /// </summary>
        public void ShowFlyer(int index, Sprite sprite, Color color, Vector2 screen, float sizePixels)
        {
            while (_flyers.Count <= index)
            {
                Image image = NewImage("Flyer", transform, Color.white); // last child: drawn over everything
                image.preserveAspect = true;
                image.gameObject.SetActive(false);
                _flyers.Add(image);
            }
            Image flyer = _flyers[index];
            flyer.sprite = sprite;
            flyer.color = color;
            float scale = _canvas.scaleFactor > 0f ? _canvas.scaleFactor : 1f;
            flyer.rectTransform.sizeDelta = new Vector2(sizePixels, sizePixels) / scale;
            flyer.rectTransform.position = new Vector3(screen.x, screen.y, 0f); // overlay canvas: world = screen
            flyer.gameObject.SetActive(true);
        }

        public void HideFlyer(int index)
        {
            if (index < _flyers.Count) _flyers[index].gameObject.SetActive(false);
        }

        public void HideFlyers()
        {
            for (int i = 0; i < _flyers.Count; i++) _flyers[i].gameObject.SetActive(false);
        }

        /// <summary>
        /// Builds the level select panel once: a button per level, numbered from 1; a press runs onPick with the
        /// 0-based index. Every level is open. The panel starts hidden.
        /// </summary>
        public void BuildLevelSelect(int levelCount, Action<int> onPick)
        {
            if (_levelSelect != null) Destroy(_levelSelect);

            Image panel = NewImage("LevelSelect", transform, new Color(0.10f, 0.10f, 0.15f, 1f));
            panel.raycastTarget = true; // the board behind takes no taps
            panel.rectTransform.anchorMin = Vector2.zero;
            panel.rectTransform.anchorMax = Vector2.one;
            panel.rectTransform.offsetMin = Vector2.zero;
            panel.rectTransform.offsetMax = Vector2.zero;
            _levelSelect = panel.gameObject;

            Text title = NewText("Title", panel.transform, 96, TextAnchor.MiddleCenter);
            title.text = "Levels";
            title.rectTransform.anchorMin = new Vector2(0f, 0.80f);
            title.rectTransform.anchorMax = new Vector2(1f, 0.92f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;

            var grid = new GameObject("Grid", typeof(RectTransform));
            grid.transform.SetParent(panel.transform, false);
            var gridRect = (RectTransform)grid.transform;
            gridRect.anchorMin = new Vector2(0.08f, 0.10f);
            gridRect.anchorMax = new Vector2(0.92f, 0.78f);
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;
            var layout = grid.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(200f, 200f);
            layout.spacing = new Vector2(36f, 36f);
            layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            layout.startAxis = GridLayoutGroup.Axis.Horizontal;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 4;

            for (int i = 0; i < levelCount; i++)
            {
                int index = i; // captured per button
                Button button = NewButton("Level " + (i + 1), grid.transform, (i + 1).ToString(), 80);
                button.onClick.AddListener(() => onPick(index));
            }

            _levelSelect.SetActive(false);
        }

        public void ShowLevelSelect()
        {
            if (_levelSelect != null) _levelSelect.SetActive(true);
        }

        public void HideLevelSelect()
        {
            if (_levelSelect != null) _levelSelect.SetActive(false);
        }

        private Button NewButton(string name, Transform parent, string text, int textSize)
        {
            Image image = NewImage(name, parent, new Color(0.20f, 0.60f, 0.86f, 1f));
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text label = NewText("Label", image.transform, textSize, TextAnchor.MiddleCenter);
            label.text = text;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private void BuildBar()
        {
            Image bar = NewImage("TopBar", transform, BarColor);
            bar.rectTransform.anchorMin = new Vector2(0f, 1f - BarFraction);
            bar.rectTransform.anchorMax = new Vector2(1f, 1f);
            bar.rectTransform.offsetMin = Vector2.zero;
            bar.rectTransform.offsetMax = Vector2.zero;

            Button levels = NewButton("LevelsButton", bar.transform, "Levels", 40);
            var levelsRect = (RectTransform)levels.transform;
            levelsRect.anchorMin = new Vector2(0f, 0.5f);
            levelsRect.anchorMax = new Vector2(0f, 0.5f);
            levelsRect.pivot = new Vector2(0f, 0.5f);
            levelsRect.sizeDelta = new Vector2(170f, 100f);
            levelsRect.anchoredPosition = new Vector2(20f, 0f);
            levels.onClick.AddListener(OnLevelsClicked);

            _levelText = NewText("Level", bar.transform, 40, TextAnchor.LowerLeft);
            _levelText.rectTransform.anchorMin = new Vector2(0.20f, 0.55f);
            _levelText.rectTransform.anchorMax = new Vector2(0.42f, 1f);
            _levelText.rectTransform.offsetMin = Vector2.zero;
            _levelText.rectTransform.offsetMax = Vector2.zero;

            _movesText = NewText("Moves", bar.transform, 56, TextAnchor.UpperLeft);
            _movesText.rectTransform.anchorMin = new Vector2(0.20f, 0f);
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
            layout.spacing = 6f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Image next = NewImage("NextBoardButton", bar.transform, new Color(0.20f, 0.60f, 0.86f, 1f));
            next.raycastTarget = true;
            next.rectTransform.anchorMin = new Vector2(1f, 0.5f);
            next.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            next.rectTransform.pivot = new Vector2(1f, 0.5f);
            next.rectTransform.sizeDelta = new Vector2(300f, 110f);
            next.rectTransform.anchoredPosition = new Vector2(-32f, 0f);
            var nextButton = next.gameObject.AddComponent<Button>();
            nextButton.targetGraphic = next;
            nextButton.onClick.AddListener(OnNextBoardClicked);
            Text nextLabel = NewText("Label", next.transform, 44, TextAnchor.MiddleCenter);
            nextLabel.text = "Next board";
            nextLabel.rectTransform.anchorMin = Vector2.zero;
            nextLabel.rectTransform.anchorMax = Vector2.one;
            nextLabel.rectTransform.offsetMin = Vector2.zero;
            nextLabel.rectTransform.offsetMax = Vector2.zero;
            _nextButton = next.gameObject;
            _nextButton.SetActive(false);
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

        private void OnLevelsClicked()
        {
            if (_onLevels != null) _onLevels();
        }

        private void OnNextBoardClicked()
        {
            if (_onNextBoard != null) _onNextBoard();
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
