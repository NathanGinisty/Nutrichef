using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(GrabableObject), typeof(PhotonView), typeof(BSName))]
public class BoxDatasController : MonoBehaviourPun, IInteractive
{
	private Canvas canvas;
	private GrabableObject grabable;
	private Aliment aliment;
	private BSName bsName;

	private bool alreadyInit;

	[Header("DATAS")]
	[SerializeField] private bool isActive;
	public string alimentName;
	public float expirationTime;
	public float boxID { get; private set; }

	public BoxState state;
	[Header("INTEFACE")]
	[SerializeField] Text productNameUI;
	[SerializeField] Text expirationTimeUI;
	[SerializeField] Text cardboardStateUI;
	[SerializeField] Text condStateUI;
	[SerializeField] Text alimentStateUI;

	PlayerController pControllerInInteraction;

	public GrabableObject Grabable { get => grabable; }
	public Aliment Aliment { get => aliment; set => aliment = value; }

	public void Init()
	{
		if (!alreadyInit)
		{

			if (this.HaveComponent<BSName>())
			{
				bsName = GetComponent<BSName>();
			}
			else
			{
				bsName = gameObject.AddComponent<BSName>();
			}

			canvas = this.GetComponentOnlyInChildren<Canvas>(true);
			grabable = GetComponent<GrabableObject>();
			Aliment = GetComponent<Aliment>();

			alreadyInit = true;
		}

		boxID = Time.time;

		grabable.Init();
		grabable.AllowGrab(false);
		grabable.AllowPhysic(false);

		canvas.gameObject.SetActive(false);

		alimentName = Aliment.alimentName;
		expirationTime = Aliment.t_expiry;

		state = new BoxState();
		state.Randomize();

		bsName.roomValue = BSGridCell.TileEnum.ReceptionMerch;

		UpdateUI();
	}

	public void GrabBox()
	{
		photonView.RPC("NetworkGrabBox", RpcTarget.All, pControllerInInteraction.photonView.OwnerActorNr);

		pControllerInInteraction.EndInteractionState(this, 0f);
		pControllerInInteraction = null;
		canvas.gameObject.SetActive(false);

		GameManager.Instance.LockMouse();
	}

	public void UpdateUI()
	{
		productNameUI.text = alimentName;
		expirationTimeUI.text = Random.Range(20.0f, 40.0f).ToString("##:##");
		cardboardStateUI.text = state.StateToString(state.isBoxDamaged);
		condStateUI.text = state.StateToString(state.isConditionmentDamaged);
		alimentStateUI.text = state.StateToString(state.isProductDamaged);
	}

	public void SetActive(bool _state)
	{
		isActive = _state;
	}

	public bool IsClean()
	{
		return state.isBoxDamaged == false && state.isConditionmentDamaged == false && state.isProductDamaged == false;
	}

	void IInteractive.Begin()
	{
		throw new System.NotImplementedException();
	}

	bool IInteractive.CanInteract(PlayerController pController)
	{
		return isActive && pControllerInInteraction == null && pController.pDatas.objectInHand == null;
	}

	void IInteractive.End()
	{
		throw new System.NotImplementedException();
	}

	void IInteractive.Interact(PlayerController pController)
	{
		pControllerInInteraction = pController;
		canvas.gameObject.SetActive(true);
		pControllerInInteraction.BeginInteractionState(false);

		GameManager.Instance.FreeMouse();
	}

	void IInteractive.StopInteraction()
	{
		if (!isActive)
		{
			return;
		}

		if (pControllerInInteraction != null)
		{
			pControllerInInteraction.EndInteractionState(this, 0f);

			pControllerInInteraction = null;
		}

		canvas.gameObject.SetActive(false);
	}

	[PunRPC]
	void NetworkGrabBox(int _actorNumber)
	{
		PlayerController photonPlayer = null;
		if (InGamePhotonManager.Instance.PlayersConnected.ContainsKey(_actorNumber))
		{
			 photonPlayer = InGamePhotonManager.Instance.PlayersConnected[_actorNumber];
		}
		else
		{
			photonPlayer = InGamePhotonManager.Instance.localPlayer;
		}
		
		grabable.AllowGrab(true);
		photonPlayer.pInteract.GrabObject(grabable);
		isActive = false;
	}

	public void CancelInteraction()
	{
		throw new System.NotImplementedException();
	}

	[System.Serializable]
	public class BoxState
	{
		public bool isBoxDamaged;
		public bool isConditionmentDamaged;
		public bool isProductDamaged;

		public BoxState(bool _isBoxDamaged, bool _isConditionmentDamaged, bool _isProductDamaged)
		{
			isBoxDamaged = _isBoxDamaged;
			isConditionmentDamaged = _isConditionmentDamaged;
			isProductDamaged = _isProductDamaged;
		}

		public BoxState()
		{
			isBoxDamaged = false;
			isConditionmentDamaged = false;
			isProductDamaged = false;
		}

		public void Randomize()
		{
			isBoxDamaged = Utilities.RandomizeBoolean();
			isConditionmentDamaged = Utilities.RandomizeBoolean();
			isProductDamaged = Utilities.RandomizeBoolean();
		}

		public void SetAll(bool _isBoxDamaged, bool _isConditionmentDamaged, bool _isProductDamaged)
		{
			isBoxDamaged = _isBoxDamaged;
			isConditionmentDamaged = _isConditionmentDamaged;
			isProductDamaged = _isProductDamaged;
		}

		public string StateToString(bool _state)
		{
			string stateStr = "";
			if (_state)
			{
				stateStr += "Mauvais";
			}
			else
			{
				stateStr += "Bon";
			}
			return stateStr;
		}

		public override string ToString()
		{
			string stateStr = "";

			stateStr += "Etat du carton : ";
			if (isBoxDamaged)
			{
				stateStr += "Mauvais";
			}
			else
			{
				stateStr += "Bon";
			}
			stateStr += "\n";
			stateStr += "Etat du conditionnement : ";
			if (isConditionmentDamaged)
			{
				stateStr += "Mauvais";
			}
			else
			{
				stateStr += "Bon";
			}
			stateStr += "\n";
			stateStr += "Etat du produit : ";
			if (isProductDamaged)
			{
				stateStr += "Mauvais";
			}
			else
			{
				stateStr += "Bon";
			}

			return stateStr;
		}
	}
}
