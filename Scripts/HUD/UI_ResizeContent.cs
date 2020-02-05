using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ResizeContent : MonoBehaviour
{
    private VerticalLayoutGroup vlg;

    private void Start()
    {
        vlg = GetComponent<VerticalLayoutGroup>();
    }

    void Update()
    {
        int nbChild = transform.childCount;

        if (nbChild > 0)
        {
            float sizeY = ((RectTransform)transform.GetChild(0)).sizeDelta.y * nbChild + vlg.spacing * nbChild;
            sizeY -= ((RectTransform)transform.GetChild(0)).sizeDelta.y * 7 + vlg.spacing * 7;
            ((RectTransform)transform).sizeDelta = new Vector2(0, sizeY);
        }
    }
}
