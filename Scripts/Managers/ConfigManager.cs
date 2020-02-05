using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigManager : MonoBehaviour
{
    public enum ConfigQuality
    {
        Low,
        Medium,
        Ultra
    }

    [HideInInspector] public Config actualConfig = new Config();
    public Dictionary<ConfigQuality, Config> mapConfigs;

    //  List of preset information for the UI
    public Dictionary<int, Vector2Int> mapPossibleResolution;
    public Dictionary<int, FullScreenMode> mapPossibleScreenmode;
    public Dictionary<int, int> mapPossibleFPSLimit;
    public Dictionary<int, int> mapPossibleAntiAliasing;
    public Dictionary<int, ShadowResolution> mapPossibleShadow;
    // Pas de dictionnaire, c'est juste un bool pour l'ambient occlusion

    void Start()
    {
        InitActualConfig();
        InitConfig();
        //SetConfig(actualConfig);

        InitListSettings();
    }

    private void Update()
    {
        //Debug.Log("FPSLIMIT : " + actualConfig.FPSLimit);
    }

    // --------------------------------------------- Private Methods --------------------------------------------- //

    // Get the config from the first start
    private void InitActualConfig()
    {
        //actualConfig = new Config();
        actualConfig.antiAliasing = QualitySettings.antiAliasing;
        actualConfig.shadowResolution = QualitySettings.shadowResolution;
        actualConfig.FPSLimit = 144;
        actualConfig.screenMode = Screen.fullScreenMode;
        actualConfig.screenSize = new Vector2Int(Screen.width, Screen.height);

        //FileManager.SaveConfig("actual.conf", actualConfig);
    }

    private void InitConfig()
    {
        // actual config will be accessible for the player
        // D:\SVN\2018-2019\Projets\ProjetAnnee2\Nutrichef\Prog\Nutrichef\Data\Config\actual.conf
        //actualConfig = FileManager.LoadConfig("actual.conf");

        // presets
        mapConfigs = FileManager.LoadPresetConfig("config.json");
    }

    // For UI
    private void InitListSettings()
    {
        // Resolution
        mapPossibleResolution = new Dictionary<int, Vector2Int>();
        mapPossibleResolution.Add(0, new Vector2Int(1920, 1080));
        mapPossibleResolution.Add(1, new Vector2Int(1600, 900));
        mapPossibleResolution.Add(2, new Vector2Int(1366, 768));
        mapPossibleResolution.Add(3, new Vector2Int(1360, 768));

        // ScreenMode
        mapPossibleScreenmode = new Dictionary<int, FullScreenMode>();
        mapPossibleScreenmode.Add(0, FullScreenMode.FullScreenWindow);
        mapPossibleScreenmode.Add(1, FullScreenMode.MaximizedWindow);
        mapPossibleScreenmode.Add(2, FullScreenMode.Windowed);

        // FPSLimit
        mapPossibleFPSLimit = new Dictionary<int, int>();
        mapPossibleFPSLimit.Add(0, 30);
        mapPossibleFPSLimit.Add(1, 60);
        mapPossibleFPSLimit.Add(2, 90);
        mapPossibleFPSLimit.Add(3, 120);
        mapPossibleFPSLimit.Add(4, 144);

        // AntiAliasing
        mapPossibleAntiAliasing = new Dictionary<int, int>();
        mapPossibleAntiAliasing.Add(0, 2);
        mapPossibleAntiAliasing.Add(1, 4);
        mapPossibleAntiAliasing.Add(2, 8);

        // ShadowResolution
        mapPossibleShadow = new Dictionary<int, ShadowResolution>();
        mapPossibleShadow.Add(0, ShadowResolution.Low);
        mapPossibleShadow.Add(1, ShadowResolution.Medium);
        mapPossibleShadow.Add(2, ShadowResolution.High);
        mapPossibleShadow.Add(3, ShadowResolution.VeryHigh);
    }

    // Debug
    void ShowConfigOnConsole()
    {
        foreach (KeyValuePair<ConfigQuality, Config> pair in mapConfigs)
        {
            Debug.Log("[ " + pair.Key + " ] antiAliasing: " + pair.Value.antiAliasing);
        }
    }

    // --------------------------------------------- Public Methods --------------------------------------------- //

    /// <summary>
    /// Set config.
    /// </summary>
    public void SetConfig(Config config)
    {
        Screen.SetResolution(config.screenSize.x, config.screenSize.y, config.screenMode, config.FPSLimit);
        QualitySettings.antiAliasing = config.antiAliasing;
        QualitySettings.shadowResolution = config.shadowResolution;
    }
}
