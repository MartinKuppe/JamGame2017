using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SwissArmyKnife;

public class PauseMenu : Singleton<PauseMenu> {

    public Button firstSelected;

    private GameObject _firstChild;

    public bool IsShown
    {
        get
        {
            return _firstChild.activeSelf;
        }
    }

    private void Awake()
    {
        _firstChild = transform.GetChild(0).gameObject;
        _firstChild.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Joystick1Button7))
        {
            if(!IsShown)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
    }

    public void Show()
    {
        _firstChild.gameObject.SetActive(true);
        FindObjectOfType<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(firstSelected.gameObject);
        Time.timeScale = 0;
    }

    public void Hide()
    {
        _firstChild.gameObject.SetActive(false);
        PropagandaButtonsPanel.Instance.RefocusControl();
        Time.timeScale = 1;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Retry()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
}
