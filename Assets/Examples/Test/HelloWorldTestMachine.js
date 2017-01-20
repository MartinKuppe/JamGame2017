import Spaghetti;

class HelloWorldTestMachine extends SpaghettiMachine
{
	var GraphAsset : TextAsset;
	
	function Start()
	{
		// Load the graph
		LoadFromTextAsset( GraphAsset );
	
		// Find the "Start" panel
		var startpanel : Panel = FindPanelByType("Start");
		
		// Run through the panels
		var panel : Panel = startpanel.FindSlot("Out").GetConnectedSlot().GetPanel();
		var iSecurityCounter = 0;
		while( panel != null && iSecurityCounter++ < 1000 )
		{
			// Print content of "Word" slot
			Debug.Log( panel.FindSlot("Word").GetDataString() );
						
			// Go to panel linked to "Next" slot (if any)			
			panel = panel.FindSlot("Next").GetConnectedPanel();
		} 
	}

}