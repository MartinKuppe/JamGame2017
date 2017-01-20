import Spaghetti;

class DialogHandler extends MonoBehaviour
{	
	var mDialogSkin : GUISkin;
	
	private var mStartDialogPanel 	 : Panel = null; // The "Start Dialog" panel 
	private var mDialogScreenPanel 	 : Panel = null; // The current dialog screen panel 
	
	// Data from the dialog screen panel
	// (We don't want to call FindSlot seven times on each and every OnGUI!)
	private var mstrQuestion : String;
	private var maAnswerSlots : Slot[] = new Slot[5];	
	private var mstrNPCName : String;
	private var mNPCPortrait : Texture;
	
	// DialogHandler is a singleton.
	private static var mInstance : DialogHandler;

	// Access to instance (google "singleton")
	static function GetInstance() : DialogHandler
	{
		return mInstance;
	}
	
	// Initialization		
	function Awake()
	{
		// Assign instance
		mInstance = this;
	}
	
	
	// Called when a StartDialog panel is activated.
	function StartDialog( panel : Panel )
	{
		var npc : NPC = panel.FindSlot("NPC").GetDataGameObject().GetComponent( NPC );
		npc.SetStatus( NPCStatus.Speaking );
		mstrNPCName = npc.mstrName;
		mNPCPortrait = npc.mPortrait;
		
		mStartDialogPanel = panel;
		
		// Note that the dialog system (blue connections) does NOT use the build-in panel activation mechanism.
		// Instead of this, we memorize the current dialog screen panel in mDialogScreenPanel and
		// update this value when the dialog goes on.
		
		OpenDialogScreen( mStartDialogPanel.FindSlot("Dialog").GetConnectedPanel() );
	}
	
	function OpenDialogScreen( screenpanel : Panel )
	{
		switch( screenpanel.GetPanelType() )
		{
		case "DialogScreen":
			mDialogScreenPanel = screenpanel;
			mstrQuestion = mDialogScreenPanel.FindSlot("Question").GetDataString();
			maAnswerSlots[0] = mDialogScreenPanel.FindSlot("Answer1");
			maAnswerSlots[1] = mDialogScreenPanel.FindSlot("Answer2");
			maAnswerSlots[2] = mDialogScreenPanel.FindSlot("Answer3");
			maAnswerSlots[3] = mDialogScreenPanel.FindSlot("Answer4");
			maAnswerSlots[4] = mDialogScreenPanel.FindSlot("Answer5");
			break;

		case "DialogSideEffect":
			mDialogScreenPanel = screenpanel;
			mDialogScreenPanel.FindSlot("Effect").ActivateConnected();
			OpenDialogScreen( mDialogScreenPanel.FindSlot("Continue").GetConnectedPanel() );
			break;
						
		case "EndDialog":
			mDialogScreenPanel = null;
			mStartDialogPanel.FindSlot("When finished").ActivateConnected();
			break;	
		}		
	}
	
	function OnGUI()
	{
		if( mDialogScreenPanel == null )
		{
			return;
		} 
		
		GUI.skin = mDialogSkin;
		GUILayout.BeginArea( Rect( 10, 10, Screen.width-20, Screen.height-20) );
		
		GUILayout.BeginHorizontal();
		GUILayout.Label( mNPCPortrait );
		GUILayout.Label( mstrNPCName + ": \n"+ mstrQuestion );
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace();
		    	
		// Go through all answer slots
		for( var i : int = 0; i < 5; i++ )
		{
			// Is this slot conected to anything?
			if( maAnswerSlots[i].GetNumberOfConnectedSlots() > 0 )
			{
				// Display answer button
				var bClicked : boolean = GUILayout.Button( maAnswerSlots[i].GetDataString() );
				
				// When button clicked...
				if( bClicked )
				{
					// ...procede to connected panel
					OpenDialogScreen( maAnswerSlots[i].GetConnectedPanel() );
				}
			}
		} 
			
		GUILayout.EndArea(); 
	
	}	
	
	function IsDialogRunning() : boolean
	{
		return ( mDialogScreenPanel != null );
	}
}

