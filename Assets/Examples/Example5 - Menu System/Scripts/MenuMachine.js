import Spaghetti;

class MenuMachine extends SpaghettiMachine
{
	var mstrGraphPath : String = "SpaghettiMachine/Examples/Example5 - Menu System/Diagrams/";
	var mstrGraphFile : String = "MyMenuSystem";
	
	var mMenuSkin : GUISkin;
	
	private var mActiveScreen : Panel = null;
	private var mstrScreenTitle : String;
	private var maButtonSlots : Slot[] = new Slot[5];
	
	//public var mAsset : TextAsset;
	
		
	function Start()
	{
		// Load the graph
		LoadFromFile( mstrGraphPath + mstrGraphFile );
		//LoadFromURL("http://dl.dropbox.com/u/12617984/MyMenuSystem.bytes");
		//LoadFromTextAsset( mAsset );
		//LoadFromResources("MyMenuSystem");
	}
		
				
	function OnGraphLoaded()
	{		
		// Find the "Start" panel and activate the connected panel
		FindPanelByType("Start").GetSlot(0).ActivateConnected();		
	}	
	
	// Called when panel activated
	// As this is a SpaghettiMachine of type StateMachine, no more than one panel can be active at a given time
	function OnPanelActivated( panel : Panel )
	{
		
		switch( panel.GetPanelType() )
		{
		case "Screen":
			mActiveScreen = panel;
			maButtonSlots[0] = panel.FindSlot("Button1");
			maButtonSlots[1] = panel.FindSlot("Button2");
			maButtonSlots[2] = panel.FindSlot("Button3");
			maButtonSlots[3] = panel.FindSlot("Button4");
			maButtonSlots[4] = panel.FindSlot("Button5");
			mstrScreenTitle = panel.FindSlot("Title").GetDataString();
			break;
			
		case "Game":
			mActiveScreen = panel;
			break;
								
		case "Exit":
			Application.Quit();
			Debug.Break();
			break;
		}
	}
	
	function OnGUI()
	{
		if( mActiveScreen == null )
		{
			return;
		} 
		
		switch( mActiveScreen.GetPanelType() )
		{
		case "Screen":
			GUI.skin = mMenuSkin;
			GUILayout.BeginArea( Rect( 0, 0, Screen.width, Screen.height) );
			GUILayout.BeginHorizontal();
	   		GUILayout.FlexibleSpace();
	    	GUILayout.BeginVertical();
	    	GUILayout.FlexibleSpace();
	    	
	    	// Display title
	    	GUILayout.Label( mstrScreenTitle );
	    	
			// Go through all button slots
			for( var i : int = 0; i < 5; i++ )
			{
				// Is this slot conected to anything?
				if( maButtonSlots[i].GetNumberOfConnectedSlots() > 0 )
				{
					// Display button
					var bClicked : boolean = GUILayout.Button( maButtonSlots[i].GetDataString() );
					
					// When button clicked, change active screen
					if( bClicked )
					{
						maButtonSlots[i].ActivateConnected();
					}
				}
			} 
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
	    	GUILayout.EndHorizontal();
	    	GUILayout.FlexibleSpace();			
			GUILayout.EndArea(); 
			break;
					
		case "Game":
			GUILayout.Label("Game is running. Press ESC to quit.");
			if( Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape )
			{
				mActiveScreen.FindSlot("Quit").ActivateConnected();
			}
			break;
		}		
	}
			
}