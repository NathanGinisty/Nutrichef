using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
	public delegate void VoidDelegate(); 

	public static bool RandomizeBoolean()
	{
		return Random.Range(0, 2) == 1 ? true : false; 
	}
}
