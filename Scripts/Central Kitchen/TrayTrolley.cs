using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class TrayTrolley : MonoBehaviourPun, IInteractive
{
    [SerializeField] Transform[] gastroPos;
    [Space]
    [SerializeField] Transform posText3D;

    Dictionary<int, GrabableObject> gastroStocked = new Dictionary<int, GrabableObject>();

    GrabableObject grabableReceived;

    bool isEmpty = false;
    bool isFull = true;

    string nameObject;

    private void Awake()
    {
        GameManager.Instance.initScripts += Init;
    }

    public void Init()
    {
        nameObject = GetComponent<Nominator>().customName;
        GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);

        Gastro[] gastroInChildren = GetComponentsInChildren<Gastro>();

        for (int i = 0; i < gastroInChildren.Length; i++)
        {
            GrabableObject newGastro = gastroInChildren[i].GetComponent<GrabableObject>();
            newGastro.AllowGrab(false);
            if (newGastro != null)
            {
                gastroStocked.Add(i, newGastro);
            }
            else
            {
                Debug.LogError("gastro non ajouté à la liste au démarage");
            }
        }
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
        if (isEmpty != true && pController.pDatas.gastroInHand == null)
        {
            return true;
        }
        else if (isFull != true && pController.pDatas.gastroInHand != null)
        {
            return true;
        }
        return false;
    }

    public void Interact(PlayerController pController)
    {
        Gastro gastroInHand = pController.pDatas.gastroInHand;

        if (gastroInHand == null)
        {
            if (isEmpty == true)
            {
                GameManager.Instance.PopUp.CreateText("Le chariot est vide", 50, new Vector2(0, 300), 3.0f);
            }
            else // Take gastro
            {
                TakeGastro(pController);
            }
        }
        else
        {
            if (isFull == true)
            {
                GameManager.Instance.PopUp.CreateText("Le chariot est plein", 50, new Vector2(0, 300), 3.0f);
            }
            else// Put gastro
            {
                if (gastroInHand.haveObject == false)
                {
                    PutGastro(pController);
                }
                else
                {
                    GameManager.Instance.PopUp.CreateText("Impossible de poser un gastro remplie", 50, new Vector2(0, 300), 3.0f);
                }
            }
        }
    }

    void TakeGastro(PlayerController _pController)
    {
        int key = -1;
        for (int i = 0; i < gastroStocked.Count; i++)
        {
            GrabableObject actualGastro = gastroStocked[i];
            if (actualGastro != null)
            {
                actualGastro.AllowGrab(true);
                key = i;
                _pController.pInteract.GrabObject(actualGastro, false);
                gastroStocked[i] = null;
                isFull = false;
                break;
            }
        }
        CheckStock();

        photonView.RPC("TakeGastroOnline", RpcTarget.Others, key, _pController.photonView.OwnerActorNr);
    }

    [PunRPC]
    void TakeGastroOnline(int _key, int _ownerID)
    {
        PlayerController photonPlayer = InGamePhotonManager.Instance.PlayersConnected[_ownerID].GetComponent<PlayerController>();
        gastroStocked[_key].AllowGrab(true);
        photonPlayer.pInteract.GrabObject(gastroStocked[_key], false);
        gastroStocked[_key] = null;
        isFull = false;
        CheckStock();
    }

    void PutGastro(PlayerController _pController)
    {
        int key = -1;
        for (int i = 0; i < gastroStocked.Count; i++)
        {
            GrabableObject actualGastro = gastroStocked[i];
            if (actualGastro == null)
            {
                key = i;
                GrabableObject gastro = _pController.pInteract.ReleaseObject(false, false, false);
                gastro.AllowGrab(false);
                gastro.AllowPhysic(false);
                gastro.transform.position = gastroPos[i].position;
                gastro.transform.rotation = gastroPos[i].rotation;
                gastroStocked[i] = gastro;

                isEmpty = false;
                break;
            }
        }

        CheckStock();

        photonView.RPC("PutGastroOnline", RpcTarget.Others, key, _pController.photonView.OwnerActorNr);
    }

    [PunRPC]
    void PutGastroOnline(int _key, int _actorNr)
    {
        PlayerController photonPlayer = InGamePhotonManager.Instance.PlayersConnected[_actorNr].GetComponent<PlayerController>();

        GrabableObject gastro = photonPlayer.pInteract.ReleaseObject(false, false, false);
        gastro.transform.position = gastroPos[_key].position;
        gastro.transform.rotation = gastroPos[_key].rotation;
        gastro.AllowGrab(false);
        gastro.AllowPhysic(false);
        gastroStocked[_key] = gastro;

        isEmpty = false;
        CheckStock();
    }

    void CheckStock()
    {
        int nbOfGastro = 0;
        for (int i = 0; i < gastroStocked.Count; i++)
        {
            if (gastroStocked[i] != null)
            {
                nbOfGastro++;
            }
        }

        if (nbOfGastro == 0)
        {
            isEmpty = true;
        }
        else if (nbOfGastro == 5)
        {
            isFull = true;
        }
    }

    public void StopInteraction()
    {

    }

    public void End()
    {
        throw new System.NotImplementedException();
    }

    public void CancelInteraction()
    {
        throw new System.NotImplementedException();
    }
}
