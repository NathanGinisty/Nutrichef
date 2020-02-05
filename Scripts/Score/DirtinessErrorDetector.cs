using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtinessErrorDetector : MonoBehaviour
{
	public bool hasToCheckError;
	public bool isDirty { get; private set; }
	Aliment aliment;
	DirtyController dirtyController;

	// Start is called before the first frame update
	void Start()
	{
		aliment = GetComponent<Aliment>();
		dirtyController = this.GetComponentOnlyInChildren<DirtyController>();
		Clean();
	}

	public void Clean(bool _forceClean = false)
	{
		if (aliment == null || _forceClean)
		{
			isDirty = false;
			SetDirtinessOnMaterial(0f);
		}
	}

	public void SetDirty(float _dirtiness)
	{
		if (_dirtiness > 0f)
		{
			isDirty = true;
			SetDirtinessOnMaterial(_dirtiness);

			if (aliment != null)
			{
				aliment.alimentStepState = AlimentStepState.Dirty;
			}
		}
	}

	private void CheckError()
	{
		isDirty = true;
		SetDirtinessOnMaterial(1f);

		if (aliment != null)
		{
			aliment.alimentStepState = AlimentStepState.Dirty;

			if (aliment.alimentState != AlimentState.Box &&
				aliment.alimentState != AlimentState.EmptyBox &&
				aliment.alimentState != AlimentState.EmptyContent
				&& hasToCheckError)
			{
					
				string str = "Un(e) " + aliment.alimentName + " s'est retrouvé(e) au sol.";
				GameManager.Instance.Score.myScore.AddError(Score.HygieneCounter.FoodOnGround, GetGridCellPos(), str);

#if UNITY_EDITOR
				Debug.Log("ERROR: FoodOnGround -> " + aliment.alimentName + " | " + aliment.alimentState);
#endif
			}
		}

		hasToCheckError = false;
	}

	private void SetDirtinessOnMaterial(float _dirtyness)
	{
		dirtyController.SetDirtyness(_dirtyness);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.GetComponent<BSGridCell>() != null)
		{
			CheckError();
		}
	}

	private Vector3Int GetGridCellPos()
	{
		return (new Vector3(transform.position.x, 0f, transform.position.z) + new Vector3(0.5f, 0, 0.5f) - BSMapCreator.Instance.grid.transform.position).ToVector3Int();
	}
}
