using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Config
{
    public Vector2Int screenSize;
    public FullScreenMode screenMode;
    public int FPSLimit;

    /// <summary> 0, 2, 4, or 8 </summary>
    public int antiAliasing;
    public ShadowResolution shadowResolution;
    public bool ambientOcclusion;

    public Config()
    {
        screenSize = new Vector2Int(1920, 1080);
        screenMode = FullScreenMode.FullScreenWindow;
        FPSLimit = 144;
        antiAliasing = 8;
        shadowResolution = ShadowResolution.VeryHigh;
        ambientOcclusion = true;

        //Debug.Log("new config(): " + FPSLimit);
    }

    public Config(Config _config)
    {
        screenSize = _config.screenSize;
        screenMode = _config.screenMode;
        FPSLimit = _config.FPSLimit;
        antiAliasing = _config.antiAliasing;
        shadowResolution = _config.shadowResolution;
        ambientOcclusion = _config.ambientOcclusion;
    }

    public Config(Vector2Int _screenSize, FullScreenMode _screenMode, int _fps, int _aa, ShadowResolution _sr, bool _ao)
    {
        screenSize = _screenSize;
        screenMode = _screenMode;
        FPSLimit = _fps;
        antiAliasing = _aa;
        shadowResolution = _sr;
        ambientOcclusion = _ao;
    }
}