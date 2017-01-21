using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropagandaButton : MonoBehaviour {

    public Button Button;
    public Image ButtonImage;
    public Image Overlay;

    [SerializeField]
    private Propaganda _propaganda;

    public void Init(Propaganda propaganda)
    {
        _propaganda = propaganda;
        Overlay.gameObject.SetActive(false);
        ButtonImage.sprite = _propaganda.Image;
        ButtonImage.type = Image.Type.Simple;
    }

    public void Trigger()
    {
        _propaganda.Trigger();
    }
}
