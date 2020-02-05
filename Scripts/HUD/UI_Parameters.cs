using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Parameters : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] Slider sliderMasterVolume;
    [SerializeField] Slider sliderMusicVolume;
    [SerializeField] Slider sliderSoundVolume;
    [SerializeField] Slider sliderAmbientVolume;
    [SerializeField] Slider sliderVoiceVolume;

    [Header("Graphism")]
    [SerializeField] int indexResolution = 0;
    [SerializeField] Transform ArrowSelectorResolution;
    [Space(6)]
    [SerializeField] int indexScreenmode = 0;
    [SerializeField] Transform ArrowSelectorScreenmode;
    [Space(6)]
    [SerializeField] int indexFPSLimit = 0;
    [SerializeField] Transform ArrowSelectorFPSLimit;
    [Space(6)]
    [SerializeField] int indexAntiAliasing = 0;
    [SerializeField] Transform ArrowSelectorAntiAliasing;
    [Space(6)]
    [SerializeField] int indexShadow = 0;
    [SerializeField] Transform ArrowSelectorShadow;
    [Space(6)]
    [SerializeField] int indexAO = 0;
    [SerializeField] Transform ArrowSelectorAO;

    private AudioManager audio;
    private string canalChanged;

    private ConfigManager config;
    private Config tempConfig;

    void Start()
    {
        audio = GameManager.Instance.Audio;

        config = GameManager.Instance.Config;

        ResetTempGraphismValue();
        RefreshSoundValue();
    }

    void Update()
    {
        UpdateVolume();
    }

    // -------------------------------------------- Sound -------------------------------------------- //

    public void RefreshSoundValue()
    {
        sliderMasterVolume.value = audio.GetVolume(AudioManager.Canal.Master);
        sliderMusicVolume.value = audio.GetVolume(AudioManager.Canal.Music);
        sliderSoundVolume.value = audio.GetVolume(AudioManager.Canal.SoundEffect);
        sliderAmbientVolume.value = audio.GetVolume(AudioManager.Canal.Ambient);
        sliderVoiceVolume.value = audio.GetVolume(AudioManager.Canal.Voice);
    }

    public void ChangeNewCanal(string _canalChanged)
    {
        canalChanged = _canalChanged;
    }

    private void UpdateVolume()
    {
        if (canalChanged == "Master")
            audio.SetVolume(sliderMasterVolume.value, AudioManager.Canal.Master);

        else if (canalChanged == "Music")
            audio.SetVolume(sliderMusicVolume.value, AudioManager.Canal.Music);

        else if (canalChanged == "Sound" || canalChanged == "SoundEffect")
            audio.SetVolume(sliderSoundVolume.value, AudioManager.Canal.SoundEffect);

        else if (canalChanged == "Ambient")
            audio.SetVolume(sliderAmbientVolume.value, AudioManager.Canal.Ambient);

        else if (canalChanged == "Voice")
            audio.SetVolume(sliderVoiceVolume.value, AudioManager.Canal.Voice);
    }


    // ------------------------------------------- Graphic ------------------------------------------- //

    // ------------ Tools

    public void ResetTempGraphismValue()
    {
        tempConfig = new Config(config.actualConfig);

        RefreshIndex();
        RefreshTempGraphismValue();
    }

    private void RefreshIndex()
    {
        foreach (KeyValuePair<int, Vector2Int> keyVP in config.mapPossibleResolution)
        {
            if (keyVP.Value == tempConfig.screenSize)
            {
                indexResolution = keyVP.Key;
            }
        }

        foreach (KeyValuePair<int, FullScreenMode> keyVP in config.mapPossibleScreenmode)
        {
            if (keyVP.Value == tempConfig.screenMode)
            {
                indexScreenmode = keyVP.Key;
            }
        }

        foreach (KeyValuePair<int, int> keyVP in config.mapPossibleFPSLimit)
        {
            if (keyVP.Value == tempConfig.FPSLimit)
            {
                indexFPSLimit = keyVP.Key;
            }
        }

        foreach (KeyValuePair<int, int> keyVP in config.mapPossibleAntiAliasing)
        {
            if (keyVP.Value == tempConfig.antiAliasing)
            {
                indexAntiAliasing = keyVP.Key;
            }
        }

        foreach (KeyValuePair<int, ShadowResolution> keyVP in config.mapPossibleShadow)
        {
            if (keyVP.Value == tempConfig.shadowResolution)
            {
                indexShadow = keyVP.Key;
            }
        }

        indexAO = tempConfig.ambientOcclusion == true ? 1 : 0;

    }

    public void RefreshTempGraphismValue()
    {
        RefreshIndex();

        ArrowSelectorResolution.GetChild(1).GetComponent<Text>().text = tempConfig.screenSize.x + "x" + tempConfig.screenSize.y;
        ArrowSelectorScreenmode.GetChild(1).GetComponent<Text>().text = GetStrForScreenMode();
        ArrowSelectorFPSLimit.GetChild(1).GetComponent<Text>().text = tempConfig.FPSLimit.ToString();
        ArrowSelectorAntiAliasing.GetChild(1).GetComponent<Text>().text = "x" + tempConfig.antiAliasing;
        ArrowSelectorShadow.GetChild(1).GetComponent<Text>().text = GetStrForShadow();
        ArrowSelectorAO.GetChild(1).GetComponent<Text>().text = GetStrForAO();
    }

    private void GetIndex(ref int _value, bool _next, int _lenghtCount)
    {
        // NEXT
        if (_next)
        {
            if (_value < _lenghtCount - 1)
                _value++;
            else
                _value = 0;
        }
        // PREVIOUS
        else
        {
            if (_value > 0)
                _value--;
            else
                _value = _lenghtCount - 1;
        }
    }

    private string GetStrForScreenMode()
    {
        string tmpStr = "";
        switch (indexScreenmode)
        {
            case 0:
                tmpStr = "Plein écran";
                break;
            case 1:
                tmpStr = "Plein écran fenêtré";
                break;
            case 2:
                tmpStr = "Fenêtré";
                break;
            default:
                break;
        }

        return tmpStr;
    }

    private string GetStrForShadow()
    {
        string tmpStr = "";
        switch (indexShadow)
        {
            case 3:
                tmpStr = "Ultra";
                break;
            case 2:
                tmpStr = "Elevé";
                break;
            case 1:
                tmpStr = "Moyen";
                break;
            case 0:
                tmpStr = "Faible";
                break;
            default:
                break;
        }

        return tmpStr;
    }

    private string GetStrForAO()
    {
        return indexAO == 1 ? "Activé" : "Desactivé";
    }

    // ------------ Buttons

    public void ApplyGraphicSetting()
    {
        config.actualConfig = tempConfig;
        config.SetConfig(tempConfig);
        Debug.Log("Apply new settings");
    }

    public void ButtonScreenResolution(bool _next)
    {
        GetIndex(ref indexResolution, _next, config.mapPossibleResolution.Count);

        tempConfig.screenSize = config.mapPossibleResolution[indexResolution];
        ArrowSelectorResolution.GetChild(1).GetComponent<Text>().text = tempConfig.screenSize.x + "x" + tempConfig.screenSize.y;
    }

    public void ButtonScreenmode(bool _next)
    {
        GetIndex(ref indexScreenmode, _next, config.mapPossibleScreenmode.Count);

        tempConfig.screenMode = config.mapPossibleScreenmode[indexScreenmode];
        ArrowSelectorScreenmode.GetChild(1).GetComponent<Text>().text = GetStrForScreenMode();

    }

    public void ButtonScreenFPSLimit(bool _next)
    {
        GetIndex(ref indexFPSLimit, _next, config.mapPossibleFPSLimit.Count);

        tempConfig.FPSLimit = config.mapPossibleFPSLimit[indexFPSLimit];
        ArrowSelectorFPSLimit.GetChild(1).GetComponent<Text>().text = tempConfig.FPSLimit.ToString();

    }

    public void ButtonScreenAntiAliasing(bool _next)
    {
        GetIndex(ref indexAntiAliasing, _next, config.mapPossibleAntiAliasing.Count);

        tempConfig.antiAliasing = config.mapPossibleAntiAliasing[indexAntiAliasing];
        ArrowSelectorAntiAliasing.GetChild(1).GetComponent<Text>().text = "x" + tempConfig.antiAliasing.ToString();
    }

    public void ButtonScreenShadow(bool _next)
    {
        GetIndex(ref indexShadow, _next, config.mapPossibleShadow.Count);

        tempConfig.shadowResolution = config.mapPossibleShadow[indexShadow];

        ArrowSelectorShadow.GetChild(1).GetComponent<Text>().text = GetStrForShadow();
    }

    public void ButtonScreenAO(bool _next)
    {
        GetIndex(ref indexAO, _next, 2);

        tempConfig.ambientOcclusion = indexAO == 1 ? true : false;

        ArrowSelectorAO.GetChild(1).GetComponent<Text>().text = GetStrForAO();
    }

    public void ButtonPreset(string _preset)
    {
        ConfigManager.ConfigQuality presetChosen = ConfigManager.ConfigQuality.Low;

        if (_preset == "Low")
            presetChosen = ConfigManager.ConfigQuality.Low;
        else if (_preset == "Medium")
            presetChosen = ConfigManager.ConfigQuality.Medium;
        else if (_preset == "High")
            presetChosen = ConfigManager.ConfigQuality.Ultra;
        else
            Debug.LogWarning("Wrong string, abort mission!");


        indexShadow = (int)config.mapConfigs[presetChosen].shadowResolution;
        tempConfig.shadowResolution = config.mapConfigs[presetChosen].shadowResolution;
        tempConfig.antiAliasing = config.mapConfigs[presetChosen].antiAliasing;
        tempConfig.FPSLimit = config.mapConfigs[presetChosen].FPSLimit;
        tempConfig.ambientOcclusion = config.mapConfigs[presetChosen].ambientOcclusion;

        RefreshTempGraphismValue();

    }
}
