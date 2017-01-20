
using UnityEngine;
using UnityEditor;
using System.Collections;
using Spaghetti;


public class SpaghettiMachineDebugger : EditorWindow, ISpaghettiDebugger 
{
	private Texture mIconNoteblock;
	private Texture mIconMore;
	private Texture mIconLess;
	
	private Texture mTableTexture;
	private Texture mIconMaster;
	private Texture mIconGameObject;	
	private Texture mPlugTexture;
	private Texture mMultiplugTexture;		
	private Texture2D mMinimapTexture;
	
	
	private Texture mIconTelltaleRed;
	private Texture mIconTelltaleGreen;	
	private Texture mIconOptionActive;		
	private Texture mIconOptionPotentials;
	private Texture mIconOptionVariables;
	
	[SerializeField]
	private Panel[] maPanels;
	private Vector2 mvBackgroundOffset = Vector2.zero;
	
	private static int OPTIONS_WINDOW_ID = 1;
	private static int START_PANELS_IDS = 1000;	
	private float MINIMAP_SCALE = 0.1f;
	
	private bool mbScrolling = false;
	private GUISkin mGUISkin;
	

	[SerializeField]
	public SpaghettiMachineCore mSpaghettiMachine;

	// Octarine
	private Color mOctarine;
	private float OCTARINE_SPEED_RED = 0.31f;
	private float OCTARINE_SPEED_GREEN = 0.56f;
	private float OCTARINE_SPEED_BLUE = 0.44f;
	
	// The Colour out of Space
	private Color mTheColourOutOfSpace;	
	private float SPACECOLOR_INTERVAL = 0.1f;
	
	private bool mbConstantRefresh = false;
	
	private string mstrDragAndDropErrorMessage = "";
	private bool mbDragAndDropErrorRed = false;
	

	
	//------------------------------------------------------------------------------------------------
	// Initialize
	// called from SpaghettiMachineEditorWindow::Init
	//------------------------------------------------------------------------------------------------
	public static void Initialize () 
	{
		//TODO
	}
	
	//================================================================================================
	//
	//   GUI METHODS
	//
	//================================================================================================
	
	//------------------------------------------------------------------------------------------------
	// OnGUI
	// Called by the engine whenever it feels so.
	//------------------------------------------------------------------------------------------------	
	public void OnGUI () 
	{
		mbConstantRefresh = false;

		if( mIconNoteblock == null )
		{
			mIconNoteblock = Resources.Load( "SMIconNoteblock"  ) as Texture;
		}
		if( mIconMore == null ) 
		{
			mIconMore = Resources.Load( "SMIconMore" ) as Texture;
		}
		if( mIconLess == null )
		{
			mIconLess = Resources.Load( "SMIconLess" ) as Texture;
		}		
		if( mTableTexture == null )
		{
			mTableTexture = Resources.Load( "SMTablecloth" ) as Texture;	
		}
		if( mIconMaster == null ) 
		{
			mIconMaster = Resources.Load( "SMIconMaster" ) as Texture;
		}		
		if( mIconGameObject == null ) 
		{
			mIconGameObject = Resources.Load( "SMIconGameObject" ) as Texture;
		}
		if( mIconTelltaleRed == null ) 
		{
			mIconTelltaleRed = Resources.Load( "SMTelltaleRed" ) as Texture;
		}		
		if( mIconTelltaleGreen == null ) 
		{
			mIconTelltaleGreen = Resources.Load( "SMTelltaleGreen" ) as Texture;
		}
		if( mIconOptionActive == null ) 
		{
			mIconOptionActive = Resources.Load( "SMDOptionActive" ) as Texture;
		}		
		if( mIconOptionPotentials == null ) 
		{
			mIconOptionPotentials = Resources.Load( "SMDOptionPotentials" ) as Texture;
		}		
		if( mIconOptionVariables == null ) 
		{
			mIconOptionVariables = Resources.Load( "SMDOptionVariables" ) as Texture;
		}		
		if( mPlugTexture == null )
		{
			mPlugTexture = Resources.Load("SMMonoplug") as Texture2D;
		}
		if( mMultiplugTexture == null )
		{
			mMultiplugTexture = Resources.Load("SMMultiplug") as Texture2D;
		}
		
		if( mGUISkin == null )
		{
			mGUISkin  = EditorGUIUtility.Load("SpaghettiMachine/WoodSkin/WoodSkin.guiskin") as GUISkin; 				
		}
		GUI.skin = mGUISkin;	
		
		if ( Event.current.type == EventType.Repaint )
		{
			// Draw table texture
			int iRows = (int)Mathf.Ceil( Screen.width / mTableTexture.width ) + 2;
			int iLines = (int)Mathf.Ceil( Screen.height / mTableTexture.height ) + 2;
			for( int i = 0; i < iRows; i++ )
			{
				for( int j = 0; j < iLines; j++ )
				{		
					Rect screenRect = new Rect (mvBackgroundOffset.x + (i-1) * mTableTexture.width, mvBackgroundOffset.y + (j-1) * mTableTexture.height, mTableTexture.width, mTableTexture.height );
					GUI.DrawTexture( screenRect, mTableTexture );
				}
			}
		}
		


		switch( Event.current.type )
		{
		case EventType.MouseDown :
			if( Event.current.button == 1)
			{
				mbScrolling = true;
				Event.current.Use();
			}
			break;
			
		case EventType.MouseUp :
			if( Event.current.button == 1 )
			{
				mbScrolling = false;
				Event.current.Use();
			}
			break;
				
		case EventType.MouseDrag :
			if( mbScrolling )
			{
				if( maPanels != null )
				{
					foreach( Panel panel in maPanels )
					{
						panel.mWindowRect.x += Event.current.delta.x;
						panel.mWindowRect.y += Event.current.delta.y;
					}
				}
				Event.current.Use();
				mvBackgroundOffset += Event.current.delta; 
				mvBackgroundOffset = new Vector2( (mvBackgroundOffset.x + mTableTexture.width) % mTableTexture.width,  (mvBackgroundOffset.y + mTableTexture.height) % mTableTexture.height );
			}
			break;
		}
		

		
		
		if ( Event.current.type == EventType.Repaint && mSpaghettiMachine != null && maPanels != null )
		{			
			// Display minimap
			if( mbScrolling )
			{
				DisplayRectangleOnMinimap( new Rect( 0, 0, this.position.width, this.position.height ) );
				foreach( Panel panel in maPanels )
				{
					DisplayRectangleOnMinimap( panel.mWindowRect ); 
				}
			}	
			
			// Display links
			foreach( Link link in mSpaghettiMachine.GetLinks() )
			{	
				if( !link.IsSparkling() )
				{				
					if( link.mStartSlot.HasOctarinePlug() )
					{
						mbConstantRefresh = true;
						GUI.color = mOctarine;
						link.DrawLink( mbScrolling );
						GUI.color = Color.white;
					}
					else if( link.mStartSlot.HasColourOutOfSpacePlug() )
					{
						mbConstantRefresh = true;
						GUI.color = mTheColourOutOfSpace;
						link.DrawLink( mbScrolling );
						GUI.color = Color.white;
					}
					else
					{
						link.DrawLink( mbScrolling );
					}
				}
			}
			
			// Display sparkling links above all other links
			foreach( Link link in mSpaghettiMachine.GetLinks() )
			{	
				if( link.IsSparkling() )
				{
					GUI.color = link.GetSparklingColor();
					link.DrawLink( true );
					GUI.color = Color.white;
				}
			}
			
		
		}
		
 		BeginWindows();
		
		// Display name of Spaghetti machine (if any) in the upper left corner
		if( mSpaghettiMachine != null )
		{			
			GUILayout.Space( 2 );
			GUILayout.BeginHorizontal();
			GUILayout.Label( "  "+mSpaghettiMachine.name );
			GUILayout.Space( 10 );
			if( GUILayout.Button( "", mGUISkin.FindStyle("xButton") ) )
			{
				SetSpaghettiMachine( null );
				maPanels = null;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		
		
		// Drag and drop SpaghettiMachine
		bool bMessage = false;
		if( Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform )
		{
			mstrDragAndDropErrorMessage = "";
			
			if( DragAndDrop.objectReferences.Length == 1 )
		    {	
				GameObject draggedObject =  DragAndDrop.objectReferences[0] as GameObject;
				SpaghettiMachineCore newMachine = ( draggedObject == null ) ? null : draggedObject.GetComponent( typeof(SpaghettiMachineCore) ) as SpaghettiMachineCore;
	
				if( newMachine == null )
				{
					mstrDragAndDropErrorMessage = "That's not a SpaghettiMachine.";
					mbDragAndDropErrorRed = true;
				}
				else if( PrefabUtility.GetPrefabType(newMachine) == PrefabType.Prefab || PrefabUtility.GetPrefabType(newMachine) == PrefabType.ModelPrefab )
				{
					
					mstrDragAndDropErrorMessage = "That's a prefab. I can't observe a prefab.";
					mbDragAndDropErrorRed = true;
				}
				else
				{
					mstrDragAndDropErrorMessage = "Yummy! Spaghetti!";
					mbDragAndDropErrorRed = false;
					
					// Show a copy icon on the drag
		        	DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					
					if( Event.current.type == EventType.DragPerform )
					{
						SetSpaghettiMachine( newMachine );
					}
					Event.current.Use();
				}
		    }
		}
		else if( Event.current.type == EventType.DragExited )
		{
			mstrDragAndDropErrorMessage = "";
		}
		
		if( mstrDragAndDropErrorMessage != "" )
		{
			if( mbDragAndDropErrorRed )
			{
				GUI.color = Color.red;
			}
			ShowCenteredMessage( mstrDragAndDropErrorMessage );
			bMessage = true;
			GUI.color = Color.white;
		}
		
		if( mSpaghettiMachine == null )
		{
			if( mstrDragAndDropErrorMessage == "" )
			{
				ShowCenteredMessage("Drag-and-drop any SpaghettiMachine object into this window.");
				bMessage = true;
			}
			EndWindows();
			return;	
		}
		
		if( maPanels == null )
		{
			if( mstrDragAndDropErrorMessage == "" )
			{
				if( Application.isPlaying )
				{
					ShowCenteredMessage("Graph not yet loaded.");
					bMessage = true;
				}
				else
				{
					ShowCenteredMessage("Run application to display spaghetti graph in real-time.");
					bMessage = true;
				}
			}
			EndWindows();
			return;	
		}
		
		if( !bMessage )
		{
			ShowCenteredMessage("");
		}
		
		//Display panels
		for( int i = 0; i < maPanels.Length; i++ )
		{
			Panel panel = maPanels[i] as Panel;	
			string strTitle = panel.GetPanelType() ;

			int iID = START_PANELS_IDS + i;
			panel.miUniqueID = iID;

			panel.mWindowRect = GUI.Window( iID, panel.mWindowRect, DoPanelWindow, strTitle );
		}		
		
		//Display options window
		Rect windowrect = new Rect( Screen.width - 203, 3, 200, 5 );
		GUILayout.Window ( OPTIONS_WINDOW_ID, windowrect, DoOptionsWindow, "Options", mGUISkin.FindStyle("Glass") );
		
		EndWindows();
		
		// Display telltales
		if( mSpaghettiMachine.showActivePanels )
		{
			for( int i = 0; i < maPanels.Length; i++ )
			{
				Panel panel = maPanels[i] as Panel;	
				
				Rect telltaleRect = new Rect( panel.mWindowRect.x + panel.mWindowRect.width - 25, panel.mWindowRect.y - 8, 32, 32 );
				GUI.DrawTexture( telltaleRect, panel.IsActive() ? mIconTelltaleGreen : mIconTelltaleRed );
			}
		}
		
		// Display potentials
		if( mSpaghettiMachine.showPotentials )
		{
			for( int i = 0; i < maPanels.Length; i++ )
			{
				Panel panel = maPanels[i] as Panel;	
				Slot[] aSlots = panel.GetInputSlots() ;
				foreach( Slot slot in aSlots)
				{
					Vector2 vPos = slot.GetGlobalPlugPosition();
					Rect rect = new Rect( vPos.x - 20, vPos.y - 8, 40, 20 );
					GUI.Label( rect, ""+slot.GetPotential(), mGUISkin.FindStyle("Voltmeter") );
				}
			}
		}
		
		//Display custom variables
		if( mSpaghettiMachine.showCustomVariables )
		{
			for( int i = 0; i < maPanels.Length; i++ )
			{
				Panel panel = maPanels[i] as Panel;	
				
				string strOutput = panel.GetVariablesDisplayString();
				if( strOutput != "" )
				{
					GUILayout.BeginArea( new Rect( panel.mWindowRect.x,  panel.mWindowRect.y +  panel.mWindowRect.height - 6, panel.mWindowRect.width, 500 ) );
					GUILayout.Label( strOutput, mGUISkin.FindStyle("LittleBlackFrame") );
					GUILayout.EndArea();
				}
				
				Slot[] aSlots = panel.GetSlots() ;
				foreach( Slot slot in aSlots)
				{
					strOutput = slot.GetVariablesDisplayString();
					if( strOutput != "" )
					{
						GUILayout.BeginArea( new Rect( panel.mWindowRect.x + panel.mWindowRect.width - 5,  slot.GetGlobalPlugPosition().y - 10 , 200, 500 ) );
						GUILayout.Label( strOutput, mGUISkin.FindStyle("LittleBlackFrame") );
						GUILayout.EndArea();
					}
				}
			}	
		}
		
		//Octarine and Thecolouroutofspace
		UpdateSpecialColors();
		
		GUI.BringWindowToFront( OPTIONS_WINDOW_ID );
	}
	
	//------------------------------------------------------------------------------------------------
	// ShowCenteredMessage
	// Displays a text at the center of the screen.
	//------------------------------------------------------------------------------------------------	
	void ShowCenteredMessage( string strMessage )
	{	
		GUILayout.BeginArea( new Rect(0, 0, Screen.width, Screen.height) );
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label( strMessage );
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();		
		GUILayout.EndArea();
	}
	
	//------------------------------------------------------------------------------------------------
	// DoOptionsWindow
	// Handles the window for one panel
	//------------------------------------------------------------------------------------------------	
	void DoOptionsWindow( int id )
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(mIconOptionActive);
		mSpaghettiMachine.showActivePanels =  GUILayout.Toggle( mSpaghettiMachine.showActivePanels, "Show active panels", mGUISkin.FindStyle("GlassToggle"));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();		
		
		GUILayout.BeginHorizontal();
		GUILayout.Label(mIconOptionVariables);
		mSpaghettiMachine.showCustomVariables =  GUILayout.Toggle( mSpaghettiMachine.showCustomVariables, "Show custom variables", mGUISkin.FindStyle("GlassToggle") );
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();		
		                
		GUILayout.BeginHorizontal();
		GUILayout.Label(mIconOptionPotentials);
		mSpaghettiMachine.showPotentials =  GUILayout.Toggle( mSpaghettiMachine.showPotentials, "Show plug potentials", mGUISkin.FindStyle("GlassToggle") );
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();		                
	}

	//------------------------------------------------------------------------------------------------
	// SetSpaghettiMachine
	// To change the SpaghettiMachine
	//------------------------------------------------------------------------------------------------	
	void SetSpaghettiMachine( SpaghettiMachineCore newSpaghettiMachine )
	{
		if( mSpaghettiMachine != newSpaghettiMachine )
		{
			if( newSpaghettiMachine != null )
			{
				mSpaghettiMachine = newSpaghettiMachine;
				maPanels = mSpaghettiMachine.GetPanels();
				mSpaghettiMachine.SetDebugger( this );				
			}
			else
			{
				maPanels = null;
				mSpaghettiMachine.SetDebugger( null );
				mSpaghettiMachine = null;
			}
		}
	}
	
	//------------------------------------------------------------------------------------------------
	// DoPanelWindow
	// Handles the window for one panel
	//------------------------------------------------------------------------------------------------	
	void DoPanelWindow( int id )
	{		
		GUI.color = Color.white;		
		Panel panel = maPanels[ id - START_PANELS_IDS ] as Panel;	
		
		if( panel.mImage != null )
		{
			Rect rectangle = new Rect( Panel.WINDOW_MARGE_LEFT, Panel.WINDOW_MARGE_TOP, panel.mImage.width, panel.mImage.height );
			GUI.DrawTexture( rectangle, panel.mImage );
		}
			
			
		bool bIgnoreFollowingSlots = false;
		foreach( Slot slot in panel.maSlots )
		{
			if( !bIgnoreFollowingSlots )
			{	
				switch( slot.mContentType )
				{
				case ContentType.None :
					if( !slot.mbCustomPosition )
					{
						switch( slot.mPlugType )
						{	
						case PlugType.None :
							GUILayout.Label( slot.mstrLabel );
							break;
							
						case PlugType.Input :
							GUILayout.Label( " "+slot.mstrLabel );
							break;
							
						case PlugType.Output :
							GUILayout.BeginHorizontal();
							GUILayout.FlexibleSpace();
							GUILayout.Label( slot.mstrLabel);
							GUILayout.Space( 5 );
							GUILayout.EndHorizontal();	
							break;
							
						case PlugType.InOut :
							GUILayout.Label( "     ");	
							break;
						}
					}
					break;
		
				case ContentType.String :			
					GUILayout.Label( " "+slot.mstrLabel + ": "+ slot.mstrDataText );							
					break;
					
				case ContentType.Text :			
					int iFirstLineLength = slot.mstrDataText.IndexOf("\n");
					string strFirstLine;
					if( iFirstLineLength == -1 )
					{
						strFirstLine = slot.mstrDataText;
					}
					else
					{
						strFirstLine = slot.mstrDataText.Substring( 0, iFirstLineLength ) + "...";
					}	

					GUILayout.BeginHorizontal( GUILayout.Width( panel.mWindowRect.width - Panel.WINDOW_MARGE_LEFT - Panel.WINDOW_MARGE_RIGHT - 30 )  );	
					GUILayout.Button( mIconNoteblock, GUILayout.Width(20) );
					GUILayout.Label(slot.mstrLabel + ": " + strFirstLine);
					GUILayout.EndHorizontal();						
					break;

				case ContentType.Float :		
					GUILayout.Label( " "+slot.mstrLabel + ": " +slot.mfDataFloat);
					break;
					
				case ContentType.Int :		
					GUILayout.Label( " "+slot.mstrLabel + ": " +slot.miDataInt);
					break;
					
				case ContentType.Bool :
					EditorGUILayout.BeginHorizontal();	
					GUILayout.Label( " "+slot.mstrLabel);
					GUILayout.Space( 5 );
					EditorGUILayout.Toggle( slot.miDataInt == 1 , GUILayout.ExpandWidth(false) );	
					EditorGUILayout.EndHorizontal();
					break;
					
				case ContentType.Master :
					GUILayout.BeginHorizontal();			
					GUILayout.Label( mIconMaster );	
					if( GameObject.Find( slot.mstrDataText ) == null )
					{
						Color oldcolor = GUI.contentColor;
						GUI.contentColor = 0.2f * Color.white + Color.red;
						GUILayout.Label( slot.mstrDataText, mGUISkin.FindStyle("PanelTextField") );	
						GUI.contentColor = oldcolor;
					}
					else
					{
						GUILayout.Label( slot.mstrDataText, mGUISkin.FindStyle("PanelTextField") );	
					}	
					GUILayout.EndHorizontal();				
					break;					

				case ContentType.GameObject :
					GUILayout.BeginHorizontal();			
					GUILayout.Label( mIconGameObject );
					if( GameObject.Find( slot.mstrDataText ) == null )
					{
						Color oldcolor = GUI.contentColor;
						GUI.contentColor = 0.2f * Color.white + Color.red;
						GUILayout.Label( slot.mstrDataText, mGUISkin.FindStyle("PanelTextField") );	
						GUI.contentColor = oldcolor;
					}
					else
					{
						GUILayout.Label( slot.mstrDataText, mGUISkin.FindStyle("PanelTextField") );	
					}	
					GUILayout.EndHorizontal();				
					break;	
					
				case ContentType.Enum :
					EditorGUILayout.BeginHorizontal();	
					GUILayout.Label( " "+slot.mstrLabel);
					GUILayout.Space( 5 );
					GUILayout.Label (slot.maEnumValues[slot.miDataInt], mGUISkin.FindStyle("EnumButton") );
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
					break;					
					
				case ContentType.More :	
					EditorGUILayout.Foldout( slot.mbMore, slot.mbMore ? "" : "more..." );
					if( !slot.mbMore )
					{
						panel.mWindowRect.height = Panel.WINDOW_MARGE_TOP + Panel.SLOT_HEIGHT * (slot.miIndex+1) + Panel.WINDOW_MARGE_BOTTOM;
						bIgnoreFollowingSlots = true;						
					}
					else 
					{
						panel.mWindowRect.height = Panel.WINDOW_MARGE_TOP + Panel.SLOT_HEIGHT * slot.mParent.maSlots.Length + Panel.WINDOW_MARGE_BOTTOM;
					}
					break;	

				case ContentType.Vector3 :
					GUILayout.Label(  " "+slot.mstrLabel + ": "+ slot.mvDataVector.x + " "+ slot.mvDataVector.y + " " + slot.mvDataVector.z, mGUISkin.FindStyle("PanelTextField"), GUILayout.MinWidth( 30 ) );
					break;
					
				case ContentType.Curve :
					GUILayout.BeginHorizontal();	
					GUILayout.Label( slot.mstrLabel );
					EditorGUILayout.CurveField( slot.mDataCurve, GUILayout.Height(11), GUILayout.MinWidth(10)  );
					GUILayout.Space( 5 );
					GUILayout.EndHorizontal();
					break;	
					
				case ContentType.Color :
					GUILayout.BeginHorizontal();	
					GUILayout.Label( slot.mstrLabel );		
					EditorGUILayout.ColorField(  slot.mDataColor, GUILayout.Height(11), GUILayout.MinWidth(10)  );
					GUILayout.EndHorizontal();
					break;					
				}
			}
		}
		
		foreach( Slot slot in panel.maSlots )
		{
			// Draw the plug (if any)
			if( slot.mPlugType != PlugType.None )
			{
				Vector2 vPos = slot.GetLocalPlugPosition();
				GUI.color = slot.GetPlugColor();
				if( slot.mbMultipleAllowed )
				{
					GUI.DrawTexture(new Rect( vPos.x -3, vPos.y - 3, 7, 7), mMultiplugTexture );
				}
				else
				{
					GUI.DrawTexture(new Rect( vPos.x -3, vPos.y - 3, 7, 7), mPlugTexture );
				}					
				GUI.color = Color.white;
			}
				
		}
	}
	
	
	
	//================================================================================================
	//
	//   DRAWING METHODS
	//
	//================================================================================================
	
	//------------------------------------------------------------------------------------------------
	// DisplayRectangleOnMinimap
	// Pretty much self explanatory: Displays actually a rectangle on the minimap.
	//------------------------------------------------------------------------------------------------		
	void DisplayRectangleOnMinimap ( Rect rectangle ) 
	{
		if( mMinimapTexture == null )
		{
			mMinimapTexture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
			mMinimapTexture.SetPixel( 0, 0, new Color( 1.0f, 1.0f, 1.0f, 0.1f ) );
			mMinimapTexture.Apply();	
		}
		
		Rect smallRectangle = new Rect();
		Vector3 vCenter;
		vCenter.x = 0.5f * this.position.width;
		vCenter.y = 0.5f * this.position.height;		
		smallRectangle.x = vCenter.x + MINIMAP_SCALE * ( rectangle.x - vCenter.x );
		smallRectangle.y = vCenter.y + MINIMAP_SCALE * ( rectangle.y - vCenter.y );
		smallRectangle.width = MINIMAP_SCALE * rectangle.width;
		smallRectangle.height = MINIMAP_SCALE * rectangle.height;
		
		GUI.DrawTexture( smallRectangle, mMinimapTexture );
	}
	
	void Update()
	{
		if( maPanels != null && (mSpaghettiMachine == null || !Application.isPlaying) )
		{
			maPanels = null;
			mSpaghettiMachine.SetDebugger( null );
		}
		else if( maPanels == null && Application.isPlaying && mSpaghettiMachine != null )
		{
			// The following line makes no sense in theory, but it seems to be a workaround
			// for a bug where mSpaghettiMachine seemed to be pointing to some shadow-machine 
			// (not identical with the actual machine but not null either, maybe a copy remaining in memory)
			mSpaghettiMachine = mSpaghettiMachine.gameObject.GetComponent( typeof(SpaghettiMachineCore)) as SpaghettiMachineCore;			
			maPanels = mSpaghettiMachine.GetPanels();
			mSpaghettiMachine.SetDebugger( this );
			Repaint();
		}
		
		if( mbConstantRefresh )
		{
			Repaint();
		}
		
		if( maPanels != null )
		{
			foreach( Panel panel in maPanels )
			{
				panel.UpdateDebugger();
			}
		}		
	}
	
	
	public void Refresh()
	{
		Repaint();
	}

	
	private void UpdateSpecialColors()
	{
		float fTimeParameter = (float)EditorApplication.timeSinceStartup;
		float fRed = 0.5f + 0.5f * Mathf.Sin( fTimeParameter * OCTARINE_SPEED_RED ); 
		float fGreen = 0.5f + 0.5f * Mathf.Sin( fTimeParameter * OCTARINE_SPEED_GREEN ); 
		float fBlue = 0.5f + 0.5f * Mathf.Sin( fTimeParameter * OCTARINE_SPEED_BLUE ); 		
		mOctarine =  new Color( fRed, fGreen, fBlue, 1.0f ); 
		
		bool bColour1 = (( fTimeParameter % SPACECOLOR_INTERVAL ) < SPACECOLOR_INTERVAL * 0.5f );
		mTheColourOutOfSpace = bColour1 ? Color.green : Color.blue + Color.red  ;
	}
}
