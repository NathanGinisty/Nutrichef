using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ColdRoom_Button : MonoBehaviour
{
    public Poolable foodPrefab { get; set; }
    PlayerController player;
    Rigidbody handPlayer;
    ColdRoom coldRoomScript;
    ColdRoomUI ui;
    Button button;

    string text_ObjectAlreadyInHand;
    Vector2 posText_ObjectAlreadyInHand;


    public void Init(ColdRoomUI _ui, Sprite _sprite)
    {
        ui = _ui;
        button = GetComponent<Button>();
        GetComponent<Image>().sprite = _sprite;

        button.onClick.AddListener(delegate { ui.SetSelectedButton(this); });
    }

    public void DisplayChoiceMenu()
    {
            ui.DisplayChoiceMenu(this);
    }
}
