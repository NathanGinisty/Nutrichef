using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class OutfitLocker : MonoBehaviourPun, IInteractive
{
    [SerializeField] Transform posText3D;

    string nameObject;

    private void Awake()
    {
        GameManager.Instance.initScripts += Init;
    }

    void Init()
    {
        nameObject = GetComponent<Nominator>().customName;
        GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
    }

    void IInteractive.Begin()
	{
		
	}

	bool IInteractive.CanInteract(PlayerController pController)
	{
		return pController.pDatas.currentOutfit != PlayerDatas.OutfitType.Cook;
	}

	void IInteractive.End()
	{
		
	}

	void IInteractive.Interact(PlayerController pController)
	{
        GameManager.Instance.Audio.PlaySound("EquipCloth", AudioManager.Canal.SoundEffect);
		pController.pAspect.SetType(PlayerDatas.OutfitType.Cook);
	}

	void IInteractive.StopInteraction()
	{
		
	}

    public void CancelInteraction()
    {
        throw new System.NotImplementedException();
    }
}
