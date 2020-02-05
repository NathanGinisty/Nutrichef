using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class WorkSpace : MonoBehaviourPun, IInteractive
{

    [SerializeField] Transform foodPos;
    [Space]
    [SerializeField] Transform posText3D;


    GrabableObject grabableReceived;

    bool haveAnObject = false;

    string nameObject;



    private void Awake()
    {
        GameManager.Instance.initScripts += Init;
    }

    public void Init()
    {
        nameObject = GetComponent<Nominator>().customName;
        GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
    }

    public void Begin()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Detect if no object on post and object in hand of player
    /// </summary>
    /// <param name="pController"></param>
    /// <returns></returns>
    public bool CanInteract(PlayerController pController)
    {
        if (haveAnObject == false && pController.pDatas.objectInHand != null)
        {
            return true;
        }
        else if (haveAnObject == true && pController.pDatas.objectInHand == null)
        {
            return true;
        }
        return false;
    }

    public void End()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Use of WorkSpace
    /// </summary>
    /// <param name="pController"></param>
    public void Interact(PlayerController pController)
    {
        GrabableObject objectInHand = pController.pDatas.objectInHand;

        // Put object = No object in workspace and object in hand of player
        if (haveAnObject == false && objectInHand != null)
        {
            grabableReceived = objectInHand;
            PutObject(pController);
            photonView.RPC("PutObjectInPost", RpcTarget.Others, pController.photonView.OwnerActorNr);
        }
        else if(objectInHand == null && haveAnObject) // Take object = object in workSpace and no object in hand of player
        {
            GeneralError.ErrorNoOutfit(pController);
        }
        else if(objectInHand != null && haveAnObject == true)// put object impossible = object in workspace and object in hand of player
        {
            GameManager.Instance.PopUp.CreateText("Il y a actuellement un objet sur le poste", 50, new Vector2(0, 300), 3.0f);
        }
    }

    /// <summary>
    /// Stop Use of Disinfection Post
    /// </summary>
    public void StopInteraction()
    {

    }

    void PutObject(PlayerController _pController)
    {
        grabableReceived = _pController.pInteract.ReleaseObject(false,false, false);
        grabableReceived.AllowGrab(true);
        grabableReceived.AllowPhysic(false);
        grabableReceived.transform.position = foodPos.position;
        grabableReceived.transform.rotation = foodPos.rotation;

        haveAnObject = true;

        grabableReceived.onGrab += RemoveObject;

        GameManager.Instance.PopUp.CreateText("Objet posé", 50, new Vector2(0, 300), 3.0f);
    }

    [PunRPC]
    private void PutObjectInPost(int _actorNumber) //Online
    {
        PlayerController photonPlayer = InGamePhotonManager.Instance.PlayersConnected[_actorNumber];

        grabableReceived = photonPlayer.pInteract.ReleaseObject(false,false, false);
        grabableReceived.AllowGrab(true);
        grabableReceived.AllowPhysic(false);
        grabableReceived.transform.position = foodPos.position;
        grabableReceived.transform.rotation = foodPos.rotation;

        haveAnObject = true;

        grabableReceived.onGrab += RemoveObject;
    }

    void RemoveObject(GrabableObject _objectGrab)
    {
        haveAnObject = false;
        grabableReceived = null;
        _objectGrab.onGrab -= RemoveObject;
    }

    public void CancelInteraction()
    {

    }
}
