using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(BSName))]
public class DeliveryMan : MonoBehaviourPun, IInteractive
{
    [SerializeField] Transform posText3D;
    [SerializeField] Transform ExitPos;
    [Space]

    [SerializeField] DeliveryMan_UI ui;
    [SerializeField] DeliveryInstantiation deliveryInstantiation;
    [Space]

    [SerializeField] ParticleSystem endParticleSystem;
    bool onUse = false;

    public Dictionary<string, int> playerOrder = new Dictionary<string, int>();
    public Dictionary<string, int> DeliveryManOrder = new Dictionary<string, int>();
    GameManager gameManager;
    PlayerController UseBy;
    BSName bsName;

    string nameObject;

    int timeBeforeLeave = 10;

    float translateTime = 0;

    public enum StateDeliveryMan
    {
        Delivering,
        Changing,
        Leaving
    }

    StateDeliveryMan actualState = StateDeliveryMan.Delivering;

    IEnumerator LeaveKitchen()
    {
        yield return new WaitForSeconds(timeBeforeLeave);
        actualState = StateDeliveryMan.Leaving;
        endParticleSystem.Emit(50);
        //NavMeshAgent agent = GetComponent<NavMeshAgent>();
        //agent.destination = ExitPos.position;

    }


    private void Awake()
    {
        GameManager.Instance.initScripts += Init;
    }

    // Start is called before the first frame update
    void Init()
    {
        gameManager = GameManager.Instance;
        bsName = GetComponent<BSName>();
        bsName.roomValue = BSGridCell.TileEnum.ReceptionMerch;

        for (int i = 0; i < FoodDatabase.mapAlimentObject.Keys.Count; i++)
        {
            string alimentName = FoodDatabase.mapAlimentObject.Keys.ToList()[i];
            int randomAlimentAmount = Random.Range(2, 4);

            playerOrder.Add(alimentName, randomAlimentAmount);

            int randomNumber = Random.Range((-randomAlimentAmount + 1), randomAlimentAmount);

            int NewrandomAlimentAmount = randomAlimentAmount + randomNumber;

            DeliveryManOrder.Add(alimentName, NewrandomAlimentAmount);
        }

        ui.CustomStart(this);

        nameObject = GetComponent<Nominator>().customName;
        GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
    }

    private void Update()
    {
        if (actualState == StateDeliveryMan.Leaving)
        {
            transform.position = Vector3.Lerp(transform.position, ExitPos.position, translateTime / 5);
            translateTime += Time.deltaTime;
        }
    }

    public void Leave()
    {
        StartCoroutine(LeaveKitchen());
    }

    public void StopInteraction()
    {
        if (UseBy != null && UseBy.photonView.IsMine)
        {
            UseBy.EndInteractionState(this);
            gameManager.LockMouse();
            ui.DisplayUI(false);
            UseBy = null;
        }
        onUse = false;
    }

    public void Interact(PlayerController pController)
    {
        if (!PhotonNetwork.IsMasterClient || !pController.photonView.IsMine)
        {
            return;
        }

        switch (actualState)
        {
            case StateDeliveryMan.Delivering:
                Deliver(pController);
                break;
            case StateDeliveryMan.Changing:
                ChangeBox(pController);
                break;
            case StateDeliveryMan.Leaving:
                break;
            default:
                break;
        }

        // ---------- Sound effect interaction

        List<string> keySound = new List<string> { "HumHum1", "HumHum2", "Hum1", "Hum2" };

        // Security, just check if all the string are correct
        if (Application.isEditor)
        {
            foreach (string tmpStr in keySound)
            {
                if (!GameManager.Instance.Audio.Exist(tmpStr))
                    Debug.LogError("Sound effect named " + tmpStr + " is inexistant, change it !");
            }
        }

        GameManager.Instance.Audio.PlaySound(keySound[Random.Range(0, keySound.Count)], AudioManager.Canal.Voice);

    }

    private void Deliver(PlayerController _pController)
    {
        UseBy = _pController;
        _pController.BeginInteractionState(PlayerAnimation.Interaction.Talking);

        gameManager.FreeMouse();
        ui.DisplayUI(true);
        onUse = true;
    }

    private void ChangeBox(PlayerController _pController)
    {
        if (_pController.pDatas.objectInHand != null)
        {
            BoxDatasController boxToCheck = _pController.pDatas.objectInHand.GetComponent<BoxDatasController>();
            if (boxToCheck != null && !boxToCheck.IsClean())
            {
                boxToCheck.expirationTime = 999.9f;
                boxToCheck.state.SetAll(false, false, false);
                boxToCheck.Grabable.dirtinessErrorDetector.Clean(true);
                GameManager.Instance.PopUp.CreateText("Carton échangé en carton neuf", 50, new Vector2(0, 300), 3.0f);
            }
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
        bool interactable = false;
        switch (actualState)
        {
            case StateDeliveryMan.Delivering:
                interactable = !onUse && pController.pDatas.objectInHand == null && InGamePhotonManager.Instance.localPlayerID == 0;
                break;
            case StateDeliveryMan.Changing:
                BoxDatasController box = null;
                if (pController.pDatas.objectInHand != null)
                {
                    box = pController.pDatas.objectInHand.GetComponent<BoxDatasController>();
                }
                interactable = !onUse && box != null && !box.IsClean();
                break;
            case StateDeliveryMan.Leaving:
                break;
            default:
                break;
        }

        return interactable;
    }

    public void ValidateList()
    {
        SearchErrorDelivery();
        GameManager.Instance.Audio.PlaySound("ConveyorBelt", AudioManager.Canal.SoundEffect);

        deliveryInstantiation.FillList();
        StopInteraction();
        actualState = StateDeliveryMan.Changing;
    }

    /// <summary>
    /// Search difference between playerOrder and DeliveryManOrder
    /// </summary>
    private void SearchErrorDelivery()
    {
        List<string> keyList = new List<string>(playerOrder.Keys);

        foreach (string key in keyList)
        {
            if (playerOrder[key] != DeliveryManOrder[key])
            {
                int difference = playerOrder[key] - DeliveryManOrder[key];

                // Generation d'un string personnalisé pour l'erreur
                string str = "Vérification de la commande raté, il y a " + difference + " " + key + " de ";
                str += difference > 0 ? "trop" : "moins";
                str += Mathf.Abs(difference) == 1 ? " que demandé(e)." : " que demandé(e)s.";

                GameManager.Instance.Score.myScore.AddError(Score.NutritionCounter.BadListDelivery, UseBy.GetGridCellPos(), str);

                Debug.Log("Order Error: " + key + " -> " + difference + " times");
            }
        }
    }

    public void CancelInteraction()
    {
        throw new System.NotImplementedException();
    }
}
