using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent background music player that survives scene transitions.
/// Owns an AudioListener so audio output works in every scene.
/// Place on a GameObject in the first scene (e.g. LobbyScene).
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music Tracks (played in order, looping)")]
    public AudioClip[] tracks;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float volume = 0.4f;

    private AudioSource _source;
    private int _currentTrack;
    private bool _playing;
    private string[] _trackResourcePaths;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Persistent AudioListener — scene cameras may also have one,
        // duplicates are cleaned up in OnSceneLoaded.
        if (GetComponent<AudioListener>() == null)
            gameObject.AddComponent<AudioListener>();

        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = false;
        _source.volume = volume;
        _source.spatialBlend = 0f;
        _source.ignoreListenerPause = true;

        // Cache resource paths so we can reload clips if references are lost
        if (tracks != null)
        {
            _trackResourcePaths = new string[tracks.Length];
            for (int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i] != null)
                    _trackResourcePaths[i] = "Audio/" + tracks[i].name;
            }
        }

        if (tracks != null && tracks.Length > 0)
            PlayTrack(0);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this)
            Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Remove duplicate AudioListeners from scene cameras
        var listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        foreach (var listener in listeners)
        {
            if (listener != null && listener.gameObject != gameObject)
                Destroy(listener);
        }

        // Resume music if the scene transition interrupted it
        if (_source != null && _playing && !_source.isPlaying)
        {
            if (_source.clip != null)
            {
                if (_source.time > 0f)
                    _source.UnPause();
                else
                    PlayTrack(_currentTrack);
            }
            else
            {
                ReloadAndPlay();
            }
        }
    }

    private void Update()
    {
        if (_source == null || tracks == null || tracks.Length == 0 || !_playing)
            return;

        if (_source.clip == null)
        {
            ReloadAndPlay();
            return;
        }

        if (!_source.isPlaying)
        {
            if (_source.time > 0f)
            {
                _source.UnPause();
            }
            else
            {
                _currentTrack = (_currentTrack + 1) % tracks.Length;
                PlayTrack(_currentTrack);
            }
        }
    }

    private void PlayTrack(int index)
    {
        if (tracks == null || index < 0 || index >= tracks.Length)
            return;

        // Try to reload from Resources if the reference was lost
        if (tracks[index] == null)
        {
            if (_trackResourcePaths != null && index < _trackResourcePaths.Length
                && _trackResourcePaths[index] != null)
            {
                tracks[index] = Resources.Load<AudioClip>(_trackResourcePaths[index]);
            }

            if (tracks[index] == null)
            {
                Debug.LogWarning($"[MusicManager] Track {index} is null, skipping.");
                return;
            }
        }

        _currentTrack = index;
        _source.clip = tracks[index];
        _source.volume = volume;
        _source.Play();
        _playing = true;
    }

    private void ReloadAndPlay()
    {
        if (_trackResourcePaths == null || tracks == null) return;

        for (int i = 0; i < tracks.Length && i < _trackResourcePaths.Length; i++)
        {
            if (tracks[i] == null && _trackResourcePaths[i] != null)
                tracks[i] = Resources.Load<AudioClip>(_trackResourcePaths[i]);
        }

        if (_currentTrack < tracks.Length && tracks[_currentTrack] != null)
            PlayTrack(_currentTrack);
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (_source != null)
            _source.volume = volume;
    }
}
