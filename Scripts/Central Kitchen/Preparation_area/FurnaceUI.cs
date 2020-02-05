using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class FurnaceUI : MonoBehaviour
{
    [SerializeField] Text textTimer;
    [SerializeField] Image foodImage;

    float timer = 10.0f;
    float cooldown = 0;
    bool onUse = false;

    // Update is called once per frame
    void Update()
    {
        if (onUse)
        {
            if (cooldown > 0.0f)
            {
                cooldown -= Time.deltaTime;
                textTimer.text = cooldown.ToString("00:00");
                if (cooldown < 0.0f)
                {
                    cooldown = 0.0f;
                }
            }

            textTimer.transform.LookAt(InGamePhotonManager.Instance.localPlayer.pCamera.Camera.transform);
            textTimer.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    public void DisplayCanvas(bool _state)
    {
        cooldown = timer;
        textTimer.text = cooldown.ToString("00:00");
        textTimer.gameObject.SetActive(_state);
        onUse = _state;
    }

    public void DisplaySpriteFood(Aliment _aliment, bool _state)
    {
        KeyValuePair<string, AlimentState> keyFood = _aliment.CreateKeyPairValue();
        Sprite foodSprite = FoodDatabase.mapSpriteAliment[keyFood];

        if (foodSprite == null)
        {
            Debug.LogError("Sprite for button not found");
        }
        else
        {
            foodImage.sprite = foodSprite;
        }
        foodImage.gameObject.SetActive(_state);
    }
}
