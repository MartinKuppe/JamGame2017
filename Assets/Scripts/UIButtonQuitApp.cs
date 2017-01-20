using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;



///=================================================================================================================
///                                                                                                       <summary>
///  UIButtonQuitApp is attached to a button to quit the application									 </summary>
///
///=================================================================================================================
public class UIButtonQuitApp : MonoBehaviour 
{
	///-------------------------------------------------------                                                 <summary>
	/// Called by UI                                                                            </summary>
	///-------------------------------------------------------
	public void OnClicked () 
	{
        Application.Quit();
	}
}

