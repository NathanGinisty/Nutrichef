using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class SampleStation : MonoBehaviourPun, IInteractive
{
    public PlayerController player { get; private set; }

    string nameObject;

    [SerializeField] Transform initPos;
    [SerializeField] Transform FinalPos;
    [SerializeField] Transform posText3D;

    [SerializeField] Transform playerPosition;

    GrabableObject grabableReceived;
    GrabableObject objectSample;
    GrabableObject objectCooked;

    bool haveAnObject = false;

    int transformationTime = 3;

    Coroutine currentStartAction;

    private void Awake()
    {
        GameManager.Instance.initScripts += Init;
    }

    // Start is called before the first frame update
    void Init()
    {
        grabableReceived = null;
        objectSample = null;
        player = null;

        nameObject = GetComponent<Nominator>().customName;
        GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
    }

    IEnumerator StartAction(Aliment _aliment, int _timeInSecond, bool instantiateViewID, bool _owner)
    {
        photonView.RPC("PutObjectOnSampleStation", RpcTarget.Others, player.photonView.OwnerActorNr);
        GameManager.Instance.Audio.PlaySound("UnpackingCardboard", AudioManager.Canal.SoundEffect); // A changer !!!//2
        Debug.Log("StartAction");
        yield return new WaitForSeconds(_timeInSecond);
        if (_owner)// player who make the action (for online)
        {
            Debug.Log("owner");
            player.EndInteractionState(this);
            FinishAction(_aliment, _timeInSecond, instantiateViewID, _owner);
        }
    }

    private void FinishAction(Aliment _aliment, int _timeInSecond, bool instantiateViewID, bool _owner)
    {
        if (_owner)
        {
            TakeSample(_aliment, instantiateViewID);

            int objectEmptyViewID = objectCooked.GetComponent<PhotonView>().ViewID;
            int objectStackViewID = objectSample.GetComponent<PhotonView>().ViewID;

            currentStartAction = null;

            photonView.RPC("FinishActionOnline", RpcTarget.Others, _aliment.alimentName, (int)_aliment.alimentState, _timeInSecond, false, objectEmptyViewID, objectStackViewID);
        }
    }

    [PunRPC]
    private void FinishActionOnline(string _nameAliment, int _alimentState, int _timeInSecond, bool instantiateViewID, int _objectEmptyViewID, int _objectStackViewID)
    {

        Gastro gastroOnPost = grabableReceived.GetComponent<Gastro>();
        Aliment aliment;

        if (gastroOnPost != null)
        {
            aliment = gastroOnPost.alimentStocked;
        }
        else
        {
            aliment = grabableReceived.GetComponent<Aliment>();
        }


        aliment.alimentName = _nameAliment;
        aliment.alimentState = (AlimentState)_alimentState;

        TakeSample(aliment, instantiateViewID);

        objectCooked.GetComponent<PhotonView>().ViewID = _objectEmptyViewID;
        objectSample.GetComponent<PhotonView>().ViewID = _objectStackViewID;
        currentStartAction = null;
    }

    public void Interact(PlayerController pController)
    {
        player = pController;

        GrabableObject objectInHand = player.pDatas.objectInHand;
        Gastro gastroInHand = player.pDatas.gastroInHand;
#if UNITY_EDITOR
        Debug.Log("objectInHand is NUll :" + (objectInHand == null));
        Debug.Log("boxToUnpack is NUll : " + (grabableReceived == null));
        Debug.Log("boxToGive is NUll: " + (objectSample == null));
#endif

        if (objectInHand != null && haveAnObject == false)
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

                if (actualAliment != null && (actualAliment.alimentState == AlimentState.Cooked || actualAliment.alimentState == AlimentState.Cut || actualAliment.alimentState == AlimentState.Standard || actualAliment.alimentState == AlimentState.Clean))
                {
                    // Put Object on post
                    grabableReceived = player.pInteract.ReleaseObject(false,false,false);
                    grabableReceived.AllowGrab(false);
                    grabableReceived.transform.position = initPos.position;
                    grabableReceived.transform.rotation = initPos.rotation;

                    haveAnObject = true;

                    // Affect the player
                    player.TeleportTo(playerPosition, true);
                    player.BeginInteractionState();


                    // transform box into crate
                    if (actualAliment != null)
                    {
                        currentStartAction = StartCoroutine(StartAction(actualAliment, transformationTime, true, true));
                    }

                    //photonView.RPC("PutObjectOnSampleStation", RpcTarget.Others, player.photonView.OwnerActorNr);
                }
                else
                {
                    GameManager.Instance.PopUp.CreateText("L'aliment n'est pas préparé", 50, new Vector2(0, 300), 3.0f);
                }
            }
        }
        else if (objectInHand != null && haveAnObject == true)
        {
            GameManager.Instance.PopUp.CreateText("Il y a actuellement un objet sur le poste", 50, new Vector2(0, 300), 3.0f);
        }
    }

    [PunRPC]
    public void PutObjectOnSampleStation(int actorNumber)
    {
        PlayerController photonPlayer = InGamePhotonManager.Instance.PlayersConnected[actorNumber].GetComponent<PlayerController>();

        player = photonPlayer;

        //if (player.pDatas.gastroInHand != null)
        //{
        //    grabableReceived = player.pDatas.gastroInHand.ReleaseObject(false, false, false);
        //}
        //else
        //{
        //    grabableReceived = player.pInteract.ReleaseObject(false, false, false);
        //}
        grabableReceived = player.pInteract.ReleaseObject(false, false, false);
        grabableReceived.AllowGrab(false);
        grabableReceived.transform.position = initPos.position;
        grabableReceived.transform.rotation = initPos.rotation;
        haveAnObject = true;

        Aliment newAliment = grabableReceived.GetComponent<Aliment>();

        // Affect the player
        photonPlayer.TeleportTo(playerPosition, true);

        //if (newAliment != null)
        //{
        //    currentStartAction = StartCoroutine(StartAction(newAliment, transformationTime, false, false));
        //}
    }

    [PunRPC]
    public void GetObjectOnUnpackPost(int actorNumber)
    {
        player = InGamePhotonManager.Instance.PlayersConnected[actorNumber].GetComponent<PlayerController>();

        objectSample.AllowGrab(true);
        //player.pInteract.GrabObject(boxToGive);
        objectSample = null;
    }

    // transform box into crate
    public void TakeSample(Aliment _aliment, bool instantiateViewID)
    {
        objectCooked = grabableReceived;

        objectCooked.AllowPhysic(false);
        objectCooked.AllowGrab(true); // To do : change to false
        objectCooked.onGrab += RemoveObject;


        // instantiation of sample aliment (bag, canned food)
        GameObject newSampleAlimentObject = PoolManager.Instance.Get(_aliment.CreateKeyPairValue(_aliment.alimentName, AlimentState.Sample), instantiateViewID).gameObject;
        newSampleAlimentObject.transform.position = FinalPos.position;
        newSampleAlimentObject.transform.rotation = FinalPos.rotation;

        objectSample = newSampleAlimentObject.GetComponent<GrabableObject>();

        objectSample.Init();

        //Aliment alimentStandard = objectSample.GetComponent<Aliment>();

        objectSample.AllowPhysic(false);
        objectSample.AllowGrab(true);
        objectSample.onGrab += RemoveObject;

        grabableReceived = null;

        if (player.photonView.IsMine)
        {
            // Affect the player
            player.EndInteractionState(this);
            GameManager.Instance.PopUp.CreateText("Echantillon prélevé", 50, new Vector2(0, 300), 3.0f);
        }

        player = null;
    }

    void RemoveObject(GrabableObject _objectGrab)
    {
        if (_objectGrab == objectCooked)
        {
            objectCooked = null;
            _objectGrab.onGrab -= RemoveObject;

            if (objectSample == null)
            {
                haveAnObject = false;
            }
        }
        else if (_objectGrab == objectSample)
        {
            objectSample = null;
            _objectGrab.onGrab -= RemoveObject;

            if (objectCooked == null)
            {
                haveAnObject = false;
            }
        }
        else
        {
            Debug.LogError("Error Remove Object");
        }
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
        return false;
    }

    public void StopInteraction()
    {

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
