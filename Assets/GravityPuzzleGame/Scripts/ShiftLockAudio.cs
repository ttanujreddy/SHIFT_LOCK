using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Runtime-generated one-shot audio. Six short mono clips are synthesized at 22.05 kHz, cached, and played through one persistent non-spatial AudioSource. No external files or background music are used.</summary>
public sealed class ShiftLockAudio : MonoBehaviour
{
    /// <summary>Sample rate used by all generated clips.</summary>
    const int SampleRate = 22050;
    static ShiftLockAudio instance;

    AudioSource source;
    AudioClip uiClick;
    AudioClip moveTick;
    AudioClip gravityShift;
    AudioClip switchActivate;
    AudioClip death;
    AudioClip victory;
    readonly HashSet<UnityEngine.EntityId> wiredButtonIds = new HashSet<UnityEngine.EntityId>();

    /// <summary>Creates the persistent sound service once so menu and gameplay share one source and clip cache.</summary>
    public static void EnsureExists()
    {
        if (instance != null) return;
        GameObject audioObject = new GameObject("SHIFT_LOCK_Audio");
        instance = audioObject.AddComponent<ShiftLockAudio>();
    }

    /// <summary>Enforces the singleton, survives scene changes, configures playback, and synthesizes the effects.</summary>
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
        death = MakeClip("Player_Death", 0.42f, WaveKind.Death);
        victory = MakeClip("Level_Victory", 0.84f, WaveKind.Victory);
    }

    /// <summary>Waveform recipes used by the clip synthesizer.</summary>
    enum WaveKind { Click, Move, Shift, Switch, Death, Victory }

    /// <summary>Generates PCM samples with oscillators/envelopes and stores them in an AudioClip.</summary>
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
                    // A clearly audible falling alarm: descending tones with a stronger transient.
                    frequency = Mathf.Lerp(560f, 105f, normalized);
                    sample = Mathf.Sin(2f * Mathf.PI * frequency * t);
                    sample += 0.42f * Mathf.Sin(2f * Mathf.PI * frequency * 1.48f * t);
                    float rasp = Mathf.Sin(2f * Mathf.PI * 39f * t) * Mathf.Sin(2f * Mathf.PI * 81f * t);
                    float deathPulse = 0.70f + 0.30f * Mathf.Sin(2f * Mathf.PI * 7f * t);
                    samples[i] = (sample * 0.72f + rasp * 0.32f) * envelope * deathPulse * 0.42f;
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

    /// <summary>Plays a cached effect at the requested event volume.</summary>
    void Play(AudioClip clip, float volume)
    {
        if (source == null || clip == null) return;
        source.PlayOneShot(clip, volume);
    }

    public static void PlayUiClick() { if (instance != null) instance.Play(instance.uiClick, 0.48f); }
    public static void PlayMove() { if (instance != null) instance.Play(instance.moveTick, 0.30f); }
    public static void PlayGravityShift() { if (instance != null) instance.Play(instance.gravityShift, 0.72f); }
    public static void PlaySwitch() { if (instance != null) instance.Play(instance.switchActivate, 0.82f); }
    public static void PlayDeath() { if (instance != null) instance.Play(instance.death, 1.0f); }
    public static void PlayVictory() { if (instance != null) instance.Play(instance.victory, 1.0f); }

    /// <summary>Hooks the click cue to every Button below a root, including inactive descendants.</summary>
    public static void AddClickSounds(GameObject root)
    {
        if (root == null) return;
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
            AddClickSound(button);
    }

    /// <summary>Hooks one button once, using its Unity entity ID to avoid duplicate click listeners.</summary>
    public static void AddClickSound(Button button)
    {
        if (button == null || instance == null) return;
        UnityEngine.EntityId id = button.GetEntityId();
        if (!instance.wiredButtonIds.Add(id)) return;
        button.onClick.AddListener(PlayUiClick);
    }
}
