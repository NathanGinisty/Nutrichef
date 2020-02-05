using UnityEngine;
using System.Collections.Generic;

public class GlowObjectCmd : MonoBehaviour
{
    public Color GlowColor;
    public bool enableGlow;

    bool prevEnableGlow;
    private Color _currentColor;
    public List<Renderer> renderersSetMalually = new List<Renderer>();

    public Renderer[] Renderers
    {
        get;
        private set;
    }

    public Color CurrentColor
    {
        get { return _currentColor; }
    }


    void Start()
    {
        if (renderersSetMalually.Count == 0)
        {
            Renderers = GetComponentsInChildren<Renderer>();
        }
        else
        {
            Renderers = renderersSetMalually.ToArray();
        }
        prevEnableGlow = false;

    }

    private void Update()
    {
        if (!_currentColor.Equals(GlowColor))
        {
            _currentColor = GlowColor;

        }
        if (prevEnableGlow != enableGlow)
        {
            prevEnableGlow = enableGlow;
            if (enableGlow)
            {
                _currentColor = GlowColor;
                GlowController.RegisterObject(this);
            }
            else
            {
                _currentColor = Color.black;
                GlowController.DeleteObject(this);
            }
        }
    }

    public void SetColor(Color _color)
    {
        GlowColor = _color;
    }

    public void ActiveGlow()
    {
        GlowController.RegisterObject(this);
        Debug.Log("add glow" + this);
    }

    public void DisableGlow()
    {
        GlowController.DeleteObject(this);
    }
}
