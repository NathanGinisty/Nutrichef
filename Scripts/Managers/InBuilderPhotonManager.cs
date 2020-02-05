using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class InBuilderPhotonManager : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{
    public static InBuilderPhotonManager Instance;

    [HideInInspector] public bool isMasterClient = false;
    bool sendToNewMaster = false;
    bool buildValidated = false;
    Dictionary<int, float> playersScore = new Dictionary<int, float>();

    private void Awake()
    {
        Instance = this;
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.OfflineMode = true;

            PhotonNetwork.CreateRoom("local");
        }


        isMasterClient = PhotonNetwork.IsMasterClient;
    }


    private void Start()
    {

    }

    private void Update()
    {

    }

    public void EndBuild()
    {
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        float score = Random.Range(1, 100);
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            score = 999;
        }
        photonView.RPC("OnPlayerEndBuild", RpcTarget.All, actorNumber, score);
    }

    [PunRPC]
    public void OnPlayerEndBuild(int _actorNumber, float _score)
    {
        if (!playersScore.ContainsKey(_actorNumber))
        {
            playersScore.Add(_actorNumber, _score);
        }

        if (isMasterClient)
        {
            if (playersScore.Count == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                Player newMaster = PhotonNetwork.CurrentRoom.GetPlayer(GetBestMap());
                //Debug.Log("actual master is " + PhotonNetwork.MasterClient.NickName + " the winner is " + newMaster.NickName);
                if (!newMaster.IsLocal)
                {
                    PhotonNetwork.SetMasterClient(newMaster);
                    sendToNewMaster = true;
                }
                else
                {
                    OnBuildWin();
                }
            }
        }
    }

    [PunRPC]
    public void OnBuildWin()
    {
        if (buildValidated)
        {
            return;
        }

        BSCheck.Instance.FillBSResultData();
        PhotonNetwork.LoadLevel("Loading Scene");
        buildValidated = true;
    }

    private int GetBestMap()
    {
        int bestActorNumber = -1;
        float bestScore = 0;

        foreach (int actorNumber in playersScore.Keys)
        {
            if (playersScore[actorNumber] > bestScore)
            {
                bestScore = playersScore[actorNumber];
                bestActorNumber = actorNumber;
            }
        }

        return bestActorNumber;
    }

    public override void OnConnectedToMaster()
    {

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {

    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (playersScore.ContainsKey(otherPlayer.ActorNumber))
        {
            playersScore.Remove(otherPlayer.ActorNumber);
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

        if (sendToNewMaster)
        {
            photonView.RPC("OnBuildWin", newMasterClient);
            sendToNewMaster = false;
        }
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        targetView.TransferOwnership(requestingPlayer);

#if UNITY_EDITOR
        Debug.Log("Transfer owner of " + targetView.ToString() + " to " + requestingPlayer.ActorNumber);
#endif
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {

    }
}
