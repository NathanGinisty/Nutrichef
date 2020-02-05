using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VestiaireButtonBuilder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    GameObject panel;
    bool inTheBox = false;
    private void Start()
    {
        panel = transform.GetChild(0).gameObject;
    }

    public void OnVestiaireClick()
    {
        panel.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (!inTheBox)
            {
                panel.SetActive(false);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        inTheBox = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inTheBox = false;
    }
}
