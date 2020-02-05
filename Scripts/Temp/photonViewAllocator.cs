using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class photonViewAllocator : MonoBehaviourPun
{
    public List<PhotonView> views = new List<PhotonView>();
    List<int> viewIDs = new List<int>();

    

    void Start()
    {
        GameManager.Instance.initScripts += Init;
    }

    private void OnDestroy()
    {
        GameManager.Instance.initScripts -= Init;
    }

    public void Init()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var view in views)
            {
                PhotonNetwork.AllocateSceneViewID(view);
                viewIDs.Add(view.ViewID);
            }
            photonView.RPC("SetChildViewID", RpcTarget.Others, viewIDs.ToArray());
        }
    }

    [PunRPC]
    public void SetChildViewID(int[] _viewIDs)
    {
        Debug.Log("Received viewIDs nb : " + _viewIDs.Length + views.Count);
        int count = 0;
        foreach (int id in _viewIDs)
        {
            views[count].ViewID = id;
            count++;
        }
    }
}
