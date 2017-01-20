using UnityEditor;
using UnityEngine;
using System.Collections;
using Spaghetti;

// This is kind of a proxy for SpaghettiMachineDebugger
// The only raion d'être of this class is the fact that
// EditorWindow.GetWindow can't be called from within a dll.

public class TSMDebugger : SpaghettiMachineDebugger  
{
	[MenuItem ("The Spaghetti Machine/TSM Debugger", false, 2)]
	//------------------------------------------------------------------------------------------------
	// Init
	// called when clicking on the menu entry
	//------------------------------------------------------------------------------------------------
	static void Init() 
	{
		//System.Type[] aDesiredDockNextTo = new System.Type[1] { typeof(TSMEditor) };
			
		// Get existing open window or if none, make a new one:
		TSMDebugger window = EditorWindow.GetWindow( typeof( TSMDebugger ) ) as TSMDebugger;		
		Initialize();
		
		window.wantsMouseMove = false;
		
		Object.DontDestroyOnLoad( window );		
	}
	
	
}

