using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "New Aliment", menuName = "Food/AlimentObject")]
public class AlimentObject : ScriptableObject
{
    [System.Serializable]
    public struct InfoState
    {
        public AlimentState state;
        public Object prefab;
        public Sprite sprite;
    }

    [Header("Aliment")]
    public AlimentType type;
    public float t_expiry;
    public List<Nutriment> nutriments;

    [Header("List")]
    public List<InfoState> listState;
}
