using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;

    public AudioManager Audio { get; private set; }
    public PopUpManager PopUp { get; private set; }
    public ScoreManager Score { get; private set; }
    public ConfigManager Config { get; private set; }
    public RoomConfigManager RoomConfig { get; private set; }
    public GameSceneManager GameSceneManager { get; private set; }

    public InGameMenu InGameMenu;

    public delegate void InitScripts();
    public InitScripts initScripts;

    public bool autoLaunch = true;
    public bool simultaneousBuild = true;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;

            Audio = GetComponentInChildren<AudioManager>();
            PopUp = GetComponentInChildren<PopUpManager>();
            Score = GetComponentInChildren<ScoreManager>();
            Config = GetComponentInChildren<ConfigManager>();
            RoomConfig = GetComponentInChildren<RoomConfigManager>();
            GameSceneManager = GetComponentInChildren<GameSceneManager>();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        initScripts += Init;

        /*if you launch directly in scene autoLaunch is by default true*/
        if (autoLaunch)
        {
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.InitPool();
            }
            initScripts.Invoke();
        }
    }

    public void Init()
    {
        LockMouse();
    }

    public void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void FreeMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
