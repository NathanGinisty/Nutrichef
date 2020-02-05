using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInvitation : MonoBehaviour
{
    [SerializeField] Text UserNameText;
    public LobbyPhotonController lobbyPhotonController;
    public string userName;
    public string userId;
    public string roomName;

    private void Start()
    {
        UserNameText.text = userName;
    }

    public void ResponseInvitation(bool accept)
    {
        lobbyPhotonController.RespondToGameInvitation(accept, this);
    }
}
