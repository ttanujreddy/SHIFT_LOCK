using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Runtime-generated pixel-art theme for SHIFT//LOCK. No external art dependencies.
public static class ShiftLockTheme
{
    public static readonly Color Background = new Color(0.018f, 0.032f, 0.060f, 1f);
    public static readonly Color Panel = new Color(0.035f, 0.075f, 0.125f, 0.98f);
    public static readonly Color Steel = new Color(0.105f, 0.20f, 0.30f, 1f);
    public static readonly Color SteelLight = new Color(0.22f, 0.38f, 0.49f, 1f);
    public static readonly Color Accent = new Color(0.12f, 0.88f, 1f, 1f);
    public static readonly Color Text = new Color(0.84f, 0.93f, 1f, 1f);
    public static readonly Color Danger = new Color(1f, 0.20f, 0.18f, 1f);
    public static readonly Color Exit = new Color(0.20f, 1f, 0.58f, 1f);
    public static readonly Color Amber = new Color(1f, 0.63f, 0.16f, 1f);

    static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    public static void ApplyCamera(Camera camera)
    {
        if (camera == null) return;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Background;
    }

    public static void ApplyTile(GameObject tileObject, int tileId)
    {
        if (tileObject == null) return;
        SpriteRenderer renderer = tileObject.GetComponent<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null) return;

        Sprite themed = CreateTileSprite(tileId, renderer.sprite);
        if (themed != null) renderer.sprite = themed;
        renderer.color = Color.white;
    }

    public static Sprite CreateTileSprite(int tileId, Sprite referenceSprite)
    {
        if (referenceSprite == null) return null;

        int width = Mathf.Clamp(Mathf.RoundToInt(referenceSprite.rect.width), 16, 96);
        int height = Mathf.Clamp(Mathf.RoundToInt(referenceSprite.rect.height), 16, 96);
        float ppu = referenceSprite.pixelsPerUnit > 0 ? referenceSprite.pixelsPerUnit : 16f;
        string cacheKey = tileId + ":" + width + "x" + height + ":" + ppu;
        Sprite cached;
        if (spriteCache.TryGetValue(cacheKey, out cached)) return cached;

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
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), ppu);
        sprite.name = "SHIFT_LOCK_Tile_" + tileId;
        spriteCache[cacheKey] = sprite;
        return sprite;
    }

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

    static void DrawSpike(Texture2D t)
    {
        DrawTriangle(t, 0.13f, 0.18f, 0.87f, 0.88f, Danger);
        DrawTriangle(t, 0.30f, 0.22f, 0.70f, 0.72f, Amber);
        FillRect(t, 0.43f, 0.23f, 0.57f, 0.40f, Background);
        FillRect(t, 0.08f, 0.12f, 0.92f, 0.20f, SteelLight);
    }

    static void DrawExit(Texture2D t)
    {
        FillRect(t, 0.20f, 0.12f, 0.32f, 0.88f, SteelLight);
        FillRect(t, 0.24f, 0.14f, 0.29f, 0.82f, Text);
        FillRect(t, 0.32f, 0.62f, 0.85f, 0.82f, Exit);
        FillRect(t, 0.32f, 0.48f, 0.70f, 0.62f, Exit);
        FillRect(t, 0.39f, 0.68f, 0.78f, 0.75f, Text);
        FillRect(t, 0.11f, 0.07f, 0.43f, 0.15f, Steel);
    }

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

    static void FillRect(Texture2D t, float left, float bottom, float right, float top, Color color)
    {
        int x0 = Mathf.Clamp(Mathf.FloorToInt(left * t.width), 0, t.width - 1);
        int y0 = Mathf.Clamp(Mathf.FloorToInt(bottom * t.height), 0, t.height - 1);
        int x1 = Mathf.Clamp(Mathf.CeilToInt(right * t.width), x0 + 1, t.width);
        int y1 = Mathf.Clamp(Mathf.CeilToInt(top * t.height), y0 + 1, t.height);
        for (int y = y0; y < y1; y++)
            for (int x = x0; x < x1; x++)
                t.SetPixel(x, y, color);
    }

    static void FillCircle(Texture2D t, float centerX, float centerY, float radius, Color color)
    {
        for (int y = 0; y < t.height; y++)
        {
            float py = (y + 0.5f) / t.height;
            for (int x = 0; x < t.width; x++)
            {
                float px = (x + 0.5f) / t.width;
                float dx = px - centerX;
                float dy = py - centerY;
                if (dx * dx + dy * dy <= radius * radius)
                    t.SetPixel(x, y, color);
            }
        }
    }

    static void DrawTriangle(Texture2D t, float left, float bottom, float right, float top, Color color)
    {
        for (int y = 0; y < t.height; y++)
        {
            float py = (y + 0.5f) / t.height;
            if (py < bottom || py > top) continue;
            float progress = (py - bottom) / Mathf.Max(0.001f, top - bottom);
            float halfWidth = (right - left) * 0.5f * progress;
            float center = (left + right) * 0.5f;
            for (int x = 0; x < t.width; x++)
            {
                float px = (x + 0.5f) / t.width;
                if (Mathf.Abs(px - center) <= halfWidth)
                    t.SetPixel(x, y, color);
            }
        }
    }

    public static void StyleText(TMP_Text text, Color color, bool bold)
    {
        if (text == null) return;
        text.color = color;
        if (bold) text.fontStyle |= FontStyles.Bold;
        text.outlineColor = new Color(0f, 0f, 0f, 0.65f);
        text.outlineWidth = 0.12f;
    }

    public static void StyleButton(Button button)
    {
        if (button == null) return;
        Image image = button.GetComponent<Image>();
        if (image != null) image.color = Panel;

        ColorBlock colors = button.colors;
        colors.normalColor = Panel;
        colors.highlightedColor = new Color(0.08f, 0.20f, 0.30f, 1f);
        colors.pressedColor = new Color(0.02f, 0.42f, 0.52f, 1f);
        colors.selectedColor = new Color(0.06f, 0.24f, 0.34f, 1f);
        colors.disabledColor = new Color(0.11f, 0.13f, 0.17f, 0.65f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null) StyleText(label, Text, true);
    }

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
            else if (original.IndexOf("LEVEL SELECT", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                label.text = "CHAMBER SELECT";
                StyleText(label, Accent, true);
            }
            else
            {
                StyleText(label, Text, true);
            }
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

    public static void StylePanel(GameObject root)
    {
        if (root == null) return;
        Image image = root.GetComponent<Image>();
        if (image != null) image.color = Panel;
        foreach (TMP_Text label in root.GetComponentsInChildren<TMP_Text>(true))
            StyleText(label, Text, true);
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
            StyleButton(button);
    }
}
