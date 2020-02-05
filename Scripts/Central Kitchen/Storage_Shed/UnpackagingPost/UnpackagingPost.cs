using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

public class UnpackagingPost : MonoBehaviourPun, IInteractive
{
    public PlayerController player { get; private set; }

    string nameObject;

    [SerializeField] Transform initPos;
    [SerializeField] Transform FinalPos;
    [SerializeField] Transform posText3D;

    [SerializeField] Transform playerPosition;
    [Space]
    [SerializeField] ParticleSystem smokeParticleSystem;
    [SerializeField] ParticleSystem endParticleSystem;

    GrabableObject grabableReceived;
    GrabableObject ObjectStack;
    GrabableObject ObjectEmpty;

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
        ObjectStack = null;
        player = null;

        nameObject = GetComponent<Nominator>().customName;
        GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
    }

    IEnumerator StartAction(Aliment _aliment, int _timeInSecond, bool instantiateViewID, bool _owner)
    {
        smokeParticleSystem.gameObject.SetActive(true);

        GameManager.Instance.Audio.PlaySound("UnpackingCardboard", AudioManager.Canal.SoundEffect);

        yield return new WaitForSeconds(_timeInSecond);
        if (_owner)// player who make the action (for online)
        {
            FinishAction(_aliment, _timeInSecond, instantiateViewID, _owner);
        }
    }

    private void FinishAction(Aliment _aliment, int _timeInSecond, bool instantiateViewID, bool _owner)
    {
        if(_owner)
        {
            UnpackObject(_aliment, instantiateViewID);
            smokeParticleSystem.gameObject.SetActive(false);
            endParticleSystem.Emit(50);

            int objectEmptyViewID = ObjectEmpty.GetComponent<PhotonView>().ViewID;
            int objectStackViewID = ObjectStack.GetComponent<PhotonView>().ViewID;

            currentStartAction = null;

            photonView.RPC("FinishActionOnline", RpcTarget.Others, _aliment.alimentName, _aliment.alimentState, _timeInSecond, false, objectEmptyViewID, objectStackViewID);
        }
    }

    [PunRPC]
    private void FinishActionOnline(string _nameAliment, AlimentState _alimentState, int _timeInSecond, bool instantiateViewID, int _objectEmptyViewID, int _objectStackViewID)
    {
        Aliment aliment = grabableReceived.GetComponent<Aliment>();
        aliment.alimentName = _nameAliment;
        aliment.alimentState = _alimentState;

        UnpackObject(aliment, instantiateViewID);
        smokeParticleSystem.gameObject.SetActive(false);
        endParticleSystem.Emit(50);

        ObjectEmpty.GetComponent<PhotonView>().ViewID = _objectEmptyViewID;
        ObjectStack.GetComponent<PhotonView>().ViewID = _objectStackViewID;
        currentStartAction = null;
    }

    public void Interact(PlayerController pController)
    {
        player = pController;

        GrabableObject objectInHand = player.pDatas.objectInHand;

#if UNITY_EDITOR
        Debug.Log("objectInHand is NUll :" + (objectInHand == null));
        Debug.Log("boxToUnpack is NUll : " + (grabableReceived == null));
        Debug.Log("boxToGive is NUll: " + (ObjectStack == null));
#endif

        if (objectInHand != null && haveAnObject == false)
        {
            if (grabableReceived == null)
            {
                Aliment actualAliment = objectInHand.GetComponent<Aliment>();

                if (actualAliment != null && actualAliment.alimentState == AlimentState.Box)
                {
					BoxDatasController boxReceived = actualAliment.GetComponent<BoxDatasController>();

					// ERROR CHECK
					if (!boxReceived.IsClean())
					{
                        string str = "Décartonnage d'un carton de " + actualAliment.alimentName + " n'étant ni au norme, ni en bonne état.";
                        GameManager.Instance.Score.myScore.AddError(Score.HygieneCounter.DecartonnageDirtyCardboard, pController.GetGridCellPos(), str);
					}

                    // Put Object on post
                    grabableReceived = player.pInteract.ReleaseObject(false,false);
                    grabableReceived.AllowGrab(false);
                    grabableReceived.transform.position = initPos.position;
                    grabableReceived.transform.rotation = initPos.rotation;

                    haveAnObject = true;

                    Aliment newAliment = grabableReceived.GetComponent<Aliment>();

                    // Affect the player
                    player.TeleportTo(playerPosition, true);
                    player.BeginInteractionState();


                    // transform box into crate
                    if (newAliment != null)
                    {
                        currentStartAction = StartCoroutine(StartAction(newAliment, transformationTime, true, true));
                    }

                    photonView.RPC("PutObjectOnUnpackPost", RpcTarget.Others, player.photonView.OwnerActorNr);
                }
                else
                {
                    GameManager.Instance.PopUp.CreateText("Uniquement les boîtes peuvent être déballées", 50, new Vector2(0, 300), 3.0f);
                }
            }
        }
        else if (objectInHand != null && haveAnObject == true)
        {
            GameManager.Instance.PopUp.CreateText("Il y a actuellement un objet sur le poste", 50, new Vector2(0, 300), 3.0f);
        }
    }

    [PunRPC]
    public void PutObjectOnUnpackPost(int actorNumber)
    {
        PlayerController photonPlayer = InGamePhotonManager.Instance.PlayersConnected[actorNumber];

        player = photonPlayer;

        grabableReceived = photonPlayer.pInteract.ReleaseObject(false,false);
        grabableReceived.AllowGrab(false);
        grabableReceived.transform.position = initPos.position;
        grabableReceived.transform.rotation = initPos.rotation;

        Aliment newAliment = grabableReceived.GetComponent<Aliment>();

        // Affect the player
        photonPlayer.TeleportTo(playerPosition, true);

        if (newAliment != null)
        {
            currentStartAction = StartCoroutine(StartAction(newAliment, transformationTime, false, false));
        }
    }

    [PunRPC]
    public void GetObjectOnUnpackPost(int actorNumber)
    {
        player = InGamePhotonManager.Instance.PlayersConnected[actorNumber].GetComponent<PlayerController>();

        ObjectStack.AllowGrab(true);
        //player.pInteract.GrabObject(boxToGive);
        ObjectStack = null;
    }

    // transform box into crate
    public void UnpackObject(Aliment _aliment, bool instantiateViewID)
    {
        // instantiation of empty aliment (bag, canned food)
        GameObject newEmptyBoxAlimentObject = PoolManager.Instance.Get(_aliment.CreateKeyPairValue(_aliment.alimentName, AlimentState.EmptyBox), instantiateViewID).gameObject;
        newEmptyBoxAlimentObject.transform.position = initPos.position;
        newEmptyBoxAlimentObject.transform.rotation = initPos.rotation;

        ObjectEmpty = newEmptyBoxAlimentObject.GetComponent<GrabableObject>();

        ObjectEmpty.Init();

        ObjectEmpty.AllowPhysic(false);
        ObjectEmpty.AllowGrab(true); // To do : change to false
        ObjectEmpty.onGrab += RemoveObject;


        // instantiation of standard aliment (bag, canned food)
        GameObject newStackAlimentObject = PoolManager.Instance.Get(_aliment.CreateKeyPairValue(_aliment.alimentName, AlimentState.Stack), instantiateViewID).gameObject;
        newStackAlimentObject.transform.position = FinalPos.position;
        newStackAlimentObject.transform.rotation = FinalPos.rotation;

        ObjectStack = newStackAlimentObject.GetComponent<GrabableObject>();

        ObjectStack.Init();

        //Aliment alimentStandard = ObjectStack.GetComponent<Aliment>();



        ObjectStack.AllowPhysic(false);
        ObjectStack.AllowGrab(true);
        ObjectStack.onGrab += RemoveObject;

        grabableReceived.AllowGrab(true);
        grabableReceived.gameObject.GetComponent<Poolable>().DelObject();
        grabableReceived = null;



        if (player.photonView.IsMine)
        {
            // Affect the player
            player.EndInteractionState(this);
            GameManager.Instance.PopUp.CreateText("Carton déballé", 50, new Vector2(0, 300), 3.0f);
        }

        player = null;
    }

    void RemoveObject(GrabableObject _objectGrab)
    {
        if (_objectGrab == ObjectEmpty)
        {
            ObjectEmpty = null;
            _objectGrab.onGrab -= RemoveObject;

            if (ObjectStack == null)
            {
                haveAnObject = false;
            }
        }
        else if (_objectGrab == ObjectStack)
        {
            ObjectStack = null;
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
        if (haveAnObject == false && pController.pDatas.objectInHand != null && pController.pDatas.gastroInHand == null)
        {
            return true;
        }
        return false;
    }

    public void StopInteraction()
    {

    }

    public void CancelInteraction()
    {
        StopCoroutine(currentStartAction);
        smokeParticleSystem.gameObject.SetActive(false);
        if(grabableReceived != null)
        {
            grabableReceived.AllowGrab(true);
        }
        player = null;
        currentStartAction = null;
    }
}
