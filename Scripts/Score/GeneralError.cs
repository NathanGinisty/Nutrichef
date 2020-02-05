using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralError : MonoBehaviour
{
    static public bool ErrorDirtyHand()
    {
        // pas de bool main propre dans le player
        return false;
    }

    static public bool ErrorNoOutfit(PlayerController _useBy)
    {
        if (_useBy.pDatas.currentOutfit != PlayerDatas.OutfitType.Cook)
        {
            string str; str = "Ne porte pas l'uniforme réglementaire.";
            GameManager.Instance.Score.myScore.AddError(Score.HygieneCounter.NoOutfit, _useBy.GetGridCellPos(), str);
            Debug.Log("Error Bad Outfit");
        }

        return false;
    }
}
