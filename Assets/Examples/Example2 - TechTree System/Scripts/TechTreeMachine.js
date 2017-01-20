import Spaghetti;

class TechTreeMachine extends SpaghettiMachine
{
	var mstrGraphPath : String = "SpaghettiMachine/Examples/Example2 - TechTree System/Diagrams/";
	var mstrGraphFile : String = "TechTree01";
	
	private var mPanelInDevelopment : Panel = null;
	private var mfDevelopmentTimer : float;
	private var mfRequiredDevelopmentTime : float;

			
	function Start()
	{
		// Load the graph
		LoadFromFile( mstrGraphPath + mstrGraphFile );
		  
		// Make all panels locked  
		for( var panel : Panel in GetPanels() )
		{
			if( panel.GetPanelType() == "Technology" )
			{
				// Attach a custom variable "status" to the panel and set it's value to "locked"
				panel.SetVariable( "status", "locked" );
			}
		}
		// Find the "Start" panel
		var startpanel : Panel = FindPanelByType("Start");
		startpanel.SetVariable( "s", "5" );
		
		//DELETEME
		startpanel.FindSlot("Unlocks").ActivateConnected();
		startpanel.FindSlot("Unlocks").SetPotential( 1 ) ;
		
		// Find slots connected to the "Start" panel's "Unlocks" slot		
		var aSlots : Slot[] = startpanel.FindSlot("Unlocks").GetConnectedSlots();
		for( slot in aSlots )
		{
			// Mark panel as unlocked
			var panel : Panel = slot.GetPanel();
			panel.SetVariable( "status", "unlocked" );
		}
		
	}
	
	function OnGUI()
	{	
		// List developed technologies
		GUILayout.Label( "Developed:" );
		for( var panel : Panel in GetPanels() )
		{
			if( panel.GetVariable( "status" ) == "finished" )
			{
				// Write the technology's name
				GUILayout.Label( " -"+panel.FindSlot("Name").GetDataString() );
			}
		}
	
		// Twenty pixels of nothingness
		GUILayout.Space(20);
		
		// Is a technology in development?
		if( mPanelInDevelopment != null )
		{
			// Write the technology's name and completion
			var iPercentage : int = 100.0 * mfDevelopmentTimer/mfRequiredDevelopmentTime;		
			GUILayout.Label( "In development:" );			
			GUILayout.Label( mPanelInDevelopment.FindSlot("Name").GetDataString()+": "+iPercentage+"%" );		
		}
		else
		{
			// Display buttons for all unlocked technologies 
			GUILayout.Label( "Available:" );
			for( panel in GetPanels() )
			{
				if( panel.GetVariable( "status" ) == "unlocked" )
				{
					// Display the button
					if( GUILayout.Button( panel.FindSlot("Name").GetDataString() ) )
					{
						// Button pressed: Develop this technology
						mPanelInDevelopment = panel;
						mfDevelopmentTimer = 0.0;
						mfRequiredDevelopmentTime = panel.FindSlot("Time").GetDataFloat();
						panel.SetVariable( "status", "in progress" );
					}
				}		
			}				
		}		
	}
	
	function Update()
	{
		// Is any technology in development?
		if( mPanelInDevelopment != null )
		{
			// Update the timer
			mfDevelopmentTimer += Time.deltaTime;
			if( mfDevelopmentTimer >= mfRequiredDevelopmentTime )
			{
				// Timer finished: Mark panel as finished 
				mPanelInDevelopment.SetVariable( "status", "finished" );
				
				// Maybe unlock new technologies
				UnlockTechnologies( mPanelInDevelopment );
				mPanelInDevelopment = null;
			}
		}
	}
	
	private function UnlockTechnologies( finishedpanel : Panel )
	{
		// Find slots connected to finishedpanel's "Unlocks" slot
		var aSlots : Slot[] = finishedpanel.FindSlot("Unlocks").GetConnectedSlots();
		for( slot in aSlots )
		{
			// The panel _may_ be unlocked, but only if ALL required technologies are developed
			UnlockIfAllRequiredDone( slot.GetPanel() );
		}
	}
	
	private function UnlockIfAllRequiredDone( panel : Panel )
	{
		// Go through all connected input slots
		var aInputSlots : Slot[] = panel.FindSlot("Requires").GetConnectedSlots();
		for( slot in aInputSlots )
		{			
			if( slot.GetPanel().GetVariable( "status" ) != "finished" )
			{
				// One of the required technologies is not finished
				return;
			}
		}
		
		// All required technologies were finished
		panel.SetVariable( "status", "unlocked" );
	}
}