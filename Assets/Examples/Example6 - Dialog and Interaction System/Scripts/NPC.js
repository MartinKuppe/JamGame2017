
var mBalloon : GameObject;
var mstrName : String;
var mPortrait : Texture;

enum NPCStatus { Default, Clickable, Speaking }; 
private var mStatus : NPCStatus = NPCStatus.Default;
private var mBalloonPanel : Panel;

function Awake() 
{
	SetStatus( NPCStatus.Default );
}

function SetStatus( newStatus : NPCStatus ) 
{
	switch( newStatus )
	{
	case NPCStatus.Default:
		mBalloon.SetActive( false );
		break;
		
	case NPCStatus.Clickable:
		mBalloon.SetActive( true );
		break;
		
	case NPCStatus.Speaking:
		mBalloon.SetActive( true );
		break;				
	}
			
	mStatus = newStatus;
}

function Update()
{
	switch( mStatus )
	{
	case NPCStatus.Default:
		//Nothing
		break;
		
	case NPCStatus.Clickable:
		// Turn the balloon towards camera
		mBalloon.transform.LookAt( Camera.main.transform.position );
		
		// Did the player just click on the character ?
		if( Input.GetKeyDown( KeyCode.Mouse0 ) && !DialogHandler.GetInstance().IsDialogRunning() )
		{
			var ray : Ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			var hit : RaycastHit;
			if (GetComponent.<Collider>().Raycast (ray, hit, 1000.0)) 
			{ 
				// Yes, he did. Activate pannels connected to the balloon's "When clicked" slot
				mBalloonPanel.FindSlot("When clicked").ActivateConnected();
			} 
		}
		break;
		
	case NPCStatus.Speaking:
		// Let the balloon rotate to indicate that this character is speaking
		mBalloon.transform.Rotate( 0.0, Time.deltaTime * 200, 0.0 );
		break;				
	}	
}

// This function is called when a panel is activated which has this object in its "master" field (the little blue top hat)
function OnPanelActivated( panel : Panel )
{
	switch( panel.GetPanelType() )
	{
	case "ShowBalloon":
		// The character gets clickable
		SetStatus( NPCStatus.Clickable );
		mBalloonPanel = panel;
		break;
		
	case "HideBalloon":
		// Reset characetr
		SetStatus( NPCStatus.Default );
		break;		
	}
}