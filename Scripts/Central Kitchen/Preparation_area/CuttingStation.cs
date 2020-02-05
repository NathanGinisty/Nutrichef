using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class CuttingStation : MonoBehaviourPun, IInteractive
{
    [SerializeField] Transform foodPos; // same pos for waste
    [SerializeField] Transform playerPosition;
    [SerializeField] Transform posText3D;

    PlayerController player;

    GrabableObject grabableReceived;
    GrabableObject objectCutted;

    Coroutine currentStartAction;

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

    bool haveAnObject = false;

    int transformationTime = 3;

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
        GrabableObject objectInHand = pController.pDatas.objectInHand;
        if (objectInHand != null)
        {
            Gastro gastroInHand = pController.pDatas.gastroInHand;
            Aliment alimentInHand = pController.pDatas.objectInHand.GetComponent<Aliment>();

            if (haveAnObject == false && (gastroInHand != null || alimentInHand != null))
            {
                return true;
            }
        }
        else if (haveAnObject == true && pController.pDatas.objectInHand == null && objectCutted != null)
        {
            return true;
        }
        return false;
    }

    public void End()
    {
        throw new System.NotImplementedException();
    }

    IEnumerator StartAction(Aliment _aliment, int _timeInSecond, bool instantiateViewID, bool _owner)
    {
        GameManager.Instance.Audio.PlaySound("FastCuttingCarrot", AudioManager.Canal.SoundEffect);

        yield return new WaitForSeconds(_timeInSecond);
        if (_owner)// player who make the action (for online)
        {
            FinishAction(_aliment, _timeInSecond, instantiateViewID, _owner);
        }
    }

    private void FinishAction(Aliment _aliment, int _timeInSecond, bool instantiateViewID, bool _owner)
    {
        if (_owner)
        {
            CutObject(_aliment, instantiateViewID);

            int ObjectCuttedViewID = objectCutted.GetComponent<PhotonView>().ViewID;
            currentStartAction = null;

            photonView.RPC("FinishActionOnline", RpcTarget.Others, _aliment.alimentName, _aliment.alimentState, _timeInSecond, false, ObjectCuttedViewID);
        }
    }

    [PunRPC]
    private void FinishActionOnline(string _nameAliment, AlimentState _alimentState, int _timeInSecond, bool instantiateViewID, int _objectCuttedViewID)
    {
        Aliment aliment = grabableReceived.GetComponent<Aliment>();
        aliment.alimentName = _nameAliment;
        aliment.alimentState = _alimentState;

        CutObject(aliment, instantiateViewID);

        objectCutted.GetComponent<PhotonView>().ViewID = _objectCuttedViewID;

        currentStartAction = null;
    }

    /// <summary>
    /// Use of Disinfection Post
    /// </summary>
    /// <param name="pController"></param>
    public void Interact(PlayerController pController)
    {
        player = pController;
        GrabableObject objectInHand = player.pDatas.objectInHand;
        Gastro gastroInHand = player.pDatas.gastroInHand;
        // Put object = No object in furnace and object in hand of player
        if (haveAnObject == false && objectInHand != null)
        {
            if (grabableReceived == null)
            {
                Aliment actualAliment;
                if (gastroInHand != null)
                {
                    actualAliment = gastroInHand.alimentStocked;
                }
                else
                {
                    actualAliment = objectInHand.GetComponent<Aliment>();
                }
                if (actualAliment != null && (actualAliment.alimentState == AlimentState.Standard || actualAliment.alimentState == AlimentState.Clean))
                {
                    AlimentObject actualAlimentObject = FoodDatabase.mapAlimentObject[actualAliment.alimentName];

                    bool alimenStateExist = false;

                    for (int i = 0; i < actualAlimentObject.listState.Count; i++)
                    {
                        if (actualAlimentObject.listState[i].state == AlimentState.Cut)
                        {
                            alimenStateExist = true;
                            break;
                        }
                    }
                    if (alimenStateExist)
                    {

                        if (player.pDatas.gastroInHand != null)
                        {
                            grabableReceived = player.pDatas.gastroInHand.ReleaseObject(false, false, false);
                        }
                        else
                        {
                            grabableReceived = player.pInteract.ReleaseObject(false, false, false);
                        }
                        grabableReceived.AllowGrab(false);
                        grabableReceived.transform.position = foodPos.position;
                        grabableReceived.transform.rotation = foodPos.rotation;

                        haveAnObject = true;

                        Aliment newAliment = grabableReceived.GetComponent<Aliment>();

                        // Affect the player
                        player.TeleportTo(playerPosition, true);
                        player.BeginInteractionState();

                        // transform dirty aliment into fresh aliment
                        if (newAliment != null)
                        {
                            GeneralError.ErrorNoOutfit(pController);
                            currentStartAction = StartCoroutine(StartAction(newAliment, transformationTime, true, true));
                        }

                        photonView.RPC("PutObjectInPost", RpcTarget.Others, pController.photonView.OwnerActorNr);
                    }
                    else
                    {
                        GameManager.Instance.PopUp.CreateText("Cet aliment ne peut pas être coupé", 50, new Vector2(0, 300), 3.0f);
                    }
                }
                else
                {
                    GameManager.Instance.PopUp.CreateText("Cet object ne peut pas être déposé ici", 50, new Vector2(0, 300), 3.0f);
                }
            }
        }
        else if (objectInHand == null && haveAnObject == true)  // Take object = object in furnace and no object in hand of player
        {
            if (player.pDatas.gastroInHand != null)
            {
                if (player.pDatas.gastroInHand.StockAliment(objectCutted, true) == false)
                {
                    GameManager.Instance.PopUp.CreateText("impossible de mettre " + objectCutted.GetComponent<Aliment>().alimentName + " dans le gastro", 50, new Vector2(0, 300), 3.0f);
                }
            }
            else
            {
                if (player.pInteract.GrabObject(objectCutted, true) == false)
                {
                    GameManager.Instance.PopUp.CreateText("impossible de prendre " + objectCutted.GetComponent<Aliment>().alimentName + " dans vos mains", 50, new Vector2(0, 300), 3.0f);
                }
            }
        }
        else if (objectInHand != null && haveAnObject == true) // put object impossible = object in furnace and object in hand of player
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

    void CutObject(Aliment _aliment, bool instantiateViewID)
    {
        // instantiation of standard aliment (bag, canned food)
        GameObject newCookedAlimentObject = PoolManager.Instance.Get(_aliment.CreateKeyPairValue(_aliment.alimentName, AlimentState.Cut), instantiateViewID).gameObject;

        objectCutted = newCookedAlimentObject.GetComponent<GrabableObject>();

        objectCutted.Init();

        Aliment alimentCooked = objectCutted.GetComponent<Aliment>();
        alimentCooked.alimentStepState = _aliment.alimentStepState;

        objectCutted.AllowPhysic(false);
        objectCutted.AllowGrab(true);
        objectCutted.transform.position = foodPos.position;
        objectCutted.transform.rotation = foodPos.rotation;
        objectCutted.onGrab += RemoveObject;

        grabableReceived.AllowGrab(true);
        grabableReceived.gameObject.GetComponent<Poolable>().DelObject();
        grabableReceived = null;


        if (player.photonView.IsMine)
        {
            // Affect the player
            player.EndInteractionState(this);
            GameManager.Instance.PopUp.CreateText("Objet coupé", 50, new Vector2(0, 300), 3.0f);
        }
    }

    void RemoveObject(GrabableObject _objectGrab)
    {
        objectCutted = null;
        _objectGrab.onGrab -= RemoveObject;
        haveAnObject = false;
    }

    [PunRPC]
    private void PutObjectInPost(int _actorNumber)
    {
        PlayerController photonPlayer = InGamePhotonManager.Instance.PlayersConnected[_actorNumber];

        player = photonPlayer;

        if (player.pDatas.gastroInHand != null)
        {
            grabableReceived = player.pDatas.gastroInHand.ReleaseObject(false, false, false);
        }
        else
        {
            grabableReceived = player.pInteract.ReleaseObject(false, false, false);
        }
        grabableReceived.AllowGrab(false);
        grabableReceived.transform.position = foodPos.position;
        grabableReceived.transform.rotation = foodPos.rotation;

        haveAnObject = true;

        Aliment newAliment = grabableReceived.GetComponent<Aliment>();

        // Affect the player
        photonPlayer.TeleportTo(playerPosition, true);

        // transform dirty aliment into fresh aliment
        if (newAliment != null)
        {
            currentStartAction = StartCoroutine(StartAction(newAliment, transformationTime, false, false));
        }
    }

    public void CancelInteraction()
    {
        StopCoroutine(currentStartAction);
        if (grabableReceived != null)
        {
            grabableReceived.AllowGrab(true);
        }
        player = null;
        currentStartAction = null;
    }
}
