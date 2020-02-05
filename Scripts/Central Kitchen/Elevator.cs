using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class Elevator : MonoBehaviourPun, IInteractive
{
    [SerializeField] Transform playerPosition;
    [SerializeField] Transform posText3D;
    [SerializeField] MeshRenderer light;
    [SerializeField] List<Trolley> trolleys;
    [SerializeField] List<Transform> finalPosTrolley;

    PlayerController player;

    Animator myAnimator;

    string nameObject;

    float movementTime = 2.0f;

    bool usable = true;

    private void Awake()
    {
        GameManager.Instance.initScripts += Init;
    }

    public void Init()
    {
        myAnimator = GetComponent<Animator>();
        nameObject = GetComponent<Nominator>().customName;
        GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
    }

    public bool CanInteract(PlayerController pController)
    {
        return pController.pDatas.objectInHand == null && player == null && usable == true;
    }

    public void Interact(PlayerController pController)
    {
        photonView.RequestOwnership();
        player = pController;
        StartCoroutine(TakeTrolley(true));
    }

    IEnumerator TakeTrolley(bool _user)
    {
        if (_user)
        {
            photonView.RPC("ChangeSTateDoor", RpcTarget.Others);
            player.TeleportTo(playerPosition, true);
            player.BeginInteractionState(false);
        }
        myAnimator.SetBool("open", true);
        yield return new WaitForSeconds(myAnimator.GetCurrentAnimationClip(0).length);
        StartCoroutine(MoveTrolley());
        yield return new WaitForSeconds(2.0f);
        myAnimator.SetBool("open", false);
        yield return new WaitForSeconds(myAnimator.GetCurrentAnimationClip(0).length);
        if (_user)
        {
            player.EndInteractionState(this);
            player = null;
        }
        usable = false;
    }

    [PunRPC]
    void ChangeSTateDoor()
    {
        StartCoroutine(TakeTrolley(false));
    }

    IEnumerator MoveTrolley()
    {
        float timer = 0.0f;
        float pos = 0.0f;
        Vector3 initPosColdTrolley = trolleys[0].transform.position;
        Vector3 initPosHotTrolley = trolleys[1].transform.position;
        trolleys[0].GetComponent<Collider>().enabled = false;
        trolleys[1].GetComponent<Collider>().enabled = false;

        do
        {
            timer += Time.deltaTime;
            pos = timer / movementTime;
            trolleys[0].transform.position = Vector3.Lerp(initPosColdTrolley, finalPosTrolley[0].position, pos);
            trolleys[1].transform.position = Vector3.Lerp(initPosHotTrolley, finalPosTrolley[1].position, pos);

            yield return true;
        } while (timer < movementTime);

        GameManager.Instance.GameSceneManager.endGame = true;

        //photonView.RPC("TakeTrolleyOnline", RpcTarget.Others);

    }

    //[PunRPC]
    //private void TakeTrolleyOnline()
    //{
    //    usable = false;
    //    GameManager.Instance.GameSceneManager.endGame = true;
    //}

    public void Begin()
    {
        throw new System.NotImplementedException();
    }

    public void CancelInteraction()
    {
        throw new System.NotImplementedException();
    }

    public void End()
    {
        throw new System.NotImplementedException();
    }

    public void StopInteraction()
    {

    }
}
