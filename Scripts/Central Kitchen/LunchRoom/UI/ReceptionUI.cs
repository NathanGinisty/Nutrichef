using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReceptionUI : MonoBehaviour
{
    public enum State
    {
        Hidden, Interaction, CustomerData
    }

    [SerializeField] Canvas canvas;
    [SerializeField] Text interactionText;
    [SerializeField] ReceptionCustomerUI customerUI;

    public State state { get; private set; }

    Reception reception;

    // Start is called before the first frame update
    void Start()
    {
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas not set on ReceptionUI !");
        }

        reception = GetComponentInParent<Reception>();
        SetState(State.Hidden);
    }

    void DisplayCanvas(bool _state)
    {
        canvas.gameObject.SetActive(_state);

        DisplayInterationText(_state);

        if (_state)
        {
            DisplayCustomerStats(!_state);
            state = State.Interaction;
        }
        else
        {
            DisplayCustomerStats(_state);
            state = State.Hidden;
        }
    }

    void DisplayInterationText(bool _state)
    {
        interactionText.gameObject.SetActive(_state);

        if (_state)
        {
            state = State.Interaction;
        }
    }

    void DisplayCustomerStats(bool _state)
    {
        customerUI.Display(_state);

        if (_state)
        {
            customerUI.SetCustomerData(reception.Customer.nameCLient.ToString(), "J'ai faim et j'ai pas mangé, du coup je veux manger.\nParcontre j'aime pas les pommes !");
            state = State.CustomerData;
        }
    }

    public void SetState(State _state)
    {
        switch (_state)
        {
            case State.Hidden:
                DisplayCanvas(false);
                break;
            case State.Interaction:

                DisplayCanvas(true);
                DisplayInterationText(true);
                DisplayCustomerStats(false);

                break;
            case State.CustomerData:

                DisplayCanvas(true);
                DisplayInterationText(false);
                DisplayCustomerStats(true);

                break;
            default:
                break;
        }
    }
}
