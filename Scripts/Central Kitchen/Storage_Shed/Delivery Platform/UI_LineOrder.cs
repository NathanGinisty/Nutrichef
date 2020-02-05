using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_LineOrder : MonoBehaviour
{
    [SerializeField] Text boxCountText;
    [SerializeField] Text designationText;
    string alimentName;
    DeliveryMan deliveryMan;
    int indexInList;

    public Text BoxCountText { get => boxCountText;}
    public Text DesignationText { get => designationText;}
    public DeliveryMan DeliveryMan { get => deliveryMan; set => deliveryMan = value; }
    public int IndexInList { get => indexInList; set => indexInList = value; }
    public string AlimentName { get => alimentName; set => alimentName = value;}

    // use by DeliveryMan
    public void InitText(string _alimentName, int _NbBox)
    {
        AlimentName = _alimentName;
        BoxCountText.text = _NbBox.ToString();
        DesignationText.text = _alimentName;
    }

    public void AddBox()
    {
        if (deliveryMan.DeliveryManOrder[alimentName] < 10)
        {
            deliveryMan.DeliveryManOrder[alimentName] += 1;
            boxCountText.text = deliveryMan.DeliveryManOrder[alimentName].ToString();
        }
    }

    public void RemoveBox()
    {
        if(deliveryMan.DeliveryManOrder[alimentName] >1)
        {
            deliveryMan.DeliveryManOrder[alimentName] -= 1;
            boxCountText.text = deliveryMan.DeliveryManOrder[alimentName].ToString();
        }
    }
}
