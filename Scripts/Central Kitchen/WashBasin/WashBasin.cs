using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class WashBasin : MonoBehaviourPun, IInteractive
{

    [SerializeField] Transform playerPosition;
    [SerializeField] Transform posText3D;

    [Space]
    [SerializeField] ParticleSystem waterParticleSystem;
    [SerializeField] ParticleSystem foamParticleSystem;

    PlayerController player;

    int transformationTime = 3;

    string nameObject;

    bool onUse = false;

    private void Awake()
    {
        GameManager.Instance.initScripts += Init;
    }

    // Start is called before the first frame update
    void Init()
    {
        nameObject = GetComponent<Nominator>().customName;
        GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
    }

    IEnumerator WashTime(int _timeInSecond)
    {
        GameManager.Instance.Audio.PlaySound("WashHand1", AudioManager.Canal.SoundEffect);
        waterParticleSystem.gameObject.SetActive(true);
        foamParticleSystem.gameObject.SetActive(true);
        yield return new WaitForSeconds(_timeInSecond);
        WashHand();
        waterParticleSystem.gameObject.SetActive(false);
        foamParticleSystem.gameObject.SetActive(false);
    }

    public void Begin()
    {
        throw new System.NotImplementedException();
    }

    public bool CanInteract(PlayerController pController)
    {
        return onUse == false &&  pController.pDatas.objectInHand == null;
    }

    public void End()
    {
        throw new System.NotImplementedException();
    }

    public void Interact(PlayerController pController)
    {
        player = pController;

        // Affect the player
        player.TeleportTo(playerPosition, true);
        player.BeginInteractionState();
        onUse = true;


        StartCoroutine(WashTime(transformationTime));
        photonView.RPC("WashHandOnline", RpcTarget.Others, pController.photonView.OwnerActorNr);

    }

    public void StopInteraction()
    {

    }

    private void WashHand()
    {
        if (player.photonView.IsMine)
        {
            // Affect the player
            player.EndInteractionState(this);
            GameManager.Instance.PopUp.CreateText("Mains nettoyées", 50, new Vector2(0, 300), 3.0f);
        }

        onUse = false;
        // Faire code pour changement état main sale -> main propre;
    }

    [PunRPC]
    private void WashHandOnline(int _actorNumber)
    {
        PlayerController photonPlayer = InGamePhotonManager.Instance.PlayersConnected[_actorNumber];

        player = photonPlayer;

        onUse = true;

        // Affect the player
        photonPlayer.TeleportTo(playerPosition, true);

        StartCoroutine(WashTime(transformationTime));
    }

    public void CancelInteraction()
    {
        throw new System.NotImplementedException();
    }
}
