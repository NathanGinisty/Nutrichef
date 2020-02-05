using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class InGamePhotonManager : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{
    public static InGamePhotonManager Instance;

    public Dictionary<int, PlayerController> PlayersConnected = new Dictionary<int, PlayerController>();
    [SerializeField] Transform[] playerSpawns = new Transform[4];
    public PlayerController localPlayer;
	public int localPlayerID { get; private set; }
    [HideInInspector] public bool isMasterClient = false;

    private void Awake()
    {
        Instance = this;
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.OfflineMode = true;
        }
        isMasterClient = PhotonNetwork.IsMasterClient;

        GameManager.Instance.initScripts += Init;
    }
    
    private void Init()
    {
        GameObject newGO = new GameObject();
        newGO.name = "Undo Don't Destroy on Load";
        PoolManager.Instance.transform.parent = newGO.transform; // NO longer DontDestroyOnLoad();

		localPlayerID = GetPlayerRoomID(PhotonNetwork.LocalPlayer);

		SpawnPlayer();
    }

	private short GetPlayerRoomID(Player player)
	{
		ExitGames.Client.Photon.Hashtable customProperties = player.CustomProperties;
		Debug.Log("player :" + player.NickName + " ID :" + (short)customProperties["ID"]);
		return (short)customProperties["ID"];
	}

	private void Update()
    {

    }

    /// <summary>
    /// Spawn the player on according to his ActorNumber.
    /// </summary>
    private void SpawnPlayer()
    {    
        Transform toSpawnAt = playerSpawns[localPlayerID];

        object[] data = new object[] { PhotonNetwork.LocalPlayer.ActorNumber, PhotonNetwork.LocalPlayer.NickName };
        PhotonNetwork.Instantiate("Player", toSpawnAt.position, toSpawnAt.rotation, 0, data);
    }


    public override void OnConnectedToMaster()
    {

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {

    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PlayersConnected.ContainsKey(otherPlayer.ActorNumber))
            {
				PlayerController player = PlayersConnected[otherPlayer.ActorNumber];
				player.pInteract.ReleaseObject(true);

				PhotonNetwork.Destroy(PlayersConnected[otherPlayer.ActorNumber].gameObject);
                PlayersConnected.Remove(otherPlayer.ActorNumber);
            }
        }
    }

    public override void OnLeftRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("Lobby");
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene("ConnectionMenu");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {

        isMasterClient = PhotonNetwork.IsMasterClient;
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        targetView.TransferOwnership(requestingPlayer);

#if UNITY_EDITOR
        //Debug.Log("Transfer owner of " + targetView.ToString() + " to " + requestingPlayer.ActorNumber);
#endif
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {

    }
}
