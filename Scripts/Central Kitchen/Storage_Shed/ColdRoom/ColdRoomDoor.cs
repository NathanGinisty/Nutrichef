using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class ColdRoomDoor : MonoBehaviourPun, IInteractive
{
    Animator animator;

    string animState;

    private void Awake()
    {
        GameManager.Instance.initScripts += Init;
    }

    void Init()
    {
        animator = GetComponent<Animator>();
    }

    public enum DoorState
    {
        Open,
        Opening,
        Close,
        Closing
    }

    DoorState state = DoorState.Close;

    public void Begin()
    {

    }

    public bool CanInteract(PlayerController pController)
    {
        return true;
    }

    public void End()
    {

    }

    public void Interact(PlayerController pController)
    {
        switch (state)
        {
            case DoorState.Open:
                animState = "CloseDoor";
                animator.SetTrigger(animState);
                state = DoorState.Closing;

                break;
            case DoorState.Opening:
                animState = "CloseDoor";
                animator.SetTrigger(animState);
                state = DoorState.Closing;
                break;
            case DoorState.Close:
                animState = "OpenDoor";
                animator.SetTrigger(animState);
                state = DoorState.Opening;
                break;
            case DoorState.Closing:
                animState = "OpenDoor";
                animator.SetTrigger(animState);
                state = DoorState.Opening;
                
                break;
            default:
                break;
        }

        photonView.RPC("UpdateState", RpcTarget.Others, pController.photonView.OwnerActorNr, state, animState);
    }

    [PunRPC]
    public void UpdateState(int _actorNumber, DoorState _doorState, string _animState)
    {
        PlayerController ownerPlayer = InGamePhotonManager.Instance.PlayersConnected[_actorNumber];
        state = _doorState;
        animator.SetTrigger(_animState);
    }

    public void SetState(DoorState _doorState)
    {
        state = _doorState;
    }

    public void StopInteraction()
    {
        
    }

    public void CancelInteraction()
    {
        throw new System.NotImplementedException();
    }
}
