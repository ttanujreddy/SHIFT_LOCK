using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Runtime-generated pixel-art theme for SHIFT//LOCK. No external art dependencies.
/// <summary>
/// Generates cached sci-fi pixel-art sprites and applies a consistent navy/cyan visual palette to tiles, menus, buttons, and completion screens.
/// </summary>
public static class ShiftLockTheme
{
    public static readonly Color Background = new Color(0.018f, 0.032f, 0.060f, 1f);
    public static readonly Color Panel = new Color(0.055f, 0.125f, 0.19f, 1f);
    public static readonly Color ButtonBorder = new Color(0.10f, 0.72f, 0.88f, 0.95f);
    public static readonly Color Steel = new Color(0.105f, 0.20f, 0.30f, 1f);
    public static readonly Color SteelLight = new Color(0.22f, 0.38f, 0.49f, 1f);
    public static readonly Color Accent = new Color(0.12f, 0.88f, 1f, 1f);
    public static readonly Color Text = new Color(0.84f, 0.93f, 1f, 1f);
    public static readonly Color Danger = new Color(1f, 0.20f, 0.18f, 1f);
    public static readonly Color Exit = new Color(0.20f, 1f, 0.58f, 1f);
    public static readonly Color Amber = new Color(1f, 0.63f, 0.16f, 1f);

    static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    static float activeArtInset = 0.10f;
    static float ActiveArtSpan { get { return 1f - (activeArtInset * 2f); } }

    /// <summary>
    /// Set the camera's solid dark navy background.
    /// </summary>
    public static void ApplyCamera(Camera camera)
    {
        if (camera == null) return;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Background;
    }

    /// <summary>
    /// Replace a tile renderer's original sprite with cached generated artwork.
    /// </summary>
    public static void ApplyTile(GameObject tileObject, int tileId)
    {
        if (tileObject == null) return;
        SpriteRenderer renderer = tileObject.GetComponent<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null) return;

        Sprite themed = CreateTileSprite(tileId, renderer.sprite);
        if (themed != null) renderer.sprite = themed;
        renderer.color = Color.white;
    }

    /// <summary>
    /// Create or reuse procedural art while preserving source sprite pivot and pixels-per-unit.
    /// </summary>
    public static Sprite CreateTileSprite(int tileId, Sprite referenceSprite)
    {
        if (referenceSprite == null) return null;

        int width = Mathf.Clamp(Mathf.RoundToInt(referenceSprite.rect.width), 16, 96);
        int height = Mathf.Clamp(Mathf.RoundToInt(referenceSprite.rect.height), 16, 96);
        float ppu = referenceSprite.pixelsPerUnit > 0 ? referenceSprite.pixelsPerUnit : 16f;
        Vector2 normalizedPivot = new Vector2(
            referenceSprite.pivot.x / Mathf.Max(1f, referenceSprite.rect.width),
            referenceSprite.pivot.y / Mathf.Max(1f, referenceSprite.rect.height));
        string cacheKey = tileId + ":" + width + "x" + height + ":" + ppu + ":" +
                          normalizedPivot.x + ":" + normalizedPivot.y;
        Sprite cached;
        if (spriteCache.TryGetValue(cacheKey, out cached)) return cached;

        // Original art uses per-sprite pivots and often leaves transparent padding. Preserve
        // the pivot, and size each generated object type to sit safely inside its grid cell.
        switch (tileId)
        {
            case 0: activeArtInset = 0.16f; break; // Blocks: retain a strong tile silhouette.
            case 1: activeArtInset = 0.22f; break; // Player: avoid covering neighboring tiles.
            case 2: activeArtInset = 0.20f; break; // Hazards: clearly visible but cell-contained.
            case 3: activeArtInset = 0.24f; break; // Flag/exit has an off-center original pivot.
            case 4: activeArtInset = 0.20f; break; // Gravity switch.
            case 5: activeArtInset = 0.18f; break; // Door.
            case 6: activeArtInset = 0.20f; break; // Activated switch.
            default: activeArtInset = 0.16f; break;
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = "SHIFT_LOCK_Tile_" + tileId;
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(new Color[width * height]);

        switch (tileId)
        {
            case 0: DrawBlock(texture); break;
            case 1: DrawPlayer(texture); break;
            case 2: DrawSpike(texture); break;
            case 3: DrawExit(texture); break;
            case 4: DrawSwitch(texture, false); break;
            case 5: DrawDoor(texture); break;
            case 6: DrawSwitch(texture, true); break;
            default: DrawBlock(texture); break;
        }

        texture.Apply(false, false);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), normalizedPivot, ppu);
        sprite.name = "SHIFT_LOCK_Tile_" + tileId;
        spriteCache[cacheKey] = sprite;
        return sprite;
    }

    /// <summary>
    /// Draw a metallic wall panel.
    /// </summary>
    static void DrawBlock(Texture2D t)
    {
        FillRect(t, 0.03f, 0.03f, 0.97f, 0.97f, Background);
        FillRect(t, 0.10f, 0.10f, 0.90f, 0.90f, SteelLight);
        FillRect(t, 0.17f, 0.17f, 0.83f, 0.83f, Panel);
        FillRect(t, 0.23f, 0.23f, 0.77f, 0.77f, Steel);
        FillRect(t, 0.29f, 0.29f, 0.71f, 0.71f, Panel);
        Color bolt = new Color(0.42f, 0.72f, 0.81f, 1f);
        FillRect(t, 0.19f, 0.19f, 0.29f, 0.29f, bolt);
        FillRect(t, 0.71f, 0.19f, 0.81f, 0.29f, bolt);
        FillRect(t, 0.19f, 0.71f, 0.29f, 0.81f, bolt);
        FillRect(t, 0.71f, 0.71f, 0.81f, 0.81f, bolt);
        FillRect(t, 0.36f, 0.43f, 0.64f, 0.48f, Accent);
        FillRect(t, 0.43f, 0.34f, 0.57f, 0.39f, SteelLight);
    }

    /// <summary>
    /// Draw the cyan player core.
    /// </summary>
    static void DrawPlayer(Texture2D t)
    {
        FillRect(t, 0.12f, 0.12f, 0.88f, 0.88f, Accent);
        FillRect(t, 0.20f, 0.20f, 0.80f, 0.80f, Background);
        FillRect(t, 0.29f, 0.29f, 0.71f, 0.71f, Steel);
        FillRect(t, 0.37f, 0.39f, 0.63f, 0.65f, Text);
        FillRect(t, 0.25f, 0.70f, 0.38f, 0.77f, Accent);
        FillRect(t, 0.62f, 0.70f, 0.75f, 0.77f, Accent);
        FillRect(t, 0.25f, 0.23f, 0.75f, 0.29f, Accent);
    }

    /// <summary>
    /// Draw the upward-pointing hazard spike.
    /// </summary>
    static void DrawSpike(Texture2D t)
    {
        // Draw a steel spike with a red warning core, rather than a flame-shaped hazard.
        DrawTriangle(t, 0.13f, 0.18f, 0.87f, 0.90f, SteelLight);
        DrawTriangle(t, 0.31f, 0.22f, 0.69f, 0.72f, Danger);
        FillRect(t, 0.08f, 0.12f, 0.92f, 0.20f, Steel);
        FillRect(t, 0.34f, 0.19f, 0.66f, 0.25f, Amber);
    }

    /// <summary>
    /// Draw the green exit flag and its pole.
    /// </summary>
    static void DrawExit(Texture2D t)
    {
        // Extend the post almost the full tile height, keeping the flag silhouette recognizable.
        FillRect(t, 0.20f, 0.04f, 0.32f, 0.97f, SteelLight);
        FillRect(t, 0.24f, 0.05f, 0.29f, 0.94f, Text);
        FillRect(t, 0.32f, 0.67f, 0.88f, 0.86f, Exit);
        FillRect(t, 0.32f, 0.52f, 0.73f, 0.67f, Exit);
        FillRect(t, 0.39f, 0.72f, 0.81f, 0.79f, Text);
        FillRect(t, 0.11f, 0.04f, 0.43f, 0.12f, Steel);
    }

    /// <summary>
    /// Draw a cyan inactive or green activated gravity switch.
    /// </summary>
    static void DrawSwitch(Texture2D t, bool activated)
    {
        Color glow = activated ? Exit : Accent;
        FillRect(t, 0.04f, 0.04f, 0.96f, 0.96f, Background);
        FillRect(t, 0.12f, 0.12f, 0.88f, 0.88f, SteelLight);
        FillRect(t, 0.19f, 0.19f, 0.81f, 0.81f, Panel);
        FillCircle(t, 0.50f, 0.50f, 0.30f, glow);
        FillCircle(t, 0.50f, 0.50f, 0.19f, Background);
        FillCircle(t, 0.50f, 0.50f, 0.09f, glow);
        FillRect(t, 0.12f, 0.44f, 0.24f, 0.56f, Text);
        FillRect(t, 0.76f, 0.44f, 0.88f, 0.56f, Text);
    }

    /// <summary>
    /// Draw a red security door.
    /// </summary>
    static void DrawDoor(Texture2D t)
    {
        FillRect(t, 0.05f, 0.03f, 0.95f, 0.97f, Background);
        FillRect(t, 0.13f, 0.10f, 0.87f, 0.90f, Danger);
        FillRect(t, 0.22f, 0.17f, 0.78f, 0.83f, Panel);
        FillRect(t, 0.29f, 0.23f, 0.71f, 0.77f, new Color(0.36f, 0.08f, 0.11f, 1f));
        FillRect(t, 0.34f, 0.29f, 0.66f, 0.34f, Danger);
        FillRect(t, 0.34f, 0.47f, 0.66f, 0.53f, Amber);
        FillRect(t, 0.34f, 0.66f, 0.66f, 0.71f, Danger);
        FillRect(t, 0.45f, 0.37f, 0.55f, 0.63f, SteelLight);
    }

    // Scale drawing coordinates into the selected tile type's safe visible footprint.
    /// <summary>
    /// Map normalized drawing coordinates into the current sprite's safe art footprint.
    /// </summary>
    static float Inset(float value)
    {
        return activeArtInset + value * ActiveArtSpan;
    }

    /// <summary>
    /// Draw a filled pixel rectangle into a texture.
    /// </summary>
    static void FillRect(Texture2D t, float left, float bottom, float right, float top, Color color)
    {
        int x0 = Mathf.Clamp(Mathf.FloorToInt(Inset(left) * t.width), 0, t.width - 1);
        int y0 = Mathf.Clamp(Mathf.FloorToInt(Inset(bottom) * t.height), 0, t.height - 1);
        int x1 = Mathf.Clamp(Mathf.CeilToInt(Inset(right) * t.width), x0 + 1, t.width);
        int y1 = Mathf.Clamp(Mathf.CeilToInt(Inset(top) * t.height), y0 + 1, t.height);
        for (int y = y0; y < y1; y++)
            for (int x = x0; x < x1; x++)
                t.SetPixel(x, y, color);
    }

    /// <summary>
    /// Draw a filled pixel circle into a texture.
    /// </summary>
    static void FillCircle(Texture2D t, float centerX, float centerY, float radius, Color color)
    {
        float cx = Inset(centerX);
        float cy = Inset(centerY);
        float rx = radius * ActiveArtSpan;
        float ry = radius * ActiveArtSpan;
        for (int y = 0; y < t.height; y++)
        {
            float py = (y + 0.5f) / t.height;
            for (int x = 0; x < t.width; x++)
            {
                float px = (x + 0.5f) / t.width;
                float dx = (px - cx) / Mathf.Max(rx, 0.001f);
                float dy = (py - cy) / Mathf.Max(ry, 0.001f);
                if (dx * dx + dy * dy <= 1f)
                    t.SetPixel(x, y, color);
            }
        }
    }

    /// <summary>
    /// Draw an upward-pointing pixel triangle.
    /// </summary>
    static void DrawTriangle(Texture2D t, float left, float bottom, float right, float top, Color color)
    {
        left = Inset(left);
        bottom = Inset(bottom);
        right = Inset(right);
        top = Inset(top);
        for (int y = 0; y < t.height; y++)
        {
            float py = (y + 0.5f) / t.height;
            if (py < bottom || py > top) continue;
            float progress = (py - bottom) / Mathf.Max(0.001f, top - bottom);
            // Point upward: widest at the base, narrowest at the tip.
            float halfWidth = (right - left) * 0.5f * (1f - progress);
            float center = (left + right) * 0.5f;
            for (int x = 0; x < t.width; x++)
            {
                float px = (x + 0.5f) / t.width;
                if (Mathf.Abs(px - center) <= halfWidth)
                    t.SetPixel(x, y, color);
            }
        }
    }

    /// <summary>
    /// Apply readable color, weight, and outline to TextMeshPro labels.
    /// </summary>
    public static void StyleText(TMP_Text text, Color color, bool bold)
    {
        if (text == null) return;
        text.color = color;
        text.alpha = 1f;
        if (bold) text.fontStyle |= FontStyles.Bold;
        text.outlineColor = new Color(0f, 0f, 0f, 0.8f);
        text.outlineWidth = 0.16f;
    }

    /// <summary>
    /// Apply the sci-fi button colors, interaction states, outlines, and labels.
    /// </summary>
    public static void StyleButton(Button button)
    {
        if (button == null) return;
        Image image = button.GetComponent<Image>();
        if (image != null) image.color = Color.white; // ColorTint applies the ColorBlock; avoid multiplying two dark colors.

        ColorBlock colors = button.colors;
        colors.normalColor = Panel;
        colors.highlightedColor = new Color(0.10f, 0.30f, 0.42f, 1f);
        colors.pressedColor = new Color(0.02f, 0.48f, 0.60f, 1f);
        colors.selectedColor = new Color(0.08f, 0.26f, 0.36f, 1f);
        colors.disabledColor = new Color(0.10f, 0.14f, 0.19f, 0.9f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        // Give buttons a visible silhouette against the near-black page background.
        Outline outline = button.GetComponent<Outline>();
        if (outline == null) outline = button.gameObject.AddComponent<Outline>();
        outline.effectColor = Accent;
        outline.effectDistance = new Vector2(2.5f, -2.5f);
        outline.useGraphicAlpha = true;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            StyleText(label, Text, true);
            label.alpha = 1f;
        }
    }

    /// <summary>
    /// Style a menu page, including TextMeshPro and legacy UI.Text headings.
    /// </summary>
    public static void StylePage(GameObject root, bool mainMenu)
    {
        if (root == null) return;
        Image rootImage = root.GetComponent<Image>();
        if (rootImage != null) rootImage.color = Background;

        foreach (TMP_Text label in root.GetComponentsInChildren<TMP_Text>(true))
        {
            string original = label.text;
            if (original.IndexOf("GRAVITY PUZZLE GAME", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                label.text = "SHIFT//LOCK";
                StyleText(label, Accent, true);
            }
            else if (original.IndexOf("LEVEL SELECT", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                     original.IndexOf("CHAMBER SELECT", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                label.text = "CHAMBER SELECT";
                StyleText(label, Accent, true);
                label.alpha = 1f;
                label.fontSize = Mathf.Max(label.fontSize, 30f);
            }
            else
            {
                StyleText(label, Text, true);
            }
        }

        // The template's chamber heading is a legacy UI.Text rather than TextMeshPro,
        // so style it explicitly too. This is why it stayed dark after the TMP-only pass.
        foreach (UnityEngine.UI.Text label in root.GetComponentsInChildren<UnityEngine.UI.Text>(true))
        {
            if (label == null) continue;
            string original = label.text;
            if (original.IndexOf("LEVEL SELECT", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                original.IndexOf("CHAMBER SELECT", System.StringComparison.OrdinalIgnoreCase) >= 0)
                label.text = "CHAMBER SELECT";
            else if (original.IndexOf("GRAVITY PUZZLE GAME", System.StringComparison.OrdinalIgnoreCase) >= 0)
                label.text = "SHIFT//LOCK";

            bool heading = label.text.IndexOf("CHAMBER SELECT", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                           label.text.IndexOf("SHIFT//LOCK", System.StringComparison.OrdinalIgnoreCase) >= 0;
            label.color = heading ? Accent : Text;
            label.fontStyle = FontStyle.Bold;
            if (heading) label.fontSize = Mathf.Max(label.fontSize, 30);
            label.canvasRenderer.SetAlpha(1f);

            Outline textOutline = label.GetComponent<Outline>();
            if (textOutline == null) textOutline = label.gameObject.AddComponent<Outline>();
            textOutline.effectColor = new Color(0f, 0f, 0f, 1f);
            textOutline.effectDistance = new Vector2(1f, -1f);
        }

        foreach (Button button in root.GetComponentsInChildren<Button>(true))
            StyleButton(button);

        if (!mainMenu)
        {
            foreach (TMP_Text label in root.GetComponentsInChildren<TMP_Text>(true))
                if (label.text.StartsWith("LEVEL ", System.StringComparison.OrdinalIgnoreCase))
                    StyleText(label, Accent, true);
        }
    }

    /// <summary>
    /// Style a completion panel and its nested image backgrounds.
    /// </summary>
    public static void StylePanel(GameObject root)
    {
        if (root == null) return;

        // The template win-screen can have its white full-screen Image nested below the root.
        // Recolor every non-button Image so the overlay never flashes white on completion.
        Image[] images = root.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            if (image == null) continue;
            if (image.GetComponent<Button>() != null || image.GetComponentInParent<Button>() != null)
                continue;

            RectTransform rect = image.rectTransform;
            bool fillsScreen = rect.anchorMin.x <= 0.05f && rect.anchorMin.y <= 0.05f &&
                               rect.anchorMax.x >= 0.95f && rect.anchorMax.y >= 0.95f;
            image.color = fillsScreen ? Background : Panel;
        }

        foreach (TMP_Text label in root.GetComponentsInChildren<TMP_Text>(true))
            StyleText(label, Text, true);
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
            StyleButton(button);
    }
}
