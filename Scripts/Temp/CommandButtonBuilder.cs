using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandButtonBuilder : MonoBehaviour
{
    [Header("Brush")]
    public List<Sprite> brushSprites = new List<Sprite>();
    public Image brushSpriteRenderer;

    [Space]
    [Header("Panels")]
    public List<GameObject> ModPaint = new List<GameObject>();
    public List<GameObject> ModPaintDefault = new List<GameObject>();
    public List<GameObject> ModObject = new List<GameObject>();
    public List<GameObject> ModObjectDefault = new List<GameObject>();
    [Space]
    public CommandFurnitureButtonBuilder buttonBuilder;

    int currentBrushSize = 0;

    public enum State
    {
        paint,
        objec
    }
    
    public void SwitchMod(int _state)
    {
        if ((State)_state == State.paint)
        {
            ModObject.ForEach(x => x.SetActive(false));
            ModPaintDefault.ForEach(x => x.SetActive(true));
        }
        else
        {
            ModPaint.ForEach(x => x.SetActive(false));
            ModObjectDefault.ForEach(x => x.SetActive(true));
            buttonBuilder.SetPanelActive(0);
        }
    }

    public void AddToBrushSize(int _value)
    {
        currentBrushSize += _value;

        currentBrushSize = Mathf.Clamp(currentBrushSize, 0, 2);
        brushSpriteRenderer.sprite = brushSprites[currentBrushSize];
    }
}
