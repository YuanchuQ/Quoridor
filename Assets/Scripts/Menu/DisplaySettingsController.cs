// Builds and applies display settings for the main menu settings panel
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Quoridor.Menu
{
    /// <summary>
    /// Builds and applies display settings for the main menu settings panel.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DisplaySettingsController : MonoBehaviour
    {
        private const string RootName = "DisplaySettingsRoot";
        private const string BackButtonName = "BackButton";
        private const string LegacySettingsTextName = "SettingsPlaceholderText";
        private const string WidthKey = "Quoridor.Display.Width";
        private const string HeightKey = "Quoridor.Display.Height";
        private const string FullscreenKey = "Quoridor.Display.Fullscreen";
        private const int FallbackWidth = 1920;
        private const int FallbackHeight = 1080;
        private const float ContentWidth = 430f;
        private const float RowHeight = 38f;

        private readonly List<Vector2Int> availableResolutions = new();

        private Toggle fullscreenToggle;
        private Dropdown resolutionDropdown;
        private Text feedbackText;
        private bool hasBuilt;

        /// <summary>
        /// Ensures the settings controls exist and are synchronized with the current display state.
        /// </summary>
        public void EnsureBuilt()
        {
            if (!hasBuilt)
            {
                HideLegacySettingsText();
                BuildInterface();
                hasBuilt = true;
            }

            RefreshAvailableResolutions();
            SyncControlsToCurrentDisplay();
        }

        /// <summary>
        /// Applies the saved display settings without requiring the settings panel to be visible.
        /// </summary>
        public static void ApplySavedDisplaySettings()
        {
            if (!PlayerPrefs.HasKey(WidthKey) || !PlayerPrefs.HasKey(HeightKey))
            {
                return;
            }

            int width = PlayerPrefs.GetInt(WidthKey, Screen.width);
            int height = PlayerPrefs.GetInt(HeightKey, Screen.height);
            bool fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreenMode == FullScreenMode.Windowed ? 0 : 1) == 1;
            ApplyResolution(width, height, fullscreen);
        }

        private void OnEnable()
        {
            EnsureBuilt();
        }

        private void HideLegacySettingsText()
        {
            Transform legacyText = transform.Find(LegacySettingsTextName);
            if (legacyText != null)
            {
                legacyText.gameObject.SetActive(false);
            }
        }

        private void BuildInterface()
        {
            Font font = ResolveFont();
            Transform existingRoot = transform.Find(RootName);
            GameObject root = existingRoot != null ? existingRoot.gameObject : CreateUiObject(RootName, transform);
            if (existingRoot != null)
            {
                ClearChildren(root.transform);
            }

            RepositionBackButton();
            RectTransform rootRect = root.GetComponent<RectTransform>();
            SetAnchored(rootRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 6f), new Vector2(ContentWidth, 190f));

            fullscreenToggle = CreateToggle(root.transform, "FullscreenToggle", "全屏显示", font);
            SetAnchored(fullscreenToggle.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(ContentWidth, RowHeight));

            Text resolutionLabel = CreateText(root.transform, "ResolutionLabel", "分辨率", 20, FontStyle.Bold, TextAnchor.MiddleRight, font);
            SetAnchored(resolutionLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-146f, -68f), new Vector2(112f, RowHeight));

            resolutionDropdown = CreateDropdown(root.transform, "ResolutionDropdown", font);
            SetAnchored(resolutionDropdown.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(70f, -68f), new Vector2(260f, RowHeight));

            Button applyButton = CreateButton(root.transform, "ApplyDisplaySettingsButton", "应用", font);
            applyButton.onClick.AddListener(ApplySelectedSettings);
            SetAnchored(applyButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -116f), new Vector2(190f, 40f));

            feedbackText = CreateText(root.transform, "DisplaySettingsFeedback", string.Empty, 16, FontStyle.Normal, TextAnchor.MiddleCenter, font);
            SetAnchored(feedbackText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(ContentWidth, 26f));
        }

        private void RefreshAvailableResolutions()
        {
            availableResolutions.Clear();
            HashSet<Vector2Int> seen = new();
            Resolution[] resolutions = Screen.resolutions;
            for (int i = 0; i < resolutions.Length; i++)
            {
                AddResolution(new Vector2Int(resolutions[i].width, resolutions[i].height), seen);
            }

            AddResolution(new Vector2Int(1280, 720), seen);
            AddResolution(new Vector2Int(1600, 900), seen);
            AddResolution(new Vector2Int(FallbackWidth, FallbackHeight), seen);
            AddResolution(new Vector2Int(2560, 1440), seen);
            availableResolutions.Sort((left, right) =>
            {
                int areaComparison = (left.x * left.y).CompareTo(right.x * right.y);
                return areaComparison != 0 ? areaComparison : left.x.CompareTo(right.x);
            });

            resolutionDropdown.ClearOptions();
            List<string> options = new(availableResolutions.Count);
            for (int i = 0; i < availableResolutions.Count; i++)
            {
                Vector2Int resolution = availableResolutions[i];
                options.Add($"{resolution.x} x {resolution.y}");
            }

            resolutionDropdown.AddOptions(options);
        }

        private void AddResolution(Vector2Int resolution, HashSet<Vector2Int> seen)
        {
            if (resolution.x <= 0 || resolution.y <= 0 || seen.Contains(resolution))
            {
                return;
            }

            seen.Add(resolution);
            availableResolutions.Add(resolution);
        }

        private void SyncControlsToCurrentDisplay()
        {
            bool savedFullscreen = PlayerPrefs.HasKey(FullscreenKey)
                ? PlayerPrefs.GetInt(FullscreenKey) == 1
                : Screen.fullScreenMode != FullScreenMode.Windowed;
            int savedWidth = PlayerPrefs.GetInt(WidthKey, Screen.width > 0 ? Screen.width : FallbackWidth);
            int savedHeight = PlayerPrefs.GetInt(HeightKey, Screen.height > 0 ? Screen.height : FallbackHeight);

            fullscreenToggle.SetIsOnWithoutNotify(savedFullscreen);
            resolutionDropdown.SetValueWithoutNotify(FindClosestResolutionIndex(savedWidth, savedHeight));
            feedbackText.text = $"当前：{Screen.width} x {Screen.height}  {(Screen.fullScreenMode == FullScreenMode.Windowed ? "窗口" : "全屏")}";
        }

        private void ApplySelectedSettings()
        {
            if (availableResolutions.Count == 0)
            {
                return;
            }

            int index = Mathf.Clamp(resolutionDropdown.value, 0, availableResolutions.Count - 1);
            Vector2Int resolution = availableResolutions[index];
            bool fullscreen = fullscreenToggle.isOn;
            PlayerPrefs.SetInt(WidthKey, resolution.x);
            PlayerPrefs.SetInt(HeightKey, resolution.y);
            PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
            PlayerPrefs.Save();
            ApplyResolution(resolution.x, resolution.y, fullscreen);
            feedbackText.text = $"已应用：{resolution.x} x {resolution.y}  {(fullscreen ? "全屏" : "窗口")}";
        }

        private static void ApplyResolution(int width, int height, bool fullscreen)
        {
            FullScreenMode mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.SetResolution(width, height, mode);
        }

        private int FindClosestResolutionIndex(int width, int height)
        {
            int bestIndex = 0;
            int bestDistance = int.MaxValue;
            for (int i = 0; i < availableResolutions.Count; i++)
            {
                Vector2Int candidate = availableResolutions[i];
                int distance = Mathf.Abs(candidate.x - width) + Mathf.Abs(candidate.y - height);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private Font ResolveFont()
        {
            Text existingText = GetComponentInChildren<Text>(true);
            if (existingText != null && existingText.font != null)
            {
                return existingText.font;
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static Toggle CreateToggle(Transform parent, string name, string label, Font font)
        {
            GameObject toggleObject = CreateUiObject(name, parent);
            Toggle toggle = toggleObject.AddComponent<Toggle>();

            Image background = CreateImage(toggleObject.transform, "Background", new Color(0.95f, 0.92f, 0.86f, 0.95f));
            SetAnchored(background.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(26f, 26f));

            Image checkmark = CreateImage(background.transform, "Checkmark", new Color(0.3f, 0.55f, 1f, 1f));
            SetAnchored(checkmark.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(16f, 16f));

            Text labelText = CreateText(toggleObject.transform, "Label", label, 20, FontStyle.Bold, TextAnchor.MiddleLeft, font);
            SetAnchored(labelText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(48f, 0f), new Vector2(-48f, 0f));

            toggle.targetGraphic = background;
            toggle.graphic = checkmark;
            return toggle;
        }

        private static Dropdown CreateDropdown(Transform parent, string name, Font font)
        {
            GameObject dropdownObject = CreateUiObject(name, parent);
            Image dropdownImage = dropdownObject.AddComponent<Image>();
            dropdownImage.color = new Color(0.95f, 0.92f, 0.86f, 0.95f);
            Dropdown dropdown = dropdownObject.AddComponent<Dropdown>();

            Text label = CreateText(dropdownObject.transform, "Label", string.Empty, 18, FontStyle.Normal, TextAnchor.MiddleLeft, font);
            label.color = new Color(0.18f, 0.18f, 0.24f, 1f);
            SetAnchored(label.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(-42f, 0f));

            Text arrow = CreateText(dropdownObject.transform, "Arrow", "▼", 16, FontStyle.Bold, TextAnchor.MiddleCenter, font);
            arrow.color = new Color(0.18f, 0.18f, 0.24f, 1f);
            SetAnchored(arrow.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-18f, 0f), new Vector2(30f, 0f));

            GameObject template = CreateDropdownTemplate(dropdownObject.transform, font);
            dropdown.targetGraphic = dropdownImage;
            dropdown.captionText = label;
            dropdown.itemText = template.transform.Find("Viewport/Content/Item/Item Label").GetComponent<Text>();
            dropdown.template = template.GetComponent<RectTransform>();
            template.SetActive(false);
            return dropdown;
        }

        private static GameObject CreateDropdownTemplate(Transform parent, Font font)
        {
            GameObject template = CreateUiObject("Template", parent);
            Image templateImage = template.AddComponent<Image>();
            templateImage.color = new Color(0.23f, 0.22f, 0.28f, 0.98f);
            ScrollRect scrollRect = template.AddComponent<ScrollRect>();
            SetAnchored(template.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(0f, 104f));

            GameObject viewport = CreateUiObject("Viewport", template.transform);
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.white;
            SetAnchored(viewport.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            GameObject content = CreateUiObject("Content", viewport.transform);
            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            SetAnchored(content.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);

            Toggle item = CreateDropdownItem(content.transform, font);
            scrollRect.content = content.GetComponent<RectTransform>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            return template;
        }

        private static Toggle CreateDropdownItem(Transform parent, Font font)
        {
            GameObject itemObject = CreateUiObject("Item", parent);
            Toggle toggle = itemObject.AddComponent<Toggle>();
            Image itemBackground = itemObject.AddComponent<Image>();
            itemBackground.color = new Color(0.31f, 0.3f, 0.36f, 0.95f);
            LayoutElement layoutElement = itemObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 30f;

            Text itemLabel = CreateText(itemObject.transform, "Item Label", string.Empty, 16, FontStyle.Normal, TextAnchor.MiddleLeft, font);
            itemLabel.color = Color.white;
            SetAnchored(itemLabel.rectTransform, Vector2.zero, Vector2.one, new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(-24f, 0f));

            toggle.targetGraphic = itemBackground;
            return toggle;
        }

        private static Button CreateButton(Transform parent, string name, string label, Font font)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.3f, 0.55f, 1f, 1f);
            Button button = buttonObject.AddComponent<Button>();

            Text text = CreateText(buttonObject.transform, "Text", label, 20, FontStyle.Bold, TextAnchor.MiddleCenter, font);
            text.color = Color.white;
            SetAnchored(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.42f, 0.63f, 1f, 1f);
            colors.pressedColor = new Color(0.22f, 0.42f, 0.86f, 1f);
            button.colors = colors;
            return button;
        }

        private static Text CreateText(Transform parent, string name, string textValue, int size, FontStyle style, TextAnchor alignment, Font font)
        {
            GameObject textObject = CreateUiObject(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.text = textValue;
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            GameObject imageObject = CreateUiObject(name, parent);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private void RepositionBackButton()
        {
            Transform backButton = transform.Find(BackButtonName);
            if (backButton != null && backButton.TryGetComponent(out RectTransform rectTransform))
            {
                SetAnchored(rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -124f), new Vector2(220f, 48f));
            }
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void SetAnchored(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 sizeDelta)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = sizeDelta;
        }
    }
}
