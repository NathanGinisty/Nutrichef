using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDock : MonoBehaviour
{
    [SerializeField] Text playerNameText;

    [HideInInspector] public string playerName;
    [HideInInspector] public Color playerColor;
    [HideInInspector] public int playerActorNumber;

    public void OnEnable()
    {
        GetComponent<MeshRenderer>().material.color = playerColor;
        playerNameText.text = playerName;
    }

    public void UpdateColor()
    {
        GetComponent<MeshRenderer>().material.color = playerColor;
    }

}
