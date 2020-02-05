using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GlovesDistributor : MonoBehaviourPun, IInteractive
{
	[SerializeField] Transform posText3D;

	private void Awake()
	{
		GameManager.Instance.initScripts += Init;
	}

	// Start is called before the first frame update
	void Init()
	{
		string nameObject = GetComponent<Nominator>().customName;
		GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
	}

	public void Begin()
	{

	}

	public void CancelInteraction()
	{

	}

	public bool CanInteract(PlayerController pController)
	{
		return pController.pDatas.objectInHand == null;
	}

	public void End()
	{

	}

	public void Interact(PlayerController pController)
	{
		bool hasGlovesEquiped = pController.pDatas.hasGlovesEquiped;
		pController.pAspect.EquipGloves(!hasGlovesEquiped);
		hasGlovesEquiped = !hasGlovesEquiped;

		if (hasGlovesEquiped)
		{
			GameManager.Instance.PopUp.CreateText("Gants équipés", 50, new Vector2(0, 300), 2.5f);
		}
		else
		{
			GameManager.Instance.PopUp.CreateText("Gants enlevés", 50, new Vector2(0, 300), 2.5f);
		}

	}

	public void StopInteraction()
	{

	}
}
