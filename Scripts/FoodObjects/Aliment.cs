using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class Aliment : MonoBehaviourPun
{
    public string alimentName;
    public AlimentState alimentState;
    public AlimentType alimentType;
    public ExpiryState expiryState;
    public AlimentStepState alimentStepState;

    public float t_expiry;
    //public Sprite sprite;
    public List<Nutriment> nutriments;

    public void Init(string _name, List<Nutriment> _nutriments)
    {
        alimentName = _name;
        nutriments = _nutriments;
    }

    public void Init(AlimentObject _alimentObject, AlimentState _state = AlimentState.None)
    {
        alimentName = _alimentObject.name;
        alimentType = _alimentObject.type;
        nutriments = _alimentObject.nutriments;
        alimentState = _state;
        alimentStepState = AlimentStepState.Dirty;
        expiryState = ExpiryState.Fresh;
        t_expiry = 9999f;
    }

    public KeyValuePair<string, AlimentState> CreateKeyPairValue()
    {
        return new KeyValuePair<string, AlimentState>(alimentName, alimentState);
    }

    public KeyValuePair<string, AlimentState> CreateKeyPairValue(string _name, AlimentState _state)
    {
        return new KeyValuePair<string, AlimentState>(_name, _state);
    }
}
