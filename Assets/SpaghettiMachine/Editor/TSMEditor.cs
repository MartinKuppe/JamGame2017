

using UnityEditor;
using UnityEngine;
using System.Collections;
using Spaghetti;

// This is kind of a proxy for SpaghettiMachineEditor
// The only raion d'être of this class is the fact that
// EditorWindow.GetWindow can't be called from within a dll.

public class TSMEditor : SpaghettiMachineEditor  
{
	[MenuItem ("The Spaghetti Machine/TSM Editor", false, 1)]
	//------------------------------------------------------------------------------------------------
	// Init
	// called when clicking on the menu entry
	//------------------------------------------------------------------------------------------------
	static void Init() 
	{

		// Get existing open window or if none, make a new one:
		
		TSMEditor window = EditorWindow.GetWindow( typeof( TSMEditor ) ) as TSMEditor;		
		Initialize( window );
		
		window.wantsMouseMove = false;
		
		Object.DontDestroyOnLoad( window );		
	}
	
	[MenuItem ("The Spaghetti Machine/Help/Manual", false, 4)]
	static void OpenManual() 
	{
		Application.OpenURL("file://"+Application.dataPath+"/SpaghettiMachine/Documentation/TheSpaghettiMachine Manual.pdf"); 	
	}
	
	[MenuItem ("The Spaghetti Machine/Help/Questions, feedback, bugs etc.", false, 5)]
	static void LinkSatisfaction() 
	{
		Application.OpenURL("https://getsatisfaction.com/mitm"); 	
	}	
}

