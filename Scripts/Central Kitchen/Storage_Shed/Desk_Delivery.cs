using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class Desk_Delivery : MonoBehaviourPun, IInteractive
{
    PlayerController user = null;
    [SerializeField] Transform posUser;
    [SerializeField] Transform posText3D;

    int _timeInSecond = 1;
    string nameObject;

    public List<float> boxChecked = new List<float>();

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

        yield return new WaitForSeconds(_timeInSecond);
        if (_owner)// player who make the action (for online)
        {
            EndBoxRegistration(_pController, _owner);
        }
    }

    private void EndBoxRegistration(PlayerController _pController, bool _owner)
    {
        if (_owner)
        {
            _pController.pDatas.boxInMind = null;
            _pController.EndInteractionState(this);
            GameManager.Instance.PopUp.CreateText("Carton enregisté", 50, new Vector2(0, 300), 3.0f);
            user = null;

            photonView.RPC("EndBoxRegistrationOnline", RpcTarget.Others);
        }
    }

    public bool CanInteract(PlayerController pController)
    {
        return pController.pDatas.boxInMind != null && pController.pDatas.objectInHand == null && user == null;
    }

    public void Interact(PlayerController pController)
    {
        if (!boxChecked.Contains(pController.pDatas.boxInMind.boxID))
        {
            CheckBox(pController);
        }
        else
        {
            GameManager.Instance.PopUp.CreateText("Carton déjà enregistré", 50, new Vector2(0, 300), 3.0f);
        }
    }

    private void CheckBox(PlayerController _pController)
    {
        BoxDatasController box = _pController.pDatas.boxInMind;
        boxChecked.Add(box.boxID);
        // Affect the player
        user = _pController;
        _pController.TeleportTo(posUser, true);
        _pController.BeginInteractionState();

        photonView.RPC("CheckBoxOnline", RpcTarget.Others, _pController.photonView.OwnerActorNr);
        StartCoroutine(StartAction(_pController, true));
    }

    [PunRPC]
    public void CheckBoxOnline(int _actorNumber)
    {
        PlayerController photonPlayer = InGamePhotonManager.Instance.PlayersConnected[_actorNumber];
        BoxDatasController box = photonPlayer.pDatas.boxInMind;
        boxChecked.Add(box.boxID);
        user = photonPlayer;
        photonPlayer.TeleportTo(posUser, true);
    }

    [PunRPC]
    private void EndBoxRegistrationOnline()
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
