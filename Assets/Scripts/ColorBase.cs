using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorBase : MonoBehaviour
{
    public Color BaseAllyColor;
    public Color BaseEnnemyColor;
    public Color OutlineAllyColor;
    public Color OutlineEnnemyColor;

    private Image _base;
    private Outline _outline;

    private bool _ally;
    private bool _highlighted;

    public bool Ally
    {
        get { return _ally; }
        set { _ally = value; UpdateBase(); }
    }

    public bool Highlighted
    {
        get { return _highlighted; }
        set { _highlighted = value; UpdateBase(); }
    }

    void Awake()
    {
        _base = GetComponent<Image>();
        _outline = GetComponent<Outline>();
    }

    private void UpdateBase()
    {
        if(_highlighted)
        {
            _outline.enabled = true;
            _outline.effectColor = _ally ? OutlineAllyColor : OutlineEnnemyColor;
        }
        else
        {
            _outline.enabled = false;
        }

        _base.color = _ally ? BaseAllyColor : BaseEnnemyColor;
    }
}
