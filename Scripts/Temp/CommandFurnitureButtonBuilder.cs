using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandFurnitureButtonBuilder : MonoBehaviour
{
    [SerializeField] List<GameObject> panels = new List<GameObject>();
    int currentPanel;

   public void SetPanelActive(int _panel)
    {
        panels[currentPanel].SetActive(false);
        panels[_panel].SetActive(true);
        currentPanel = _panel;
    }
}
