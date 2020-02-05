using System.Collections;
using UnityEngine;

public class WallCutter : MonoBehaviour
{
	BSNameWall bsNameWall;
	MeshFilter meshFilter;
	MeshRenderer meshRenderer;
	MeshRenderer fadeMeshRenderer;

	const float fadeTime = 0.2f;
	float currentFadeTime;
	float fadeTimeFactor;

	[SerializeField] Mesh fullMesh;
	[SerializeField] Mesh cuttedHalfMesh;
	[SerializeField] Mesh cuttedRightMesh;
	[SerializeField] Mesh cuttedLeftMesh;
	[SerializeField] bool isCutted;
	[SerializeField] bool hasReverseMaterials;
	[SerializeField] bool isFadeInRunning;
	[SerializeField] bool isFadeOutRunning;

	public Mesh FullMesh { get => fullMesh; }
	public Mesh CuttedHalfMesh { get => cuttedHalfMesh; }
	public Mesh CuttedRightMesh { get => cuttedRightMesh; }
	public Mesh CuttedLeftMesh { get => cuttedLeftMesh; }
	public bool IsCutted { get => isCutted; }

	// Start is called before the first frame update
	void Start()
	{
		bsNameWall = GetComponent<BSNameWall>();
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = this.GetComponent<MeshRenderer>();
		fadeMeshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();

		CopyMaterialsToChildren();

		currentFadeTime = fadeTime;

		meshFilter.mesh = fullMesh;
		hasReverseMaterials = false;
	}

	private void LateUpdate()
	{
		if (isFadeInRunning && currentFadeTime < fadeTime)
		{
			currentFadeTime += Time.deltaTime;

			if (currentFadeTime > fadeTime)
			{
				currentFadeTime = fadeTime;
				meshFilter.mesh = fullMesh;
				fadeMeshRenderer.gameObject.SetActive(false);

				if (IsWall() && hasReverseMaterials)
				{
					ReverseMaterials();
				}

				isFadeInRunning = false;
			}

			fadeTimeFactor = currentFadeTime / fadeTime;

			for (int i = 0; i < fadeMeshRenderer.materials.Length; i++)
			{
				fadeMeshRenderer.materials[i].SetFloat("_Alpha", fadeTimeFactor);
			}
		}
		else if (isFadeOutRunning && currentFadeTime > 0f)
		{
			currentFadeTime -= Time.deltaTime;

			if (currentFadeTime < 0f)
			{
				currentFadeTime = 0f;
				fadeMeshRenderer.gameObject.SetActive(false);
				isFadeOutRunning = false;
			}

			fadeTimeFactor = currentFadeTime / fadeTime;

			for (int i = 0; i < fadeMeshRenderer.materials.Length; i++)
			{
				fadeMeshRenderer.materials[i].SetFloat("_Alpha", fadeTimeFactor);
			}
		}
	}

	public bool IsWall()
	{
		return bsNameWall.wallValue == BSNameWall.WallEnum.Wall;
	}

	public bool IsCorner()
	{
		return bsNameWall.wallValue == BSNameWall.WallEnum.Corner;
	}

	public bool HasCuttableNeighbor(BSGridCell.NeighborPosition _position)
	{
		GameObject neighbor = bsNameWall.neighbors[(int)_position];

		if (neighbor != null)
		{
			return neighbor.GetComponent<WallCutter>() != null;
		}

		return false;
	}

	public bool IsNeighborInSameRoom(BSGridCell.NeighborPosition _position)
	{
		bool state = false;

		GameObject neighbor = bsNameWall.neighbors[(int)_position];

		if (neighbor != null)
		{
			BSNameWall neighborBSNameWall = neighbor.GetComponent<BSNameWall>();

			if (neighborBSNameWall != null)
			{
				state = neighborBSNameWall.bsGridCell.tileValue == bsNameWall.bsGridCell.tileValue;
			}
		}

		return state;
	}

	public bool IsIntersectionCorner(BSGridCell.NeighborPosition _position)
	{
		bool state = false;


		switch (_position)
		{
			case BSGridCell.NeighborPosition.Right:
			case BSGridCell.NeighborPosition.Left:
				state = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Back] != null || bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Front] != null;
				break;
			case BSGridCell.NeighborPosition.Front:
			case BSGridCell.NeighborPosition.Back:
				state = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Right] != null || bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Left] != null;
				break;
			default:
				break;
		}

		return state;
	}

	public void Cut()
	{
		if (bsNameWall.wallValue == BSNameWall.WallEnum.Wall)
		{
			CutWall();
		}
		else if (bsNameWall.wallValue == BSNameWall.WallEnum.Corner)
		{
			CutCorner();
		}

		StartFade(false);
	}

	public void Restore()
	{
		if (IsWall())
		{
			RestoreWall();
		}
		else if (IsCorner())
		{
			RestoreCorner();
		}

		StartFade(true);
	}

	public void CutThis(BSGridCell.NeighborPosition _position)
	{
		if (HasNeighbor(_position))
		{
			meshFilter.mesh = cuttedHalfMesh;
			GameObject neighbor = bsNameWall.neighbors[(int)_position];

			StartFade(false);

			isCutted = true;

			if (neighbor == null) { return; }

			WallCutter neighborWallCutter = neighbor.GetComponent<WallCutter>();

			if (IsWall())
			{
				if (neighborWallCutter != null && neighborWallCutter.HasCuttableNeighbor(_position) && !neighborWallCutter.IsIntersectionCorner(_position))
				{
					CutNeighbor(neighbor, _position);
				}
				else
				{
					switch (_position)
					{
						case BSGridCell.NeighborPosition.Right:
							meshFilter.mesh = IsReversed() ? cuttedLeftMesh : cuttedRightMesh;
							break;
						case BSGridCell.NeighborPosition.Left:
							meshFilter.mesh = IsReversed() ? cuttedRightMesh : cuttedLeftMesh;
							break;
						case BSGridCell.NeighborPosition.Front:
							meshFilter.mesh = IsReversed() ? cuttedLeftMesh : cuttedRightMesh;
							break;
						case BSGridCell.NeighborPosition.Back:
							meshFilter.mesh = IsReversed() ? cuttedRightMesh : cuttedLeftMesh;
							break;
						default:
							break;
					}

					if (!hasReverseMaterials)
					{
						ReverseMaterials();
					}
				}
			}
			else if (IsCorner() && !IsIntersectionCorner(_position))
			{
				CutNeighbor(neighbor, _position);
			}
		}
	}

	public void RestoreThis(BSGridCell.NeighborPosition _position)
	{
		//meshFilter.mesh = fullMesh;
		StartFade(true);
		isCutted = false;

		GameObject neighbor = bsNameWall.neighbors[(int)_position];
		RestoreNeighbor(neighbor, _position);

	}

	private void CopyMaterialsToChildren()
	{
		for (int i = 0; i < fadeMeshRenderer.materials.Length; i++)
		{
			fadeMeshRenderer.materials[i].color = meshRenderer.materials[i].color;
		}
	}

	private void CutCorner()
	{
		meshFilter.mesh = cuttedHalfMesh;

		isCutted = true;

		GameObject leftNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Left];
		GameObject rightNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Right];
		GameObject frontNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Front];
		GameObject backNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Back];

		CutNeighbor(leftNeighbor, BSGridCell.NeighborPosition.Left);
		CutNeighbor(rightNeighbor, BSGridCell.NeighborPosition.Right);
		CutNeighbor(frontNeighbor, BSGridCell.NeighborPosition.Front);
		CutNeighbor(backNeighbor, BSGridCell.NeighborPosition.Back);
	}

	private void CutWall()
	{
		meshFilter.mesh = cuttedHalfMesh;

		isCutted = true;

		GameObject leftNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Left];
		GameObject rightNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Right];
		GameObject frontNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Front];
		GameObject backNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Back];

		CutNeighbor(leftNeighbor, BSGridCell.NeighborPosition.Left);
		CutNeighbor(rightNeighbor, BSGridCell.NeighborPosition.Right);
		CutNeighbor(frontNeighbor, BSGridCell.NeighborPosition.Front);
		CutNeighbor(backNeighbor, BSGridCell.NeighborPosition.Back);
	}

	private void RestoreWall()
	{
		//meshFilter.mesh = fullMesh;
		isCutted = false;

		GameObject leftNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Left];
		GameObject rightNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Right];
		GameObject frontNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Front];
		GameObject backNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Back];

		RestoreNeighbor(leftNeighbor, BSGridCell.NeighborPosition.Left);
		RestoreNeighbor(rightNeighbor, BSGridCell.NeighborPosition.Right);
		RestoreNeighbor(frontNeighbor, BSGridCell.NeighborPosition.Front);
		RestoreNeighbor(backNeighbor, BSGridCell.NeighborPosition.Back);
	}

	private void RestoreCorner()
	{
		//meshFilter.mesh = fullMesh;
		isCutted = false;

		// NEIGHBORS

		GameObject leftNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Left];
		GameObject rightNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Right];
		GameObject frontNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Front];
		GameObject backNeighbor = bsNameWall.neighbors[(int)BSGridCell.NeighborPosition.Back];

		RestoreNeighbor(leftNeighbor, BSGridCell.NeighborPosition.Left);
		RestoreNeighbor(rightNeighbor, BSGridCell.NeighborPosition.Right);
		RestoreNeighbor(frontNeighbor, BSGridCell.NeighborPosition.Front);
		RestoreNeighbor(backNeighbor, BSGridCell.NeighborPosition.Back);
	}

	private void CutNeighbor(GameObject _neighbor, BSGridCell.NeighborPosition _position)
	{
		if (_neighbor != null)
		{
			WallCutter NeighborWallCutter = _neighbor.GetComponent<WallCutter>();

			if (NeighborWallCutter != null && ((NeighborWallCutter.IsCorner() && !NeighborWallCutter.IsIntersectionCorner(_position)) || NeighborWallCutter.IsWall()))
			{
				NeighborWallCutter.CutThis(_position);
			}
		}
	}

	private void RestoreNeighbor(GameObject _neighbor, BSGridCell.NeighborPosition _position)
	{
		if (_neighbor != null)
		{
			WallCutter NeighborWallCutter = _neighbor.GetComponent<WallCutter>();

			if (NeighborWallCutter != null && (NeighborWallCutter.IsCorner() || NeighborWallCutter.IsWall()))
			{
				NeighborWallCutter.RestoreThis(_position);
			}
		}
	}

	private void ReverseMaterials()
	{
		Material savedMat1;
		Material[] materials = meshRenderer.materials;
		if (materials.Length >= 3)
		{
			savedMat1 = materials[1];
			materials[1] = materials[2];
			materials[2] = savedMat1;
			meshRenderer.materials = materials;

			hasReverseMaterials = !hasReverseMaterials;
		}
	}

	private void StartFade(bool _in)
	{
		fadeMeshRenderer.gameObject.SetActive(true);
		isFadeOutRunning = !_in;

		isFadeInRunning = _in;

	}

	private bool HasNeighbor(BSGridCell.NeighborPosition _position)
	{
		return (bsNameWall.neighbors[(int)_position] != null);
	}

	private bool HasCuttedNeighbor(BSGridCell.NeighborPosition _position)
	{
		bool state = false;
		GameObject neighbor = bsNameWall.neighbors[(int)_position];
		if (neighbor != null)
		{
			WallCutter wallCutter = neighbor.GetComponent<WallCutter>();
			if (wallCutter != null)
			{
				return wallCutter.IsCutted;
			}
		}

		return state;
	}

	private bool IsReversed()
	{
		return transform.rotation.eulerAngles.y >= -90f && transform.rotation.eulerAngles.y < 91.0f;
	}

}
