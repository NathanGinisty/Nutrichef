using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI_TextWindow2D : MonoBehaviour
{
    [SerializeField] Image background;
    [SerializeField] Text text;
    [SerializeField] TypeTextWindow type;

    public enum TypeTextWindow
    {
        /// <summary> The window always keep the same determined size, the text will change size to fit the window.</summary>
        WindowPriority,
        /// <summary> The text always keep the same determined size, the window will change size to fit the text.</summary>
        TextPriority,
        COUNT
    }


    void Start()
    {
        ResizeBackground_v2();
    }

    void Update()
    {
        if (Application.isEditor)
        {

        }
    }

    #region PUBLIC

    public void SetAsWindowPriority(string _str, Vector2 _sizeWindow)
    {
        type = TypeTextWindow.WindowPriority;
        background.rectTransform.sizeDelta = _sizeWindow;

        text.rectTransform.anchorMin = new Vector2(0, 0);
        text.rectTransform.anchorMax = new Vector2(1, 1);

        text.rectTransform.offsetMin = new Vector2(30, 30);
        text.rectTransform.offsetMax = new Vector2(-30, -30);
    }

    [System.Obsolete("Don't use that for the moment, its very clunky and might create error!")]
    public void SetAsTextPriority(string _str, int _fontSize)
    {
        type = TypeTextWindow.TextPriority;
        text.resizeTextMinSize = _fontSize;

        text.rectTransform.anchorMin = new Vector2(0, 1);
        text.rectTransform.anchorMax = new Vector2(0, 1);

        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;

        text.rectTransform.sizeDelta = background.rectTransform.sizeDelta;
    }

    public void SetText(string _str, int _fontSize = 0)
    {
        text.text = _str;
        if (_fontSize != 0)
        {
            if (type == TypeTextWindow.TextPriority)
            {
                text.fontSize = _fontSize;
            }
        }

    }

    public void SetPos(Vector2 _pos)
    {
        transform.position = _pos;
    }

    #endregion


    #region PRIVATE

    // ------------ METHOD V3; modification de la taille par rapport a la box

    private void ResizeBackground_v3()
    {
        if (type == TypeTextWindow.TextPriority)
        {

        }
    }

    // ------------ METHOD V1
    private void ResizeBackground()
    {
        int lenghtStr = GetLenghtString(text.text);

        float sizeX = text.rectTransform.sizeDelta.x + text.rectTransform.localPosition.x * 2; // local position to keep decal for each side

        // Calcul temporaire, a modifier, ça va le faire pour le moment
        float sizeY =
            (

            (1 + Mathf.FloorToInt(lenghtStr / text.rectTransform.sizeDelta.x))

            * (35)

            )

        + Mathf.Abs(text.rectTransform.localPosition.y) * 2; // local position to keep decal for each side

        //Debug.Log(1 + Mathf.FloorToInt(lenghtStr / text.rectTransform.sizeDelta.x));

        background.rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
    }

    private int GetLenghtString(string _str)
    {
        int totalLength = 0;

        CharacterInfo characterInfo = new CharacterInfo();
        Font font = text.font;

        font.RequestCharactersInTexture(_str, text.fontSize);

        foreach (char c in _str)
        {
            font.GetCharacterInfo(c, out characterInfo, text.fontSize);

            //Debug.Log(array[i] + " = " + characterInfo.advance);
            totalLength += characterInfo.advance;
        }

        Debug.Log(totalLength);
        return totalLength;
    }


    // ------------ METHOD V2

    private int GetNbLineForString(string _str)
    {
        int totalLength = 0;
        int nbLine = 0;

        CharacterInfo characterInfo = new CharacterInfo();
        Font font = text.font;

        font.RequestCharactersInTexture(_str, text.fontSize);

        foreach (char c in _str)
        {
            font.GetCharacterInfo(c, out characterInfo, text.fontSize);

            totalLength += characterInfo.advance;

            if (totalLength > 425)
            {
                text.text.Insert(0, "OOOOOO");
                totalLength = 0;
                nbLine++;
            }
        }

        Debug.Log(nbLine);
        return nbLine;
    }

    private void ResizeBackground_v2()
    {
        int nbLine = GetNbLineForString(text.text);

        float sizeX = text.rectTransform.sizeDelta.x + Mathf.Abs(text.rectTransform.localPosition.x) * 2;
        float sizeY = (nbLine * 35) + Mathf.Abs(text.rectTransform.localPosition.y) * 2;

        background.rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
    }



    #endregion
}
