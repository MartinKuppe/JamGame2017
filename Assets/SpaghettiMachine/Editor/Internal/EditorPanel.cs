using UnityEngine;
using System.Collections;
using System.Xml; 

[System.Serializable]
public class EditorPanel // : ScriptableObject
{
	public string mstrType = "EditorPanel"; 
	public string mstrTypeOld = "xxx";  
	
	[SerializeField]
	public EditorSlot[] maSlots;
	[SerializeField]	
	public Rect mWindowRect = new Rect (20,100,200,20);
	public bool mbBringToFrontOnceCreated = false; 	
	public int miUniqueID;
	public Texture2D mImage;
	public int miLastDisplayedSlot;
	public bool mbFixedHeight = false;
	
	public bool mbSelected = false;
	public Vector2 mvHoveringOffset = Vector2.zero;
	
	public static int WINDOW_MARGE_TOP = 20;
	public static int WINDOW_MARGE_LEFT = 3;
	public static int WINDOW_MARGE_RIGHT = 3;	
	public static int WINDOW_MARGE_BOTTOM = 3;
	public static float HEADER_HEIGHT = 31.0f;	
	public static float SLOT_HEIGHT = 17.0f;
	
	private enum SpaceMadnessState { 	Sane, 
										Waiting, 
										RightwardsStreching, 
										RightwardsContracting, 
										LeftwardsStreching, 
										LeftwardsContracting,
										UpwardsRaisingRight,
										UpwardsCatchingupLeft,
										UpwardsRaisingLeft,
										UpwardsCatchingupRight,		
										DownwardsLoweringRight,
										DownwardsCatchingupLeft,
										DownwardsLoweringLeft,
										DownwardsCatchingupRight };
	
	private SpaceMadnessState mMadnessState = SpaceMadnessState.Sane;
	private float mfMadnessWaitingTimer;
	private float mfMadnessMovingProgress;
	
	private Vector2 mvCurrentMadnessMovingPivot;
	private Vector2 mvCurrentMadnessStretching;
	private Vector2 mvCurrentMadnessPositionJump;
	private float 	mfCurrentMadnessRotation;
	
	private static float MADNESS_MIN_WAITING_TIME = 5.0f;
	private static float MADNESS_MAX_WAITING_TIME = 20.0f;	
	private static float MADNESS_MIN_INITIAL_WAITING_TIME = 20.0f;
	private static float MADNESS_MAX_INITIAL_WAITING_TIME = 60.0f;
	private static float MADNESS_MAX_STRETCHING = 1.05f;
	private static float MADNESS_MAX_INCLINATION = 2.0f;
	
	private static float MADNESS_STRETCH_INTERVAL = 0.2f;
	private static float MADNESS_TURN_INTERVAL = 0.1f;
	
	// Probabilities for walk directions
	private static float P_LEFT = 0.25f;
	private static float P_RIGHT = 0.25f;
	private static float P_UP = 0.25f;
	//private static float P_DOWN = 0.25f;
	
	// Probability to stop after a step
	private static float P_STOP = 0.25f;
	
		
	private string mstrPanelSetName = "";
	private string mstrPanelSetPath = "";	
	
	
	//DELETEME
	public int miLayoutCallOrder = -1;
	public int miRepaintCallOrder = -1;
	public int miLayoutLoopOrder = -1;
	public int miRepaintLoopOrder = -1;	
	
	public void CloneFrom( EditorPanel prototype  )
	{
		mbFixedHeight = prototype.mbFixedHeight;
		mWindowRect.height = ( mbFixedHeight ? prototype.mWindowRect.height : HEADER_HEIGHT ) + WINDOW_MARGE_BOTTOM;

		mstrType = prototype.mstrType;
		mstrPanelSetName = prototype.mstrPanelSetName;
		mstrPanelSetPath = prototype.mstrPanelSetPath;		
		
		mWindowRect.width = prototype.mWindowRect.width;	
		miUniqueID = 0;
		mImage = prototype.mImage;
		
		maSlots = new EditorSlot[prototype.maSlots.Length ];
		for( int i = 0; i < prototype.maSlots.Length; i++ )
		{
			EditorSlot protoslot = prototype.maSlots[i];

			maSlots[i]  =  new EditorSlot(); //ScriptableObject.CreateInstance( typeof( EditorSlot ) ) as EditorSlot;
			maSlots[i].CloneFromSlot( this, i, protoslot );
			if( !mbFixedHeight )
			{
				mWindowRect.height += SLOT_HEIGHT;
			}
		}

		if( mImage != null && !mbFixedHeight )
		{
			mWindowRect.width = mImage.width + WINDOW_MARGE_LEFT + WINDOW_MARGE_RIGHT;
			if( !mbFixedHeight )
			{
				mWindowRect.height = mImage.height + WINDOW_MARGE_TOP + WINDOW_MARGE_BOTTOM;
			}
		}		
		miLastDisplayedSlot = prototype.maSlots.Length -1;
		SpaghettiMachineEditor.GetInstance().AddPanel( this );
	}
	
	
	public bool HasIdenticalStructure( EditorPanel otherpanel )
	{
		if( mstrType 		!= 	otherpanel.mstrType &&  mstrType != otherpanel.mstrTypeOld ) { return false; }
		if( mbFixedHeight 	!=  otherpanel.mbFixedHeight ) { return false; }

		if( (mImage==null) 	!=  (otherpanel.mImage==null) ) { return false; }
		if( mImage !=null &&  mImage.name != otherpanel.mImage.name ) { return false; }
		
		if( maSlots.Length 	!=  otherpanel.maSlots.Length ) { return false; }
		for( int i = 0; i < maSlots.Length; i++ )
		{
			EditorSlot slot = maSlots[i];
			EditorSlot otherslot = otherpanel.maSlots[i];
			if( !slot.HasIdenticalStructure(otherslot)  ) { return false; }
		}
		
		return true;
	}
	
	public void SetPanelset( string strPanelSetName,  string strPanelSetPath )
	{
		mstrPanelSetName = 	strPanelSetName;
		mstrPanelSetPath = 	strPanelSetPath;
	}
	
	public void OnDestroy()
	{	
		if( maSlots != null )
		{
			foreach( EditorSlot slot in maSlots )
			{
				slot.DeleteAllConnections();
			}
		}
		SpaghettiMachineEditor.GetInstance().RemovePanel( this );
	}
	
	public EditorSlot GetNearSlot( Vector2 vPosition )
	{
		foreach( EditorSlot slot in maSlots )
		{
			if( slot.IsInSlot( vPosition ) )
			{
				return slot;
			}
		}
		
		return null;
	}
	
	public bool FindAndReplace( string strFind, string strReplaceOrNull )
	{
		bool bFound = false;
		foreach( EditorSlot slot in maSlots )
		{
			bFound = slot.FindAndReplace( strFind, strReplaceOrNull ) || bFound;
		}
		return bFound;
	}
	
	public Vector2 GetUpperLeftCorner()
	{
		return new Vector2( mWindowRect.x, mWindowRect.y );	
	}
	
	public void Write( XmlWriter writer )
	{		
		// <panel>
		writer.WriteStartElement("panel");	
		writer.WriteAttributeString( "type", mstrType );
		writer.WriteAttributeString( "panelset", mstrPanelSetName );
		writer.WriteAttributeString( "panelsetpath", mstrPanelSetPath );		
		writer.WriteAttributeString( "x", ""+ (int)(mWindowRect.x) );				
		writer.WriteAttributeString( "y", ""+ (int)(mWindowRect.y) );
		writer.WriteAttributeString( "width", ""+ (int)(mWindowRect.width) );
		writer.WriteAttributeString( "height", ""+ (int)(mWindowRect.height) );
		writer.WriteAttributeString( "image", ( mImage != null ) ? mImage.name : "" );	
		
		//<ID>24</ID>
		writer.WriteStartElement("ID");
		writer.WriteValue( miUniqueID );	
		writer.WriteEndElement();			
	
		// <slots number = "3">
		writer.WriteStartElement("slots");		
		writer.WriteAttributeString( "number", ""+maSlots.Length );
		
		// Write slots
		foreach( EditorSlot slot in maSlots )
		{
			slot.Write( writer );
		}
		
		// </slots>		
		writer.WriteEndElement();		
		
		// </panel>		
		writer.WriteEndElement();		
	}
	
	
	public void Read( XmlReader reader )
	{
		// <panel>
		mstrType = reader["type"];	
			
		mstrPanelSetName = (reader["panelset"] != null) ? reader["panelset"] : "";
		mstrPanelSetPath = (reader["panelsetpath"] != null) ? reader["panelsetpath"] : "";	
		
		mWindowRect.x = System.Convert.ToInt32( reader["x"] );
		mWindowRect.y = System.Convert.ToInt32( reader["y"] );
		mWindowRect.width = System.Convert.ToInt32( reader["width"] );
		mWindowRect.height = System.Convert.ToInt32( reader["height"] );		
		string strImage = reader["image"];	
		if( strImage != null )
		{
			mImage =  (Texture2D)( Resources.Load( strImage ) );
		}
		reader.ReadStartElement("panel");			
		
		//<ID>24</ID>
		reader.ReadStartElement("ID");
		miUniqueID = reader.ReadContentAsInt();	
		reader.ReadEndElement();
		
		// <slots number = "3">
		int iSlots = System.Convert.ToInt32( reader["number"] );		
		reader.ReadStartElement("slots");	
		int iIndex = 0;
		maSlots = new EditorSlot[iSlots];
		
		// Read slots
		while( reader.NodeType == XmlNodeType.Element && reader.Name == "slot" )
		{	
			EditorSlot newSlot = new EditorSlot(); //ScriptableObject.CreateInstance( typeof( EditorSlot ) ) as EditorSlot;
			newSlot.InitSlot( this, 
			                 iIndex, 
			                 reader["label"],
							 reader["label_old"],
			                 reader["plug"], 
			                 reader["content"], 
			                 reader["color"], 
			                 reader["multiple"], 
			                 reader["default"], 
			                 reader["enum"],
			                 reader["plug_x"], 			                 
			                 reader["plug_y"] );
			newSlot.Read( reader );
			maSlots[iIndex] = newSlot;
			iIndex++;
		}
		
		// </slots>		
		reader.ReadEndElement();	
		
		// </panel>		
		reader.ReadEndElement();	
	}
	
	
	public void PostReadInitialization()
	{
		foreach( EditorSlot slot in maSlots )
		{
			slot.PostReadInitialization();
		}
	}
	
	public void GoMad()
	{
		mMadnessState = SpaceMadnessState.Waiting;
		mfMadnessWaitingTimer = UnityEngine.Random.Range( MADNESS_MIN_INITIAL_WAITING_TIME, MADNESS_MAX_INITIAL_WAITING_TIME );
	}
	
	public void MadnessUpdate( float fDeltaTime )
	{

		switch( mMadnessState )
		{
		case SpaceMadnessState.Sane: 
			//Must be new panel
			mMadnessState = SpaceMadnessState.Waiting;
			Wait();
			break;
			
		case SpaceMadnessState.Waiting: 
			// Set deformation parameters	
			mvCurrentMadnessStretching.x = 1.0f;
			mvCurrentMadnessStretching.y = 1.0f;
			mfCurrentMadnessRotation = 0.0f;					
			mvCurrentMadnessPositionJump = Vector2.zero;
			
			// Handle timer
			mfMadnessWaitingTimer -= fDeltaTime;
			if( mfMadnessWaitingTimer <= 0.0f )
			{
				StartWalking();
			}
			break;
			
		case SpaceMadnessState.RightwardsStreching:
				
			mfMadnessMovingProgress = Mathf.Clamp01( mfMadnessMovingProgress +  fDeltaTime/MADNESS_STRETCH_INTERVAL);
			
			mvCurrentMadnessMovingPivot.x = mWindowRect.x;
			mvCurrentMadnessMovingPivot.y = mWindowRect.y + 0.5f * mWindowRect.height;
			mvCurrentMadnessStretching.x = (1.0f - mfMadnessMovingProgress) + mfMadnessMovingProgress * MADNESS_MAX_STRETCHING;
			mvCurrentMadnessStretching.y = (1.0f - mfMadnessMovingProgress) + mfMadnessMovingProgress / MADNESS_MAX_STRETCHING;
			mfCurrentMadnessRotation = 0.0f;
			mvCurrentMadnessPositionJump = Vector2.zero;
				

			if( mfMadnessMovingProgress == 1.0f )
			{
				mvCurrentMadnessPositionJump.x = mWindowRect.width * ( MADNESS_MAX_STRETCHING - 1.0f );
				mvCurrentMadnessMovingPivot.x +=mWindowRect.width * MADNESS_MAX_STRETCHING;
				mMadnessState = SpaceMadnessState.RightwardsContracting;
				mfMadnessMovingProgress = 0.0f;
			}
			break;
			
		case SpaceMadnessState.RightwardsContracting: 
				
			mfMadnessMovingProgress = Mathf.Clamp01( mfMadnessMovingProgress + fDeltaTime/MADNESS_STRETCH_INTERVAL);			
			
			mvCurrentMadnessMovingPivot.x = mWindowRect.x + mWindowRect.width;
			mvCurrentMadnessMovingPivot.y = mWindowRect.y + 0.5f * mWindowRect.height;
			mvCurrentMadnessStretching.x = mfMadnessMovingProgress + (1.0f - mfMadnessMovingProgress) * MADNESS_MAX_STRETCHING;
			mvCurrentMadnessStretching.y = mfMadnessMovingProgress + (1.0f - mfMadnessMovingProgress) / MADNESS_MAX_STRETCHING;
			mfCurrentMadnessRotation = 0.0f;
			mvCurrentMadnessPositionJump = Vector2.zero;

			if( mfMadnessMovingProgress == 1.0f )
			{
				mvCurrentMadnessStretching.x = 1.0f;
				mvCurrentMadnessStretching.y = 1.0f;
				if( UnityEngine.Random.value < P_STOP )
				{
					mMadnessState = SpaceMadnessState.Waiting;
					Wait();
				}
				else
				{
					mvCurrentMadnessMovingPivot.x = mWindowRect.x;
					mMadnessState = SpaceMadnessState.RightwardsStreching;
					mfMadnessMovingProgress = 0.0f;
				}
			}
			break;
			
		case SpaceMadnessState.LeftwardsStreching: 
				
			mfMadnessMovingProgress = Mathf.Clamp01( mfMadnessMovingProgress + fDeltaTime/MADNESS_STRETCH_INTERVAL);			

			mvCurrentMadnessMovingPivot.x = mWindowRect.x + mWindowRect.width;
			mvCurrentMadnessMovingPivot.y = mWindowRect.y + 0.5f * mWindowRect.height;
			mvCurrentMadnessStretching.x = (1.0f - mfMadnessMovingProgress) + mfMadnessMovingProgress * MADNESS_MAX_STRETCHING;
			mvCurrentMadnessStretching.y = (1.0f - mfMadnessMovingProgress) + mfMadnessMovingProgress / MADNESS_MAX_STRETCHING;
			mfCurrentMadnessRotation = 0.0f;
			mvCurrentMadnessPositionJump = Vector2.zero;
				
			
			if( mfMadnessMovingProgress == 1.0f )
			{
				mvCurrentMadnessPositionJump.x = - mWindowRect.width * ( MADNESS_MAX_STRETCHING - 1.0f );
				mvCurrentMadnessMovingPivot.x -= mWindowRect.width * MADNESS_MAX_STRETCHING;
				mMadnessState = SpaceMadnessState.LeftwardsContracting;
				mfMadnessMovingProgress = 0.0f;
			}
			break;
			
		case SpaceMadnessState.LeftwardsContracting:
				
			mfMadnessMovingProgress = Mathf.Clamp01( mfMadnessMovingProgress + fDeltaTime/MADNESS_STRETCH_INTERVAL);
			
			mvCurrentMadnessMovingPivot.x = mWindowRect.x;
			mvCurrentMadnessMovingPivot.y = mWindowRect.y + 0.5f * mWindowRect.height;
			mvCurrentMadnessStretching.x = mfMadnessMovingProgress + (1.0f - mfMadnessMovingProgress) * MADNESS_MAX_STRETCHING;
			mvCurrentMadnessStretching.y = mfMadnessMovingProgress + (1.0f - mfMadnessMovingProgress) / MADNESS_MAX_STRETCHING;
			mfCurrentMadnessRotation = 0.0f;
			mvCurrentMadnessPositionJump = Vector2.zero;
			
			if( mfMadnessMovingProgress == 1.0f )
			{
				mvCurrentMadnessStretching.x = 1.0f;
				mvCurrentMadnessStretching.y = 1.0f;
				if( UnityEngine.Random.value < P_STOP )
				{
					mMadnessState = SpaceMadnessState.Waiting;
					Wait();
				}
				else
				{
					mvCurrentMadnessMovingPivot.x = mWindowRect.x + mWindowRect.width * ( MADNESS_MAX_STRETCHING - 1.0f );
					mMadnessState = SpaceMadnessState.LeftwardsStreching;
					mfMadnessMovingProgress = 0.0f;
				}
			}
			break;
			
		case SpaceMadnessState.UpwardsRaisingRight:
				
			mfMadnessMovingProgress = Mathf.Clamp01( mfMadnessMovingProgress + fDeltaTime/MADNESS_TURN_INTERVAL);			

			mvCurrentMadnessMovingPivot.x = mWindowRect.x;
			mvCurrentMadnessMovingPivot.y = mWindowRect.y + 0.5f * mWindowRect.height;
			mvCurrentMadnessStretching.x = 1.0f;
			mvCurrentMadnessStretching.y = 1.0f;
			mfCurrentMadnessRotation = -mfMadnessMovingProgress * MADNESS_MAX_INCLINATION;
			mvCurrentMadnessPositionJump = Vector2.zero;
			
			if( mfMadnessMovingProgress == 1.0f )
			{
				float fSin = Mathf.Sin( Mathf.Deg2Rad * MADNESS_MAX_INCLINATION );
				float fCos = Mathf.Cos( Mathf.Deg2Rad * MADNESS_MAX_INCLINATION );
				mvCurrentMadnessPositionJump.x = -mWindowRect.width * (1.0f - fCos);
				mvCurrentMadnessPositionJump.y = - mWindowRect.width * fSin;
				mvCurrentMadnessMovingPivot.x += mWindowRect.width * fCos;
				mvCurrentMadnessMovingPivot.y -= mWindowRect.width * fSin;
				mMadnessState = SpaceMadnessState.UpwardsCatchingupLeft;
				mfMadnessMovingProgress = 0.0f;
			}
			break;
			
		case SpaceMadnessState.UpwardsCatchingupLeft:
				
			mfMadnessMovingProgress = Mathf.Clamp01( mfMadnessMovingProgress + fDeltaTime/MADNESS_TURN_INTERVAL);			

			mvCurrentMadnessMovingPivot.x = mWindowRect.x + mWindowRect.width;
			mvCurrentMadnessMovingPivot.y = mWindowRect.y + 0.5f * mWindowRect.height;
			mvCurrentMadnessStretching.x = 1.0f;
			mvCurrentMadnessStretching.y = 1.0f;
			mfCurrentMadnessRotation = -(1.0f - mfMadnessMovingProgress) * MADNESS_MAX_INCLINATION;
			mvCurrentMadnessPositionJump = Vector2.zero;
			
			if( mfMadnessMovingProgress == 1.0f )
			{
				if( UnityEngine.Random.value < P_STOP )
				{
					mfCurrentMadnessRotation = 0.0f;
					mMadnessState = SpaceMadnessState.Waiting;
					Wait();
				}
				else
				{
					mvCurrentMadnessMovingPivot.x -= mWindowRect.width;
					mMadnessState = SpaceMadnessState.UpwardsRaisingLeft;
					mfMadnessMovingProgress = 0.0f;
				}
			}
			break;
			
		case SpaceMadnessState.UpwardsRaisingLeft:
				
			mfMadnessMovingProgress = Mathf.Clamp01( mfMadnessMovingProgress + fDeltaTime/MADNESS_TURN_INTERVAL);			

			mvCurrentMadnessMovingPivot.x = mWindowRect.x + mWindowRect.width;
			mvCurrentMadnessMovingPivot.y = mWindowRect.y + 0.5f * mWindowRect.height;
			mvCurrentMadnessStretching.x = 1.0f;
			mvCurrentMadnessStretching.y = 1.0f;
			mfCurrentMadnessRotation = mfMadnessMovingProgress * MADNESS_MAX_INCLINATION;
			mvCurrentMadnessPositionJump = Vector2.zero;
			
			if( mfMadnessMovingProgress == 1.0f )
			{
				float fSin = Mathf.Sin( Mathf.Deg2Rad * MADNESS_MAX_INCLINATION );
				float fCos = Mathf.Cos( Mathf.Deg2Rad * MADNESS_MAX_INCLINATION );
				mvCurrentMadnessPositionJump.x = mWindowRect.width * (1.0f - fCos);
				mvCurrentMadnessPositionJump.y = - mWindowRect.width * fSin;
				mvCurrentMadnessMovingPivot.x -= mWindowRect.width * fCos;
				mvCurrentMadnessMovingPivot.y -= mWindowRect.width * fSin;
				mMadnessState = SpaceMadnessState.UpwardsCatchingupRight;
				mfMadnessMovingProgress = 0.0f;
			}
			break;
			
		case SpaceMadnessState.UpwardsCatchingupRight:	
				
			mfMadnessMovingProgress = Mathf.Clamp01( mfMadnessMovingProgress + fDeltaTime/MADNESS_TURN_INTERVAL);			

			mvCurrentMadnessMovingPivot.x = mWindowRect.x;
			mvCurrentMadnessMovingPivot.y = mWindowRect.y + 0.5f * mWindowRect.height;
			mvCurrentMadnessStretching.x = 1.0f;
			mvCurrentMadnessStretching.y = 1.0f;
			mfCurrentMadnessRotation = (1.0f - mfMadnessMovingProgress) * MADNESS_MAX_INCLINATION;
			mvCurrentMadnessPositionJump = Vector2.zero;
			
			if( mfMadnessMovingProgress == 1.0f )
			{
				if( UnityEngine.Random.value < P_STOP )
				{
					mfCurrentMadnessRotation = 0.0f;
					mMadnessState = SpaceMadnessState.Waiting;
					Wait();
				}
				else
				{				
					mvCurrentMadnessMovingPivot.x += mWindowRect.width;
					mMadnessState = SpaceMadnessState.UpwardsRaisingRight; //UpwardsRaisingLeft
					mfMadnessMovingProgress = 0.0f;
				}
			}
			break;
			
		case SpaceMadnessState.DownwardsLoweringRight:
				
			mfMadnessMovingProgress = Mathf.Clamp01( mfMadnessMovingProgress + fDeltaTime/MADNESS_TURN_INTERVAL);			

			mvCurrentMadnessMovingPivot.x = mWindowRect.x;
			mvCurrentMadnessMovingPivot.y = mWindowRect.y + 0.5f * mWindowRect.height;
			mvCurrentMadnessStretching.x = 1.0f;
			mvCurrentMadnessStretching.y = 1.0f;
			mfCurrentMadnessRotation = mfMadnessMovingProgress * MADNESS_MAX_INCLINATION;
			mvCurrentMadnessPositionJump = Vector2.zero;
			
			if( mfMadnessMovingProgress == 1.0f )
			{
				float fSin = Mathf.Sin( Mathf.Deg2Rad * MADNESS_MAX_INCLINATION );
				float fCos = Mathf.Cos( Mathf.Deg2Rad * MADNESS_MAX_INCLINATION );
				mvCurrentMadnessPositionJump.x = -mWindowRect.width * (1.0f - fCos);
				mvCurrentMadnessPositionJump.y = mWindowRect.width * fSin;
				mvCurrentMadnessMovingPivot.x += mWindowRect.width * fCos;
				mvCurrentMadnessMovingPivot.y += mWindowRect.width * fSin;
				mMadnessState = SpaceMadnessState.DownwardsCatchingupLeft;
				mfMadnessMovingProgress = 0.0f;
			}
			break;
			
		case SpaceMadnessState.DownwardsCatchingupLeft:
				
			mfMadnessMovingProgress = Mathf.Clamp01( mfMadnessMovingProgress + fDeltaTime/MADNESS_TURN_INTERVAL);			

			mvCurrentMadnessMovingPivot.x = mWindowRect.x + mWindowRect.width;
			mvCurrentMadnessMovingPivot.y = mWindowRect.y + 0.5f * mWindowRect.height;
			mvCurrentMadnessStretching.x = 1.0f;
			mvCurrentMadnessStretching.y = 1.0f;
			mfCurrentMadnessRotation =  (1.0f - mfMadnessMovingProgress) * MADNESS_MAX_INCLINATION;
			mvCurrentMadnessPositionJump = Vector2.zero;
			
			if( mfMadnessMovingProgress == 1.0f )
			{
				if( UnityEngine.Random.value < P_STOP )
				{
					mfCurrentMadnessRotation = 0.0f;
					mMadnessState = SpaceMadnessState.Waiting;
					Wait();
				}
				else
				{				
					mvCurrentMadnessMovingPivot.x -= mWindowRect.width;
					mMadnessState = SpaceMadnessState.DownwardsLoweringLeft;
					mfMadnessMovingProgress = 0.0f;
				}
			}
			break;
			
		case SpaceMadnessState.DownwardsLoweringLeft:
				
			mfMadnessMovingProgress = Mathf.Clamp01( mfMadnessMovingProgress + fDeltaTime/MADNESS_TURN_INTERVAL);			

			mvCurrentMadnessMovingPivot.x = mWindowRect.x + mWindowRect.width;
			mvCurrentMadnessMovingPivot.y = mWindowRect.y + 0.5f * mWindowRect.height;
			mvCurrentMadnessStretching.x = 1.0f;
			mvCurrentMadnessStretching.y = 1.0f;
			mfCurrentMadnessRotation = -mfMadnessMovingProgress * MADNESS_MAX_INCLINATION;
			mvCurrentMadnessPositionJump = Vector2.zero;
			
			if( mfMadnessMovingProgress == 1.0f )
			{
				float fSin = Mathf.Sin( Mathf.Deg2Rad * MADNESS_MAX_INCLINATION );
				float fCos = Mathf.Cos( Mathf.Deg2Rad * MADNESS_MAX_INCLINATION );
				mvCurrentMadnessPositionJump.x = mWindowRect.width * (1.0f - fCos);
				mvCurrentMadnessPositionJump.y =  mWindowRect.width * fSin;
				mvCurrentMadnessMovingPivot.x -= mWindowRect.width * fCos;
				mvCurrentMadnessMovingPivot.y += mWindowRect.width * fSin;
				mMadnessState = SpaceMadnessState.DownwardsCatchingupRight;
				mfMadnessMovingProgress = 0.0f;
			}
			break;
			
		case SpaceMadnessState.DownwardsCatchingupRight: 
				
			mfMadnessMovingProgress = Mathf.Clamp01( mfMadnessMovingProgress + fDeltaTime/MADNESS_TURN_INTERVAL);			

			mvCurrentMadnessMovingPivot.x = mWindowRect.x;
			mvCurrentMadnessMovingPivot.y = mWindowRect.y + 0.5f * mWindowRect.height;
			mvCurrentMadnessStretching.x = 1.0f;
			mvCurrentMadnessStretching.y = 1.0f;
			mfCurrentMadnessRotation = -(1.0f - mfMadnessMovingProgress) * MADNESS_MAX_INCLINATION;
			mvCurrentMadnessPositionJump = Vector2.zero;
			
			if( mfMadnessMovingProgress == 1.0f )
			{
				if( UnityEngine.Random.value < P_STOP )
				{
					mfCurrentMadnessRotation = 0.0f;
					mMadnessState = SpaceMadnessState.Waiting;
					Wait();
				}
				else
				{
					mvCurrentMadnessMovingPivot.x += mWindowRect.width;
					mMadnessState = SpaceMadnessState.DownwardsLoweringRight;
					mfMadnessMovingProgress = 0.0f;
				}
			}
			break;
		}
	}
	
	public void DoMadnessTransform()
	{
		if( mvCurrentMadnessPositionJump != Vector2.zero )
		{
			mWindowRect.x += mvCurrentMadnessPositionJump.x;
			mWindowRect.y += mvCurrentMadnessPositionJump.y;			
			mvCurrentMadnessPositionJump = Vector2.zero;	
		}
		
		if( mvCurrentMadnessStretching != Vector2.zero )
		{
			GUIUtility.ScaleAroundPivot( mvCurrentMadnessStretching, mvCurrentMadnessMovingPivot );
		}
		
		if( mfCurrentMadnessRotation != 0.0f )
		{
			GUIUtility.RotateAroundPivot( mfCurrentMadnessRotation, mvCurrentMadnessMovingPivot );
		}		
		
	}
	
	private void StartWalking()
	{
		float fRandomValue = UnityEngine.Random.value;
		if( fRandomValue < P_LEFT )
		{
			StartWalkingLeftwards();
		}
		else if( fRandomValue < P_LEFT + P_RIGHT )
		{
			StartWalkingRightwards();
		}
		else if( fRandomValue < P_LEFT + P_RIGHT + P_UP )
		{
			StartWalkingUpwards();
		}
		else
		{
			StartWalkingDownwards();
		}		
	}
	
	private void StartWalkingRightwards()
	{
		mMadnessState = SpaceMadnessState.RightwardsStreching;
		mfMadnessMovingProgress = 0.0f;
	}
	
	private void StartWalkingLeftwards()
	{
		mMadnessState = SpaceMadnessState.LeftwardsStreching;
		mfMadnessMovingProgress = 0.0f;
	}
	
	private void StartWalkingUpwards()
	{
		mMadnessState = SpaceMadnessState.UpwardsRaisingRight;
		mfMadnessMovingProgress = 0.0f;		
	}
	
	private void StartWalkingDownwards()
	{
		mMadnessState = SpaceMadnessState.DownwardsLoweringRight;
		mfMadnessMovingProgress = 0.0f;		
	}	
	
	private void Wait()
	{
		mfMadnessWaitingTimer = UnityEngine.Random.Range( MADNESS_MIN_WAITING_TIME, MADNESS_MAX_WAITING_TIME );
	}
	
	public Vector2 MadnessTransform( Vector2 vPosition )
	{
		if( mMadnessState == SpaceMadnessState.Sane || mMadnessState ==  SpaceMadnessState.Waiting )
		{
			return vPosition;
		}
		Vector2 vPivotToPosition = vPosition - mvCurrentMadnessMovingPivot;
		
		if( mvCurrentMadnessStretching != Vector2.zero )
		{
			vPivotToPosition.x *= mvCurrentMadnessStretching.x;
			vPivotToPosition.y *= mvCurrentMadnessStretching.y;
		}
		
		if( mfCurrentMadnessRotation != 0.0f )
		{
			float fSin = Mathf.Sin( Mathf.Deg2Rad * mfCurrentMadnessRotation );
			float fCos = Mathf.Cos( Mathf.Deg2Rad * mfCurrentMadnessRotation );			
			vPivotToPosition = new Vector2( fCos * vPivotToPosition.x - fSin * vPivotToPosition.y, fSin * vPivotToPosition.x + fCos * vPivotToPosition.y );
		}
		
		return mvCurrentMadnessMovingPivot + vPivotToPosition;
	}

	public string	GetPanelSetName()
	{
		return mstrPanelSetName;
	}
	
	public string	GetPanelSetPath()
	{
		return mstrPanelSetPath;
	}	
	
	public void	UpdateFrom( EditorPanel prototype )
	{
		mbFixedHeight = prototype.mbFixedHeight;
		mWindowRect.height = ( mbFixedHeight ? prototype.mWindowRect.height : HEADER_HEIGHT ) + WINDOW_MARGE_BOTTOM;

		mstrType = prototype.mstrType;
		mstrPanelSetName = prototype.mstrPanelSetName;
		mstrPanelSetPath = prototype.mstrPanelSetPath;		
		
		mWindowRect.width = prototype.mWindowRect.width;	
		miUniqueID = 0;
		mImage = prototype.mImage;
		
		EditorSlot[] aOldSlots = maSlots;
		
		maSlots = new EditorSlot[prototype.maSlots.Length ];
		for( int i = 0; i < prototype.maSlots.Length; i++ )
		{
			EditorSlot protoslot = prototype.maSlots[i];

			maSlots[i]  =  new EditorSlot(); //ScriptableObject.CreateInstance( typeof( EditorSlot ) ) as EditorSlot;
			maSlots[i].CloneFromSlot( this, i, protoslot );
			
			// Look for old slot with the corresponding label
			int  iCorrespondingSlotIndex = -1;
			for( int j = 0; j < aOldSlots.Length; j++ )
			{
				EditorSlot oldSlot = aOldSlots[j];
			
				if( oldSlot != null && (oldSlot.mstrLabel == protoslot.mstrLabel || oldSlot.mstrLabel == protoslot.mstrLabelOld || (oldSlot.IsMasterSlot() && protoslot.IsMasterSlot()) ) )
				{
					iCorrespondingSlotIndex = j;
					break;
				}
			}
			
			if( iCorrespondingSlotIndex != -1 )
			{
				// This is the most important line in the method.
				// Transfer content and links from old slot to new slot
				
				Debug.Log( "Updating slot "+ maSlots[i].miUniqueID + " from old slot "+ aOldSlots[iCorrespondingSlotIndex].miUniqueID );
				maSlots[i].UpdateFrom( aOldSlots[iCorrespondingSlotIndex] );
				
				// Delete old slot in list, so we can see at the end which slots couldn't be saved, in order to cut the links 
				aOldSlots[iCorrespondingSlotIndex] = null;
			}
			

			
			if( !mbFixedHeight )
			{
				mWindowRect.height += SLOT_HEIGHT;
			}
			

		}
		

	
		for( int j = 0; j < aOldSlots.Length; j++ )
		{
			EditorSlot oldSlot = aOldSlots[j];
			if( oldSlot != null )
			{
				// Slot couldn't be saved
				
				Debug.Log( "Deleting "+oldSlot.maIncomingLinks.Length+"+"+oldSlot.maOutgoingLinks.Length+" connections of old slot "+oldSlot.miUniqueID );
				oldSlot.DeleteAllConnections();
			}
		}
		
		if( mImage != null && !mbFixedHeight )
		{
			mWindowRect.width = mImage.width + WINDOW_MARGE_LEFT + WINDOW_MARGE_RIGHT;
			if( !mbFixedHeight )
			{
				mWindowRect.height = mImage.height + WINDOW_MARGE_TOP + WINDOW_MARGE_BOTTOM;
			}
		}		
		miLastDisplayedSlot = prototype.maSlots.Length -1;
	}
			
		
}