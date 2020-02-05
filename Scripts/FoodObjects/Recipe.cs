using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Food/Recipe")]
public class Recipe : ScriptableObject
{
    [Header("Aliments Of the Recipe")]
    public List<FoodDatabase.AlimentOfRecipe> listAlimentsOfRecipe;

    [Header("Prefab")]
    public Object prefab;

}
