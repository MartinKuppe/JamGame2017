using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

///=================================================================================================================
///                                                                                                       <summary>
///  UIButtonStartApp is attached to a menu button to load the next level in the level list.  
///  Usually the menu ist the first level, the first game level is the second one.                         </summary>
///
///=================================================================================================================
public class UIButtonStartApp : MonoBehaviour
{
    ///-------------------------------------------------------                                                 <summary>
    /// Called by UI                                                                            </summary>
    ///-------------------------------------------------------
    public void OnClicked()
    {
        Application.LoadLevel(Application.loadedLevel + 1);
    }
	
	public void LaunchNamedLevel(string levelName)
	{
		Application.LoadLevel(levelName);
	}
}


