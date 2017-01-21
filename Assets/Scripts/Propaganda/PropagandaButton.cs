using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropagandaButton : MonoBehaviour {

    public Button Button;
    public Image ButtonImage;
    public Image Overlay;
    public bool active = false;

    [SerializeField]
    private Propaganda _propaganda;

    public void Init(Propaganda propaganda)
    {
        _propaganda = propaganda;
        ButtonImage.sprite = _propaganda.Poster;
        ButtonImage.type = Image.Type.Simple;
    }

    public void Trigger()
    {
        _propaganda.Trigger();
    }

    public void Display()
    {
        PropagandaButtonsPanel.Instance.DisableOthers(this);

        active = !active;
        PropagandaDescription.SetPropaganda(active ? _propaganda : null);
    }

    public void Disable()
    {
        active = false;
    }
}
