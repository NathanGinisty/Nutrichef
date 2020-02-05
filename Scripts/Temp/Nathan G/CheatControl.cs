using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sert juste a activer des machins de test quand on a des machins a moitié fait
public class CheatControl : MonoBehaviour
{
    [SerializeField] Transform CanvasEndGame;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (!CanvasEndGame.gameObject.activeSelf)
            {
                CanvasEndGame.gameObject.SetActive(true);
                GameManager.Instance.FreeMouse();
            }
            else
            {
                CanvasEndGame.gameObject.SetActive(false);
                GameManager.Instance.LockMouse();
            }
        }
        
    }
}
