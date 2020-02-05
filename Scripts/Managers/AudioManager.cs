using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [SerializeField] [Range(0, 1)] float volumeMaster = 1f;
    [SerializeField] [Range(0, 1)] float volumeMusic = 1f;
    [SerializeField] [Range(0, 1)] float volumeSoundEffect = 1f;
    [SerializeField] [Range(0, 1)] float volumeAmbient = 1f;
    [SerializeField] [Range(0, 1)] float volumeVoice = 1f;
    [Header("--------------------")]
    [SerializeField] [Range(0, 1)] float progressionMusic = 1f;
    [SerializeField] int nbInScene = 0;

    private GameObject childMusic;
    private GameObject childSound;

    private Dictionary<Canal, List<AudioSource>> mapSources;

    private AudioMixer mixer;

    private Dictionary<string, AudioClip> mapSoundBank;
    private Dictionary<string, AudioClip> mapMusicBank;

    /// <summary>
    /// key is SceneName
    /// </summary>
    private Dictionary<string, List<AudioClip>> playlistMusic;

    private bool isPlaylistPaused = false;

    private float savedMusicVolume = 0f;

    public enum Canal
    {
        None = -1,
        Master,
        Music,
        SoundEffect,
        Ambient,
        Voice
    }

    private void Start()
    {
        // Get The Child GameObject
        childMusic = transform.GetChild(0).gameObject;
        childSound = transform.GetChild(1).gameObject;

        // Init The Mixer
        InitAudioMixer();

        // Get/Add The Sources
        InitSourceList();

        // Init the SoundBank
        InitSoundBank();
        InitMusicBank();

        // Init the Playlist
        playlistMusic = new Dictionary<string, List<AudioClip>>();

        volumeAmbient = 0f;

    }

    private void Update()
    {
        //Cheat();

        UpdateMusicLoop();

        // temp update for testing
        //TestingZone();

        CleanSoundSources();
        UpdateVolume();
        CountNbInScene();
    }

    #region PRIVATE METHODS
    // --------------------------------------------- Private Methods -------------------------------------------- //

    private void Cheat()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            string sceneName = SceneManager.GetActiveScene().name;
            Playlist_Next(sceneName);
        }
    }

    private void InitAudioMixer()
    {
        mixer = Resources.Load<AudioMixer>("Mixers/General");
    }

    private void InitSourceList()
    {
        mapSources = new Dictionary<Canal, List<AudioSource>>();

        mapSources[Canal.Master] = new List<AudioSource>();
        mapSources[Canal.Music] = new List<AudioSource>();
        mapSources[Canal.SoundEffect] = new List<AudioSource>();
        mapSources[Canal.Ambient] = new List<AudioSource>();
        mapSources[Canal.Voice] = new List<AudioSource>();

        mapSources[Canal.Music].Add(childMusic.GetComponent<AudioSource>());

        if (mapSources[Canal.Music][0] == null) //Avoid Crash
        {
            mapSources[Canal.Music][0] = childMusic.AddComponent<AudioSource>();
            mapSources[Canal.Music][0].outputAudioMixerGroup = mixer.FindMatchingGroups("Music")[0];
        }
    }

    private void InitSoundBank()
    {
        mapSoundBank = new Dictionary<string, AudioClip>();

        foreach (AudioClip tmp in Resources.LoadAll<AudioClip>("Sounds"))
        {
            mapSoundBank.Add(tmp.name, tmp);
        }
    }

    private void InitMusicBank()
    {
        mapMusicBank = new Dictionary<string, AudioClip>();

        foreach (AudioClip tmp in Resources.LoadAll<AudioClip>("Musics"))
        {
            mapMusicBank.Add(tmp.name, tmp);
        }
    }

    private void ChangeVolumeInSources(Canal canal)
    {
        float tmpVolume = 1;

        switch (canal)
        {
            case Canal.Master:
                tmpVolume = volumeMaster;
                break;
            case Canal.Music:
                tmpVolume = volumeMusic * volumeMaster;
                break;
            case Canal.SoundEffect:
                tmpVolume = volumeSoundEffect * volumeMaster;
                break;
            case Canal.Ambient:
                tmpVolume = volumeAmbient * volumeMaster;
                break;
            case Canal.Voice:
                tmpVolume = volumeVoice * volumeMaster;
                break;
            default:
                Debug.LogError("SetVolume() Error : wrong canal");
                break;
        }

        mapSources[canal].Where(x => x != null).ToList().ForEach(x => x.volume = tmpVolume);
    }

    /// <summary>
    /// Forced update, only for testing when change are made since the inspector.
    /// </summary>
    private void UpdateVolume()
    {
        mapSources[Canal.Master].Where(x => x != null).ToList().ForEach(x => x.volume = volumeMaster);
        mapSources[Canal.Music].Where(x => x != null).ToList().ForEach(x => x.volume = volumeMusic * volumeMaster);
        mapSources[Canal.SoundEffect].Where(x => x != null).ToList().ForEach(x => x.volume = volumeSoundEffect * volumeMaster);
        mapSources[Canal.Ambient].Where(x => x != null).ToList().ForEach(x => x.volume = volumeAmbient * volumeMaster);
        mapSources[Canal.Voice].Where(x => x != null).ToList().ForEach(x => x.volume = volumeVoice * volumeMaster);
    }

    private void CleanSoundSources()
    {
        foreach (KeyValuePair<Canal, List<AudioSource>> pair in mapSources)
        {
            pair.Value.RemoveAll(x => x as AudioSource == null);
        }
    }

    private void CountNbInScene()
    {
        nbInScene = 0;
        var arrayKeys = mapSources.Keys.ToArray();
        for (int i = 0; i < arrayKeys.Length; i++)
        {
            nbInScene += mapSources[arrayKeys[i]].Count();
        }
    }
    #endregion

    #region GLOBAL TOOLS
    /// <summary>
    /// Verify is the soundEffect actually exist
    /// </summary>
    public bool Exist(string _soundName)
    {
        return mapSoundBank.ContainsKey(_soundName);
    }

    /// <summary>
    /// Set Volume of AudioManager.
    /// </summary>
    public void SetVolume(float value, Canal canal)
    {
        switch (canal)
        {
            case Canal.Master:
                volumeMaster = value;
                break;
            case Canal.Music:
                volumeMusic = value;
                break;
            case Canal.SoundEffect:
                volumeSoundEffect = value;
                break;
            case Canal.Ambient:
                volumeAmbient = value;
                break;
            case Canal.Voice:
                volumeVoice = value;
                break;
            default:
                Debug.LogError("SetVolume() Error : wrong canal");
                break;
        }

        ChangeVolumeInSources(canal);
    }

    /// <summary>
    /// Get Volume of AudioManager.
    /// </summary>
    public float GetVolume(Canal canal)
    {
        switch (canal)
        {
            case Canal.Master:
                return volumeMaster;
            case Canal.Music:
                return volumeMusic;
            case Canal.SoundEffect:
                return volumeSoundEffect;
            case Canal.Ambient:
                return volumeAmbient;
            case Canal.Voice:
                return volumeVoice;
            default:
                Debug.LogError("GetVolume() Error : wrong canal");
                break;
        }

        return 0;
    }


    /// <summary>
    /// Pause/UnPause every sounds.
    /// </summary>
    public void PauseAll(bool isPaused)
    {
        if (isPaused)
        {
            foreach (KeyValuePair<Canal, List<AudioSource>> pair in mapSources)
            {
                pair.Value.Where(x => x as AudioSource == null).ToList().ForEach(x => x.Pause());
            }
        }
        else
        {
            foreach (KeyValuePair<Canal, List<AudioSource>> pair in mapSources)
            {
                pair.Value.Where(x => x as AudioSource == null).ToList().ForEach(x => x.UnPause());
            }
        }
    }

    /// <summary>
    /// Pause/UnPause the music.
    /// </summary>
    public void Pause(bool isPaused, Canal canal)
    {
        if (isPaused)
        {
            mapSources[canal].Where(x => x != null).ToList().ForEach(x => x.Pause());
        }
        else
        {
            mapSources[canal].Where(x => x != null).ToList().ForEach(x => x.UnPause());
        }

        if (canal == Canal.Music)
        {
            isPlaylistPaused = isPaused;
        }
    }

    #endregion

    #region SOUNDS

    /// <summary>
    /// With coroutine
    /// </summary>
    public IEnumerator PlaySoundLowerMusic(string _soundName, Canal _canal)
    {
        AudioSource audioSource = childSound.AddComponent<AudioSource>();
        audioSource.clip = mapSoundBank[_soundName];

        switch (_canal)
        {
            case Canal.Music:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Music")[0];
                audioSource.volume = volumeMusic * volumeMaster;
                break;
            case Canal.SoundEffect:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SoundEffect")[0];
                audioSource.volume = volumeSoundEffect * volumeMaster;
                break;
            case Canal.Ambient:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Ambient")[0];
                audioSource.volume = volumeAmbient * volumeMaster;
                break;
            case Canal.Voice:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Voice")[0];
                audioSource.volume = volumeVoice * volumeMaster;
                break;
            default:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
                audioSource.volume = volumeSoundEffect * volumeMaster;
                break;
        }

        audioSource.Play();

        mapSources[_canal].Add(audioSource);
        savedMusicVolume = volumeMusic;
        Pause(true, Canal.Music);

        yield return new WaitForSeconds(mapSoundBank[_soundName].length);
        Destroy(audioSource);
        Pause(false, Canal.Music);
    }

    /// <summary>
    /// No coroutine
    /// </summary>
    //public AudioSource PlaySoundLowerMusic(string _soundName, Canal _canal, GameObject go = null)
    //{
    //    AudioSource audioSource = go == null ? childSound.AddComponent<AudioSource>() : go.AddComponent<AudioSource>();
    //    audioSource.clip = mapSoundBank[_soundName];

    //    switch (_canal)
    //    {
    //        case Canal.Music:
    //            audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Music")[0];
    //            audioSource.volume = volumeMusic * volumeMaster;
    //            break;
    //        case Canal.SoundEffect:
    //            audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SoundEffect")[0];
    //            audioSource.volume = volumeSoundEffect * volumeMaster;
    //            break;
    //        case Canal.Ambient:
    //            audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Ambient")[0];
    //            audioSource.volume = volumeAmbient * volumeMaster;
    //            break;
    //        case Canal.Voice:
    //            audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Voice")[0];
    //            audioSource.volume = volumeVoice * volumeMaster;
    //            break;
    //        default:
    //            audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
    //            audioSource.volume = volumeSoundEffect * volumeMaster;
    //            break;
    //    }

    //    audioSource.Play();

    //    mapSources[_canal].Add(audioSource);
    //    Destroy(audioSource, mapSoundBank[_soundName].length);

    //    return audioSource;
    //}


    /// <summary>
    /// Add AudioSource to AudioManager/Sound to play an audioclip from the soundBank, it is advisable to use one of the overloads. 
    /// </summary>
    public void PlaySound(string soundName)
    {
        AudioSource audioSource = childSound.AddComponent<AudioSource>();
        audioSource.clip = mapSoundBank[soundName];
        audioSource.volume = volumeSoundEffect * volumeMaster;
        audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
        audioSource.Play();

        mapSources[Canal.SoundEffect].Add(audioSource);
        Destroy(audioSource, mapSoundBank[soundName].length);
    }

    /// <summary>
    /// Add AudioSource to AudioManager/Sound to play an audioclip from the soundBank, with the choice of the AudioMixer. 😂🎸🎶
    /// </summary>
    public void PlaySound(string soundName, Canal canal)
    {
        AudioSource audioSource = childSound.AddComponent<AudioSource>();
        audioSource.clip = mapSoundBank[soundName];

        switch (canal)
        {
            case Canal.Music:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Music")[0];
                audioSource.volume = volumeMusic * volumeMaster;
                break;
            case Canal.SoundEffect:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SoundEffect")[0];
                audioSource.volume = volumeSoundEffect * volumeMaster;
                break;
            case Canal.Ambient:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Ambient")[0];
                audioSource.volume = volumeAmbient * volumeMaster;
                break;
            case Canal.Voice:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Voice")[0];
                audioSource.volume = volumeVoice * volumeMaster;
                break;
            default:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
                audioSource.volume = volumeSoundEffect * volumeMaster;
                break;
        }

        audioSource.Play();

        mapSources[canal].Add(audioSource);
        Destroy(audioSource, mapSoundBank[soundName].length);
    }

    /// <summary>
    /// Add AudioSource on the GameObject to play an audioclip from the soundBank, no choice of AudioMixer. 
    /// </summary>
    public void PlaySound(string soundName, GameObject go)
    {
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.clip = mapSoundBank[soundName];
        audioSource.volume = volumeSoundEffect * volumeMaster;
        audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
        audioSource.Play();

        mapSources[Canal.SoundEffect].Add(audioSource);
        Destroy(audioSource, mapSoundBank[soundName].length);
    }

    /// <summary>
    /// Add AudioSource on the GameObject to play an audioclip from the soundBank, with the choice of the AudioMixer.
    /// </summary>
    public void PlaySound(string soundName, Canal canal, GameObject go)
    {
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.clip = mapSoundBank[soundName];

        switch (canal)
        {
            case Canal.Music:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Music")[0];
                audioSource.volume = volumeMusic * volumeMaster;
                break;
            case Canal.SoundEffect:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SoundEffect")[0];
                audioSource.volume = volumeSoundEffect * volumeMaster;
                break;
            case Canal.Ambient:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Ambient")[0];
                audioSource.volume = volumeAmbient * volumeMaster;
                break;
            case Canal.Voice:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Voice")[0];
                audioSource.volume = volumeVoice * volumeMaster;
                break;
            default:
                audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
                audioSource.volume = volumeSoundEffect * volumeMaster;
                break;
        }

        audioSource.Play();

        mapSources[canal].Add(audioSource);
        Destroy(audioSource, mapSoundBank[soundName].length);
    }
    #endregion

    #region MUSIC
    /// <summary>
    /// Create AudioSource on AudioManager/Music to play an audioclip from the musicBank. 
    /// </summary>
    public void PlayMusic(string musicName)
    {
        mapSources[Canal.Music][0].clip = mapMusicBank[musicName];
        mapSources[Canal.Music][0].volume = volumeMusic * volumeMaster;
        mapSources[Canal.Music][0].loop = false;
        mapSources[Canal.Music][0].Play();
    }

    /// <summary>
    /// Create AudioSource on AudioManager/Music to play an audioclip from the musicBanks.
    /// If loop is true, it will desactivate the music list loop. Only this music will be played.
    /// </summary>
    public void PlayMusic(string musicName, bool loop)
    {
        mapSources[Canal.Music][0].clip = mapMusicBank[musicName];
        mapSources[Canal.Music][0].volume = volumeMusic * volumeMaster;
        mapSources[Canal.Music][0].loop = loop;
        mapSources[Canal.Music][0].Play();
    }
    #endregion

    #region PLAYLIST
    /// <summary>
    /// Add an AudioClip on the Music Loop List.
    /// </summary>
    public void Playlist_AddMusic(string _sceneName, string _soundName)
    {
        // if not exist then create
        if (!playlistMusic.ContainsKey(_sceneName))
        {
            List<AudioClip> newList = new List<AudioClip>();
            playlistMusic.Add(_sceneName, newList);
        }

        playlistMusic[_sceneName].Add(mapMusicBank[_soundName]);
    }

    /// <summary>
    /// Go to next music (actually random).
    /// </summary>
    public void Playlist_Next(string _sceneName)
    {
        if (playlistMusic.ContainsKey(_sceneName))
        {
            int rand = Random.Range(0, playlistMusic[_sceneName].Count);
            mapSources[Canal.Music][0].clip = playlistMusic[_sceneName].GetRange(rand, playlistMusic[_sceneName].Count).ToArray()[0];
            mapSources[Canal.Music][0].volume = volumeMusic;
            mapSources[Canal.Music][0].loop = false;
            mapSources[Canal.Music][0].Play();
        }
    }

    private void UpdateMusicLoop()
    {
        if (mapSources[Canal.Music][0] != null)
        {
            if (mapSources[Canal.Music][0].clip != null)
            {
                progressionMusic = mapSources[Canal.Music][0].time / mapSources[Canal.Music][0].clip.length;
            }

            if (!mapSources[Canal.Music][0].isPlaying && !isPlaylistPaused)
            {
                string sceneName = SceneManager.GetActiveScene().name;
                Playlist_Next(sceneName);
            }
        }
    }

    #endregion
}
