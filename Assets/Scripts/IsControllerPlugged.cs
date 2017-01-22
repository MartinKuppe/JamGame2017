using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;
using UnityEngine.SceneManagement;


///=================================================================================================================
///                                                                                                       <summary>
///  IsControllerPlugged is a MonoBehaviour that does important stuff. 											 </summary>
///
///=================================================================================================================
public class IsControllerPlugged : MonoBehaviour 
{
    public GameObject _displayWhenControllerPlugged;
    public GameObject _displayWhenNoControllerPlugged;

    public GameObject _activateOnA;
    public GameObject _hideOnA;
    public GameObject _activateOnB;
    public GameObject _hideOnB;
    public string _loadLevelA = "";
    public string _loadLevelB = "";

    public void Start()
    {
        if( Input.GetJoystickNames().Length > 0)
        {
            // Controller is plugged
            if (_displayWhenControllerPlugged != null) _displayWhenControllerPlugged.SetActive(true);
            if (_displayWhenNoControllerPlugged != null) _displayWhenNoControllerPlugged.SetActive(false);
        }
        else
        {
            // No controller is plugged
            if (_displayWhenControllerPlugged != null) _displayWhenControllerPlugged.SetActive(false);
            if (_displayWhenNoControllerPlugged != null) _displayWhenNoControllerPlugged.SetActive(true);
        }
    }

    public void Update()
    {
        if( Input.GetKeyDown( KeyCode.Joystick1Button0))
        {
            if (_activateOnA != null) _activateOnA.SetActive(true);
            if (_hideOnA != null) _hideOnA.SetActive(false);
            if (_loadLevelA != "") SceneManager.LoadScene(_loadLevelA);
        }
        if (Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            if(_activateOnB != null ) _activateOnB.SetActive(true);
            if (_hideOnB != null) _hideOnB.SetActive(false);
            if (_loadLevelB != "") SceneManager.LoadScene(_loadLevelB);
        }
    }
 }

