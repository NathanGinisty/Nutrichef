using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomConfigManager : MonoBehaviour
{
    public enum GameMode
    {
        MultiContruction,
        SoloContruction,
        QuickGame
    }

    public GameMode gameMode = GameMode.MultiContruction;
    public int levelSelected = 1;
    
}
