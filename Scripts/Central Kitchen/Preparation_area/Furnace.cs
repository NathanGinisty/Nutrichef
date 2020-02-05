using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class Furnace : MonoBehaviourPun, IInteractive
{
    [SerializeField] Transform foodPos; // same pos for waste
    [SerializeField] Transform playerPosition;
    [SerializeField] Transform posText3D;
    [Space]
    [SerializeField] FurnaceUI uiFurnace;
    [SerializeField] MeshRenderer light;
    PlayerController player;

    GrabableObject grabableReceived;
    GrabableObject objectCooked;

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

    float animationtime = 3.0f;
    int transformationTime = 10;

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
        if (haveAnObject == true && pController.pDatas.gastroInHand != null && objectCooked != null)
        {
            return true;
        }
        return false;
    }

    public void End()
    {
        throw new System.NotImplementedException();
    }

    IEnumerator StartAction(Aliment _aliment, int _transformationTime, bool instantiateViewID)
    {
        yield return new WaitForSeconds(animationtime);
        // Affect the player
        player.EndInteractionState(this);
        StartDisplay(true, true);

        GameManager.Instance.PopUp.CreateText("Objet en cours de cuisson", 50, new Vector2(0, 300), 3.0f);
        yield return new WaitForSeconds(_transformationTime);

        // player who make the action (for online)
        FinishAction(_aliment, _transformationTime, instantiateViewID);
    }

    [PunRPC]
    public void StartActionOnline(int _actorNumber)
    {
        Debug.Log("StartActionOnline");
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
    }

    private void FinishAction(Aliment _aliment, int _transformationTime, bool instantiateViewID)
    {
        CookObject(_aliment, instantiateViewID);

        int ObjectCookedViewID = objectCooked.GetComponent<PhotonView>().ViewID;
        currentStartAction = null;

        photonView.RPC("FinishActionOnline", RpcTarget.Others, _aliment.alimentName, (int)_aliment.alimentState, _transformationTime, false, ObjectCookedViewID);
    }

    [PunRPC]
    private void FinishActionOnline(string _nameAliment, int _alimentState, int _timeInSecond, bool instantiateViewID, int _objectCookedViewID)
    {
        Aliment aliment = grabableReceived.GetComponent<Aliment>();
        aliment.alimentName = _nameAliment;
        aliment.alimentState = (AlimentState)_alimentState;

        CookObject(aliment, instantiateViewID);

        objectCooked.GetComponent<PhotonView>().ViewID = _objectCookedViewID;

        currentStartAction = null;
    }

    IEnumerator TakeFood()
    {
        StartDisplay(false, true);
        //Ajouter animation et timing//
        player.TeleportTo(playerPosition, true);
        player.BeginInteractionState();
        yield return new WaitForSeconds(animationtime);
        // Affect the player
        player.EndInteractionState(this);

        if (player.pDatas.gastroInHand != null)
        {
            if (player.pDatas.gastroInHand.StockAliment(objectCooked, true) == false)
            {
                GameManager.Instance.PopUp.CreateText("impossible de mettre " + objectCooked.GetComponent<Aliment>().alimentName + " dans le gastro", 50, new Vector2(0, 300), 3.0f);
            }
        }
        else
        {
            if (player.pInteract.GrabObject(objectCooked, true) == false)
            {
                GameManager.Instance.PopUp.CreateText("impossible de prendre " + objectCooked.GetComponent<Aliment>().alimentName + " dans vos mains", 50, new Vector2(0, 300), 3.0f);
            }
        }
    }


    /// <summary>
    /// Use of Disinfection Post
    /// </summary>
    /// <param name="pController"></param>
    public void Interact(PlayerController pController)
    {
        player = pController;
        GrabableObject objectInHand = pController.pDatas.objectInHand;
        Gastro gastroInHand = player.pDatas.gastroInHand;

        // Put object = No object in furnace and object in hand of player
        if (haveAnObject == false)
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

                if (actualAliment != null && actualAliment.alimentState >= AlimentState.Standard && actualAliment.alimentState <= AlimentState.Sample)
                {
                    AlimentObject actualAlimentObject = FoodDatabase.mapAlimentObject[actualAliment.alimentName];

                    bool alimenStateExist = false;

                    for (int i = 0; i < actualAlimentObject.listState.Count; i++)
                    {
                        if (actualAlimentObject.listState[i].state == AlimentState.Cooked)
                        {
                            alimenStateExist = true;
                            break;
                        }
                    }
                    if (alimenStateExist)
                    {
                        if (gastroInHand != null)
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
                            currentStartAction = StartCoroutine(StartAction(newAliment, transformationTime, true));
                            photonView.RPC("StartActionOnline", RpcTarget.Others, pController.photonView.OwnerActorNr);
                        }
                    }
                    else
                    {
                        GameManager.Instance.PopUp.CreateText("Cet aliment ne peut pas être cuit", 50, new Vector2(0, 300), 3.0f);
                    }

                }
                else
                {
                    GameManager.Instance.PopUp.CreateText("Cet object ne peut pas être déposé ici", 50, new Vector2(0, 300), 3.0f);
                }
            }
        }
        else if (haveAnObject == true && objectInHand != null)
        {
            if (gastroInHand != null && gastroInHand.alimentStocked == null)
            {
                StartCoroutine(TakeFood());
            }
        }
        else if (objectInHand == null && haveAnObject == true)  // Take object = object in furnace and no object in hand of player
        {
            StartCoroutine(TakeFood());
        }
        else if (gastroInHand != null && haveAnObject == true) // put object impossible = object in furnace and object in hand of player
        {
            GameManager.Instance.PopUp.CreateText("Il y a actuellement un objet dans la sauteuse", 50, new Vector2(0, 300), 3.0f);
        }
    }

    /// <summary>
    /// Stop Use of Disinfection Post
    /// </summary>
    public void StopInteraction()
    {

    }

    void CookObject(Aliment _aliment, bool instantiateViewID)
    {
        // instantiation of standard aliment (bag, canned food)
        GameObject newCookedAlimentObject = PoolManager.Instance.Get(_aliment.CreateKeyPairValue(_aliment.alimentName, AlimentState.Cooked), instantiateViewID).gameObject;

        objectCooked = newCookedAlimentObject.GetComponent<GrabableObject>();

        objectCooked.Init();

        Aliment alimentCooked = objectCooked.GetComponent<Aliment>();
        alimentCooked.alimentStepState = _aliment.alimentStepState;

        objectCooked.AllowPhysic(false);
        objectCooked.AllowGrab(true);
        objectCooked.transform.position = foodPos.position;
        objectCooked.transform.rotation = foodPos.rotation;
        objectCooked.onGrab += RemoveObject;

        grabableReceived.AllowGrab(true);
        grabableReceived.gameObject.GetComponent<Poolable>().DelObject();
        grabableReceived = null;
    }

    void RemoveObject(GrabableObject _objectGrab)
    {
        objectCooked = null;
        haveAnObject = false;
        _objectGrab.onGrab -= RemoveObject;
    }

    [PunRPC]
    private void PutObjectInPost(int _actorNumber)
    {
        PlayerController photonPlayer = InGamePhotonManager.Instance.PlayersConnected[_actorNumber];

        player = photonPlayer;

        grabableReceived = photonPlayer.pInteract.ReleaseObject(false,false, false);
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
            currentStartAction = StartCoroutine(StartAction(newAliment, transformationTime, false));
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
        StartDisplay(false, true);
    }

    void StartDisplay(bool _state, bool _user)
    {
        if (_user)
        {
            photonView.RPC("StartDisplayOnline", RpcTarget.Others, _state);
        }
        uiFurnace.DisplayCanvas(_state);
        light.material.color = _state ? Color.red : Color.white;
        Aliment alimentFood;
        if (grabableReceived != null)
        {
            alimentFood = grabableReceived.GetComponent<Aliment>();
        }
        else
        {
            alimentFood = objectCooked.GetComponent<Aliment>();
        }
        uiFurnace.DisplaySpriteFood(alimentFood, _state);
    }

    [PunRPC]
    public void StartDisplayOnline(bool _state)
    {
        StartDisplay(_state, false);
    }

    bool HasObjectInside()
    {
        return objectCooked != null || grabableReceived != null;
    }
}
