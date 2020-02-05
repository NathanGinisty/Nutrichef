using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameArea : MonoBehaviour
{
    [SerializeField] UI_EndGame canvasEndGame;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController pController = other.GetComponent<PlayerController>();
        if (pController != null)
        if (GameManager.Instance.GameSceneManager.endGame == true)
        {
            canvasEndGame.gameObject.SetActive(true);
            pController.BeginInteractionState(false);
        }
    }
}
