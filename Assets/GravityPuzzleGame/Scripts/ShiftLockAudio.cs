using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Small synthesized one-shot effects for SHIFT//LOCK. No music and no imported audio assets.
public sealed class ShiftLockAudio : MonoBehaviour
{
    const int SampleRate = 22050;
    static ShiftLockAudio instance;

    AudioSource source;
    AudioClip uiClick;
    AudioClip moveTick;
    AudioClip gravityShift;
    AudioClip switchActivate;
    AudioClip death;
    AudioClip victory;
    readonly HashSet<int> wiredButtonIds = new HashSet<int>();

    public static void EnsureExists()
    {
        if (instance != null) return;
        GameObject audioObject = new GameObject("SHIFT_LOCK_Audio");
        instance = audioObject.AddComponent<ShiftLockAudio>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = 0.8f;

        uiClick = MakeClip("UI_Click", 0.055f, WaveKind.Click);
        moveTick = MakeClip("Move_Tick", 0.045f, WaveKind.Move);
        gravityShift = MakeClip("Gravity_Shift", 0.19f, WaveKind.Shift);
        switchActivate = MakeClip("Switch_Activate", 0.28f, WaveKind.Switch);
        death = MakeClip("Player_Death", 0.32f, WaveKind.Death);
        victory = MakeClip("Level_Victory", 0.78f, WaveKind.Victory);
    }

    enum WaveKind { Click, Move, Shift, Switch, Death, Victory }

    AudioClip MakeClip(string clipName, float duration, WaveKind kind)
    {
        int count = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / SampleRate;
            float normalized = t / duration;
            float envelope = Mathf.Clamp01(1f - normalized);
            envelope *= envelope;
            float frequency;
            float sample;

            switch (kind)
            {
                case WaveKind.Click:
                    frequency = Mathf.Lerp(1050f, 540f, normalized);
                    sample = Mathf.Sin(2f * Mathf.PI * frequency * t);
                    sample += 0.18f * Mathf.Sin(2f * Mathf.PI * frequency * 2.01f * t);
                    samples[i] = sample * envelope * 0.18f;
                    break;

                case WaveKind.Move:
                    frequency = Mathf.Lerp(510f, 320f, normalized);
                    sample = Mathf.Sin(2f * Mathf.PI * frequency * t);
                    samples[i] = sample * envelope * 0.12f;
                    break;

                case WaveKind.Shift:
                    frequency = Mathf.Lerp(145f, 650f, normalized);
                    sample = Mathf.Sin(2f * Mathf.PI * frequency * t);
                    sample += 0.24f * Mathf.Sin(2f * Mathf.PI * frequency * 2f * t);
                    sample += 0.08f * Mathf.Sin(2f * Mathf.PI * frequency * 3f * t);
                    samples[i] = sample * envelope * 0.25f;
                    break;

                case WaveKind.Switch:
                    frequency = normalized < 0.42f
                        ? Mathf.Lerp(420f, 540f, normalized / 0.42f)
                        : Mathf.Lerp(660f, 890f, (normalized - 0.42f) / 0.58f);
                    sample = Mathf.Sin(2f * Mathf.PI * frequency * t);
                    sample += 0.18f * Mathf.Sin(2f * Mathf.PI * frequency * 2f * t);
                    float switchPulse = Mathf.Clamp01(Mathf.Min(normalized * 12f + 0.25f, (1f - normalized) * 5f));
                    samples[i] = sample * switchPulse * 0.22f;
                    break;

                case WaveKind.Death:
                    frequency = Mathf.Lerp(330f, 68f, normalized);
                    sample = Mathf.Sin(2f * Mathf.PI * frequency * t);
                    sample += 0.35f * Mathf.Sin(2f * Mathf.PI * frequency * 0.5f * t);
                    float rasp = Mathf.Sin(2f * Mathf.PI * 37f * t) * Mathf.Sin(2f * Mathf.PI * 73f * t);
                    samples[i] = (sample * 0.65f + rasp * 0.20f) * envelope * 0.24f;
                    break;

                default: // Victory: a short three-note arpeggio, not background music.
                    float note = normalized < 0.34f ? 523.25f :
                                 normalized < 0.67f ? 659.25f : 783.99f;
                    float localT = normalized < 0.34f ? normalized / 0.34f :
                                   normalized < 0.67f ? (normalized - 0.34f) / 0.33f :
                                   (normalized - 0.67f) / 0.33f;
                    float noteEnvelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(localT));
                    sample = Mathf.Sin(2f * Mathf.PI * note * t);
                    sample += 0.15f * Mathf.Sin(2f * Mathf.PI * note * 2f * t);
                    samples[i] = sample * Mathf.Max(0.12f, noteEnvelope) * envelope * 0.19f;
                    break;
            }
        }

        AudioClip clip = AudioClip.Create(clipName, count, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    void Play(AudioClip clip, float volume)
    {
        if (source == null || clip == null) return;
        source.PlayOneShot(clip, volume);
    }

    public static void PlayUiClick() { if (instance != null) instance.Play(instance.uiClick, 0.48f); }
    public static void PlayMove() { if (instance != null) instance.Play(instance.moveTick, 0.30f); }
    public static void PlayGravityShift() { if (instance != null) instance.Play(instance.gravityShift, 0.72f); }
    public static void PlaySwitch() { if (instance != null) instance.Play(instance.switchActivate, 0.82f); }
    public static void PlayDeath() { if (instance != null) instance.Play(instance.death, 0.85f); }
    public static void PlayVictory() { if (instance != null) instance.Play(instance.victory, 0.9f); }

    public static void AddClickSounds(GameObject root)
    {
        if (root == null) return;
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
            AddClickSound(button);
    }

    public static void AddClickSound(Button button)
    {
        if (button == null || instance == null) return;
        int id = button.GetEntityId();
        if (!instance.wiredButtonIds.Add(id)) return;
        button.onClick.AddListener(PlayUiClick);
    }
}
