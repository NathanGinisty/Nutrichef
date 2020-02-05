using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class Container : MonoBehaviourPun, IInteractive
{
	[SerializeField] Transform posText3D;

	string nameObject;

	enum TypeOfContainer
	{
		All,
		Paper,
		Count
	}

	[SerializeField] TypeOfContainer typeOfContainer;

	private void Awake()
	{
		GameManager.Instance.initScripts += Init;
	}

	private void Init()
	{
		nameObject = GetComponent<Nominator>().customName;
		GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
	}

	//Open fridge and play opening animation
	public void Interact(PlayerController pController)
	{
		GrabableObject handPlayer = pController.pDatas.objectInHand;
		if (handPlayer != null)
		{
			TrashBag trashBag = handPlayer.GetComponent<TrashBag>();
			if (trashBag != null)
			{
				GrabableObject grabableReceived = pController.pInteract.ReleaseObject(false, false, false);
				Destroy(grabableReceived.gameObject);

				photonView.RPC("DestroyTrashBagOnline", RpcTarget.Others, pController.photonView.OwnerActorNr);
			}
			else
			{
				Poolable food = handPlayer.GetComponent<Poolable>();

				if (food != null)
				{
					GameManager.Instance.Audio.PlaySound("Trash", AudioManager.Canal.SoundEffect);
					pController.pInteract.ReleaseObject(true);
					food.photonView.RPC("DelObjectOnline", RpcTarget.Others);
					food.DelObject();
				}
			}
		}
	}

	//Open fridge and play closing animation
	public void StopInteraction()
	{

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
		if (pController.pDatas.objectInHand != null && pController.pDatas.gastroInHand == null)
		{
			return true;
		}

		return false;
	}

	public void CancelInteraction()
	{

	}

	[PunRPC]
	private void DestroyTrashBagOnline(int _ownerID)
	{
		PlayerController _pController = InGamePhotonManager.Instance.PlayersConnected[_ownerID];
		if (_pController != null)
		{
			GrabableObject grabableReceived = _pController.pInteract.ReleaseObject(false, false, false);
			Destroy(grabableReceived.gameObject);
		}
	}
}
