using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class InGameMenu : MonoBehaviour
{
    [SerializeField] GameObject gameMenuPanel;

    GameManager gameManager;
	PlayerController activatedBy;


    private void Start()
    {
        gameManager = GameManager.Instance;
        gameManager.InGameMenu = this;
        gameManager.FreeMouse();
    }
    
    public void OpenMenu(PlayerController _activatedBy)
    {
        gameManager.FreeMouse();
        gameMenuPanel.SetActive(true);

        _activatedBy.AllowInteraction(false);
        _activatedBy.EnableMovementControl(false);
        _activatedBy.EnableRotationControl(false);
        activatedBy = _activatedBy;
    }

    public void OnResumeClick()
    {
        gameManager.LockMouse();
        gameMenuPanel.SetActive(false);

		activatedBy.AllowInteraction(true);
		activatedBy.EnableMovementControl(true);
		activatedBy.EnableRotationControl(true);
		activatedBy = null;
	}

    public void OnLeaveRoomClick()
    {
        PhotonNetwork.LeaveRoom();
        gameManager.FreeMouse();
        if (PhotonNetwork.OfflineMode)
        {
            SceneManager.LoadScene("Lobby");
            return;
        }
    }

    public void OnDisconnectClick()
    {
        gameManager.FreeMouse();

        if (PhotonNetwork.OfflineMode)
        {
            SceneManager.LoadScene("ConnectionMenu");
        }
        else
        {
            PhotonNetwork.Disconnect();
        }

    }

    public void OnLeaveGameClick()
    {
        PhotonNetwork.LeaveRoom();
        Application.Quit();
    }
    
}
