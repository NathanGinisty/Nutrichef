using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ColdRoom : MonoBehaviourPun, IInteractive
{
	string nameObject;

	[SerializeField] Transform posText3D;


	public enum TypeOfContent
	{
		MEAT,
		FISH,
		VEGETABLES,
		BOF,
		COUNT
	}

	TypeOfContent typeOfColdRoom = TypeOfContent.MEAT;

	List<KeyValuePair<string, AlimentState>> foodsStocked = new List<KeyValuePair<string, AlimentState>>();
	Dictionary<KeyValuePair<string, AlimentState>, int> limitedFoodsStocked = new Dictionary<KeyValuePair<string, AlimentState>, int>();

	public PlayerController player { get; private set; }

	// Canvas
	Canvas canvasColdRoom;
	GameManager gameManager;
	[SerializeField] ColdRoomUI ui;

	private void Awake()
	{
		GameManager.Instance.initScripts += Init;
	}

	// Start is called before the first frame update
	void Init()
	{
		gameManager = GameManager.Instance;
		canvasColdRoom = GetComponentInChildren<Canvas>();

		nameObject = GetComponent<Nominator>().customName;
		GameManager.Instance.PopUp.CreateText3D(nameObject, 15, posText3D.localPosition, transform);
	}

	// Add food in list:  foodsStocked and add  button in UI
	public void AddFood(KeyValuePair<string, AlimentState> _keyFood, bool _checkError)
	{
		if (!foodsStocked.Contains(_keyFood) || !limitedFoodsStocked.ContainsKey(_keyFood))
		{
			string str;

			switch (_keyFood.Value)
			{
				case AlimentState.Stack:

					foodsStocked.Add(_keyFood);
					ui.AddButton(_keyFood); // ADD BUTTON TO UI
					break;

				case AlimentState.EmptyBox:
					if (_checkError)
					{
						str = "Boite en carton vide rangée dans une chambre froide.";
						GameManager.Instance.Score.myScore.AddError(Score.HygieneCounter.WrongObjInColdRoom, player.GetGridCellPos(), str);
						//Debug.Log("Stockage Error: " + _keyFood.Key + " " + _keyFood.Value);
					}
					break;

				case AlimentState.Box:

					foodsStocked.Add(_keyFood);
					ui.AddButton(_keyFood); // ADD BUTTON TO UI

					if (_checkError)
					{
						str = "Boite en carton sale rangée dans une chambre froide.";
						GameManager.Instance.Score.myScore.AddError(Score.HygieneCounter.WrongObjInColdRoom, player.GetGridCellPos(), str);
						//Debug.Log("Stockage Error: " + _keyFood.Key + " " + _keyFood.Value);
					}
					break;

				default:

					limitedFoodsStocked.Add(_keyFood, 1);
					ui.AddButton(_keyFood); // ADD BUTTON TO UI

					if (_checkError)
					{
						str = "Aliment rangé dans une chambre froide.";
						GameManager.Instance.Score.myScore.AddError(Score.HygieneCounter.WrongObjInColdRoom, player.GetGridCellPos(), str);
						//Debug.Log("Stockage Error: " + _keyFood.Key + " " + _keyFood.Value);
					}
					break;
			}

			GameManager.Instance.Audio.PlaySound("PutObject", AudioManager.Canal.SoundEffect);


		}
	}

	// Remove food from list:  foodsStocked and remove  button from UI
	public void RemoveFood()
	{
		GetFood(true);
	}

	// instantiate food object in hand of player
	public void GetFood(bool _Remove = false)
	{
		KeyValuePair<string, AlimentState> keyFood;
		Poolable foodToGet = null;

		if (_Remove == false) // "se servir" button
		{
			// Take the key of prefab stock in slected button
			keyFood = ui.GetPoolableFromSelectedButton();
			// change state of the prefab stock in selected button if it's a box
			if (keyFood.Value == AlimentState.Box || keyFood.Value == AlimentState.Stack)
			{
				KeyValuePair<string, AlimentState> newKeyFood = new KeyValuePair<string, AlimentState>(keyFood.Key, AlimentState.InContent);
				foodToGet = PoolManager.Instance.Get(newKeyFood, true);
			}
			else // remove object if state different of box or stack (limited food)
			{
				limitedFoodsStocked[keyFood] -= 1;
				if (limitedFoodsStocked[keyFood] == 0)
				{
					if (ui.RemoveButton(out keyFood))
					{
						limitedFoodsStocked.Remove(keyFood);

						foodToGet = PoolManager.Instance.Get(keyFood, true);
					}
				}
				else
				{
					foodToGet = PoolManager.Instance.Get(keyFood, true);
				}

			}
		}
		else// remove object "Tout prendre" button
		{
			if (ui.RemoveButton(out keyFood))
			{
				foodsStocked.Remove(keyFood);
				foodToGet = PoolManager.Instance.Get(keyFood, true);


				if (keyFood.Value == AlimentState.Box)
				{
					foodToGet.GetComponent<BoxDatasController>().Init();
				}
			}
		}

		GrabableObject toGrab = foodToGet.GetComponent<GrabableObject>();
		toGrab.Init();
		toGrab.AllowGrab(true);

		if (!player.pInteract.GrabObject(toGrab, false))
		{
			// player can't grab object.
			Debug.Log("grab");
		}
		else
		{
			photonView.RPC("GetObjectInColdRoom", RpcTarget.Others, keyFood.Key, keyFood.Value, foodToGet.photonView.ViewID, player.photonView.OwnerActorNr, _Remove);
			StopInteraction();

			GameManager.Instance.Audio.PlaySound("GrabCardboard", AudioManager.Canal.SoundEffect);
		}

	}

	//Set Player who use ColdRoom
	public void Interact(PlayerController pController)
	{
		gameManager.FreeMouse();

		player = pController;

		if (player.pDatas.objectInHand == null)
		{
			ui.DisplayUiMenu(); // Display Menu
		}
		else // put object in coldRoom
		{
			Gastro gastroInHand = pController.pDatas.gastroInHand;

			if (gastroInHand == null)
			{
				Aliment foodToAdd = player.pDatas.objectInHand.GetComponent<Aliment>();
				Nominator nominator = player.pDatas.objectInHand.GetComponent<Nominator>();

				if (foodToAdd == null) // EXIT IF NO ALIMENT FOUND
				{
					GameManager.Instance.PopUp.CreateText("Imossible de placer " + nominator.customName + " dans la chambre froide", 50, new Vector2(0, 300), 3.0f);
					return;
				}

				photonView.RPC("PutObjectInColdRoom", RpcTarget.Others, player.photonView.OwnerActorNr);

				if ((foodToAdd.alimentState == AlimentState.Stack || foodToAdd.alimentState == AlimentState.Box)) // unlimited Food
				{
					if (!foodsStocked.Contains(foodToAdd.CreateKeyPairValue()))
					{
						AddFood(foodToAdd.CreateKeyPairValue(), true);
						player.pInteract.ReleaseObject(true, true, false);
						foodToAdd.GetComponent<Poolable>().DelObject(); // !!! chose : put food in pool (or put it on shelf)
						GeneralError.ErrorNoOutfit(pController);
						GameManager.Instance.PopUp.CreateText(nominator.customName + " déposé", 50, new Vector2(0, 300), 3.0f);
					}
					else
					{
						player.pInteract.ReleaseObject(true, true, false);
						foodToAdd.GetComponent<Poolable>().DelObject(); // !!! chose : put food in pool (or put it on shelf)
						GeneralError.ErrorNoOutfit(pController);
						GameManager.Instance.PopUp.CreateText(nominator.customName + " déposé", 50, new Vector2(0, 300), 3.0f);
					}
				}
				else // limited food
				{
					if (!limitedFoodsStocked.ContainsKey(foodToAdd.CreateKeyPairValue())) //first deposit
					{
						AddFood(foodToAdd.CreateKeyPairValue(), true);
						player.pInteract.ReleaseObject(true, true, false);
						foodToAdd.GetComponent<Poolable>().DelObject(); // !!! chose : put food in pool (or put it on shelf)
						GeneralError.ErrorNoOutfit(pController);
						GameManager.Instance.PopUp.CreateText(nominator.customName + " déposé", 50, new Vector2(0, 300), 3.0f);
					}
					else
					{
						limitedFoodsStocked[foodToAdd.CreateKeyPairValue()] += 1;
						player.pInteract.ReleaseObject(true, true, false);
						foodToAdd.GetComponent<Poolable>().DelObject(); // !!! chose : put food in pool (or put it on shelf)
						GeneralError.ErrorNoOutfit(pController);
						GameManager.Instance.PopUp.CreateText(nominator.customName + " déposé", 50, new Vector2(0, 300), 3.0f);
					}
				}
			}
			else
			{
				GameManager.Instance.PopUp.CreateText("Il n'est pas possible de déposer/utiliser un gastro ici", 50, new Vector2(0, 300), 3.0f);
			}
		}
	}

	[PunRPC]
	public void GetObjectInColdRoom(string _alimentName, AlimentState _alimentState, int alimentViewID, int actorNumber, bool _Remove = false)
	{
		PlayerController ownerPlayer = InGamePhotonManager.Instance.PlayersConnected[actorNumber];

		KeyValuePair<string, AlimentState> keyFood;
		keyFood = new KeyValuePair<string, AlimentState>(_alimentName, _alimentState);
		Poolable foodToGet = null;

		if (_Remove == false)
		{

			if (keyFood.Value != AlimentState.Box && keyFood.Value != AlimentState.Stack)
			{
				foodToGet = PoolManager.Instance.Get(keyFood, alimentViewID);
				limitedFoodsStocked[keyFood] -= 1;
				if (limitedFoodsStocked[keyFood] == 0)
				{
					limitedFoodsStocked.Remove(keyFood);
					ui.RemoveButton(keyFood);
				}
			}
			else
			{
				keyFood = new KeyValuePair<string, AlimentState>(_alimentName, AlimentState.InContent);
				foodToGet = PoolManager.Instance.Get(keyFood, alimentViewID);
			}
		}
		else
		{
			foodToGet = PoolManager.Instance.Get(keyFood, alimentViewID);
			foodsStocked.Remove(keyFood);
			ui.RemoveButton(keyFood);

		}

		if (foodToGet != null)
		{
			GrabableObject toGrab = foodToGet.GetComponent<GrabableObject>();
			toGrab.Init();
			toGrab.AllowGrab(true);
			ownerPlayer.pInteract.GrabObject(toGrab, false);
		}
	}

	[PunRPC]
	public void PutObjectInColdRoom(int actorNumber)
	{
		PlayerController ownerPlayer = InGamePhotonManager.Instance.PlayersConnected[actorNumber];
		Aliment foodToAdd = ownerPlayer.pDatas.objectInHand.GetComponent<Aliment>();

		if (foodToAdd.alimentState == AlimentState.Stack || foodToAdd.alimentState == AlimentState.Box) // unlimited Food
		{
			if (!foodsStocked.Contains(foodToAdd.CreateKeyPairValue()))
			{
				AddFood(foodToAdd.CreateKeyPairValue(), false);
				ownerPlayer.pInteract.ReleaseObject(false);

				foodToAdd.GetComponent<Poolable>().DelObject(); // !!! chose : put food in pool (or put it on shelf)
			}
			else
			{
				ownerPlayer.pInteract.ReleaseObject(false);
				foodToAdd.GetComponent<Poolable>().DelObject(); // !!! chose : put food in pool (or put it on shelf)
			}
		}
		else // limited food
		{
			if (!limitedFoodsStocked.ContainsKey(foodToAdd.CreateKeyPairValue())) //first deposit
			{
				AddFood(foodToAdd.CreateKeyPairValue(), false);
				ownerPlayer.pInteract.ReleaseObject(false);

				foodToAdd.GetComponent<Poolable>().DelObject(); // !!! chose : put food in pool (or put it on shelf)
			}
			else
			{
				limitedFoodsStocked[foodToAdd.CreateKeyPairValue()] += 1;
				ownerPlayer.pInteract.ReleaseObject(false);
				foodToAdd.GetComponent<Poolable>().DelObject(); // !!! chose : put food in pool (or put it on shelf)
			}
		}
	}

	public void StopInteraction()
	{
		gameManager.LockMouse();
		ui.HideChoiceMenu();
		ui.HideUiMenu();
	}

	public void SetTypeOfColdRoom(TypeOfContent _typeOfColdRoom)
	{
		typeOfColdRoom = _typeOfColdRoom;
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
		return !ui.IsDisplayed;
	}

	public void CancelInteraction()
	{
		throw new System.NotImplementedException();
	}
}
