using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Trash_Can : MonoBehaviourPun, IInteractive
{
    [SerializeField] Transform posText3D;
    [SerializeField] GameObject wasteItem;
    [SerializeField] TrashBag trashBagPrefab;

    string nameObject;

    int nbMaxOfElement = 5;
    int nbOfElementInTrash = 0;
    Vector3 initPosWasteItem;

    private void Awake()
    {
        GameManager.Instance.initScripts += Init;
    }

    private void Init()
    {
        nameObject = GetComponent<Nominator>().customName;
        GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
        initPosWasteItem = wasteItem.transform.position;
    }

    //Open fridge and play opening animation
    public void Interact(PlayerController pController)
    {
        Gastro gastroInHand = pController.pDatas.gastroInHand;
        GrabableObject handPlayer = pController.pDatas.objectInHand;
        if (handPlayer != null)
        {
            Poolable food;
            if (gastroInHand != null)
            {
                Aliment alimentToThrow = gastroInHand.alimentStocked;
                if(alimentToThrow != null)
                {
                    food = gastroInHand.alimentStocked.GetComponent<Poolable>();
                }
                else
                {
                    GameManager.Instance.PopUp.CreateText("Aucun élément à jeter", 50, new Vector2(0, 300), 3.0f);
                    return;
                }
            }
            else
            {
                food = handPlayer.GetComponent<Poolable>();
            }

            if (food != null)
            {
                if (nbOfElementInTrash == nbMaxOfElement)
                {
                    GameManager.Instance.PopUp.CreateText("Poubelle pleine", 50, new Vector2(0, 300), 3.0f);
                }
                else
                {
                    ThrowObject(pController, food);
                }
            }
        }
        else if (nbOfElementInTrash > 0)
        {
            CleanTrash(pController);
        }
    }

    void CleanTrash(PlayerController _pController)
    {
        GrabableObject trashBag = PhotonNetwork.Instantiate("Furniture/P_TrashBag", Vector3.zero, Quaternion.identity).GetComponent<GrabableObject>();
        trashBag.Init();
        trashBag.AllowGrab(true);
        _pController.pInteract.GrabObject(trashBag, false);

        nbOfElementInTrash = 0;

        wasteItem.transform.position = initPosWasteItem;
        wasteItem.SetActive(false);

        photonView.RPC("CleanTrashOnline", RpcTarget.Others, trashBag.photonView.ViewID , _pController.photonView.OwnerActorNr);
    }

    [PunRPC]
    void CleanTrashOnline(int _bagViewID, int _ownerID)
    {
        GrabableObject trashBag = PhotonView.Find(_bagViewID).GetComponent<GrabableObject>();
        trashBag.Init();
        trashBag.AllowGrab(true);

        PlayerController photonPlayer = InGamePhotonManager.Instance.PlayersConnected[_ownerID];

        photonPlayer.pInteract.GrabObject(trashBag, false);

        nbOfElementInTrash = 0;

        wasteItem.transform.position = initPosWasteItem;
        wasteItem.SetActive(false);
    }

    public void ThrowObject(PlayerController _pController, Poolable _food)
    {
        Gastro gastroInHand = _pController.pDatas.gastroInHand;
        if (gastroInHand != null)
        {
            gastroInHand.ReleaseObject(true, true, false);
        }
        else
        {
            _pController.pInteract.ReleaseObject(true, true, false);
        }
        _food.photonView.RPC("DelObjectOnline", RpcTarget.Others);
        _food.DelObject();
        if (nbOfElementInTrash == 0)
        {
            wasteItem.SetActive(true);
        }
        nbOfElementInTrash++;
        wasteItem.transform.position = initPosWasteItem + Vector3.up * 0.1f * nbOfElementInTrash;

        GameManager.Instance.Audio.PlaySound("Trash", AudioManager.Canal.SoundEffect);

        photonView.RPC("ThrowObjectOnline", RpcTarget.Others, _pController.photonView.OwnerActorNr);
    }

    [PunRPC]
    public void ThrowObjectOnline(int _actorNumber)
    {
        PlayerController photonPlayer = InGamePhotonManager.Instance.PlayersConnected[_actorNumber];
        photonPlayer.pInteract.ReleaseObject(false, true, false);
        if (nbOfElementInTrash == 0)
        {
            wasteItem.SetActive(true);
        }
        nbOfElementInTrash++;
        wasteItem.transform.position = initPosWasteItem + Vector3.up * 0.1f * nbOfElementInTrash;
    }

    //Open fridge and play closing animation
    public void StopInteraction()
    {

    }

    public void Begin()
    {
        throw new System.NotImplementedException();
    }

    public void End()
    {
        throw new System.NotImplementedException();
    }

    public bool CanInteract(PlayerController pController)
    {
        return true;
    }

    public void CancelInteraction()
    {
        throw new System.NotImplementedException();
    }
}
