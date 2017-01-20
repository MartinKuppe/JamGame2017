import Spaghetti;

class GameMachine extends SpaghettiMachine
{
	var mstrGraphPath : String = "SpaghettiMachine/Examples/Example6 - Dialog and Interaction System/Diagrams/";
	var mstrGraphFile : String = "MyGame";

	// GameMachine is a singleton.
	private static var mInstance : GameMachine;

	// Access to instance (google "singleton")
	static function GetInstance() : GameMachine
	{
		return mInstance;
	}
	
	// Initialization		
	function Awake()
	{
		// Assign instance
		mInstance = this;
		
		// This is a typical trigger machine
		// (You can also set this in the inspector)
		mMachineType = MachineType.TriggerMachine;
		
		// Load the graph
		LoadFromFile( mstrGraphPath + mstrGraphFile );
	}
	
	// Start() contains a line that causes errors when put into Awake, because some stuff is not yet initialized
	function Start()
	{		
		// Find the "StartLevel" panel and activate the connected panel
		FindPanelByType("StartLevel").GetSlot(0).ActivateConnected();
	}
	
	// Called when a panel is activated which hasn't an explicit "master"
	function OnPanelActivated( panel : Panel )
	{
		switch( panel.GetPanelType() )
		{
		case "SetActive":
			// Panel to activate / deactivate game objects
			var bActive = panel.FindSlot("Active").GetDataBool();
			panel.FindSlot("GameObject").GetDataGameObject().SetActive( bActive );
			break;
			
		case "StartSound":
			// Panel to play a sound
			panel.FindSlot("AudioSource").GetDataGameObject().GetComponent(AudioSource).Play();
			break;
			
		case "PlayAnimation":
			// Panel to play an animation of a game object
			var strAnimation : String = panel.FindSlot("Animation").GetDataString();
			var gobject : GameObject = panel.FindSlot("GameObject").GetDataGameObject();
			gobject.GetComponent.<Animation>().Play( strAnimation );
			var animation : Animation = gobject.GetComponent("Animation");
			var state : AnimationState = animation[strAnimation];
			yield WaitForSeconds( state.length );
			panel.FindSlot("When done").ActivateConnected();
			break;
				
		case "StartDialog":
			// Panel to start a dialog 
			DialogHandler.GetInstance().StartDialog( panel );
			// The dialog handler takes care of calling panel.ActivateConnected() when the dialog is over
			break;			
			
		case "ExitLevel":
			// Panel to end the game
			Application.Quit();
			Debug.Break();
			break;					
			
			// Those are just some examples.
			// Put other "commands" here.
			
			// (You might also put commands into scripts of other objects 
			//  and direct the OnPanelActivated message to those objects via the "master" field.
			//  See "ShowBalloon" and "HideBalloon" in script NPC as examples.)											

		}
	}
}

