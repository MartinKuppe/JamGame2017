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

    void Update()
    {
        float percent = _propaganda.Overlay;

        if (percent == 0)
        {
            Overlay.gameObject.SetActive(false);
        }
        else
        {
            Overlay.gameObject.SetActive(true);
            Overlay.fillAmount = percent;
        }
    }

    public void Hover()
    {
        PropagandaHover.Instance.SetHover(_propaganda, transform.position);
    }

    public void OnPointerExit()
    {
        PropagandaHover.Instance.Hide();
    }

    public void Display()
    {
        PropagandaButtonsPanel.Instance.DisableOthers(this);

        active = !active;
        PropagandaDescription.SetPropaganda(active ? _propaganda : null);

        SoundSystem.Play("SelectPropaganda");

        PropagandaHover.Instance.Hide();
    }

    public void Disable()
    {
        active = false;
    }
}
