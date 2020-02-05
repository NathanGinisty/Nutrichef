using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class SampleStock : MonoBehaviourPun, IInteractive
{
    [SerializeField] Transform posText3D;

    int _timeInSecond = 1;
    string nameObject;

    List<Aliment> alimentStocked = new List<Aliment>();

    private void Awake()
    {
        GameManager.Instance.initScripts += Init;
    }

    void Init()
    {
        nameObject = GetComponent<Nominator>().customName;
        GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
    }

    public bool CanInteract(PlayerController pController)
    {
        GrabableObject objectInHand = pController.pDatas.objectInHand;
        if(objectInHand != null)
        {
            Aliment alimentInHand = objectInHand.GetComponent<Aliment>();
            if (alimentInHand != null)
            {
                return true;
            }
        }
        return false;
    }

    public void Interact(PlayerController pController)
    {
        Aliment alimentInHand = pController.pDatas.objectInHand.GetComponent<Aliment>();

        if (alimentInHand.alimentState == AlimentState.Sample)
        {
            PutSample(pController);
        }
        else
        {
            GameManager.Instance.PopUp.CreateText("Seul les échantillons peuvent être déposé ici", 50, new Vector2(0, 300), 3.0f);
        }
    }

    private void PutSample(PlayerController _pController)
    {
        Aliment alimentInHand = _pController.pDatas.objectInHand.GetComponent<Aliment>();
        alimentStocked.Add(alimentInHand);
        // Affect the player
        photonView.RPC("PutSampleOnline", RpcTarget.Others, _pController.photonView.OwnerActorNr);

        GrabableObject grabable = _pController.pInteract.ReleaseObject(false, false, false);
        grabable.AllowGrab(true);
        grabable.gameObject.GetComponent<Poolable>().DelObject();

    }

    [PunRPC]
    public void PutSampleOnline(int _actorNumber)
    {
        PlayerController photonPlayer = InGamePhotonManager.Instance.PlayersConnected[_actorNumber];

        Aliment alimentInHand = photonPlayer.pDatas.objectInHand.GetComponent<Aliment>();
        alimentStocked.Add(alimentInHand);
        // Affect the player
        GrabableObject grabable = photonPlayer.pInteract.ReleaseObject(false, false, false);
        grabable.AllowGrab(true);
        grabable.gameObject.GetComponent<Poolable>().DelObject();
    }

    public void Begin()
    {
        throw new System.NotImplementedException();
    }

    public void End()
    {
        throw new System.NotImplementedException();
    }

    public void CancelInteraction()
    {

    }

    public void StopInteraction()
    {

    }
}
