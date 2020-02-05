using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Gastro : MonoBehaviourPun
{
	public Aliment alimentStocked { get; private set; }
    [SerializeField] Transform posAliment;

	public bool isClean { get; private set; }

	public bool haveObject { get; private set; }

	private void Awake()
	{
		GameManager.Instance.initScripts += Init;
	}

	// Start is called before the first frame update
	void Init()
	{
		isClean = true;
        alimentStocked = null;

    }

	public bool CanTakeGrabable(GrabableObject _grabable)
	{
		Aliment aliment = _grabable.GetComponent<Aliment>();
		bool canGrab = aliment != null;
		return canGrab;
	}

	public bool StockAliment(GrabableObject _grabable, bool _sendOnline)
	{
		if (haveObject == false)
		{
			Debug.Log("Gastro");
			Aliment newAliment = _grabable.GetComponent<Aliment>();
			if (newAliment != null && _grabable.Grab(null, transform, false))
			{
				Debug.Log("Take");
				alimentStocked = newAliment;
				alimentStocked.transform.position = posAliment.transform.position;
				alimentStocked.transform.rotation = posAliment.transform.rotation;

				haveObject = true;
				isClean = false;

				if (_sendOnline)
				{
					photonView.RPC("NetworkStockAliment", RpcTarget.Others, _grabable.photonView.ViewID);
				}
				return true;
			}
			return false;
		}

		return false;
	}

	public GrabableObject ReleaseObject(bool _sendOnline, bool reactivatePhysic = true, bool searchError = true)
	{
		if (haveObject == true)
		{
			GrabableObject newGrabable = alimentStocked.GetComponent<GrabableObject>();
			newGrabable.Release(reactivatePhysic, searchError);
			haveObject = false;
			alimentStocked = null;

			if (_sendOnline)
			{
				photonView.RPC("NetworkReleaseObject", RpcTarget.Others, reactivatePhysic, searchError);
			}

			return newGrabable;
		}
		return null;
	}

	public void Clean()
	{
		isClean = true;
	}

	[PunRPC]
	private void NetworkStockAliment(int _alimentViewID)
	{
		PhotonView objPhotonView = PhotonView.Find(_alimentViewID);
		Aliment newAliment = objPhotonView.GetComponent<Aliment>();
		GrabableObject grabableObject = objPhotonView.GetComponent<GrabableObject>();

		if (newAliment != null && grabableObject.Grab(null, transform, false))
		{
			alimentStocked = newAliment;
			alimentStocked.transform.position = posAliment.transform.position;
			alimentStocked.transform.rotation = posAliment.transform.rotation;
			haveObject = true;
			isClean = false;
		}
	}

	[PunRPC]
	private void NetworkReleaseObject(bool reactivatePhysic, bool searchError)
	{
		GrabableObject newGrabable = alimentStocked.GetComponent<GrabableObject>();
		newGrabable.Release(reactivatePhysic, searchError);
		haveObject = false;
		alimentStocked = null;
	}
}
