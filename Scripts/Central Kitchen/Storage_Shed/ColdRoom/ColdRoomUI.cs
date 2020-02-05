using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class ColdRoomUI : MonoBehaviour
{
    public ColdRoom_Button selectedButton;

    [SerializeField] Object buttonPrefab; // button click
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Transform buttonsContent; // scrollBar Content
    [SerializeField] GameObject choiceMenu; // button take 1 food  or remove food from CorldRoom
    [SerializeField] Button buttonTakeOne;
    [SerializeField] Button buttonTakeAll;


    Dictionary<ColdRoom_Button, KeyValuePair<string, AlimentState>> buttons = new Dictionary<ColdRoom_Button, KeyValuePair<string, AlimentState>>(); // Button associate to a food object

    public bool IsDisplayed { get; private set; }

    //add button in dictionnary and UI with good sprite
    public void AddButton(KeyValuePair<string, AlimentState> _keyFood)
    {
        if (!buttons.ContainsValue(_keyFood))
        {
            ColdRoom_Button newButton = (Instantiate(buttonPrefab, buttonsContent) as GameObject).GetComponent<ColdRoom_Button>();

            Sprite sprite = FoodDatabase.mapSpriteAliment[_keyFood];

            if (sprite == null)
            {
                Debug.LogError("Sprite for button not found");
            }

            newButton.Init(this, sprite);
            buttons.Add(newButton, _keyFood);
        }
    }

    //remove button from UI
    public bool RemoveButton(out KeyValuePair<string, AlimentState> keyToRemove)
    {

        if (selectedButton != null)
        {
            keyToRemove = buttons[selectedButton];
            Destroy(selectedButton.gameObject);
            buttons.Remove(selectedButton);
            selectedButton = null;
            return true;
        }

        return false;
    }

    //remove button from UI
    public void RemoveButton(KeyValuePair<string, AlimentState> keyToRemove)
    {
        KeyValuePair<ColdRoom_Button, KeyValuePair<string, AlimentState>> toDel = buttons.First(x => x.Value.Equals(keyToRemove));
        if (selectedButton == toDel.Key)
        {
            selectedButton = null;
        }
        Destroy(toDel.Key.gameObject);
        buttons.Remove(toDel.Key);
    }

    // return food associate to button
    public KeyValuePair<string, AlimentState> GetPoolableFromSelectedButton()
    {
        return buttons[selectedButton];
    }

    // selectedButton = button click
    public void SetSelectedButton(ColdRoom_Button _button)
    {
        selectedButton = _button;
    }

    public void DisplayUiMenu()
    {
        buttonTakeOne.interactable = false;
        buttonTakeAll.interactable = false;
        scrollRect.gameObject.SetActive(true);
        choiceMenu.SetActive(true);
        IsDisplayed = true;
    }

    public void HideUiMenu()
    {
        scrollRect.gameObject.SetActive(false);
        choiceMenu.SetActive(false);
        buttonTakeOne.interactable = false;
        buttonTakeAll.interactable = false;
        IsDisplayed = false;
    }

    public void DisplayChoiceMenu(ColdRoom_Button _button)
    {
        AlimentState alimentState = buttons[_button].Value;

        if (alimentState == AlimentState.Box || alimentState == AlimentState.Stack)
        {
            buttonTakeOne.interactable = true;
            buttonTakeAll.interactable = true;
        }
        else
        {
            buttonTakeOne.interactable = true;
            buttonTakeAll.interactable = false;
        }
    }

    public void HideChoiceMenu()
    {
        choiceMenu.SetActive(false);
    }


}
