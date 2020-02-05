using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class TreatmentPost : MonoBehaviourPun, IInteractive
{
    [SerializeField] Nominator nominator;

    PlayerController player;
    GrabableObject grabableReceived;
    GrabableObject ObjectWaste;
    GrabableObject ObjectTreated;

    [SerializeField] Transform initPos; // same pos for waste
    [SerializeField] Transform FinalPos;
    [SerializeField] Transform playerPosition;
    [SerializeField] Transform posText3D;

    Coroutine currentStartAction;

    int transformationTime = 3;

    bool haveAnObject = false;
    bool sameState = false;

    string nameObject;

    IEnumerator StartAction(Aliment _aliment, int _timeInSecond, bool instantiateViewID, bool _owner)
    {
        GameManager.Instance.Audio.PlaySound("CuttingCarrot", AudioManager.Canal.SoundEffect);

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
            TreatObject(_aliment, instantiateViewID);

            int objectWasteViewID = ObjectWaste.GetComponent<PhotonView>().ViewID;
            int objectTreatedViewID = ObjectTreated.GetComponent<PhotonView>().ViewID;

            currentStartAction = null;

            photonView.RPC("FinishActionOnline", RpcTarget.Others, _aliment.alimentName, _aliment.alimentState, _timeInSecond, false, objectWasteViewID, objectTreatedViewID);
        }
    }

    [PunRPC]
    private void FinishActionOnline(string _nameAliment, AlimentState _alimentState, int _timeInSecond, bool instantiateViewID, int _objectWasteViewID, int _objectTreatedViewID)
    {
        Aliment aliment = grabableReceived.GetComponent<Aliment>();
        aliment.alimentName = _nameAliment;
        aliment.alimentState = _alimentState;

        TreatObject(aliment, instantiateViewID);

        ObjectWaste.GetComponent<PhotonView>().ViewID = _objectWasteViewID;
        ObjectTreated.GetComponent<PhotonView>().ViewID = _objectTreatedViewID;
        currentStartAction = null;
    }

    enum TypeOftreatment
    {
        Fish,
        Meat,
        Vegetable,
        BOF
    }

    [SerializeField] TypeOftreatment typeOfTreament;

    private void Awake()
    {

        GameManager.Instance.initScripts += Init;
    }

    // Start is called before the first frame update
    void Init()
    {
        string name;
        if (typeOfTreament == TypeOftreatment.Vegetable)
        {
            name = "Fruit/Légume";
        }
        else if (typeOfTreament == TypeOftreatment.Meat)
        {
            name = "Viande";
        }
        else if (typeOfTreament == TypeOftreatment.BOF)
        {
            name = "BOF";
        }
        else
        {
            name = "Poisson";
        }

        nameObject = "Poste de traitement de " + name;
        nominator.customName = nameObject;

        GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Begin()
    {
        throw new System.NotImplementedException();
    }

    public bool CanInteract(PlayerController pController)
    {

        GrabableObject objectInHand = pController.pDatas.objectInHand;
        if(objectInHand != null)
        {
            Gastro gastroInHand = pController.pDatas.gastroInHand;
            Aliment alimentInHand = pController.pDatas.objectInHand.GetComponent<Aliment>();

            if (haveAnObject == false && (gastroInHand != null || alimentInHand != null))
            {
                return true;
            }
        }
        return false;
    }

    public void End()
    {
        throw new System.NotImplementedException();
    }

    public void Interact(PlayerController pController)
    {
        player = pController;
        GrabableObject objectInHand = player.pDatas.objectInHand;
        Gastro gastroInHand = pController.pDatas.gastroInHand;
        // Put object = No object on post and object in hand of player
        if (haveAnObject == false && objectInHand != null)
        {
            if(gastroInHand == null)
            {
                if (grabableReceived == null)
                {
                    Aliment actualAliment = objectInHand.GetComponent<Aliment>();
                    string alimentType = actualAliment.alimentType.ToString();
                    string postType = typeOfTreament.ToString();

                    if (alimentType == postType)
                    {
                        sameState = true;
                    }
                    else
                    {
                        sameState = false;
                    }

                    Debug.Log(alimentType);
                    Debug.Log(postType);
                    Debug.Log(sameState);

                    if (actualAliment != null && (actualAliment.alimentState == AlimentState.InContent || actualAliment.alimentState == AlimentState.Standard))
                    {
                        AlimentObject actualAlimentObject = FoodDatabase.mapAlimentObject[actualAliment.alimentName];

                        bool alimenStateExist = false;
                        for (int i = 0; i < actualAlimentObject.listState.Count; i++)
                        {
                            if(actualAlimentObject.listState[i].state == AlimentState.Clean)
                            {
                                alimenStateExist = true;
                                break;
                            }
                        }

                        if (sameState && alimenStateExist)
                        {
                            grabableReceived = player.pInteract.ReleaseObject(false, false, false);
                            grabableReceived.AllowGrab(false);
                            grabableReceived.AllowPhysic(false);
                            grabableReceived.transform.position = initPos.position;
                            grabableReceived.transform.rotation = initPos.rotation;

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
                            GameManager.Instance.PopUp.CreateText("Cet Objet ne peut pas être déposé ici", 50, new Vector2(0, 300), 3.0f);
                        }

                    }
                    else if (objectInHand != null && haveAnObject == true)
                    {
                        GameManager.Instance.PopUp.CreateText("Il y a actuellement un objet sur le poste", 50, new Vector2(0, 300), 3.0f);
                    }
                    else
                    {
                        GameManager.Instance.PopUp.CreateText("Cet Objet ne peut pas être déposé ici", 50, new Vector2(0, 300), 3.0f);
                    }
                }
            }
            else
            {
                GameManager.Instance.PopUp.CreateText("Il n'est pas possible d'utiliser un gastro ici", 50, new Vector2(0, 300), 3.0f);
            }
            
        }
        else
        {
            GameManager.Instance.PopUp.CreateText("Cet object ne peut pas être déposé ici", 50, new Vector2(0, 300), 3.0f);
        }
    }

    public void StopInteraction()
    {

    }

    void TreatObject(Aliment _aliment, bool instantiateViewID)
    {
        // instantiation of empty aliment (bag, canned food)
        GameObject newWasteAlimentObject = PoolManager.Instance.Get(_aliment.CreateKeyPairValue(_aliment.alimentName, AlimentState.Waste), instantiateViewID).gameObject;
        newWasteAlimentObject.transform.position = initPos.position;
        newWasteAlimentObject.transform.rotation = initPos.rotation;

        ObjectWaste = newWasteAlimentObject.GetComponent<GrabableObject>();

        ObjectWaste.Init();

        ObjectWaste.AllowPhysic(false);
        ObjectWaste.AllowGrab(true);
        ObjectWaste.onGrab += RemoveObject;


        // instantiation of standard aliment (bag, canned food)
        GameObject newTreatedAlimentObject = PoolManager.Instance.Get(_aliment.CreateKeyPairValue(_aliment.alimentName, AlimentState.Clean), instantiateViewID).gameObject;
        newTreatedAlimentObject.transform.position = FinalPos.position;
        newTreatedAlimentObject.transform.rotation = FinalPos.rotation;

        ObjectTreated = newTreatedAlimentObject.GetComponent<GrabableObject>();

        ObjectTreated.Init();

        Aliment alimentStandard = ObjectTreated.GetComponent<Aliment>();

        if (alimentStandard.alimentType == AlimentType.Vegetable || alimentStandard.alimentType == AlimentType.Meat || alimentStandard.alimentType == AlimentType.Fish)
        {
            alimentStandard.alimentStepState = AlimentStepState.Treated;
        }
        else
        {
            alimentStandard.alimentStepState = AlimentStepState.Treated;
        }

        ObjectTreated.AllowPhysic(false);
        ObjectTreated.AllowGrab(true);
        ObjectTreated.onGrab += RemoveObject;

        grabableReceived.AllowGrab(true);
        grabableReceived.gameObject.GetComponent<Poolable>().DelObject();
        grabableReceived = null;

        if (player.photonView.IsMine)
        {
            // Affect the player
            player.EndInteractionState(this);

            GameManager.Instance.PopUp.CreateText("Objet déconditionné", 50, new Vector2(0, 300), 3.0f);
        }
    }



    void RemoveObject(GrabableObject _objectGrab)
    {
        if (_objectGrab == ObjectWaste)
        {
            ObjectWaste = null;
            _objectGrab.onGrab -= RemoveObject;

            if (ObjectTreated == null)
            {
                haveAnObject = false;
            }
        }
        else if (_objectGrab == ObjectTreated)
        {
            ObjectTreated = null;
            _objectGrab.onGrab -= RemoveObject;

            if (ObjectWaste == null)
            {
                haveAnObject = false;
            }
        }
        else
        {
            Debug.LogError("Error Remove Object");
        }
    }

    [PunRPC]
    private void PutObjectInPost(int _actorNumber)
    {
        PlayerController photonPlayer = InGamePhotonManager.Instance.PlayersConnected[_actorNumber].GetComponent<PlayerController>();

        player = photonPlayer;

        grabableReceived = photonPlayer.pInteract.ReleaseObject(false,false, false);
        grabableReceived.AllowGrab(false);
        grabableReceived.transform.position = initPos.position;
        grabableReceived.transform.rotation = initPos.rotation;

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
