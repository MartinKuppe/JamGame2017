import Spaghetti;

class AdventureMachine extends SpaghettiMachine
{
	var mstrGraphPath : String = "SpaghettiMachine/Examples/Example4 - Text Adventure Map/Diagrams/";
	var mstrGraphFile : String = "MyAdventure";
	
	private var mActivePanel : Panel = null; 
	private var maDirectionNames =  [ "NW", "N", "NE", "E", "SE", "S", "SW", "W", "Down", "Up" ]; 
	
	private var NUMBER_OF_DIRECTIONS = 10;
	
	
	// Slots of active panel
	// (to avoid making a dozen of "FindSlot" at every call of OnGUI)
	private var mSlotLocation 	: Slot;
	private var maExitSlots		: Slot[] = new Slot[NUMBER_OF_DIRECTIONS];
	private var mSlotItems 		: Slot;	
	
	// Input text
	private var mstrInput			: String = "";   // For example "NW" or "Look"
	private var mstrFeedback		: String = "";	 // For example "You can't do that."
	 
	function Start()
	{
		// Load the graph
		LoadFromFile( mstrGraphPath + mstrGraphFile );
		
		// Find the "Start" panel
		var startpanel : Panel = FindPanelByType("Start");
		
		// Activate the connected panel
		startpanel.GetSlot(0).ActivateConnected();				
	}

	// Called when panel activated
	// As this is a SpaghettiMachine of type StateMachine, no more than one panel can be active at a given time
	function OnPanelActivated( panel : Panel )
	{
		mActivePanel = panel;
		mSlotLocation 	= panel.FindSlot("Location"); 
		for( var i : int = 0; i < NUMBER_OF_DIRECTIONS; i++ )
		{
			 maExitSlots[i] =  panel.FindSlot( maDirectionNames[i] );
		} 
		mSlotItems  	= panel.FindSlot("Items"); 
	}
	
	function OnGUI()
	{
		if( mActivePanel == null )
		{
			return;
		} 
	     
	    GUILayout.BeginArea( Rect( 0, 0, Screen.width, Screen.height) ); 
	    GUILayout.FlexibleSpace();
	    GUILayout.BeginHorizontal();
	    GUILayout.FlexibleSpace();
	    GUILayout.BeginVertical();   
	      
	    if( mstrFeedback != "" )
	    {
	   		// Print feedback
			GUILayout.Label( mstrFeedback );
		
			GUILayout.Space( 12 );	    
	    }
	    else
	    {  
		    // (1) Print location
			GUILayout.Label("  You are: "+mSlotLocation.GetDataString() );
			
			GUILayout.Space( 12 ); 
			
			// (2) Print exits
			var strExits : String = "nowhere"; 
			// Run through all exit slots
			for( var i : int = 0; i < NUMBER_OF_DIRECTIONS; i++ )
			{ 
				// Is anything connected to this slot ?
				if(  maExitSlots[i].GetNumberOfConnectedSlots() > 0 )
				{   
					// There is an exit in this direction
					if(  strExits == "nowhere" )
					{
						// First found exit
						strExits =  maDirectionNames[i]; 
					}
					else
					{ 
						// Other exits were found before
					 	strExits +=  ", "+maDirectionNames[i]; 
					} 
				}
			} 
			GUILayout.Label("  Exits: "+strExits );
			
			GUILayout.Space( 12 );
			 
			// (3) Print items	
			GUILayout.Label("  You see: " );
			var aConnectedSlots : Slot[] = mSlotItems.GetConnectedSlots();
			if( aConnectedSlots.Length == 0 )
			{
				GUILayout.Label("    Nothing in particular." );
			} 
			else
			{
				for(  var connectedslot : Slot in aConnectedSlots )
				{
					GUILayout.Label(  "    -"+connectedslot.GetPanel().GetSlot(0).GetDataString() );  
				}  
			}  
		}
		
		// Get input 
		GUILayout.BeginHorizontal();
		mstrInput = GUILayout.TextField( mstrInput, GUILayout.MinWidth( 100 ) ).ToUpper();
		if( GUILayout.Button("Ok") && mstrInput != "" )
		{
			 EvaluateInput( mstrInput ); 
			 mstrInput = "";
		} 
		GUILayout.EndHorizontal();
		
		GUILayout.EndVertical();
	    GUILayout.FlexibleSpace();
	    GUILayout.EndHorizontal();
	    GUILayout.FlexibleSpace();		
		GUILayout.EndArea();
	} 
	
	function EvaluateInput( strInput : String )
	{
	 	// Only move commands for now
		for( var i : int = 0; i < NUMBER_OF_DIRECTIONS; i++ )
		{
			if( strInput ==  maDirectionNames[i].ToUpper() )
			{ 
				//Go to the panel connected to this slot
				maExitSlots[i].ActivateConnected(); 
				return;
			}	
		} 
		
		if(  strInput == "LOOK" )
		{
			// Delete whatever feedback message may be displayed instead of the default info
			mstrFeedback = "";
			return;
		}
		
		// Add here evaluation of other commands		
		// ...
		
		// Note that you can't actually modify the graph during runtime, e.g. remove an item panel when taken.
		// But you can for example attach a boolean "taken" as custom variable to each item panel,
		// or even attach item lists as custom variables to the location panels. 
		
		
		// Input couldn't be evaluated
		mstrFeedback = "You can't do that.";
	
	}
	
}