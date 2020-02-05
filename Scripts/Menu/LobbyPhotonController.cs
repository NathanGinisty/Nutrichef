using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

using Photon.Pun;
using Photon.Chat;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using ExitGames.Client.Photon;

public class LobbyPhotonController : MonoBehaviourPunCallbacks
{
    struct PlayerInfo
    {
        public string userId;
        public string userName;
    }

    #region Private Serializable Fields

    [Header("Transform parents")]
    [SerializeField] Transform GameInvitationList = null;
    [SerializeField] GameObject GameInvitationCount = null;
    [SerializeField] Text GameInvitationCountText = null;
    [Space]
    [Header("Prefabs")]
    [SerializeField] GameObject prefabsGameInvitation = null;
    [Space]
    [Header("Panels")]
    [SerializeField] GameObject panelInviteToGame = null;
    [SerializeField] GameObject panelInviteAFriend = null;
    [SerializeField] List<PlayerDock> playersDock = new List<PlayerDock>();
    [Space]
    [Header("Texts")]
    [SerializeField] Text inputFriendName = null;
    [SerializeField] Text InfoText = null;
    [SerializeField] Text roomName = null;
    [Space]
    [Header("Buttons")]
    [SerializeField] Button playButton = null;
    [SerializeField] Button JoinFriendButton = null;
    [SerializeField] Button invitationGameButton = null;

    #endregion

    #region Private Fields

    Dictionary<string, int> DicPlayerDock = new Dictionary<string, int>();
    List<int> PlayerDockFree = new List<int>() { 0, 1, 2, 3 };

    ChatListner chatListner;
    int invitationCount = 0;
    int roomSize = 4;
    string playerName = "";

    bool joinFriend = false;
    string roomToJoin = "";

    short playerRoomID = -1;

    #endregion

    public bool offlineMode = false;

    #region InfoText Couroutine

    IEnumerator WaitingForConnectMaster()
    {
        InfoText.gameObject.SetActive(true);
        int nbPoint = 0;
        while (!PhotonNetwork.IsConnectedAndReady)
        {
            InfoText.text = "Connecting";
            for (int i = 0; i < nbPoint; i++)
            {
                InfoText.text += ".";
            }
            nbPoint++;
            if (nbPoint > 3) { nbPoint = 0; }

            yield return new WaitForSeconds(0.2f);
        }
        InfoText.gameObject.SetActive(false);
    }

    IEnumerator WaitingForJoinlobby()
    {
        InfoText.gameObject.SetActive(true);
        int nbPoint = 0;
        while (!PhotonNetwork.InLobby)
        {
            InfoText.text = "Join lobby";
            for (int i = 0; i < nbPoint; i++)
            {
                InfoText.text += ".";
            }
            nbPoint++;
            if (nbPoint > 3) { nbPoint = 0; }

            yield return new WaitForSeconds(0.2f);
        }
        InfoText.gameObject.SetActive(false);
    }

    IEnumerator WaitingForJoinRoom()
    {
        InfoText.gameObject.SetActive(true);
        int nbPoint = 0;
        while (!PhotonNetwork.InRoom)
        {
            InfoText.text = "Join Room";
            for (int i = 0; i < nbPoint; i++)
            {
                InfoText.text += ".";
            }
            nbPoint++;
            if (nbPoint > 3) { nbPoint = 0; }

            yield return new WaitForSeconds(0.2f);
        }
        InfoText.gameObject.SetActive(false);
    }

    IEnumerator WaitingForLeaveRoom()
    {
        InfoText.gameObject.SetActive(true);
        int nbPoint = 0;

        while (PhotonNetwork.InRoom)
        {
            InfoText.text = "Leave lobby";
            for (int i = 0; i < nbPoint; i++)
            {
                InfoText.text += ".";
            }
            nbPoint++;
            if (nbPoint > 3) { nbPoint = 0; }

            yield return new WaitForSeconds(0.2f);
        }
        InfoText.gameObject.SetActive(false);
    }

    #endregion

    #region MonoBehaviour CallBacks

    void Awake()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically

    }

    void Start()
    {
        chatListner = GetComponent<ChatListner>();
        if (PhotonNetwork.IsConnected == false || PhotonNetwork.OfflineMode)
        {
            offlineMode = true;
            PhotonNetwork.OfflineMode = true;
            if (!PhotonNetwork.InRoom)
            {
                string roomName = playerName + System.DateTime.Now.ToLongTimeString();
                PhotonNetwork.CreateRoom(roomName, DefaultRoomOptions());
            }
        }
        else
        {
            //Debug.Log("pass");
            StartCoroutine(WaitingForJoinlobby());
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.initScripts = GameManager.Instance.LockMouse;
        }

        playerName = PhotonNetwork.LocalPlayer.NickName;
    }

    #endregion

    #region Public Methods

    RoomOptions DefaultRoomOptions()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = (byte)roomSize;
        options.IsOpen = true;
        options.IsVisible = true;
        options.PublishUserId = true;
        options.CleanupCacheOnLeave = false;

        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable();
        short[] playersID = new short[4] { 0, 1, 2, 3 };
        customRoomProperties.Add("PID", playersID);
        options.CustomRoomProperties = customRoomProperties;
        //timeout : 5000 millisec
        options.PlayerTtl = 1000;
        options.EmptyRoomTtl = 0;

        return options;
    }

    void SetPlayerRoomID(short ID)
    {
        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        customProperties["ID"] = ID;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
    }


    short GetPlayerRoomID(Player player)
    {
        ExitGames.Client.Photon.Hashtable customProperties = player.CustomProperties;
        Debug.Log("player :" + player.NickName + " ID :" + (short)customProperties["ID"]);
        return (short)customProperties["ID"];
    }

    void GetNewPlayerRoomID()
    {
        Room room = PhotonNetwork.CurrentRoom;
        ExitGames.Client.Photon.Hashtable customRoomProperties = room.CustomProperties;
        object outObject;
        customRoomProperties.TryGetValue("PID", out outObject);
        short[] playersID = (short[])outObject;

        for (int i = 0; i < playersID.Length; i++)
        {
            if (playersID[i] != -1)
            {
                playerRoomID = playersID[i];
                playersID[i] = -1;
                break;          /* /!\ */
            }
        }
        customRoomProperties["PID"] = playersID;
        room.SetCustomProperties(customRoomProperties);
        SetPlayerRoomID(playerRoomID);
    }

    void AddPlayerIDToRoom(short ID)
    {
        Room room = PhotonNetwork.CurrentRoom;
        ExitGames.Client.Photon.Hashtable customRoomProperties = room.CustomProperties;
        object outObject;
        customRoomProperties.TryGetValue("PID", out outObject);
        short[] playersID = (short[])outObject;
        playersID[ID] = ID;
        customRoomProperties["PID"] = playersID;
        room.SetCustomProperties(customRoomProperties);
    }

    void InstantiatePlayerDock(string _playerName, int actorNumber)
    {
        int pos = PlayerDockFree[0];
        PlayerDockFree.RemoveAt(0);

        playersDock[pos].playerName = _playerName;
        playersDock[pos].playerActorNumber = pos;

        switch (actorNumber)
        {
            case 0:
                playersDock[pos].playerColor = new Color(19f / 255f, 19f / 255f, 19f / 255f);
                break;
            case 1:
                playersDock[pos].playerColor = new Color(200f / 255f, 58f / 255f, 58f / 255f);
                break;
            case 2:
                playersDock[pos].playerColor = new Color(35f / 255f, 112f / 255f, 186f / 255f);
                break;
            case 3:
                playersDock[pos].playerColor = new Color(255f / 255f, 199f / 255f, 49f / 255f);
                break;
            default:
                playersDock[pos].playerColor = new Color(0.8f, 0.8f, 0.8f);
                break;
        }

        if (playersDock[pos].gameObject.activeInHierarchy)
        {
            playersDock[pos].OnEnable();
        }
        else
        {
            playersDock[pos].gameObject.SetActive(true);
        }

        DicPlayerDock.Add(_playerName, pos);
    }

    void UpdatePlayerDockColor(int _playerRoomID, string _playerName)
    {
        if (!DicPlayerDock.ContainsKey(_playerName))
        {
            return;
        }

        int pos = DicPlayerDock[_playerName];
        switch (_playerRoomID)
        {
            case 0:
                playersDock[pos].playerColor = new Color(19f/255f,19f/255f,19f/255f);
                break;
            case 1:
                playersDock[pos].playerColor = new Color(200f/255f,58f/255f,58f/255f); 
                break;
            case 2:
                playersDock[pos].playerColor = new Color(35f / 255f, 112f / 255f, 186f / 255f);
                break;
            case 3:
                playersDock[pos].playerColor = new Color(255f / 255f, 199f / 255f, 49f / 255f);
                break;
            default:
                playersDock[pos].playerColor = new Color(0.8f,0.8f,0.8f);
                break;
        }
        playersDock[pos].UpdateColor();
    }

    public string GetPlayerName()
    {
        return PhotonNetwork.LocalPlayer.NickName;
    }

    public string GetPlayerUserId()
    {
        return PhotonNetwork.LocalPlayer.UserId;
    }

    public void RespondToFriendInvitation(bool accept, GameObject invit)
    {
        // chatListner.SendMessageCommand(invitations[invit].userId, "|act |invitation| " + accept + "| userId:" + PhotonNetwork.LocalPlayer.UserId + "|userName:" + PhotonNetwork.LocalPlayer.NickName);
    }

    public void RespondToGameInvitation(bool accept, GameInvitation invit)
    {

        invitationCount--;
        if (invitationCount < 1)
        {
            GameInvitationCount.SetActive(false);
        }
        else
        {
            GameInvitationCountText.text = invitationCount.ToString();
        }

        if (accept)
        {
            PhotonNetwork.LeaveRoom();
            joinFriend = true;
            roomToJoin = invit.roomName;
            Debug.Log("accept invitation roomName:" + invit.roomName);
        }
        Destroy(invit.gameObject);
    }

    #endregion

    #region Buttons

    public void OnDisconnectButton()
    {
        if (PhotonNetwork.OfflineMode)
        {
            SceneManager.LoadScene("ConnectionMenu");
        }

        PhotonNetwork.Disconnect();
    }

    public void OnExitGameButton()
    {
        Application.Quit();
    }

    public void OnPanelInviteButton()
    {
        panelInviteToGame.SetActive(!panelInviteToGame.activeInHierarchy);
    }

    public void OnPlayButton()
    {
        playButton.interactable = false;
        JoinFriendButton.interactable = false;
        invitationGameButton.interactable = false;

        PhotonNetwork.LoadLevel("BuildScene " + GameManager.Instance.RoomConfig.levelSelected);
    }

    public void OnInviteFriendButton()
    {
        panelInviteAFriend.SetActive(true);
    }

    public void OnValidateFriendName()
    {
        panelInviteAFriend.SetActive(false);
        //Debug.Log("friend name: " + inputFriendName.text);
        SqlManager.Instance.GetPlayerId(inputFriendName.text, InviteFriendToGameCallback);
    }

    void InviteFriendToGameCallback(GetPlayerIdResponse getPlayerIdResponse)
    {
        //Debug.Log(getPlayerIdResponse.command);
        if (getPlayerIdResponse.command == GetPlayerIdCommandLog.succes)
        {
            //Debug.Log(getPlayerIdResponse.userName);
            chatListner.SendMessageCommand(getPlayerIdResponse.userId, "</From " + PhotonNetwork.LocalPlayer.UserId + " /Name " + PhotonNetwork.LocalPlayer.NickName + " /Into " + PhotonNetwork.CurrentRoom.Name + " /Act " + ChatActionEnum.InvitationGame + " />");
        }
    }

    #endregion

    #region MonoBehaviourPunCallbacks Callbacks

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRoomFailed : " + message);
        string roomName = playerName + System.DateTime.Now.ToLongTimeString();
        PhotonNetwork.JoinOrCreateRoom(roomName, DefaultRoomOptions(), TypedLobby.Default);
    }

    public override void OnConnectedToMaster()
    {
        if (!offlineMode)
        {
            StartCoroutine(WaitingForJoinlobby());
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
        else
        {
            string roomName = playerName + System.DateTime.Now.ToLongTimeString();
            PhotonNetwork.CreateRoom(roomName, DefaultRoomOptions(), TypedLobby.Default);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {

        if (newMasterClient.IsLocal)
        {
            playButton.gameObject.SetActive(true);
            playButton.interactable = true;
        }
    }

    public override void OnJoinedLobby()
    {

        StopAllCoroutines();
        StartCoroutine(WaitingForJoinRoom());

        if (joinFriend)
        {
            PhotonNetwork.JoinRoom(roomToJoin);
            joinFriend = false;
        }
        else
        {
            string roomName = playerName + System.DateTime.Now.ToLongTimeString();
            PhotonNetwork.JoinOrCreateRoom(roomName, DefaultRoomOptions(), TypedLobby.Default);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene("ConnectionMenu");
        joinFriend = false;
    }

    public override void OnJoinedRoom()
    {
        roomName.gameObject.SetActive(true);
        roomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;


        if (!PhotonNetwork.IsMasterClient)
        {
            playButton.gameObject.SetActive(false);
        }
        else
        {
            playButton.interactable = true;
        }


        if (!offlineMode)
        {
            JoinFriendButton.interactable = true;
            invitationGameButton.interactable = true;
        }

        GetNewPlayerRoomID();

        InstantiatePlayerDock(playerName, playerRoomID);

        PhotonNetwork.PlayerListOthers.ToList().ForEach(x => InstantiatePlayerDock(x.NickName, GetPlayerRoomID(x)));
    }

    public override void OnLeftRoom()
    {
        roomName.gameObject.SetActive(false);

        playButton.interactable = false;
        JoinFriendButton.interactable = false;
        invitationGameButton.interactable = false;

        SetPlayerRoomID(-1);
        playerRoomID = -1;

        for (int i = 1; i < 4; i++)
        {
            playersDock[i].gameObject.SetActive(false);
        }

        DicPlayerDock.Clear();
        PlayerDockFree = new List<int>() { 0, 1, 2, 3 };
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        InstantiatePlayerDock(newPlayer.NickName, GetPlayerRoomID(newPlayer));
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            AddPlayerIDToRoom(GetPlayerRoomID(otherPlayer));
        }

        if (DicPlayerDock.ContainsKey(otherPlayer.NickName))
        {
            PlayerDockFree.Add(DicPlayerDock[otherPlayer.NickName]);
            PlayerDockFree.Sort();

            playersDock[DicPlayerDock[otherPlayer.NickName]].gameObject.SetActive(false);
            DicPlayerDock.Remove(otherPlayer.NickName);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log(target.NickName + " Update his Properties");
        UpdatePlayerDockColor(GetPlayerRoomID(target), target.NickName);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Create Room: " + PhotonNetwork.CurrentRoom.Name);
    }

    #endregion

    #region ChatCommand

    public void OnReceiveGameInvitation(string _userName, string _userID, string _roomName)
    {
        GameObject newInvit = Instantiate(prefabsGameInvitation, GameInvitationList);
        GameInvitation gameInvitation = newInvit.GetComponent<GameInvitation>();
        gameInvitation.lobbyPhotonController = this;
        gameInvitation.userName = _userName;
        gameInvitation.userId = _userID;
        gameInvitation.roomName = _roomName;
        invitationCount++;

        GameInvitationCount.SetActive(true);
        GameInvitationCountText.text = invitationCount.ToString();

    }

    #endregion

    #region Debug

    public void ButtonDisplayState()
    {
        Debug.Log("current state:\n" +
                    "\n ID :" + PhotonNetwork.LocalPlayer.UserId +
                    "\n Co :" + PhotonNetwork.IsConnected +
                    "\n Game Version :" + PhotonNetwork.GameVersion +
                    "\n Region :" + PhotonNetwork.CloudRegion +
                    "\n Co & rdy :" + PhotonNetwork.IsConnectedAndReady +
                    "\n Offline Mode :" + PhotonNetwork.OfflineMode +
                    "\n inLobby :" + PhotonNetwork.InLobby +
                    "\n inRoom :" + PhotonNetwork.InRoom);
    }

    #endregion
}

