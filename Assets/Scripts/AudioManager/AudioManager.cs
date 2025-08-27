using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(.1f, 3f)] public float pitch = 1f;
        public bool loop;

        [HideInInspector] public AudioSource source;
    }

    public List<Sound> sounds;
    private Dictionary<string, Sound> soundDict;

    private AudioSource bgmSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        soundDict = new Dictionary<string, Sound>();
        foreach (var s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            soundDict[s.name] = s;
        }

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
    }

    public void PlayBGM(string name)
    {
        if (!soundDict.ContainsKey(name))
        {
            Debug.LogWarning("BGM not found: " + name);
            return;
        }

        Sound s = soundDict[name];
        bgmSource.clip = s.clip;
        bgmSource.volume = s.volume;
        bgmSource.pitch = s.pitch;
        bgmSource.loop = true;
        bgmSource.Play();

        Debug.Log("[AudioManager] BGM playing: " + name);
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void Play(string name)
    {
        if (soundDict.ContainsKey(name))
        {
            soundDict[name].source.Play();
        }
        else
        {
            Debug.LogWarning($"AudioManager: no audio {name}");
        }
    }

    public void PlayOneShot(string name)
    {
        if (soundDict.ContainsKey(name))
        {
            soundDict[name].source.PlayOneShot(soundDict[name].clip);
        }
    }

    public void PlayRandomByPrefix(string prefix)
    {
        var matches = sounds.FindAll(s => s.name.StartsWith(prefix));
        if (matches.Count == 0)
        {
            Debug.LogWarning("no sound name " + prefix);
            return;
        }

        int index = Random.Range(0, matches.Count);
        Play(matches[index].name);
    }

    public void Stop(string name)
    {
        if (soundDict.ContainsKey(name))
        {
            soundDict[name].source.Stop();
        }
    }
}
