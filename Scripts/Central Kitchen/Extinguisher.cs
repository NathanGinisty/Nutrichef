using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(GrabableObject))]
public class Extinguisher : MonoBehaviourPun, IUsable
{
    public enum State
    {
        Not_Use, Use
    }

    [SerializeField] private State state;
    [SerializeField] float repulseForce;
    [SerializeField] ParticleSystem particles;

    GrabableObject grabable;
    PlayerController pController;

    public bool CanBeUsed(object _useBy)
    {
        return pController != null && pController.pDatas.inInteractionWith.ToMonoBehaviour() == null;
    }

    public void StopUse()
    {
        state = State.Not_Use;
        particles.Stop();

        photonView.RPC("NetworkStopUseExtinguisher", RpcTarget.Others);
    }

    public bool Use(object _useBy)
    {
        state = State.Use;
        if (!particles.isPlaying)
        {
            particles.Play();
        }
        photonView.RPC("NetworkUseExtinguisher", RpcTarget.Others);

        return true;
    }

    private void SetUser(PlayerController _pController)
    {
        pController = _pController;
    }

    private void OnRelease()
    {
        pController = null;
        state = State.Not_Use;
        particles.Stop();
    }

    // Start is called before the first frame update
    private void Start()
    {
        grabable = GetComponent<GrabableObject>();
        grabable.onPlayerGrab += SetUser;
        grabable.onRelease += OnRelease;
        state = State.Not_Use;
        particles.Stop();
    }

    // Update is called once per frame
    private void Update()
    {
        if (pController == null)
        {
            return;
        }

        if (CanBeUsed(pController) && pController.pInputs.GetButton("Use"))
        {
            Use(pController);
        }
        else if (state == State.Use && pController.pInputs.GetButtonUp("Use"))
        {
            StopUse();
        }
    }

    private void FixedUpdate()
    {
        if (state == State.Use)
        {
            Vector3 forceToAdd = transform.forward * repulseForce;
            pController.pMovement.rigidbody.AddForce(forceToAdd, ForceMode.Force);
        }
    }

    [PunRPC]
    private void NetworkUseExtinguisher()
    {
        state = State.Use;
        if (!particles.isPlaying)
        {
            particles.Play();
        }
    }

    [PunRPC]
    private void NetworkStopUseExtinguisher()
    {
        state = State.Not_Use;
        particles.Stop();
    }
}