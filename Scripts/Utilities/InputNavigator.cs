using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class InputNavigator : MonoBehaviour
{
    EventSystem system;
    [SerializeField] List<Selectable> selectable = new List<Selectable>();
    int actualSelectable = 0;
    bool isInit = false;
    void Awake()
    {
        

    }

    private void Start()
    {
        isInit = true;
        actualSelectable = 0;
        system = EventSystem.current;
        InputField inputfield = selectable[actualSelectable].GetComponent<InputField>();
        if (inputfield != null)
            inputfield.OnPointerClick(new PointerEventData(system));
        system.SetSelectedGameObject(selectable[actualSelectable].gameObject, new BaseEventData(system));
    }

    private void OnEnable()
    {
        if (!isInit)
        {
            return;
        }


        actualSelectable = 0;
        InputField inputfield = selectable[actualSelectable].GetComponent<InputField>();
        if (inputfield != null)
            inputfield.OnPointerClick(new PointerEventData(system));
        system.SetSelectedGameObject(selectable[actualSelectable].gameObject, new BaseEventData(system));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && system.currentSelectedGameObject != null)
        {
            Selectable current = system.currentSelectedGameObject.GetComponent<Selectable>();
            if (current != null)
            {
                int newIndex = selectable.FindIndex(0, selectable.Count, x => x = current);
                if (newIndex != -1 && newIndex != actualSelectable)
                {
                    actualSelectable = newIndex;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameObject current = system.currentSelectedGameObject;

            if (current != null)
            {
                actualSelectable++;
            }
            else
            {
                actualSelectable = 0;
            }

            actualSelectable = actualSelectable > selectable.Count - 1 ? 0 : actualSelectable;

            InputField inputfield = selectable[actualSelectable].GetComponent<InputField>();
            if (inputfield != null)
                inputfield.OnPointerClick(new PointerEventData(system));
            system.SetSelectedGameObject(selectable[actualSelectable].gameObject, new BaseEventData(system));
        }
    }
}
