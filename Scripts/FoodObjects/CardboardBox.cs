using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardboardBox : MonoBehaviour, IInteractive
{
    public float t_expiry;
    public AlimentObject alimentObject;
    public PlayerController player { get; private set; }

    public Text textInfoUse;

    public void Init(float _t_expiry, AlimentObject _alimentObject)
    {
        t_expiry = _t_expiry;
        alimentObject = _alimentObject;
    }

    public void StopInteraction()
    {
        throw new System.NotImplementedException();
    }

    public void Interact(PlayerController pController)
    {
        player = pController;
		
		if (!player.pInteract.GrabObject(GetComponent<GrabableObject>()))
		{
			Debug.LogError("Can't grab box");
            // player can't grab box.
		}
        else
        {
            textInfoUse.gameObject.SetActive(false);
        }


		Debug.Log("Use");
        //stopUse();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController tmpPC = other.GetComponent<PlayerController>();
        if (tmpPC != null && player != tmpPC)
        {
            textInfoUse.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        textInfoUse.gameObject.SetActive(false);
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
		throw new System.NotImplementedException();
	}

    public void CancelInteraction()
    {
        throw new System.NotImplementedException();
    }
}
