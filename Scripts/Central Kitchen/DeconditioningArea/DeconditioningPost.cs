using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class DeconditioningPost : MonoBehaviourPun, IInteractive
{
    [SerializeField] Transform initPos; // same pos for waste
    [SerializeField] Transform FinalPos;
    [SerializeField] Transform playerPosition;
    [SerializeField] Transform posText3D;

    [Space]
    [SerializeField] ParticleSystem waterParticleSystem;
    [SerializeField] ParticleSystem foamParticleSystem;

    PlayerController player;

    GrabableObject grabableReceived;
    GrabableObject ObjectEmpty;
    GrabableObject ObjectStandard;

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
        if (haveAnObject == false && pController.pDatas.objectInHand != null)
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
        GameManager.Instance.Audio.PlaySound("Washing", AudioManager.Canal.SoundEffect);
        waterParticleSystem.gameObject.SetActive(true);
        foamParticleSystem.gameObject.SetActive(true);
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
            CleanObject(_aliment, instantiateViewID);
            waterParticleSystem.gameObject.SetActive(false);
            foamParticleSystem.gameObject.SetActive(false);

            int objectEmptyViewID = ObjectEmpty.GetComponent<PhotonView>().ViewID;
            int objectStandardViewID = ObjectStandard.GetComponent<PhotonView>().ViewID;
            currentStartAction = null;

            photonView.RPC("FinishActionOnline", RpcTarget.Others, _aliment.alimentName, _aliment.alimentState, _timeInSecond, false, objectEmptyViewID, objectStandardViewID);
        }
    }

    [PunRPC]
    private void FinishActionOnline(string _nameAliment, AlimentState _alimentState, int _timeInSecond, bool instantiateViewID, int _objectEmptyViewID, int _objectStandardViewID)
    {
        Aliment aliment = grabableReceived.GetComponent<Aliment>();
        aliment.alimentName = _nameAliment;
        aliment.alimentState = _alimentState;

        CleanObject(aliment, instantiateViewID);
        waterParticleSystem.gameObject.SetActive(false);
        foamParticleSystem.gameObject.SetActive(false);

        ObjectEmpty.GetComponent<PhotonView>().ViewID = _objectEmptyViewID;
        ObjectStandard.GetComponent<PhotonView>().ViewID = _objectStandardViewID;

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

        // Put object = No object on post and object in hand of player
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
                if (actualAliment != null && actualAliment.alimentState == AlimentState.InContent)
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
                    GameManager.Instance.PopUp.CreateText("Cet object ne peut pas être déposé ici", 50, new Vector2(0, 300), 3.0f);
                }
            }
        }
        else if (objectInHand != null && haveAnObject == true)
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

    void CleanObject(Aliment _aliment, bool instantiateViewID)
    {
        // instantiation of empty aliment (bag, canned food)
        GameObject newEmptyAlimentObject = PoolManager.Instance.Get(_aliment.CreateKeyPairValue(_aliment.alimentName, AlimentState.EmptyContent), instantiateViewID).gameObject;
        newEmptyAlimentObject.transform.position = initPos.position;
        newEmptyAlimentObject.transform.rotation = initPos.rotation;

        ObjectEmpty = newEmptyAlimentObject.GetComponent<GrabableObject>();

        ObjectEmpty.Init();

        ObjectEmpty.AllowPhysic(false);
        ObjectEmpty.AllowGrab(true); // To do : change to false
        ObjectEmpty.onGrab += RemoveObject;


        // instantiation of standard aliment (bag, canned food)
        GameObject newStandardAlimentObject = PoolManager.Instance.Get(_aliment.CreateKeyPairValue(_aliment.alimentName, AlimentState.Standard), instantiateViewID).gameObject;
        newStandardAlimentObject.transform.position = FinalPos.position;
        newStandardAlimentObject.transform.rotation = FinalPos.rotation;

        ObjectStandard = newStandardAlimentObject.GetComponent<GrabableObject>();

        ObjectStandard.Init();

        Aliment alimentStandard = ObjectStandard.GetComponent<Aliment>();

        if (alimentStandard.alimentType == AlimentType.Vegetable || alimentStandard.alimentType == AlimentType.Meat || alimentStandard.alimentType == AlimentType.Fish)
        {
            alimentStandard.alimentStepState = AlimentStepState.Deconditionning;
        }
        else
        {
            alimentStandard.alimentStepState = AlimentStepState.Treated;
        }


        ObjectStandard.AllowPhysic(false);
        ObjectStandard.AllowGrab(true);
        ObjectStandard.onGrab += RemoveObject;

        grabableReceived.AllowGrab(true);
        grabableReceived.gameObject.GetComponent<Poolable>().DelObject();
        grabableReceived = null;



        if (player.photonView.IsMine)
        {
            // Affect the player
            player.EndInteractionState(this);
            GameManager.Instance.PopUp.CreateText("Objet nettoyé", 50, new Vector2(0, 300), 3.0f);
        }
    }

    void RemoveObject(GrabableObject _objectGrab)
    {
        if (_objectGrab == ObjectEmpty)
        {
            ObjectEmpty = null;
            _objectGrab.onGrab -= RemoveObject;

            if (ObjectStandard == null)
            {
                haveAnObject = false;
            }
        }
        else if (_objectGrab == ObjectStandard)
        {
            ObjectStandard = null;
            _objectGrab.onGrab -= RemoveObject;

            if (ObjectEmpty == null)
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
        waterParticleSystem.gameObject.SetActive(false);
        foamParticleSystem.gameObject.SetActive(false);
        if (grabableReceived != null)
        {
            grabableReceived.AllowGrab(true);
        }
        player = null;
        currentStartAction = null;
    }
}
