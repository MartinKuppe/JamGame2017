using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

///=================================================================================================================
///                                                                                                       <summary>
///  LoadLevelButton is a MonoBehaviour that does important stuff. 											 </summary>
///
///=================================================================================================================
public class LoadLevelButton : MonoBehaviour 
{
    public enum WhichLevel {  Next, Previous, Menu, This };
    public bool _swirlEffect = false;
    public WhichLevel _whichLevel;

	///-------------------------------------------------------                                                 <summary>
	/// Update is called once per frame                                                                            </summary>
	///-------------------------------------------------------
	public void OnClicked() 
	{
        SoundSystem.Play("Click");

        switch(_whichLevel)
        {
            case WhichLevel.Next:
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                break;

            case WhichLevel.Previous:
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
                break;

            case WhichLevel.Menu:
                SceneManager.LoadScene("Menu");
                break;

            case WhichLevel.This:
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex );
                break;
        }

	}
}
