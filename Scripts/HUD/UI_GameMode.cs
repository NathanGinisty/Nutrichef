using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameMode : MonoBehaviour
{
    //[Header("Game Mode")]
    [Header("Maps")]
    [SerializeField] int indexMap = 0;
    [SerializeField] Transform[] mapButton;

    void Start()
    {

    }

    // --------------------------------- PRIVATE METHODS --------------------------------- //


    // --------------------------------- PUBLIC METHODS ---------------------------------- //

    public void ChooseGamemode(string _gamemode)
    {
        if (_gamemode == "MultiContruction")
            GameManager.Instance.RoomConfig.gameMode = RoomConfigManager.GameMode.MultiContruction;
        else if (_gamemode == "SoloContruction")
            GameManager.Instance.RoomConfig.gameMode = RoomConfigManager.GameMode.SoloContruction;
        else if (_gamemode == "QuickGame")
            GameManager.Instance.RoomConfig.gameMode = RoomConfigManager.GameMode.QuickGame;

        else
        {
            GameManager.Instance.RoomConfig.gameMode = RoomConfigManager.GameMode.MultiContruction;
            Debug.Log("NULL STRING VALUE !!!");
        }
    }

}
