import Spaghetti;

class LogicMachine extends SpaghettiMachine
{
	var mstrGraphPath : String = "SpaghettiMachine/Examples/Example3 - Logic Gates/Diagrams/";
	var mstrGraphFile : String = "DoubleNor";

	private var maInputPanels : Array = new Array();
	private var maOutputPanels : Array = new Array();
	
	function Awake()
	{
		// Load the graph
		LoadFromFile( mstrGraphPath + mstrGraphFile );
	}
	
	public function HelloMaster( panel : Panel )
	{
		if( panel.GetPanelType() == "Input" )
		{
			maInputPanels.Add( panel );
		}
		else if( panel.GetPanelType() == "Output" )
		{
			maOutputPanels.Add( panel );
		}	
		UpdatePotential( panel );
	}
	
	public function OnInputPotentialChanged( panel : Panel )
	{
		UpdatePotential( panel );
	}
	
	public function UpdatePotential( panel : Panel )
	{	                              
		var fOutputPotential : float;
		switch( panel.GetPanelType() )
		{
		case "AND":
			fOutputPotential = ( panel.GetSumOfInputPotentials() >= 2.0 ) ? 1.0 : 0.0;
			panel.FindSlot("Out").SetPotential( fOutputPotential ); 
			break;

		case "OR":
			fOutputPotential = ( panel.GetSumOfInputPotentials() >= 1.0 ) ? 1.0 : 0.0;
			panel.FindSlot("Out").SetPotential( fOutputPotential ); 
			break;
			
		case "NOT":
			fOutputPotential = ( panel.GetSumOfInputPotentials() == 0.0 ) ? 1.0 : 0.0;
			panel.FindSlot("Out").SetPotential( fOutputPotential ); 
			break;
			
		case "NAND":
			fOutputPotential = ( panel.GetSumOfInputPotentials() < 2.0 ) ? 1.0 : 0.0;
			panel.FindSlot("Out").SetPotential( fOutputPotential ); 
			break;																																	
		}
	}


	function OnGUI()
	{
		// Input checkboxes
		GUILayout.Label("Inputs:");
		for( var panel : Panel in maInputPanels )
		{
			var bOldValue : boolean = ( panel.GetSlot(0).GetPotential() > 0 );
			var bNewValue = GUILayout.Toggle( bOldValue, panel.GetSlot(0).GetDataString() ); 
			if( bNewValue != bOldValue )
			{
				panel.GetSlot(0).SetPotential( bNewValue ? 1.0 : 0.0 );
			}
		}
		GUILayout.Space(10);
		GUILayout.Label("Outputs:");
		for( var panel : Panel in maOutputPanels )
		{
			var bValue : boolean = ( panel.GetSlot(0).GetPotential() > 0 );
			var strSymbol : String = bValue ? "[x]" : "[ ]"; 
			GUILayout.Label( panel.GetSlot(0).GetDataString() + ": "+ strSymbol );
		}	
	}
}