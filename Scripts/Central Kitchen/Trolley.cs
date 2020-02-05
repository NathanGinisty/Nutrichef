using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class Trolley : MonoBehaviourPun, IInteractive
{
    enum TypeOfTrolley
    {
        Cold,
        Hot,
        COUNT
    }

    [SerializeField] Transform posText3D;
    [SerializeField] TypeOfTrolley typeOfTrolley;
    [SerializeField] Transform gastroPos;

    PlayerController player;

    GrabableObject grabableReceived = null;
    public List<Aliment> alimentStocked = new List<Aliment>();

    string nameObject;
    bool onUse = false;

    private void Awake()
    {
        GameManager.Instance.initScripts += Init;
    }

    public void Init()
    {
        nameObject = GetComponent<Nominator>().customName;
        GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
    }

    public bool CanInteract(PlayerController pController)
    {
        return pController.pDatas.gastroInHand != null && !onUse || pController.pDatas.objectInHand == null;
    }

    public void Interact(PlayerController pController)
    {
        player = pController;
        Gastro gastroInHand = player.pDatas.gastroInHand;

        if (gastroInHand != null) // Put object in trolley
        {

            Aliment actualAliment = gastroInHand.alimentStocked;
            if (actualAliment != null && (actualAliment.alimentState == AlimentState.Cooked || actualAliment.alimentState == AlimentState.Cut || actualAliment.alimentState == AlimentState.Standard || actualAliment.alimentState == AlimentState.Clean))
            {
                onUse = true;

                PutObjectInTrolley(actualAliment);
            }
            else
            {
                GameManager.Instance.PopUp.CreateText("Le gastro ne contient pas d'aliment", 50, new Vector2(0, 300), 3.0f);
            }
        }
        else // check température
        {
            player.pDatas.temperatureInMind = true;
            GameManager.Instance.PopUp.CreateText("Température relevée", 50, new Vector2(0, 300), 3.0f);
        }
    }

    void PutObjectInTrolley(Aliment _aliment)
    {
        // Affect the players
        Gastro gastroInHand = player.pDatas.gastroInHand;
        grabableReceived = gastroInHand.ReleaseObject(false,false, false);
        alimentStocked.Add(_aliment);
        grabableReceived.AllowGrab(true);
        grabableReceived.gameObject.GetComponent<Poolable>().DelObject();
        grabableReceived = null;

        photonView.RPC("PutObjectInTrolleyOnline", RpcTarget.Others, player.photonView.OwnerActorNr);
        player = null;
        onUse = false;

        gastroInHand.transform.position = gastroPos.position;
        gastroInHand.gameObject.SetActive(false);
    }

    [PunRPC]
    void PutObjectInTrolleyOnline(int _actorNumber)
    {
        PlayerController photonPlayer = InGamePhotonManager.Instance.PlayersConnected[_actorNumber];

        Gastro gastroInHand = photonPlayer.pDatas.gastroInHand;
        grabableReceived = gastroInHand.ReleaseObject(false, false, false);

        Aliment playerAliment = grabableReceived.GetComponent<Aliment>();

        alimentStocked.Add(playerAliment);
        grabableReceived.AllowGrab(true);
        grabableReceived.gameObject.GetComponent<Poolable>().DelObject();
        grabableReceived = null;
        onUse = false;

        gastroInHand.transform.position = gastroPos.position;
        gastroInHand.gameObject.SetActive(false);
    }

    public void Begin()
    {
        throw new System.NotImplementedException();
    }

    public void CancelInteraction()
    {

    }

    public void End()
    {
        throw new System.NotImplementedException();
    }

    public void StopInteraction()
    {

    }
}
