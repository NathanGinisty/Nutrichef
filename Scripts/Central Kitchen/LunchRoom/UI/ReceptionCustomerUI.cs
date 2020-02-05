using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReceptionCustomerUI : MonoBehaviour
{
    [SerializeField] Transform anchor;
    [Space]
    [SerializeField] Text customerNameText;
    [SerializeField] Text customerDescriptionText;
    [Space]
    [SerializeField] Image customerPicture;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Display(bool _state)
    {
        anchor.gameObject.SetActive(_state);
    }

    public void SetCustomerData(string _customerName, string _customerDescription, Sprite _customerPicture = null)
    {
        customerNameText.text = _customerName;
        customerDescriptionText.text = _customerDescription;
        customerPicture.sprite = _customerPicture;
    }
}
