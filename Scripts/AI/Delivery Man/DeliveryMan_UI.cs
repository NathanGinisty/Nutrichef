using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class DeliveryMan_UI : MonoBehaviour, ICustomBehaviour
{
    DeliveryMan deliveryMan;
    [SerializeField] GameObject uiAnchor;

    [SerializeField] UI_LineOrder DeliveryLine;
    [SerializeField] UI_LineOrder OrderLine;
    Text Designation_Text;
    Text Qte_Carton_Text;

    [SerializeField] Transform ContentOfDelivery;
    [SerializeField] Transform ContentOfOrder;

    GameObject[] childrens;

    int randomBox = 0;


    // start by DeliveryMan
    public void CustomStart(Object _callBy)
    {
        deliveryMan = (DeliveryMan)_callBy;

        // Instantiation of Player order UI
        for (int i = 0; i < deliveryMan.playerOrder.Keys.Count; i++)
        {
            string alimentName = deliveryMan.playerOrder.Keys.ToList()[i];
            int AlimentAmount = deliveryMan.playerOrder[alimentName];

            UI_LineOrder newLine = Instantiate(OrderLine, ContentOfOrder);
            newLine.InitText(alimentName, AlimentAmount);
            // set variable for DeliveryList
            newLine.DeliveryMan = deliveryMan;
            newLine.IndexInList = i;
        }

        // Instantiation of DeliveryMan Delivery UI
        for (int i = 0; i < deliveryMan.DeliveryManOrder.Keys.Count; i++)
        {
            string alimentName = deliveryMan.DeliveryManOrder.Keys.ToList()[i];
            int AlimentAmount = deliveryMan.DeliveryManOrder[alimentName];

            UI_LineOrder newLine = Instantiate(DeliveryLine, ContentOfDelivery);
            newLine.InitText(alimentName, AlimentAmount);
            newLine.DeliveryMan = deliveryMan;
        }
    }

    // Display delivery and order
    public void DisplayUI(bool _state)
    {
        uiAnchor.gameObject.SetActive(_state);
    }

    public void CustomAwake()
    {
        throw new System.NotImplementedException();
    }

    public void CustomUpdate()
    {
        throw new System.NotImplementedException();
    }

    public void CustomFixedUpdate()
    {
        throw new System.NotImplementedException();
    }

    public void CustomLateUpdate()
    {
        throw new System.NotImplementedException();
    }


}
