using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class Desk_Interchange : MonoBehaviourPun, IInteractive
{
    PlayerController user = null;
    [SerializeField] Transform posUser;
    [SerializeField] Transform posText3D;

    int _timeInSecond = 1;
    string nameObject;
    bool temperatureWritten = false;

    private void Awake()
    {
        GameManager.Instance.initScripts += Init;
    }

    void Init()
    {
        nameObject = GetComponent<Nominator>().customName;
        GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
    }

    IEnumerator StartAction(PlayerController _pController, bool _owner)
    {
        GameManager.Instance.Audio.PlaySound("Writing", AudioManager.Canal.SoundEffect);

        // lancer animation a faire //
        yield return new WaitForSeconds(_timeInSecond);

        EndTemperatureRegistration(_pController, _owner);

    }

    private void EndTemperatureRegistration(PlayerController _pController, bool _owner)
    {
        // stopper animation a faire //
        if (_owner)
        {
            _pController.pDatas.temperatureInMind = false;
            _pController.EndInteractionState(this);
            GameManager.Instance.PopUp.CreateText("Température enregistrée", 50, new Vector2(0, 300), 3.0f);
            user = null;
            photonView.RPC("EndTemperatureRegistrationOnline", RpcTarget.Others);
        }
    }

    public bool CanInteract(PlayerController pController)
    {
        return pController.pDatas.temperatureInMind == true && pController.pDatas.objectInHand == null;
    }

    public void Interact(PlayerController pController)
    {
        CheckTemperature(pController);
    }

    private void CheckTemperature(PlayerController _pController)
    {
        temperatureWritten = true;
        // Affect the player
        user = _pController;
        _pController.TeleportTo(posUser, true);
        _pController.BeginInteractionState();

        int _pControllerViewID = _pController.photonView.OwnerActorNr;
        photonView.RPC("CheckTemperatureOnline", RpcTarget.Others, _pControllerViewID);
        StartCoroutine(StartAction(_pController, true));

    }

    [PunRPC]
    public void CheckTemperatureOnline(int _pControllerViewID)
    {
        temperatureWritten = true;
        PlayerController _pController = InGamePhotonManager.Instance.PlayersConnected[_pControllerViewID];
        user = _pController;
        _pController.TeleportTo(posUser, true);
        //StartCoroutine(StartAction(_pController, false));
    }

    [PunRPC]
    private void EndTemperatureRegistrationOnline()
    {
        user = null;
    }

    public void Begin()
    {
        throw new System.NotImplementedException();
    }

    public void End()
    {
        throw new System.NotImplementedException();
    }

    public void CancelInteraction()
    {

    }

    public void StopInteraction()
    {
        throw new System.NotImplementedException();
    }
}
