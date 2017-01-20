#define _NODEMO_

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Xml; 
using System;
using System.Security;
using System.IO;
using System.Threading; 

[System.Serializable]
public class Group
{	
	[SerializeField]
	public EditorPanel[] maPanels; 
	
	[SerializeField]
	public int[] maPanelIDs;
	
	[SerializeField]
	public int miSize; 
}

[System.Serializable]
public class SpaghettiMachineEditor : EditorWindow 
{
	public static string 	mstrVersion = "v1.14u5";  
	
	enum MouseStatus  { Default, DrawingLine, DrawingBox, MovingPanels, Scrolling, DrawScrolling };
	enum MouseHandlingCall  { BeforeDrawingAllPanels, StartDrawingPanel, EndDrawingPanel, AfterDrawingAllPanels };
	
	MouseStatus mMouseStatus = MouseStatus.Default;
	
    public static Texture2D mLineTexture = null;
	
	private static SpaghettiMachineEditor mInstance;
	
	[SerializeField]
	private EditorPanel[] maPanels;
	
	[SerializeField]
	private PanelSet[] maPanelSets;

	[SerializeField]
	private EditorLink[] maLinks;

	[SerializeField]
	private Group[] maGroups;
	
	//[SerializeField]
	//public int[] maGroupSizes; 		
	
	private Group mSelectedGroup = null;
	private int miSelectedPanels = 0;
	
	private static XmlReader mReader;
	private static XmlWriter mWriter;
	
	private EditorSlot mDrawingStartSlot  = null;
	//private Color mDrawingColor;
	
	private Rect mWindowRectActions = new Rect( 5, 20, 15, 5 );	
	private Rect mWindowRectEditor = new Rect( 5, 5, 10, 5 );		
		
	private Rect mWindowRectFind = new Rect( -1, -1, 200, 180 );	
	
	private Texture mIconNoteblock;
	private Texture mIconMaster;
	private Texture mIconGameObject;	
	private Texture mIconMore;
	private Texture mIconLess;	
	private Texture mIconNew;		
	private Texture mIconLoad;	
	private Texture mIconLoadAdd;	
	private Texture mIconSave;
	private Texture mIconSaveAs;	
	private Texture mIconAllignLeft;	
	private Texture mIconAllignTop;	
	private Texture mIconAllignRight;	
	private Texture mIconAllignBottom;
	private Texture mIconGroup;	
	private Texture mIconUngroup;
	private Texture mIconRefresh;	
	private Texture mIconFind;
	private static int miNextUnusedSlotID;
	private Texture mTableTexture;
	private Texture mForkTexture;
	private Texture mForkSpaghettiTexture;	

	private Texture mPlugTexture;
	private Texture mMultiplugTexture;		
	
	private Texture2D mMinimapTexture;
	private Texture2D mSelectionTexture;
	
	private float MINIMAP_SCALE = 0.1f;
	private static int ACTION_WINDOW_ID = 0;
	private static int EDITOR_WINDOW_ID = 1;	
	private static int PANELSETS_WINDOW_ID = 2;	
	private static int EDIT_WINDOW_ID = 3;	
	private static int FIND_WINDOW_ID = 4;	
	private static int POPUP_WINDOW_ID = 5;
	private static int ENUM_WINDOW_ID = 6;
	private static int START_PANELS_IDS = 1000;	
	private static int START_SLOT_IDS = 1;
	private static int MAX_DUPLICATE_OFFSET = 500;	
	
	private string mstrPathPanelsets;
	private string mstrPathDiagrams;
	private string mstrLastDiagramName = "";
	
	private string mstrFind = "";
	private string mstrReplace = "";
	private bool mbFindWindowOpen = false;	
	private bool mbFindAndReplace = false;
	private bool mbFindSelectionOnly = true;
	
	private Vector2 mvDragStart;
	
	[SerializeField]
	private bool mbDirty = false;
	
	[SerializeField]
	private EditorSlot mEditingSlot = null;
	
	[SerializeField]
	private bool mbCtrlKeyDown = false;

	[SerializeField]
	private bool mbAltKeyDown = false;	
	
	[SerializeField]
	private bool mbShiftKeyDown = false;		
	
	private GUISkin mGUISkin;
	
	public float mfBackgroundOffset = 0;
	
	[SerializeField]	
	private Vector2 mvPanelSetsScrollPosition; 
	private Vector2 mvEditWindowScrollPosition;
	
	private bool mbEditingSlot = false;
	
	public Vector2 mvDemoPostitPosition;
	public Vector2 mvDemoPostitShadowPosition;	
	public float mfDemoPostitAngle;
	public Texture2D mDemoPostitTexture; 
	public Texture2D mDemoPostitShadowTexture;
	public Texture mDemoPopupTexture; 	
	

	
	private bool mbPopup = false;
	private bool mbConfirmDeletePopup = false;
	private string mstrPopupMessage = "...";

	// Octarine
	private Color mOctarine;
	private float OCTARINE_SPEED_RED = 0.31f;
	private float OCTARINE_SPEED_GREEN = 0.56f;
	private float OCTARINE_SPEED_BLUE = 0.44f;
	
	// The Colour out of Space
	private Color mTheColourOutOfSpace;	
	private float SPACECOLOR_INTERVAL = 0.1f;	
	private bool  mbGoneMad = false;
	private float mfLastMadnessUpdateTime = 0.0f;
	private float MADNESS_UPDATE_INTERVAL = 0.03f;

    private Texture2D[] maDrawingBezierCurveTextures   = null; 
	private Rect[]		maDrawingBezierCurveRectangles;
	
	private EditorSlot mActiveEnumSlot = null;
	private Rect mEnumRect;
	
	private EditorSlot mFocusedSlot = null;
	private EditorPanel mPrimarySelectedPanel = null;
	
	private Vector2    mvDuplicateOffset = new Vector2( 50, 50 );
	private bool       mModifyingDuplicateOffset = false;

#if _DEMO_	
	private static float POSTIT_PARALLAX_FACTOR = 1.2f;
	private string DEMO_PRIME_MESSAGE = "In the demo version, you can't save graphs with more than 17 nodes.";
#endif
	
	//------------------------------------------------------------------------------------------------
	// Initialize
	// called from SpaghettiMachineEditorWindow::Init
	//------------------------------------------------------------------------------------------------
	public static void Initialize ( SpaghettiMachineEditor instance ) 
	{

        mInstance = instance;

		miNextUnusedSlotID = START_SLOT_IDS;

		if( mInstance.maPanels == null )
		{
			mInstance.maPanels = new EditorPanel[0];	
		}	
		if( mInstance.maPanelSets == null )
		{
			mInstance.maPanelSets = new PanelSet[0];	
		}			
		if( mInstance.maLinks == null )
		{
			mInstance.maLinks = new EditorLink[0];	
		}		
		mInstance.mstrPathPanelsets = PlayerPrefs.GetString ( "path_panelsets", Application.dataPath );
		mInstance.mstrPathDiagrams = PlayerPrefs.GetString ( "path_diagrams", Application.dataPath );		
	}

	//------------------------------------------------------------------------------------------------
	// ClearAll
	// nukes everything
	//------------------------------------------------------------------------------------------------	
	void ClearAll()
	{
		maPanels =  new EditorPanel[0];	
		maLinks =  new EditorLink[0];
		maGroups = new Group[0];
	}

	//------------------------------------------------------------------------------------------------
	// GetInstance
	// If you wonder about this, google "Singleton"
	//------------------------------------------------------------------------------------------------
	public static SpaghettiMachineEditor GetInstance()
	{
		if( mInstance == null ) 
		{
			mInstance = (SpaghettiMachineEditor)EditorWindow.GetWindow( typeof( SpaghettiMachineEditor ) );
		}
		return mInstance;
	}	
	
	//================================================================================================
	//
	//   ID MANAGEMENT
	//
	//   Each panel or slot has an unique ID (which also serves as window ID)
	//
	//================================================================================================
	

	//------------------------------------------------------------------------------------------------
	// GetUnusedUniqueIDfor
	// creates a new ID and registers the object for that ID
	//------------------------------------------------------------------------------------------------	
	public int GetUnusedUniqueID() 
	{
		return miNextUnusedSlotID++;
	}

	//------------------------------------------------------------------------------------------------
	// GetSlotWithID
	// Returns the slot with a certain ID, if any
	//------------------------------------------------------------------------------------------------		
	public EditorSlot GetSlotWithID( int id  )
	{
		foreach( EditorPanel panel in maPanels )
		{
			foreach( EditorSlot slot in panel.maSlots )
			{
				if( slot.miUniqueID == id )
				{
					return slot;
				}
			}
		}
		return null;
	}
	
	//------------------------------------------------------------------------------------------------
	// GetPanelWithID
	// Returns the panel with a certain ID, if any
	//------------------------------------------------------------------------------------------------		
	public EditorPanel GetPanelWithID( int id  )
	{
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.miUniqueID == id )
			{
				return panel;
			}
		}
		Debug.LogWarning("Couldn't find panel with ID "+id );
		return null;
	}


	
	//================================================================================================
	//
	//   PANEL AND PANELSET MANAGEMENT
	//
	//   Panels (graph nodes) are stored in maPanels, panelsets (lists of panel types) in maPanelSets.
	//   Note that those arraylists must be initialized "on the fly" as we are in editor mode 
	//   and we cannot be sure that Awake or Start is called upfront.
	//
	//================================================================================================
	
	//------------------------------------------------------------------------------------------------
	// AddPanel
	// Adds a (already created) panel to maPanels
	//------------------------------------------------------------------------------------------------	
	public void AddPanel( EditorPanel panel )
	{
		if( maPanels == null )
		{
			maPanels = new EditorPanel[0];	
		}
		
		Array.Resize( ref maPanels, maPanels.Length + 1 );
		maPanels[maPanels.Length-1]=panel;
	}

	//------------------------------------------------------------------------------------------------
	// RemovePanel
	// Removes a panel from maPanels
	//------------------------------------------------------------------------------------------------
	public void RemovePanel( EditorPanel panel )
	{
		foreach( EditorSlot slot in panel.maSlots )
		{
			slot.DeleteAllConnections();	
		}
		
		if( maPanels == null )
		{
			return;
		}
		
		BreakGroupsWithPanel( panel );
		
		if( mActiveEnumSlot != null && mActiveEnumSlot.mParent == panel )
		{
			mActiveEnumSlot = null;
			mEnumRect.x = -1000;
		}
		
		int iRemoveIndex = Array.IndexOf( maPanels, panel );
		if( iRemoveIndex == -1 )
		{
			return;
		}			
		
		// Shift rest of the array
		for( int i = iRemoveIndex; i < maPanels.Length-1; i++ )
		{
			maPanels[i]	= maPanels[i+1];
		}
		Array.Resize( ref maPanels, maPanels.Length - 1 );
	}	
	
	//------------------------------------------------------------------------------------------------
	// AddPanelSet
	// Adds a (already created) panelset to maPanelSet
	//------------------------------------------------------------------------------------------------	
	public void AddPanelSet( PanelSet panelset )
	{
		if( maPanelSets == null )
		{
			maPanelSets = new PanelSet[0];	
		}
		
		// Append at the end
		Array.Resize( ref maPanelSets, maPanelSets.Length + 1 );
		maPanelSets[maPanelSets.Length-1] = panelset;
	}	

	//------------------------------------------------------------------------------------------------
	// RemovePanelset
	// Removes a panelset from maPanelSets
	//------------------------------------------------------------------------------------------------
	public void RemovePanelset( PanelSet panelset )
	{	
		if( maPanelSets == null )
		{
			return;
		}
		
		int iRemoveIndex = Array.IndexOf( maPanelSets, panelset );
		if( iRemoveIndex == -1 )
		{
			return;
		}			
		
		for( int i = iRemoveIndex; i < maPanelSets.Length-1; i++ )
		{
			maPanelSets[i]	= maPanelSets[i+1];
		}
		Array.Resize( ref maPanelSets, maPanelSets.Length - 1 );
	}

	//------------------------------------------------------------------------------------------------
	// ReloadPanelset
	// Reloads a panelset from its file
	//------------------------------------------------------------------------------------------------
	public void ReloadPanelset( PanelSet panelset )
	{	
		if( maPanelSets == null )
		{
			return;
		}
		
		int iReloadIndex = Array.IndexOf( maPanelSets, panelset );
		if( iReloadIndex == -1 )
		{
			return;
		}	
		
		PanelSet newPanelset = new PanelSet(); 			
		newPanelset.ReadFromFile( panelset.mstrPath );	
		maPanelSets[iReloadIndex] = newPanelset;			
	}
	
	
	//------------------------------------------------------------------------------------------------
	// AddLink
	// Adds an (already created) link to maLinks
	//------------------------------------------------------------------------------------------------	
	public void AddLink( EditorLink link )
	{
		if( maLinks == null )
		{
			maLinks = new EditorLink[0];	
		}
		
		Array.Resize( ref maLinks, maLinks.Length + 1 );
		maLinks[maLinks.Length-1] = link;
	}	

	//------------------------------------------------------------------------------------------------
	// RemoveLink
	// Removes a link from maLinks
	//------------------------------------------------------------------------------------------------
	public void RemoveLink( EditorLink link )
	{	
		if( maLinks == null )
		{
			return;
		}
		
		int iRemoveIndex = Array.IndexOf( maLinks, link );
		if( iRemoveIndex == -1 )
		{
			return;
		}			
		
		for( int i = iRemoveIndex; i < maLinks.Length-1; i++ )
		{
			maLinks[i]	= maLinks[i+1];
		}
		Array.Resize( ref maLinks, maLinks.Length - 1 );
	}	
	
	//================================================================================================
	//
	//   GUI METHODS
	//
	//================================================================================================

	//------------------------------------------------------------------------------------------------
	// LoadTextureIfNull
	// Called by the engine whenever it feels so.
	//------------------------------------------------------------------------------------------------	
	void LoadTextureIfNull( ref Texture texture, string strName ) 
	{
		if( texture == null )
		{
			texture = Resources.Load( strName  ) as Texture;
		}		
	}
	
		
	//------------------------------------------------------------------------------------------------
	// OnGUI
	// Called by the engine whenever it feels so.
	//------------------------------------------------------------------------------------------------	
	public void OnGUI () 
	{	

		
		if( maPanels == null )
		{
			maPanels = new EditorPanel[0];	
		}
		if( maPanelSets == null )
		{
			maPanelSets = new PanelSet[0];
		}		
		
		LoadTextureIfNull( ref mIconNoteblock, "SMIconNoteblock"  );
		LoadTextureIfNull( ref mIconMore, "SMIconMore"  );
		LoadTextureIfNull( ref mIconLess, "SMIconLess"  );
		LoadTextureIfNull( ref mIconMaster, "SMIconMaster"  );
		LoadTextureIfNull( ref mIconGameObject, "SMIconGameObject"  );
		LoadTextureIfNull( ref mIconNew, "SMNew"  );
		LoadTextureIfNull( ref mIconLoad, "SMLoad"  );
		LoadTextureIfNull( ref mIconLoadAdd, "SMLoadAdd"  );
		LoadTextureIfNull( ref mIconSave, "SMSave"  );
		LoadTextureIfNull( ref mIconSaveAs, "SMSaveAs"  );
		LoadTextureIfNull( ref mIconAllignLeft, "SMAllignLeft"  );
		LoadTextureIfNull( ref mIconAllignTop, "SMAllignTop"  );
		LoadTextureIfNull( ref mIconAllignRight, "SMAllignRight"  );
		LoadTextureIfNull( ref mIconAllignBottom, "SMAllignBottom"  );
		LoadTextureIfNull( ref mTableTexture, "SMTableTexture"  );
		LoadTextureIfNull( ref mForkTexture, "SMFork"  );
		LoadTextureIfNull( ref mForkSpaghettiTexture, "SMForkSpaghetti"  );
		LoadTextureIfNull( ref mIconGroup, "SMGroup"  );
		LoadTextureIfNull( ref mIconUngroup, "SMUngroup"  );
		LoadTextureIfNull( ref mIconRefresh, "SMRefreshGraph"  );
		LoadTextureIfNull( ref mIconFind, "SMFind"  );
		LoadTextureIfNull( ref mPlugTexture, "SMMonoplug"  );
		LoadTextureIfNull( ref mMultiplugTexture, "SMMultiplug"  );

		if( mSelectionTexture == null )
		{
			mSelectionTexture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
			mSelectionTexture.SetPixel( 0, 0, new Color(0.0f, 1.0f, 0.0f, 0.1f ) );
			mSelectionTexture.Apply();	
		}
		
#if _DEMO_
		if( mDemoPostitTexture == null || mDemoPostitShadowTexture == null )
		{
			mDemoPostitTexture = Resources.Load("DemoPostit") as Texture2D;
			
			mDemoPostitShadowTexture= Resources.Load("DemoPostitShadow") as Texture2D;
			mvDemoPostitPosition.x = Screen.width / 2;
			mvDemoPostitPosition.y = Screen.height / 2;
			mvDemoPostitShadowPosition = mvDemoPostitPosition + new Vector2( 50.0f, 50.0f );
			mfDemoPostitAngle = 0.0f;
			

		}

		if( mDemoPostitTexture == null || mDemoPostitTexture.width < 150 || mDemoPostitTexture.height < 150 || mDemoPostitTexture.GetPixelBilinear( 0.5f, 0.5f ).a < 1.0f )
		{
			Debug.LogError("The demo popup texture has been manipulated. Nice try.");
			mDemoPostitTexture = new Texture2D( 200, 200 );
			for( int i = 0; i < 200; i++ )
			{
				for( int j = 0; j < 200; j++ )
				{	
					mDemoPostitTexture.SetPixel( i, j, Color.yellow );
				}
			} 
			mDemoPostitTexture.Apply();
		}
#endif
		

		
		if( mGUISkin == null )
		{
			mGUISkin  = EditorGUIUtility.Load("SpaghettiMachine/WoodSkin/WoodSkin.guiskin") as GUISkin; 				
		}
		GUI.skin = mGUISkin;	
		

		
		ApplyIdentityTransform();
		
		if( GUI.GetNameOfFocusedControl() == "" )
		{
			mFocusedSlot = null;
		}
		
		wantsMouseMove = true;
		
		// Take track of ctrl key status:
		switch( Event.current.keyCode )
		{
		case KeyCode.LeftControl:
		case KeyCode.RightControl:
		case KeyCode.LeftApple:
		case KeyCode.RightApple:				
			switch ( Event.current.type )
			{
			case EventType.KeyDown :
				mbCtrlKeyDown = true;
				break;
				
			case EventType.KeyUp :
				mbCtrlKeyDown = false;
				break;
			}
			break;			
		}	
		
		mbAltKeyDown = Event.current.alt;
		bool bShiftKeyWasDownLastFrame = mbShiftKeyDown;
		mbShiftKeyDown = Event.current.shift;
		

		HandleMouseAction( MouseHandlingCall.BeforeDrawingAllPanels, null );
		
		if ( Event.current.type == EventType.Repaint )
		{
			//*/			
			// Draw table texture
			Rect screenRect = new Rect (0, -mfBackgroundOffset, Screen.width, Screen.height );
			GUI.DrawTexture( screenRect, mTableTexture );
			screenRect.y += Screen.height;
			GUI.DrawTexture( screenRect, mTableTexture );
			//*/

			// Display minimap
			if( mMouseStatus == MouseStatus.Scrolling || mMouseStatus == MouseStatus.DrawScrolling || mbAltKeyDown  )
			{
				DisplayRectangleOnMinimap( new Rect( 0, 0, this.position.width, this.position.height ) );
				foreach( EditorPanel panel in maPanels )
				{
					if( panel.mbSelected )
					{
						GUI.color = Color.green;	
					}
					DisplayRectangleOnMinimap( panel.mWindowRect ); 
					GUI.color = Color.white;
				}
			}

			
			
			// Draw selection rectangle if any
			if( mMouseStatus == MouseStatus.DrawingBox )
			{	
				Rect rectangle = new Rect( mvDragStart.x, mvDragStart.y, Event.current.mousePosition.x - mvDragStart.x, Event.current.mousePosition.y - mvDragStart.y );		
				GUI.DrawTexture( rectangle, mSelectionTexture );
			}	
	
			//*/
			// Display links
			foreach( EditorLink link in maLinks )
			{	
				if( link.mStartSlot.HasOctarinePlug() )
				{
					GUI.color = mOctarine;
					link.DrawLink( mMouseStatus == MouseStatus.MovingPanels );
					GUI.color = Color.white;
				}
				else if( link.mStartSlot.HasColourOutOfSpacePlug() )
				{
					GUI.color = mTheColourOutOfSpace;
					link.DrawLink( mMouseStatus == MouseStatus.MovingPanels );
					GUI.color = Color.white;
				}
				else
				{
					link.DrawLink( mMouseStatus == MouseStatus.MovingPanels );
				}
			}
			//*/

			
			//Mark selected group
			if( mSelectedGroup != null )
			{
				float xMin = Mathf.Infinity;
				float xMax = -Mathf.Infinity;
				float yMin = Mathf.Infinity;
				float yMax = -Mathf.Infinity;
				foreach( EditorPanel panel in mSelectedGroup.maPanels )
				{
					xMin = Mathf.Min( xMin, panel.mWindowRect.x );
					xMax = Mathf.Max( xMax, panel.mWindowRect.x +  panel.mWindowRect.width );
					yMin = Mathf.Min( yMin, panel.mWindowRect.y );
					yMax = Mathf.Max( yMax, panel.mWindowRect.y +  panel.mWindowRect.height );				
				}
				xMin -= 3;
				xMax -= 7;
				yMin -= 3;
				yMax -= 7;		
				if( xMax > 0 && xMin < Screen.width && yMax > 0 && yMin < Screen.height )
				{
					GUI.DrawTexture( new Rect( xMin, yMin, xMax - xMin, yMax - yMin ), mSelectionTexture );
				}
			}
		}
		
		ApplyIdentityTransform();  
		
		//Display name in the upper left corner
		GUILayout.Space( 2 );
		GUILayout.Label( "  "+mstrLastDiagramName + (mbDirty ? "*" : ""));
					
		//Display help string
		string strHint = "";
		if( mbShiftKeyDown )
		{
			if( mbAltKeyDown )
			{
				strHint = "Fast minimap scrolling active. Selected panels won't scroll while Shift is hold down.";
			}
			else
			{
				strHint = "Selected panels won't scroll while Shift is hold down.";
			}
		}
		else
		{
			if( mbAltKeyDown )
			{
				strHint = "Fast minimap scrolling active.";
			}
		}	
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label( strHint );

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		
        BeginWindows();
		
		
		if( mbPopup )
		{
			Rect rect = new Rect( Screen.width/2 - 100, Screen.height/2 - 100, 200, 30 );
			GUILayout.Window( POPUP_WINDOW_ID, rect, DoPopupWindow, "Demo version", mGUISkin.GetStyle("Glass") );
			GUI.BringWindowToFront( POPUP_WINDOW_ID );
		}
		else if( mbConfirmDeletePopup )
		{
			Rect rect = new Rect( Screen.width/2 - 100, Screen.height/2 - 100, 200, 30 );
			GUILayout.Window( POPUP_WINDOW_ID, rect, DoConfirmDeleteWindow, "Srsly?", mGUISkin.GetStyle("Glass") );
			GUI.BringWindowToFront( POPUP_WINDOW_ID );
		}	
		
			
		//Display panels
		miSelectedPanels = 0;
		mbEditingSlot = false;
		for( int i = 0; i < maPanels.Length; i++ )
		{
			EditorPanel panel = maPanels[i] as EditorPanel;	
			
			int iID = START_PANELS_IDS + i;
			panel.miUniqueID = iID;
			
			string strTitle = panel.mstrType ;
			GUIStyle windowstyle = GUI.skin.GetStyle("Window");
			panel.mvHoveringOffset = Vector2.zero;

			//strTitle = panel.mstrType + " (" + panel.miUniqueID + ")";
			if( panel.mbSelected )
			{
				miSelectedPanels++;
				GUI.color = Color.green + 0.5f * Color.grey;
				if( mbShiftKeyDown )
				{
					windowstyle = GUI.skin.GetStyle("WindowHovering");	
					panel.mvHoveringOffset.x = -2;
					panel.mvHoveringOffset.y = -2;
				}
			}
			else
			{
				GUI.color = Color.white;
			}

			if( mbGoneMad )
			{
				panel.DoMadnessTransform();	
			}
			panel.mWindowRect.x += panel.mvHoveringOffset.x;
			panel.mWindowRect.y += panel.mvHoveringOffset.y;
			if( panel.mWindowRect.x < Screen.width && panel.mWindowRect.y < Screen.height && panel.mWindowRect.xMax > 0 &&  panel.mWindowRect.yMax > 0 )
			{
				panel.mWindowRect = GUI.Window( iID, panel.mWindowRect, DoPanelWindow, strTitle, windowstyle );
			}
			panel.mWindowRect.x -= panel.mvHoveringOffset.x;
			panel.mWindowRect.y -= panel.mvHoveringOffset.y;			
			ApplyIdentityTransform();

			GUI.color = Color.white;			
			if( panel.mbBringToFrontOnceCreated )
			{
				GUI.BringWindowToFront( iID );
				panel.mbBringToFrontOnceCreated = false;			
			}
			
			foreach( EditorSlot slot in panel.maSlots )
			{
				if( slot.mbEditing )
				{
					
					mbEditingSlot = true;
					string strHeader = "Edit \""+slot.mstrLabel+"\"";					
					slot.mWindowRect = GUILayout.Window ( EDIT_WINDOW_ID, slot.mWindowRect, DoSlotEditWindow, strHeader );					
					if( slot.mbBringWindowToFrontOnceCreated )
					{
						GUI.BringWindowToFront( EDIT_WINDOW_ID );
						slot.mbBringWindowToFrontOnceCreated = false;
					}
					
					break;
				}					
			}



		}
		
		ApplyIdentityTransform();

		//Display actions and editor window (upper right corner)		
		mWindowRectActions.x = -2;
		mWindowRectActions = GUILayout.Window ( ACTION_WINDOW_ID, mWindowRectActions, DoActionsWindow, "Graph ", mGUISkin.GetStyle("Glass") );
		
		mWindowRectEditor.x = -2;
		mWindowRectEditor.y = mWindowRectActions.y + mWindowRectActions.height + 5;
		mWindowRectEditor = GUILayout.Window ( EDITOR_WINDOW_ID, mWindowRectEditor, DoEditorWindow, "Tools ", mGUISkin.GetStyle("Glass") );		
		
		
		
		//Display find window, if any
		if( mbFindWindowOpen )
		{
			mWindowRectFind.x = 0;
			mWindowRectFind.y = Screen.height - mWindowRectFind.height; 
			mWindowRectFind = GUILayout.Window ( FIND_WINDOW_ID, mWindowRectFind, DoFindWindow, mbFindAndReplace ? "Replace" : "Find", mGUISkin.GetStyle("Glass") );
		}
	

	
		
		
		//Display panelset bar
		Rect windowrect = new Rect( Screen.width - 200, 0, 200, Screen.height - 20 );
		GUILayout.Window ( PANELSETS_WINDOW_ID, windowrect, DoPanelSetsWindow, "Panel Sets", mGUISkin.GetStyle("Glass") );
		
		//Display enum list, if any
		if( mActiveEnumSlot != null  )
		{
			windowrect = mEnumRect;
			windowrect.width = 10;
			windowrect.height = 10;
			windowrect = GUILayout.Window ( ENUM_WINDOW_ID, windowrect, DoEnumWindow, " ", mGUISkin.GetStyle("TextField") );
			
			if( Event.current.type == EventType.MouseUp || ( Input.GetMouseButtonDown(0) &&  !windowrect.Contains( Event.current.mousePosition )))
			{
				// Whereever you clicked, close the enum window
				mActiveEnumSlot = null;
			}
		}

        EndWindows();
		
	
		if( !bShiftKeyWasDownLastFrame && mbShiftKeyDown )
		{
			BringSelectedToFront();
		}
		
		HandleMouseAction( MouseHandlingCall.AfterDrawingAllPanels, null );
			
		//Draw the connection the user is currently drawing around
		if (Event.current.type == EventType.Repaint )
		{
			if( mMouseStatus == MouseStatus.DrawingLine || mMouseStatus == MouseStatus.DrawScrolling )
			{	
				if( mDrawingStartSlot.HasOctarinePlug() )
				{
					GUI.color = mOctarine;
				}
				else if( mDrawingStartSlot.HasColourOutOfSpacePlug() )
				{
					if( !mbGoneMad )
					{
						GoMad();	
					}
					GUI.color = mTheColourOutOfSpace;
				}
				else
				{
					GUI.color = mDrawingStartSlot.mColor;
				}
					
				
				if( maDrawingBezierCurveRectangles == null || maDrawingBezierCurveTextures == null )
				{
					maDrawingBezierCurveRectangles = new Rect[ 0 ];
					maDrawingBezierCurveTextures = new Texture2D[ 0 ];	
				}
				  
                // Draw link onto textures
				if( mDrawingStartSlot.mPlugType == EditorPlugType.Input )
				{
					EditorLink.DrawBezierChainBetween( Event.current.mousePosition, mDrawingStartSlot.GetGlobalSlotPosition(), null, mDrawingStartSlot, ref maDrawingBezierCurveRectangles, ref maDrawingBezierCurveTextures, Color.white ); // mDrawingStartSlot.mColor );
				}
				else
				{	
					EditorLink.DrawBezierChainBetween( mDrawingStartSlot.GetGlobalSlotPosition(), Event.current.mousePosition, mDrawingStartSlot, null, ref maDrawingBezierCurveRectangles, ref maDrawingBezierCurveTextures, Color.white ); // mDrawingStartSlot.mColor );
				}
				
				// Draw textures onto screen
				for( int i = 0; i < maDrawingBezierCurveTextures.Length; i++ )
				{
					if( maDrawingBezierCurveTextures[i] != null  )
					{
						Rect rect = new Rect( maDrawingBezierCurveRectangles[i] );
						
						if( rect.xMax > 0 && rect.xMin < Screen.width && rect.yMax > 0 && rect.yMin < Screen.height )
						{
			
							GUI.DrawTexture( rect, maDrawingBezierCurveTextures[i] );
						}
					}
				}
				GUI.color = Color.white;
					
			}
		}
		

		// Ctrl-D			
		if( Event.current.isKey && Event.current.keyCode == KeyCode.D && Event.current.control )
		{
			CloneSelectedPanels();
		}
	
		
		if( Event.current.isKey && Event.current.keyCode == KeyCode.Delete && !mbEditingSlot && mFocusedSlot == null && miSelectedPanels > 0 )
		{
			mbConfirmDeletePopup = true;
		}
		
		// Fork cursor, or not
		switch( mMouseStatus )
		{
		case MouseStatus.DrawingLine:
		case MouseStatus.DrawScrolling:			
			Vector2 vMouse = Event.current.mousePosition;
			Rect rect = new Rect (vMouse.x - 2, vMouse.y - 3, mForkTexture.width, mForkTexture.height );
			GUI.DrawTexture( rect, mForkTexture );
			GUI.color = mDrawingStartSlot.GetPlugColor();
			GUI.DrawTexture( rect, mForkSpaghettiTexture );	
			GUI.color = Color.white;
			Cursor.visible = false;
			break;
			
		default:			
			Cursor.visible = true;	
			break;			
		}
		
		// We always want the auxiliary windows in front of the panels
		if( mActiveEnumSlot != null )
		{
			GUI.BringWindowToFront( ENUM_WINDOW_ID );
		}			
		GUI.BringWindowToFront( ACTION_WINDOW_ID );
		GUI.BringWindowToFront( EDITOR_WINDOW_ID );
		GUI.BringWindowToFront( PANELSETS_WINDOW_ID );
		if( mbFindWindowOpen )
		{
			GUI.BringWindowToFront( FIND_WINDOW_ID );
		}	
	
#if _DEMO_
		if( !mbPopup )
		{
			// Postit shadow
			GUIUtility.RotateAroundPivot( mfDemoPostitAngle, mvDemoPostitShadowPosition );
			Rect rect = new Rect( mvDemoPostitShadowPosition.x - mDemoPostitTexture.width / 2, mvDemoPostitShadowPosition.y - mDemoPostitTexture.height / 2, mDemoPostitTexture.width, mDemoPostitTexture.height );
			GUI.DrawTexture( rect, mDemoPostitShadowTexture );
			ApplyIdentityTransform();		
			
			//Postit
			GUIUtility.RotateAroundPivot( mfDemoPostitAngle, mvDemoPostitPosition );
			rect = new Rect( mvDemoPostitPosition.x - mDemoPostitTexture.width / 2, mvDemoPostitPosition.y - mDemoPostitTexture.height / 2, mDemoPostitTexture.width, mDemoPostitTexture.height );
			GUI.DrawTexture( rect, mDemoPostitTexture );
			ApplyIdentityTransform();
		}
#endif		
		
		// Mad graphs only
		if( mbGoneMad && (float)EditorApplication.timeSinceStartup - mfLastMadnessUpdateTime > MADNESS_UPDATE_INTERVAL )
		{
			MadnessUpdate();	
		}
		
		//Octarine and Thecolouroutofspace
		UpdateSpecialColors();
		
		Repaint();
		
		//Hack to be able to unfocus items with GUI.FocusControl("");
		GUI.SetNextControlName("");
	}

	//------------------------------------------------------------------------------------------------
	// DoActionsWindow
	// Handles the window containing basic actions like "Load Panelset" or "Load Diagram"
	//------------------------------------------------------------------------------------------------
	void DoActionsWindow( int id )
	{	
		if( maPanels == null || maPanels.Length == 0 )
		{
			GUI.contentColor = Color.grey;
			GUILayout.Button ( new GUIContent( mIconNew, "New graph"), mGUISkin.GetStyle("GlassButton") );
			GUI.contentColor = Color.white;	
		}
		else
		{		
			if( GUILayout.Button ( new GUIContent( mIconNew, "New graph"), mGUISkin.GetStyle("GlassButton") ) && maPanels != null )
			{
				ClearAll();
				mstrLastDiagramName = "";
				//RegisterAllPanelSetWindows();
				mbDirty = false;
			}	
		}

		if( GUILayout.Button ( new GUIContent( mIconLoad, "Load graph..."), mGUISkin.GetStyle("GlassButton") ) )
		{	
			string strPath = EditorUtility.OpenFilePanel( "Load graph...", mstrPathDiagrams, "bytes" );
			if( strPath != null && strPath != "" )
			{
				int iLastSlash = strPath.LastIndexOf("/");
				mstrPathDiagrams = strPath.Substring( 0, iLastSlash );
				mstrLastDiagramName = strPath.Substring( iLastSlash+1 );		
				
				Read( strPath );

				PlayerPrefs.SetString( "path_diagrams", mstrPathDiagrams );	
				mbDirty = false;
			}
		}

		if( GUILayout.Button ( new GUIContent( mIconLoadAdd, "Load graph and add to current..."), mGUISkin.GetStyle("GlassButton") ) )
		{				
			string strPath = EditorUtility.OpenFilePanel( "Load and add graph...", mstrPathDiagrams, "bytes" );
			if( strPath != null && strPath != "" )
			{
				ReadAndAdd( strPath, true );
			}
		}
		
		if( !mbDirty || mstrLastDiagramName == "" )
		{
			GUI.contentColor = Color.grey;
			GUILayout.Button ( new GUIContent( mIconSave, "Save graph"), mGUISkin.GetStyle("GlassButton") );
			GUI.contentColor = Color.white;	
		}
		else
		{
			if( GUILayout.Button ( new GUIContent( mIconSave, "Save graph"), mGUISkin.GetStyle("GlassButton") ) )
			{
#if _DEMO_				
				if( maPanels.Length * maPanels.Length > 300 ) // maPanels.Length > 17. (To prevent sunday crackers from finding this piece of code by searching for "17") 
				{
					DemoPopup( DEMO_PRIME_MESSAGE );
				}
				else
				{
					string strPath = mstrPathDiagrams + "/" + mstrLastDiagramName;
					if( strPath != null && strPath != "" )
					{
		
						int iLastSlash = strPath.LastIndexOf("/");
						mstrPathDiagrams = strPath.Substring( 0, iLastSlash );
						mstrLastDiagramName = strPath.Substring( iLastSlash+1 );		
						Write( strPath );	
						
						PlayerPrefs.SetString( "path_diagrams", mstrPathDiagrams );
						mbDirty = false;
					}
				}
#else
				string strPath = mstrPathDiagrams + "/" + mstrLastDiagramName;
				if( strPath != null && strPath != "" )
				{
	
					int iLastSlash = strPath.LastIndexOf("/");
					mstrPathDiagrams = strPath.Substring( 0, iLastSlash );
					mstrLastDiagramName = strPath.Substring( iLastSlash+1 );		
					Write( strPath );	
					
					PlayerPrefs.SetString( "path_diagrams", mstrPathDiagrams );	
					mbDirty = false;
				}				
#endif
				
			}
		}
		if( maPanels == null || maPanels.Length == 0 )
		{
			GUI.contentColor = Color.grey;
			GUILayout.Button ( new GUIContent( mIconSaveAs, "Save graph as..."), mGUISkin.GetStyle("GlassButton") );
			GUI.contentColor = Color.white;	
		}
		else
		{		
			if( GUILayout.Button ( new GUIContent( mIconSaveAs, "Save graph as..."), mGUISkin.GetStyle("GlassButton") ) )
			{
#if _DEMO_ 
				if( maPanels.Length * maPanels.Length > 300 ) // maPanels.Length > 17
				{
					DemoPopup( DEMO_PRIME_MESSAGE );
				}
				else
				{
					string strPath = EditorUtility.SaveFilePanel( "Save graph...", mstrPathDiagrams, mstrLastDiagramName, "bytes" );
					if( strPath != null && strPath != "" )
					{
		
						int iLastSlash = strPath.LastIndexOf("/");
						mstrPathDiagrams = strPath.Substring( 0, iLastSlash );
						mstrLastDiagramName = strPath.Substring( iLastSlash+1 );		
						Write( strPath );	
						
						PlayerPrefs.SetString( "path_diagrams", mstrPathDiagrams );	
						mbDirty = false;
					}
				}
#else
				string strPath = EditorUtility.SaveFilePanel( "Save graph...", mstrPathDiagrams, mstrLastDiagramName, "bytes" );
				if( strPath != null && strPath != "" )
				{
	
					int iLastSlash = strPath.LastIndexOf("/");
					mstrPathDiagrams = strPath.Substring( 0, iLastSlash );
					mstrLastDiagramName = strPath.Substring( iLastSlash+1 );		
					Write( strPath );	
					
					PlayerPrefs.SetString( "path_diagrams", mstrPathDiagrams );	
					mbDirty = false;
				}				
#endif				
			}
		}
	}
		
	//------------------------------------------------------------------------------------------------
	// DoEditorWindow
	// Handles the window containing editor actions like the allignment buttons
	//------------------------------------------------------------------------------------------------
	void DoEditorWindow( int id )
	{
		if( miSelectedPanels <= 1 )
		{	
			GUI.contentColor = Color.grey;
		}
		
		if( GUILayout.Button( new GUIContent( mIconAllignLeft, "Allign left" ), mGUISkin.GetStyle("GlassButton") ) && !IgnoreInput() )
		{
			AllignSelectedLeft();
		}
		if( GUILayout.Button( new GUIContent( mIconAllignRight, "Allign right" ), mGUISkin.GetStyle("GlassButton") ) && !IgnoreInput() )
		{
			AllignSelectedRight();
		}
		if( GUILayout.Button( new GUIContent( mIconAllignTop, "Allign top" ), mGUISkin.GetStyle("GlassButton") )	&& !IgnoreInput() )	
		{
			AllignSelectedTop();
		}		
		if( GUILayout.Button( new GUIContent( mIconAllignBottom, "Allign bottom" ), mGUISkin.GetStyle("GlassButton") )	&& !IgnoreInput() )	
		{
			AllignSelectedBottom();
		}	
		
		GUILayout.Space( 5 );
		
		if( mSelectedGroup == null )
		{
			// Show Group button
			if( GUILayout.Button( new GUIContent( mIconGroup, "Group" ), mGUISkin.GetStyle("GlassButton")) && miSelectedPanels > 1	&& !IgnoreInput() )	
			{
				GroupPanels();
			}
			
			// Show grey Ungroup button
			Color oldcolor = GUI.contentColor;
			GUI.contentColor =  Color.grey;
			GUILayout.Button( new GUIContent( mIconUngroup, "Ungroup"), mGUISkin.GetStyle("GlassButton") );
			GUI.contentColor = oldcolor;
		}
		else
		{
			// Show grey Group button
			Color oldcolor = GUI.contentColor;
			GUI.contentColor =  Color.grey;
			GUILayout.Button( new GUIContent( mIconGroup, "Group" ), mGUISkin.GetStyle("GlassButton") );
			GUI.contentColor = oldcolor;
			
			// Show Ungroup button
			if( GUILayout.Button( new GUIContent( mIconUngroup, "Ungroup" ), mGUISkin.GetStyle("GlassButton") ) && miSelectedPanels > 1 && !IgnoreInput() )	
			{
				UngroupPanels();
			}
		}	
		GUI.contentColor = Color.white;
		
		GUILayout.Space( 5 );
		
		if( maPanels == null || maPanels.Length == 0 )
		{
			GUI.contentColor = Color.grey;
			GUILayout.Button ( new GUIContent( mIconFind, "Find/Replace" ), mGUISkin.GetStyle("GlassButton") );
			GUILayout.Button ( new GUIContent( mIconRefresh, "Update graph when panelsets have changed ." ), mGUISkin.GetStyle("GlassButton") );
			
			GUI.contentColor = Color.white;	
		}
		else
		{			
			if( GUILayout.Button( new GUIContent( mIconFind, "Find/Replace" ), mGUISkin.GetStyle("GlassButton") ) && !mbFindWindowOpen	&& !IgnoreInput() )	
			{
				if( mWindowRectFind.x == -1 )
				{
					mWindowRectFind.x = Screen.width - mWindowRectFind.width;
					mWindowRectFind.y = Screen.height - mWindowRectFind.height;
				}
				mbFindWindowOpen = true;
			}
			if( GUILayout.Button( new GUIContent( mIconRefresh, "Update graph when \npanelsets have changed ." ), mGUISkin.GetStyle("GlassButton") )	&& !IgnoreInput() )	
			{
				Debug.Log(" before RefreshGraph : maLinks.Length = "+maLinks.Length );
				RefreshGraph();
				Debug.Log(" after RefreshGraph : maLinks.Length = "+maLinks.Length );
			}			
		}
	}
	
	//------------------------------------------------------------------------------------------------
	// DoFindWindow
	// Handles the window to find/replace
	//------------------------------------------------------------------------------------------------
	void DoFindWindow( int id )
	{
		if ( GUI.Button( new Rect(mWindowRectFind.width - 17,4,13,13),"", new GUIStyle("xButton")) && !IgnoreInput() )
		{
			mbFindWindowOpen = false;
		}
		GUILayout.Label( "Find:");
		mstrFind = GUILayout.TextField( mstrFind );
		
		mbFindSelectionOnly = GUILayout.Toggle( mbFindSelectionOnly, "Selection only", mGUISkin.GetStyle("GlassToggle") );
		mbFindAndReplace = GUILayout.Toggle( mbFindAndReplace, mbFindAndReplace ? "Replace:" : "Replace", mGUISkin.GetStyle("GlassToggle") );		
		
		if( mbFindAndReplace )
		{
			mstrReplace = GUILayout.TextField( mstrReplace );
			
			GUILayout.BeginHorizontal();
			if( mstrFind == "" || ( mbFindSelectionOnly && miSelectedPanels == 0 ) )
			{
				GUI.contentColor =  Color.grey;
				GUILayout.Button("Replace", mGUISkin.GetStyle("GlassButton"));
				GUI.contentColor =  Color.white;
			}
			else
			{
				if( GUILayout.Button("Replace", mGUISkin.GetStyle("GlassButton")) && !IgnoreInput() )
				{
					FindAndReplace( mstrFind, mstrReplace, mbFindSelectionOnly );
					mbFindWindowOpen = false;
				}
			}
			if( GUILayout.Button("Cancel", mGUISkin.GetStyle("GlassButton")) && !IgnoreInput() )
			{
				mbFindWindowOpen = false;
			}			
			GUILayout.EndHorizontal();			
		}
		else
		{	
			GUILayout.Space ( 26 );
			GUILayout.BeginHorizontal();
			if( mstrFind == "" || ( mbFindSelectionOnly && miSelectedPanels == 0 ) )
			{
				GUI.contentColor =  Color.grey;
				GUILayout.Button("Find", mGUISkin.GetStyle("GlassButton"));
				GUI.contentColor =  Color.white;
			}
			else
			{
				if( GUILayout.Button("Find", mGUISkin.GetStyle("GlassButton")) && !IgnoreInput() )
				{
					FindAndReplace( mstrFind, null, mbFindSelectionOnly );
					mbFindWindowOpen = false;
				}
			}
			if( GUILayout.Button("Cancel", mGUISkin.GetStyle("GlassButton"))&& !IgnoreInput() )
			{
				mbFindWindowOpen = false;
			}			
			GUILayout.EndHorizontal();			
		}
	}
	
	//------------------------------------------------------------------------------------------------
	// DoEnumWindow
	// Handles the window corresponding to a panelset 
	//------------------------------------------------------------------------------------------------			
	void  DoEnumWindow( int ID )
	{	
		for( int i = 0; i < mActiveEnumSlot.maEnumValues.Length; i++ )
		{
			if( i != mActiveEnumSlot.miDataInt )
			{
			 	GUI.contentColor = 0.8f * Color.white;	
			}
				
			bool bClick = GUILayout.Button( mActiveEnumSlot.maEnumValues[i], mGUISkin.GetStyle("EnumButton") ) && !IgnoreInput() ;
			GUI.contentColor = Color.white;
			
			if( bClick )	
			{
				mActiveEnumSlot.miDataInt = i;
				mActiveEnumSlot = null;
				mEnumRect.x = -1000;
				mbDirty = true;
				break;
			}
		}

	}

	//------------------------------------------------------------------------------------------------
	// DoPanelSetWindow
	// Handles the window corresponding to a panelset 
	//------------------------------------------------------------------------------------------------			
	void  DoPanelSetsWindow( int ID )
	{
		if( GUILayout.Button ( "Load Panelset", mGUISkin.GetStyle("GlassButton") ) && !IgnoreInput() )
		{
			LoadNewPanelSet();
		}

		if( GUILayout.Button ( "Reload all Panelsets", mGUISkin.GetStyle("GlassButton") ) && !IgnoreInput() )
		{
			for( int i = 0; i < maPanelSets.Length; i++ )
			{
				ReloadPanelset( maPanelSets[i] as PanelSet );
			}
		}
		
		mvPanelSetsScrollPosition = GUILayout.BeginScrollView( mvPanelSetsScrollPosition, false, true );
		
		if( maPanelSets.Length == 0 )
		{
			GUILayout.Label( "No panelsets loaded");
		}

		
		for( int i = 0; i < maPanelSets.Length; i++ )
		{
			PanelSet panelset = maPanelSets[i] as PanelSet;
			GUILayout.BeginHorizontal();
			EditorStyles.foldout.fontStyle = FontStyle.Bold; 
			EditorStyles.foldout.normal.textColor = Color.white;
			EditorStyles.foldout.onNormal.textColor = Color.white; 
			panelset.mbUnfolded = EditorGUILayout.Foldout( panelset.mbUnfolded, panelset.mstrName );
			EditorStyles.foldout.fontStyle = FontStyle.Normal;
			EditorStyles.foldout.normal.textColor = Color.gray;
			EditorStyles.foldout.onNormal.textColor = Color.white;				
			GUILayout.FlexibleSpace();
			bool bDelete = GUILayout.Button( "", new GUIStyle("xButton") ) && !IgnoreInput() ;
			GUILayout.EndHorizontal();
			if( bDelete )
			{
				RemovePanelset( panelset );			
			}			
			else if( panelset.mbUnfolded && panelset.maPanelPrototypes != null )
			{
				foreach( EditorPanel panelprototype in panelset.maPanelPrototypes )
				{
					if( panelprototype != null && GUILayout.Button ( panelprototype.mstrType, mGUISkin.GetStyle("GlassButton") ) )
					{
						EditorPanel newPanel = new EditorPanel(); //ScriptableObject.CreateInstance( typeof( EditorPanel ) ) as EditorPanel;	
						newPanel.CloneFrom( panelprototype );
						newPanel.mWindowRect.x = Screen.width - 200 - panelprototype.mWindowRect.width;
						newPanel.mWindowRect.y = 0;	
						newPanel.mbBringToFrontOnceCreated	= true;	
						mbDirty = true;
						Event.current.Use();
					}
				}				
				
			}
		}
		GUILayout.EndScrollView();
		


	
	}
	
	//------------------------------------------------------------------------------------------------
	// LoadNewPanelSet
	// Loads a new panelset 
	//------------------------------------------------------------------------------------------------			
	void LoadNewPanelSet()
	{			

		mstrPathPanelsets = PlayerPrefs.GetString( "path_panelsets", Application.dataPath );
		string strPathAndFile = EditorUtility.OpenFilePanel( "Load panelset...", mstrPathPanelsets, "xml" );

		if( strPathAndFile != null && strPathAndFile != "" && strPathAndFile.Contains("/") )
		{
			int iLastSlash = strPathAndFile.LastIndexOf("/"); 

			mstrPathPanelsets = strPathAndFile.Substring( 0, iLastSlash );
			PlayerPrefs.SetString( "path_panelsets", mstrPathPanelsets );
			
			PanelSet panelset = new PanelSet(); //ScriptableObject.CreateInstance( typeof( PanelSet ) ) as  PanelSet;				
			panelset.ReadFromFile( strPathAndFile );	
			if( maPanelSets == null )
			{
				maPanelSets = new PanelSet[0];
			}
			
			// Is there a panelset with the same name ?	
			int iIndexOfOldVersion = -1;
			for( int i = 0; i < maPanelSets.Length; i++ )
			{
				PanelSet otherset =  maPanelSets[i];
				if( otherset.mstrPath == strPathAndFile )
				{
					iIndexOfOldVersion = i;
					break;
				}
			}

			if( iIndexOfOldVersion != -1 )
			{
				PanelSet oldversion = maPanelSets[iIndexOfOldVersion];
				panelset.mWindowRect = oldversion.mWindowRect;
				//DestroyImmediate( oldversion );
				maPanelSets[iIndexOfOldVersion]  = panelset;
			}
			else
			{
				if( maPanelSets.Length > 0 )
				{
					PanelSet predecessor = maPanelSets[ maPanelSets.Length-1];
					panelset.mWindowRect = predecessor.mWindowRect;
					panelset.mWindowRect.y += predecessor.mWindowRect.height + 5;
				}
				else
				{
					panelset.mWindowRect.y = mWindowRectActions.y + mWindowRectActions.height + 5; 
				}
				AddPanelSet( panelset );
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
		EditorPanel panel = maPanels[ id - START_PANELS_IDS ] as EditorPanel;	
		
		if( panel.mImage != null )
		{
			Rect rectangle = new Rect( EditorPanel.WINDOW_MARGE_LEFT, EditorPanel.WINDOW_MARGE_TOP, panel.mImage.width, panel.mImage.height );
			GUI.DrawTexture( rectangle, panel.mImage );
		}
		
		if ( GUI.Button( new Rect(panel.mWindowRect.width - 16,1,13,13),"", new GUIStyle("xButton")) && !IgnoreInput() )
		{
			RemovePanel( panel );
			mbDirty = true;
			return;
		}
		
		HandleMouseAction( MouseHandlingCall.StartDrawingPanel, panel );
		string strOld;
		int iOld;
		float fOld;
		
		//*/
		bool bIgnoreFollowingSlots = false;
		foreach( EditorSlot slot in panel.maSlots )
		{		
			if( !bIgnoreFollowingSlots )
			{	
				switch( slot.mContentType )
				{
				case EditorContentType.None :
					if( !slot.mbCustomPosition )
					{

						
						switch( slot.mPlugType )
						{	
						case EditorPlugType.None :
							GUILayout.Label( slot.mstrLabel );
							break;
							
						case EditorPlugType.Input :
							GUILayout.Label( " "+slot.mstrLabel );
							break;
							
						case EditorPlugType.Output :
							GUILayout.BeginHorizontal();
							GUILayout.FlexibleSpace();
							GUILayout.Label( slot.mstrLabel );
							GUILayout.Space( 5 );
							GUILayout.EndHorizontal();	
							break;
							
						case EditorPlugType.InOut :
							GUILayout.Label( "     ");	
							break;
						}
					}
					break;
		
				case EditorContentType.String :
					GUILayout.BeginHorizontal();			
					if( slot.mstrDataText == null ) slot.mstrDataText = "";
					if( panel == mPrimarySelectedPanel && !IgnoreInput() )
					{
						strOld = slot.mstrDataText;
						GUILayout.Label( " "+slot.mstrLabel + ":" );
						GUILayout.Space( 1 );
						GUILayout.BeginHorizontal();
						GUI.SetNextControlName("Control_"+slot.miUniqueID);
						slot.mstrDataText = EditorGUILayout.TextField (slot.mstrDataText, mGUISkin.GetStyle("PanelTextField") );
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();						
						if( strOld != slot.mstrDataText )
						{
							mbDirty = true;	
						}
					}
					else
					{
						// Dirty hack to work around a bug where you click a text field in panel A and 
						// Unity thinks a text field in panel B is selected, resulting in a "remote controlled" field in B.
						GUILayout.Label(  " "+slot.mstrLabel + ":" );
						GUILayout.Space( 16 );
						GUILayout.BeginHorizontal( mGUISkin.GetStyle("PanelFalseTextField") );
						GUILayout.Label( ""+slot.mstrDataText );
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();	
					}					
					GUILayout.EndHorizontal();	
					UpdateFocusedSlot( slot );
					break;

				case EditorContentType.Vector3 :
					GUILayout.BeginHorizontal();
					GUILayout.Label( slot.mstrLabel );
					Vector3 vOld = slot.mvDataVector;
					slot.mvDataVector.x = EditorGUILayout.FloatField( slot.mvDataVector.x, mGUISkin.GetStyle("PanelTextField"), GUILayout.MinWidth( 30 ) );
					slot.mvDataVector.y = EditorGUILayout.FloatField( slot.mvDataVector.y, mGUISkin.GetStyle("PanelTextField"), GUILayout.MinWidth( 30 ) );
					slot.mvDataVector.z = EditorGUILayout.FloatField( slot.mvDataVector.z, mGUISkin.GetStyle("PanelTextField"), GUILayout.MinWidth( 30 ) );
					//slot.mvDataVector = EditorGUILayout.Vector3Field( slot.mstrLabel, slot.mvDataVector, mGUISkin.GetStyle("PanelTextField" ) );	
					if( vOld != slot.mvDataVector )
					{
						mbDirty = true;	
					}
					GUILayout.EndHorizontal();
					break;
					
				case EditorContentType.Curve :
					GUILayout.BeginHorizontal();	
					GUILayout.Label( slot.mstrLabel );
					AnimationCurve curveOld = slot.mDataCurve;	
					
					//DELETEME ?
					if( slot.mDataCurve == null )
					{
						slot.mDataCurve = AnimationCurve.Linear( 0.0f, 0.0f, 1.0f, 1.0f );
					}
					
					slot.mDataCurve = EditorGUILayout.CurveField( slot.mDataCurve, GUILayout.Height(11), GUILayout.MinWidth(10)  );
					if( curveOld != slot.mDataCurve )
					{
						mbDirty = true;	
					}
					
					GUILayout.Space( 5 );
					GUILayout.EndHorizontal();
					break;	
					
				case EditorContentType.Color :
					GUILayout.BeginHorizontal();	
					GUILayout.Label( slot.mstrLabel );
					Color colorOld = slot.mDataColor;	
					
					slot.mDataColor = EditorGUILayout.ColorField(  slot.mDataColor  );;
					
					if( colorOld != slot.mDataColor )
					{
						mbDirty = true;	
					}
					GUILayout.EndHorizontal();
					break;						
					
				case EditorContentType.Text :
					if( slot.mstrDataText == null ) slot.mstrDataText = "";
					strOld = slot.mstrDataText;					
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

					GUILayout.BeginHorizontal( GUILayout.Width( panel.mWindowRect.width - EditorPanel.WINDOW_MARGE_LEFT - EditorPanel.WINDOW_MARGE_RIGHT - 30 )  );	
					if(  GUILayout.Button( mIconNoteblock, GUILayout.Width(20) ) && !IgnoreInput() )
					{
						if( slot == mEditingSlot )
						{
							slot.mbEditing = false;
							mEditingSlot = null;
						}
						else
						{
							slot.mbEditing = true;
							if( mEditingSlot != null )
							{
								mEditingSlot.mbEditing = false;	
							}
							mEditingSlot = slot;
							
							slot.mWindowRect.x = panel.mWindowRect.x + panel.mWindowRect.width - 5;
							slot.mWindowRect.y = panel.mWindowRect.y + EditorPanel.HEADER_HEIGHT + EditorPanel.SLOT_HEIGHT * slot.miIndex;
							slot.mWindowRect.width = 400;
							slot.mWindowRect.height = 200;	
							slot.mbBringWindowToFrontOnceCreated = true;
							mvEditWindowScrollPosition = Vector2.zero;
							GUIUtility.keyboardControl = 0;
							GUIUtility.hotControl = 0;
						}
						mbDirty = true;
					}		

					if( slot == mEditingSlot ) GUI.contentColor = Color.yellow;
					GUILayout.Label(slot.mstrLabel + ": " + strFirstLine);
					GUI.contentColor = Color.white;					
					if( strOld != slot.mstrDataText )
					{
						mbDirty = true;	
					}					
					GUILayout.EndHorizontal();						
					break;

				case EditorContentType.Float :
					GUILayout.BeginHorizontal();			
					if( slot.mstrDataText == null ) slot.mstrDataText = "";
					if( panel == mPrimarySelectedPanel && !IgnoreInput() )
					{
						fOld = slot.mfDataFloat;
						GUILayout.Label( " "+slot.mstrLabel + ":" );
						GUILayout.Space( 1 );
						GUILayout.BeginHorizontal();
						GUI.SetNextControlName("Control_"+slot.miUniqueID);
						slot.mfDataFloat = EditorGUILayout.FloatField(slot.mfDataFloat, mGUISkin.GetStyle("PanelTextField") );
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();						
						if( fOld != slot.mfDataFloat )
						{
							mbDirty = true;	
						}
					}
					else
					{
						// Dirty hack to work around a bug where you click a text field in panel A and 
						// Unity thinks a text field in panel B is selected, resulting in a "remote controlled" field in B.
						GUILayout.Label( " "+slot.mstrLabel + ":" );
						GUILayout.Space( 16 );
						GUILayout.BeginHorizontal( mGUISkin.GetStyle("PanelFalseTextField") );
						GUILayout.Label( ""+slot.mfDataFloat );
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();	
					}					
					GUILayout.EndHorizontal();	
					UpdateFocusedSlot( slot );
					break;					
					
				case EditorContentType.Int :
					GUILayout.BeginHorizontal();			
					if( slot.mstrDataText == null ) slot.mstrDataText = "";
					if( panel == mPrimarySelectedPanel && !IgnoreInput() )
					{
						iOld = slot.miDataInt;
						GUILayout.Label( " "+slot.mstrLabel + ":" );
						GUILayout.Space( 1 );
						GUILayout.BeginHorizontal();
						GUI.SetNextControlName("Control_"+slot.miUniqueID);
						slot.miDataInt = EditorGUILayout.IntField(slot.miDataInt, mGUISkin.GetStyle("PanelTextField") );
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();						
						if( iOld != slot.miDataInt )
						{
							mbDirty = true;	
						}
					}
					else
					{
						// Dirty hack to work around a bug where you click a text field in panel A and 
						// Unity thinks a text field in panel B is selected, resulting in a "remote controlled" field in B.
						GUILayout.Label( " "+slot.mstrLabel + ":" );
						GUILayout.Space( 16 );
						GUILayout.BeginHorizontal( mGUISkin.GetStyle("PanelFalseTextField") );
						GUILayout.Label( ""+slot.miDataInt );
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();	
					}					
					GUILayout.EndHorizontal();	
					UpdateFocusedSlot( slot );
					break;						
					
				case EditorContentType.Bool :
					GUILayout.BeginHorizontal();	
					GUILayout.Label( " "+slot.mstrLabel);
					GUILayout.Space( 5 );
					iOld = slot.miDataInt;
					if(  !IgnoreInput() )
					{
						slot.miDataInt = EditorGUILayout.Toggle( slot.miDataInt == 1 , GUILayout.ExpandWidth(false) ) ? 1 : 0;	
					}
					else
					{
						EditorGUILayout.Toggle( slot.miDataInt == 1 , GUILayout.ExpandWidth(false) );
					}
					if( iOld != slot.miDataInt )
					{
						mbDirty = true;	
					}
					GUILayout.EndHorizontal();
					break;
					
				case EditorContentType.Enum :
					GUILayout.BeginHorizontal();	
					GUILayout.Label( " "+slot.mstrLabel+ ":");
					GUILayout.Space( 5 );

					if( mActiveEnumSlot != slot )
					{
						if( GUILayout.Button (slot.maEnumValues[slot.miDataInt], mGUISkin.GetStyle("EnumButton") )  && !IgnoreInput())
						{	
							mActiveEnumSlot = slot;
						}
					}
					else
					{
						GUILayout.Label( "." );
						if(Event.current.type == EventType.Repaint )
						{
							mEnumRect = GUILayoutUtility.GetLastRect();
							mEnumRect.x += panel.mWindowRect.x;
							mEnumRect.y += panel.mWindowRect.y;								
						}
					}
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					break;
					
				case EditorContentType.Master :
					GUILayout.BeginHorizontal();			
					GUILayout.Label( mIconMaster );	
					GUILayout.Space( 5 );										
					if( slot.mstrDataText == null ) slot.mstrDataText = "";

					if( panel == mPrimarySelectedPanel  && !IgnoreInput() )
					{
						strOld = slot.mstrDataText;
						GUI.SetNextControlName("Control_"+slot.miUniqueID);
						if( GameObject.Find( slot.mstrDataText ) == null )
						{
							Color oldcolor = GUI.contentColor;
							GUI.contentColor = 0.2f * Color.white + Color.red;
							slot.mstrDataText = EditorGUILayout.TextField (slot.mstrDataText, mGUISkin.GetStyle("PanelTextField") );	
							GUI.contentColor = oldcolor;
						}
						else
						{
							slot.mstrDataText = EditorGUILayout.TextField (slot.mstrDataText, mGUISkin.GetStyle("PanelTextField") );	
						}	
						if( strOld != slot.mstrDataText )
						{
							mbDirty = true;	
						}	
					}
					else
					{
						// Dirty hack (see EditorContentType.String)
						if( GameObject.Find( slot.mstrDataText ) == null )
						{
							Color oldcolor = GUI.contentColor;
							GUI.contentColor = 0.2f * Color.white + Color.red;
							GUILayout.BeginHorizontal( mGUISkin.GetStyle("PanelFalseTextField") );
							GUILayout.Space(6);
							GUILayout.Label( slot.mstrDataText );
							GUILayout.FlexibleSpace();
							GUILayout.EndHorizontal();	
							GUI.contentColor = oldcolor;
						}
						else
						{
							GUILayout.BeginHorizontal( mGUISkin.GetStyle("PanelFalseTextField") );
							GUILayout.Space(6);
							GUILayout.Label( slot.mstrDataText );
							GUILayout.FlexibleSpace();
							GUILayout.EndHorizontal();	
						}						
					}						
					GUILayout.EndHorizontal();	
					UpdateFocusedSlot( slot );
					break;					

				case EditorContentType.GameObject :
					GUILayout.BeginHorizontal();			
					GUILayout.Label( mIconGameObject );		
					GUILayout.Space( 5 );					
					if( slot.mstrDataText == null ) slot.mstrDataText = "";

					if( panel == mPrimarySelectedPanel  && !IgnoreInput() )
					{
						strOld = slot.mstrDataText;
						GUI.SetNextControlName("Control_"+slot.miUniqueID);
						if( GameObject.Find( slot.mstrDataText ) == null )
						{
							Color oldcolor = GUI.contentColor;
							GUI.contentColor = 0.2f * Color.white + Color.red;
							slot.mstrDataText = EditorGUILayout.TextField (slot.mstrDataText, mGUISkin.GetStyle("PanelTextField") );	
							GUI.contentColor = oldcolor;
						}
						else
						{
							slot.mstrDataText = EditorGUILayout.TextField (slot.mstrDataText, mGUISkin.GetStyle("PanelTextField") );	
						}	
						if( strOld != slot.mstrDataText )
						{
							mbDirty = true;	
						}		
					}
					else
					{
						// Dirty hack (see EditorContentType.String)
						if( GameObject.Find( slot.mstrDataText ) == null )
						{
							Color oldcolor = GUI.contentColor;
							GUI.contentColor = 0.2f * Color.white + Color.red;
							GUILayout.BeginHorizontal( mGUISkin.GetStyle("PanelFalseTextField") );
							GUILayout.Space(6);
							GUILayout.Label( slot.mstrDataText );
							GUILayout.FlexibleSpace();
							GUILayout.EndHorizontal();	
							GUI.contentColor = oldcolor;
						}
						else
						{
							GUILayout.BeginHorizontal( mGUISkin.GetStyle("PanelFalseTextField") );
							GUILayout.Space(6);
							GUILayout.Label( slot.mstrDataText );
							GUILayout.FlexibleSpace();
							GUILayout.EndHorizontal();	
						}						
					}						
					GUILayout.EndHorizontal();
					UpdateFocusedSlot( slot );
					break;					
					
				case EditorContentType.More :	
					if(  !IgnoreInput() )
					{
						slot.mbMore = EditorGUILayout.Foldout( slot.mbMore, slot.mbMore ? "" : "more..." );
					}
					else
					{
						EditorGUILayout.Foldout( slot.mbMore, slot.mbMore ? "" : "more..." );
					}	
					
					if( !slot.mbMore )
					{
						panel.mWindowRect.height = EditorPanel.WINDOW_MARGE_TOP + EditorPanel.SLOT_HEIGHT * (slot.miIndex+1) + EditorPanel.WINDOW_MARGE_BOTTOM;
						bIgnoreFollowingSlots = true;						
					}
					else 
					{
						panel.mWindowRect.height = EditorPanel.WINDOW_MARGE_TOP + EditorPanel.SLOT_HEIGHT * slot.mParent.maSlots.Length + EditorPanel.WINDOW_MARGE_BOTTOM;
					}
					break;				
				}
			}
		}
		//*/
		
		//*/		
		foreach( EditorSlot slot in panel.maSlots )
		{
			// Draw the plug (if any)
			if( slot.mPlugType != EditorPlugType.None )
			{
				Vector2 vPos = slot.GetLocalSlotPosition();
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
		//*/		

		HandleMouseAction( MouseHandlingCall.EndDrawingPanel, panel );		
				
	}

	//------------------------------------------------------------------------------------------------
	// IgnoreInput()
	// True iff any popup open
	//------------------------------------------------------------------------------------------------
	bool IgnoreInput()
	{	
		return ( mbPopup || mbConfirmDeletePopup );
	}
	
	//------------------------------------------------------------------------------------------------
	// UpdateFocusedSlot
	// Updates mFocusedSlot if slot has been focussed or unfocussed
	//------------------------------------------------------------------------------------------------
	void UpdateFocusedSlot( EditorSlot slot )
	{
		if( mFocusedSlot != slot && GUI.GetNameOfFocusedControl() ==  "Control_"+slot.miUniqueID )
		{
			mFocusedSlot = slot;
		}
		else if( mFocusedSlot == slot )
		{
			mFocusedSlot = null;
		}
	}
	
	/*
	// Dirty fix
	void ForceFocusTo( EditorSlot slot )
	{
		if( mFocusedSlot != slot )
		{
			Debug.Log("Forced focus to slot "+slot.miUniqueID );
			GUI.FocusControl( "Control_"+slot.miUniqueID );
			mFocusedSlot = slot;
		}
	}
	*/
	
	
	//------------------------------------------------------------------------------------------------
	// RefreshGraph
	// Refreshs the graph when panel sets have changed
	//------------------------------------------------------------------------------------------------	
	void RefreshGraph()
	{
		// First update all panel sets. (Omit this?)
		for( int i = 0; i < maPanelSets.Length; i++ )
		{
			ReloadPanelset( maPanelSets[i] as PanelSet );
		}
		
		foreach( EditorPanel panel in maPanels )
		{
			// We are looking for the panel set belonging to this panel
			EditorPanel panelPrototypeToUpdateFrom = null;
			bool bPanelsAreIdentical = false;

			if( panel.GetPanelSetName() != "" )
			{
				// The panel set name has been stored in the panel. 
				// In this case look in the loaded panel sets.				
				foreach( PanelSet panelset in maPanelSets )
				{
					if( panelset.mstrName == panel.GetPanelSetName() )
					{
						// Search prototype in panel set
						foreach( EditorPanel panelprototype in panelset.maPanelPrototypes )
						{
							if( panel.HasIdenticalStructure( panelprototype ) )
							{
								panelPrototypeToUpdateFrom = panelprototype;
								bPanelsAreIdentical = ( panel.mstrType == panelprototype.mstrType );
								break;								
							}
							else if( panel.mstrType == panelprototype.mstrType  ||  panel.mstrType == panelprototype.mstrTypeOld )
							{
								panelPrototypeToUpdateFrom = panelprototype;
								break;
							}
						}	
						break;
					}
				}
			}
			else
			{				
				// The panel set name has NOT been stored in the panel. 
				// (This is the case for graphs made with older versions) 
				
				// First attempt : Look if you find an IDENTICAL panel type in the loaded panel sets.
				// (This is done in order to avoid unnecessary changes. 
				//  Exemple: There are two panel sets having a "Start" panel, but with different plug colors.
				//  These panels haven't changed, the graph is updated for other reasons.
				//  We don't want start panels in the graph be modified just because this
				//  method attributed the wrong panel set to the panels.)
				foreach( PanelSet panelset in maPanelSets )
				{
					foreach( EditorPanel panelprototype in panelset.maPanelPrototypes )
					{
						if( panel.HasIdenticalStructure( panelprototype ) )
						{
							panelPrototypeToUpdateFrom = panelprototype;
							bPanelsAreIdentical =  ( panel.mstrType == panelprototype.mstrType );
							break;	
						}
					}
				}	
				
				if( panelPrototypeToUpdateFrom == null )
				{
					// Second attempt : Look if you find a panel type WITH THE RIGHT NAME (mstrTypeOld) in the loaded panel sets,					
					foreach( PanelSet panelset in maPanelSets )
					{
						foreach( EditorPanel panelprototype in panelset.maPanelPrototypes )
						{
							if( panel.mstrType == panelprototype.mstrType  ||  panel.mstrType == panelprototype.mstrTypeOld )
							{
								panelPrototypeToUpdateFrom = panelprototype;
								break;
							}
						}
					}						
				}
				
			}
			
			if( panelPrototypeToUpdateFrom != null && !bPanelsAreIdentical )
			{
				// We have found the corresponding prototype, and it doesn't have identical structure with the panel, so wee need to update the panel.
				panel.UpdateFrom( panelPrototypeToUpdateFrom );
			}
			

			// Break Incompatible links
			ArrayList aLinksToDestroy = new ArrayList();
			foreach( EditorLink link in maLinks )
			{
				if( !link.mStartSlot.CanRemainConnectedWith( link.mEndSlot ) )
				{
					Debug.Log("Breaking link !");
					aLinksToDestroy.Add( link );
				}
			}
			
			foreach( EditorLink link in aLinksToDestroy )
			{
				RemoveLink( link );	
			}
		}
	}
	
	//------------------------------------------------------------------------------------------------
	// DebugTestLinks
	// For debug purposes
	//------------------------------------------------------------------------------------------------		
	void DebugTestLinks( string strMessage )
	{	
		Debug.Log( "Checking links "+strMessage+": " );
		foreach( EditorPanel panel in maPanels )
		{		
			for( int i = 0; i < panel.maSlots.Length; i++ )
			{		
				foreach( EditorLink inlink in panel.maSlots[i].maIncomingLinks )
				{	
					if( inlink != null )
					{
						Debug.Log( panel.maSlots[i].miUniqueID+": Incoming link ["+inlink.mStartSlot.miUniqueID+"-"+inlink.mEndSlot.miUniqueID+"]" );
					}
					else
					{
						Debug.LogWarning( panel.maSlots[i].miUniqueID+": Incoming link is null!" );
					}
				}
				foreach( EditorLink outlink in panel.maSlots[i].maOutgoingLinks )
				{	
					if( outlink != null )
					{
						Debug.Log( panel.maSlots[i].miUniqueID+": Outgoing link ["+outlink.mStartSlot.miUniqueID+"-"+outlink.mEndSlot.miUniqueID+"]" );
					}
					else
					{
						Debug.LogWarning( panel.maSlots[i].miUniqueID+": Outgoing link is null!" );
					}
				}				
			}
		}		
	}
	
	//------------------------------------------------------------------------------------------------
	// DoSlotEditWindow
	// Handles the window where we enter some lengthy text for a "text" slot
	//------------------------------------------------------------------------------------------------		
	void DoSlotEditWindow( int id )
	{		
		if ( GUI.Button( new Rect(mEditingSlot.mWindowRect.width - 16,1,13,13),"", new GUIStyle("xButton")))
		{
			mEditingSlot.mbEditing = false;
			mEditingSlot = null;
			return;
		}
		
		switch( mEditingSlot.mContentType )
		{
		case EditorContentType.Text :
			string strOld = mEditingSlot.mstrDataText;
			EditorStyles.textField.wordWrap = true;
			mvEditWindowScrollPosition = GUILayout.BeginScrollView( mvEditWindowScrollPosition );
			mEditingSlot.mstrDataText = EditorGUILayout.TextArea( mEditingSlot.mstrDataText, GUILayout.MinHeight(500) );
			GUILayout.EndScrollView();
			if( strOld != mEditingSlot.mstrDataText )
			{
				mbDirty = true;	
			}			
			break;
		}

		
		if(  GUILayout.Button( "Ok" ) )
		{
			mEditingSlot.mbEditing = false;
			mEditingSlot = null;
		}

		GUI.DragWindow ();			
	}
	
	//------------------------------------------------------------------------------------------------
	// DoConfirmDeleteWindow
	// Just a popup window to confirm deleting selected panels
	//------------------------------------------------------------------------------------------------		
	void DoConfirmDeleteWindow( int id )
	{
		// Blinking text
		GUI.contentColor = ( (Time.realtimeSinceStartup % 1.0f )< 0.5f ) ? Color.red : Color.white;
		
		if( miSelectedPanels > 1 )
		{
			GUILayout.Label( "Delete selected panels?", mGUISkin.GetStyle("labelCentered") );
		}
		else
		{
			GUILayout.Label( "Delete selected panel?", mGUISkin.GetStyle("labelCentered") );
		}
		GUI.contentColor = Color.white;
		
		GUILayout.BeginHorizontal();
		
		if( GUILayout.Button("No", mGUISkin.GetStyle("GlassButton")) )
		{
			mbConfirmDeletePopup = false;
		}	
		GUI.contentColor = Color.red;
		if( GUILayout.Button("Yes", mGUISkin.GetStyle("GlassButton")) )
		{
			DeleteSelectedPanels();
			mbConfirmDeletePopup = false;
		}		
		GUI.contentColor = Color.white;
		GUILayout.EndHorizontal();		
	}
	
	//------------------------------------------------------------------------------------------------
	// DoPopupWindow
	// Just a popup window with some text
	//------------------------------------------------------------------------------------------------		
	void DoPopupWindow( int id )
	{
		GUILayout.Label( mDemoPopupTexture );
		GUILayout.Label( mstrPopupMessage, mGUISkin.GetStyle("labelCentered") );
		if( GUILayout.Button("Ok", mGUISkin.GetStyle("GlassButton")) )
		{
			mbPopup = false;
		}	
	}
	
	//------------------------------------------------------------------------------------------------
	// DemoPopup
	// Just a popup window with some text
	//------------------------------------------------------------------------------------------------		
	void DemoPopup( string strMessage )
	{
		if( mDemoPopupTexture == null )
		{
			mDemoPopupTexture = Resources.Load("DemoPopup") as Texture;	
		}
		
		if( mbPopup )
		{
			Debug.LogError("Trying to show two popup windows simultaneously");
			return;
		}	
		mstrPopupMessage = strMessage;
		mbPopup = true;
	}
	
	//================================================================================================
	//
	//   MOUSE METHODS
	//
	//================================================================================================

	//------------------------------------------------------------------------------------------------
	// SetMouseStatus
	//------------------------------------------------------------------------------------------------		
	void SetMouseStatus( MouseStatus newStatus )
	{
		mMouseStatus = newStatus;
	}

	//------------------------------------------------------------------------------------------------
	// HandleMouseAction
	// Handles clicks, right clicks, drags etc.
	//------------------------------------------------------------------------------------------------		
	void HandleMouseAction( MouseHandlingCall call, EditorPanel panel  )
	{
		if( IgnoreInput() )
		{
			return;	
		}
		
		switch( Event.current.type )
		{
		case EventType.MouseDown :
			switch( Event.current.button )
			{
			case 0 :			
				HandleMouseClick( call, panel );
				break;
				
			case 1 :			
				HandleMouseRightClick( call, panel );
				break;
					
			default:
				//NOP
				break;				
			}
			break;
			
		case EventType.MouseUp :
			switch( Event.current.button )
			{
			case 0 :
				HandleMouseUp();
				break;
				
			case 1 :
				HandleRightMouseUp();
				break;		
			}
			break;
				
		case EventType.MouseDrag :
			switch( Event.current.button )
			{
			case 0 :
				if( mMouseStatus == MouseStatus.MovingPanels )
				{
					MoveSelectedWindows( Event.current.delta, panel );
					Event.current.Use();
				}
				break;

			case 1 :
				if( mMouseStatus == MouseStatus.Scrolling || mMouseStatus == MouseStatus.DrawScrolling )
				{
					if( mbShiftKeyDown )
					{
						if( mbAltKeyDown )
						{
							MoveAllUnselectedWindows( Event.current.delta / MINIMAP_SCALE );
						}
						else
						{
							MoveAllUnselectedWindows( Event.current.delta );
						}	
					}
					else
					{
						if( mbAltKeyDown )
						{
							MoveAllWindows( Event.current.delta / MINIMAP_SCALE );
						}
						else
						{
							MoveAllWindows( Event.current.delta );
						}	
					}						
					Event.current.Use();
				}
				break;
					
			default:
				//NOP
				break;								
			}
			break;
			
		default:
			//NOP
			break;
		}		
	}
	
	//------------------------------------------------------------------------------------------------
	// HandleMouseClick
	// Omigod, the user just clicked somewhere, what shall we do ?
	//------------------------------------------------------------------------------------------------		
	void HandleMouseClick( MouseHandlingCall call, EditorPanel clickedpanel )
	{
		Vector2 vOffset = (clickedpanel != null ) ? clickedpanel.GetUpperLeftCorner() : Vector2.zero;
		EditorSlot slot;

		switch( mMouseStatus )
		{
		case MouseStatus.Default:
			
			switch( call )
			{
			case MouseHandlingCall.BeforeDrawingAllPanels :
				// Check if slot clicked
				slot = GetSlotUnderMouse( vOffset );
				if( slot != null && slot.CanBeConnected())
				{
					mDrawingStartSlot = slot;
					SetMouseStatus( MouseStatus.DrawingLine );
					Event.current.Use();
				}
				break;
			
			case MouseHandlingCall.StartDrawingPanel :
				if( !clickedpanel.mbSelected )
				{
					if( mbCtrlKeyDown )
					{
						AddToSelection( clickedpanel );	
					}
					else
					{
						Select( clickedpanel );					
					}						
				}
				else
				{
					mPrimarySelectedPanel = clickedpanel;	
				}
				GUI.BringWindowToFront( clickedpanel.miUniqueID );
				break;
				
			case MouseHandlingCall.EndDrawingPanel :
				SetMouseStatus( MouseStatus.MovingPanels );
				Event.current.Use();
				break;
				
			case MouseHandlingCall.AfterDrawingAllPanels :
				SetMouseStatus( MouseStatus.DrawingBox );
				mvDragStart = Event.current.mousePosition;
				break;
			}
			break;
			
		case MouseStatus.DrawingLine:
			// Check if slot clicked
			slot = GetSlotUnderMouse( vOffset );
			if( slot != null && slot.CanBeConnectedWith( mDrawingStartSlot ) )
			{
				slot.ConnectWith( mDrawingStartSlot );
				mbDirty = true;
			}
			SetMouseStatus( MouseStatus.Default );
			//Event.current.Use();
			break;	
		/*	
		default:
			if( clickedpanel != null )
			{
				GUI.BringWindowToFront( clickedpanel.miUniqueID );
			}
			break;
			*/
		}
	}
	
	//------------------------------------------------------------------------------------------------
	// HandleMouseUp
	// The left mouse button has been released
	//------------------------------------------------------------------------------------------------		
	void HandleMouseUp()
	{
		switch( mMouseStatus )
		{
		case MouseStatus.DrawingBox:
			SetMouseStatus( MouseStatus.Default );
			Rect rect = new Rect( mvDragStart.x, mvDragStart.y, Event.current.mousePosition.x - mvDragStart.x, Event.current.mousePosition.y - mvDragStart.y );			
			if( mbCtrlKeyDown )
			{
			   SelectInsideAndAlreadySelected( rect );
			}
			else
			{
			   SelectInside( rect );
			}				
			break;
			
		case MouseStatus.MovingPanels:					
			SetMouseStatus( MouseStatus.Default );
			break;			
		}
	}
	
	//------------------------------------------------------------------------------------------------
	// HandleRightMouseUp
	// The right mouse button has been released
	//------------------------------------------------------------------------------------------------		
	void HandleRightMouseUp()
	{
		switch( mMouseStatus )
		{
		case MouseStatus.Scrolling:	
			SetMouseStatus( MouseStatus.Default );
			break;	
			
		case MouseStatus.DrawScrolling:					
			SetMouseStatus( MouseStatus.DrawingLine );
			break;			
		}
	}	

	//------------------------------------------------------------------------------------------------
	// HandleMouseRightClick
	// Now the user did a right click. I suppose he expects me to do something.
	//------------------------------------------------------------------------------------------------
	void HandleMouseRightClick( MouseHandlingCall call, EditorPanel clickedpanel )
	{
		Vector2 vOffset = (clickedpanel != null ) ? clickedpanel.GetUpperLeftCorner() : Vector2.zero;
		
		switch( mMouseStatus )
		{
		case MouseStatus.Default:
			// Check if slot clicked
			EditorSlot slot = GetSlotUnderMouse( vOffset );
			if( slot != null  )
			{
				slot.DeleteAllConnections();
				mbDirty = true;
			}
			mMouseStatus = MouseStatus.Scrolling;
			break;
			
		case MouseStatus.DrawingLine:
			mMouseStatus = MouseStatus.DrawScrolling;
			break;		
		}
	}

	//================================================================================================
	//
	//   AUXILIARY GUI METHODS
	//
	//================================================================================================
	
	//------------------------------------------------------------------------------------------------
	// MoveAllWindows
	// Used for scrolling
	//------------------------------------------------------------------------------------------------	
	void MoveAllWindows( Vector2 vDelta )
	{	
		foreach( EditorPanel panel in maPanels )
		{
			panel.mWindowRect.x += vDelta.x;
			panel.mWindowRect.y += vDelta.y;	
			
			GUI.BringWindowToBack( panel.miUniqueID );

			foreach( EditorSlot slot in panel.maSlots )
			{
				if( slot.mbEditing )
				{
					slot.mWindowRect.x += vDelta.x;
					slot.mWindowRect.y += vDelta.y;	
				}
			}
		}
		
#if _DEMO_
		mvDemoPostitPosition += vDelta * POSTIT_PARALLAX_FACTOR;
		mvDemoPostitShadowPosition += vDelta;
		
		if( mvDemoPostitPosition.x < Mathf.Min( Screen.width / 2, mDemoPostitTexture.width* 0.5f ) 
		   ||  mvDemoPostitPosition.x > Mathf.Max( Screen.width / 2, Screen.width - mDemoPostitTexture.width* 0.5f )
		   || mvDemoPostitPosition.y < Mathf.Min( Screen.height / 2, mDemoPostitTexture.height* 0.5f ) 
		   ||  mvDemoPostitPosition.y >  Mathf.Max( Screen.height / 2, Screen.height - mDemoPostitTexture.height * 0.5f ) )
		{
			// Postit almost offscreen - reteleport to screen center
			mvDemoPostitPosition.x = Screen.width / 2;
			mvDemoPostitPosition.y = Screen.height / 2;
			mvDemoPostitShadowPosition = mvDemoPostitPosition + new Vector2( 50.0f, 50.0f );
			mfDemoPostitAngle = UnityEngine.Random.Range( - 30.0f, 30.0f );
		}
#endif
		
		mbDirty = true;
		
		mfBackgroundOffset = ( mfBackgroundOffset - vDelta.y + Screen.height ) % Screen.height;
	}
	
	//------------------------------------------------------------------------------------------------
	// MoveAllUnselectedWindows
	// Used for scrolling
	//------------------------------------------------------------------------------------------------	
	void MoveAllUnselectedWindows( Vector2 vDelta )
	{		
		foreach( EditorPanel panel in maPanels )
		{
			if( !panel.mbSelected )
			{
				panel.mWindowRect.x += vDelta.x;
				panel.mWindowRect.y += vDelta.y;	
				
				GUI.BringWindowToBack( panel.miUniqueID );
	
				foreach( EditorSlot slot in panel.maSlots )
				{
					if( slot.mbEditing )
					{
						slot.mWindowRect.x += vDelta.x;
						slot.mWindowRect.y += vDelta.y;	
					}
				}
			}
		}
		
#if _DEMO_
		mvDemoPostitPosition += vDelta * POSTIT_PARALLAX_FACTOR;
		mvDemoPostitShadowPosition += vDelta;
		
		if( mvDemoPostitPosition.x < Mathf.Min( Screen.width / 2, mDemoPostitTexture.width* 0.5f ) 
		   ||  mvDemoPostitPosition.x > Mathf.Max( Screen.width / 2, Screen.width - mDemoPostitTexture.width* 0.5f )
		   || mvDemoPostitPosition.y < Mathf.Min( Screen.height / 2, mDemoPostitTexture.height* 0.5f ) 
		   ||  mvDemoPostitPosition.y >  Mathf.Max( Screen.height / 2, Screen.height - mDemoPostitTexture.height * 0.5f ) )
		{
			// Postit almost offscreen - reteleport to screen center
			mvDemoPostitPosition.x = Screen.width / 2;
			mvDemoPostitPosition.y = Screen.height / 2;
			mvDemoPostitShadowPosition = mvDemoPostitPosition + new Vector2( 50.0f, 50.0f );
			mfDemoPostitAngle = UnityEngine.Random.Range( - 30.0f, 30.0f );
		}
#endif
		
		mbDirty = true;
		
		mfBackgroundOffset = ( mfBackgroundOffset - vDelta.y + Screen.height ) % Screen.height;
	}	
	
	
	//------------------------------------------------------------------------------------------------
	// MoveSelectedWindows
	// Like MoveAllWindows but only moves the selected windows
	//------------------------------------------------------------------------------------------------	
	void MoveSelectedWindows( Vector2 vDelta, EditorPanel focussedPanel )
	{
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.mbSelected  )
			{
				panel.mWindowRect.x += vDelta.x;
				panel.mWindowRect.y += vDelta.y;	
	
				foreach( EditorSlot slot in panel.maSlots )
				{
					if( slot.mbEditing )
					{
						slot.mWindowRect.x += vDelta.x;
						slot.mWindowRect.y += vDelta.y;	
					}
				}
			}
		}
		
		if( mModifyingDuplicateOffset )
		{
			mvDuplicateOffset += vDelta;
		}
		mbDirty = true;
	}	
	
	
	//------------------------------------------------------------------------------------------------
	// GetSlotUnderMouse
	// What slot is the mouse cursor over ? 
	//------------------------------------------------------------------------------------------------		
	public EditorSlot GetSlotUnderMouse( Vector2 vOffset )
	{
		foreach( EditorPanel panel in maPanels )
		{
			EditorSlot slot = panel.GetNearSlot( vOffset + Event.current.mousePosition );
			if( slot != null )
			{
				return slot;
			}
		}
		
		return null;
	}
	
	//------------------------------------------------------------------------------------------------
	// CloneSelectedPanels
	// Makes copies of all selected panels and selects them
	//------------------------------------------------------------------------------------------------	
	void CloneSelectedPanels()
	{
		// First we create an arraylist with the selected panels
		// (We don't want to read and modify maPanels in the same loop, do we ?)
		ArrayList aSelectedPanels = new ArrayList();
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.mbSelected  )
			{
				aSelectedPanels.Add( panel );
			}
		}
		
		// Create clones 
		ArrayList aNewPanels = new ArrayList();		
		for( int i = 0; i < aSelectedPanels.Count; i++ )
		{
			EditorPanel panel = aSelectedPanels[i] as EditorPanel;
			
			// Clone the panel
			EditorPanel newPanel = new EditorPanel(); //ScriptableObject.CreateInstance( typeof( EditorPanel ) ) as EditorPanel;	
			newPanel.CloneFrom( panel );
			
			// Truncate the offset (we don't want panels to be teleported to Australia)
			mvDuplicateOffset.x = Mathf.Clamp( mvDuplicateOffset.x, -MAX_DUPLICATE_OFFSET, MAX_DUPLICATE_OFFSET );
			mvDuplicateOffset.y = Mathf.Clamp( mvDuplicateOffset.y, -MAX_DUPLICATE_OFFSET, MAX_DUPLICATE_OFFSET );
			
			// Position the new panel
			newPanel.mWindowRect.x = panel.mWindowRect.x; 
			newPanel.mWindowRect.y = panel.mWindowRect.y;
			newPanel.mWindowRect.width = panel.mWindowRect.width;
			newPanel.mWindowRect.height = panel.mWindowRect.height;			
			newPanel.mbBringToFrontOnceCreated	= true;
			
			// Select it in place of the old one
			panel.mbSelected = false;
			newPanel.mbSelected = true;
			
			//Oh, and we also need to add it to aNewPanels
			// Note that the ith element of aNewPanels is a clone of the ith element of aSelectedPanels
			aNewPanels.Add( newPanel );
		}	
		
		// Finally each link between selected panels is "cloned" as a link between new panels
		// That's why we created the aNewPanels array which mirrors aSelectedPanels	
		for( int i = 0; i < aSelectedPanels.Count; i++ )
		{
			EditorPanel oldPanel = aSelectedPanels[i] as EditorPanel;
			EditorPanel newPanel = aNewPanels[i] as EditorPanel;	
			
			for( int s = 0; s < oldPanel.maSlots.Length; s++ )
			{
				EditorSlot oldSlot = oldPanel.maSlots[s];
				EditorSlot newSlot = newPanel.maSlots[s];
				
				foreach( EditorLink oldLink in oldSlot.maIncomingLinks )
				{
					EditorSlot oldConnectedSlot = oldLink.mStartSlot;
					EditorPanel oldConnectedPanel = oldConnectedSlot.mParent;
									
					// See if we find the connected panel in the old selected panels
					for( int j = 0; j < aSelectedPanels.Count; j++ )
					{
						if( oldConnectedPanel == aSelectedPanels[j] )
						{
							// Bingo ! Determine the corresponding new panel and slot
							EditorPanel newConnectedPanel = aNewPanels[j] as EditorPanel;
							EditorSlot newConnectedSlot = newConnectedPanel.maSlots[ oldConnectedSlot.miIndex ];
							
							// Create the new link
							newConnectedSlot.ConnectWith( newSlot );
							
							// No need to continue for loop
							break;
						}
					}
					
				}

				foreach( EditorLink oldLink in oldSlot.maOutgoingLinks )
				{
					EditorSlot oldConnectedSlot = oldLink.mEndSlot;
					EditorPanel oldConnectedPanel = oldConnectedSlot.mParent;
									
					// See if we find the connected panel in the old selected panels
					for( int j = 0; j < aSelectedPanels.Count; j++ )
					{
						if( oldConnectedPanel == aSelectedPanels[j] )
						{
							// Bingo ! Determine the corresponding new panel and slot
							EditorPanel newConnectedPanel = aNewPanels[j] as EditorPanel;
							EditorSlot newConnectedSlot = newConnectedPanel.maSlots[ oldConnectedSlot.miIndex ];
							
							// Create the new link
							newSlot.ConnectWith( newConnectedSlot );
							
							// No need to continue for loop
							break;
						}
					}
				}			
			}		
		}
		mbDirty = true;
		
		//TODO: Clone nested group structure ?
		
		if( mSelectedGroup != null )
		{
			// Original panels were a group, the selection should as well
			mSelectedGroup = null;
			GroupPanels();
		}
		RefreshSelectedGroup();	
		
		mModifyingDuplicateOffset = true;
	}
	

	//------------------------------------------------------------------------------------------------
	// DeleteSelectedPanels
	// Deletes all selected panels 
	//------------------------------------------------------------------------------------------------	
	void DeleteSelectedPanels()
	{		 
		ArrayList aSelectedPanels = new ArrayList();
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.mbSelected  )
			{
				aSelectedPanels.Add( panel );
				BreakGroupsWithPanel( panel );
			}
		}
		
		while( aSelectedPanels.Count > 0 )
		{
			EditorPanel moriturus = aSelectedPanels[0] as EditorPanel;
			aSelectedPanels.RemoveAt(0);
			RemovePanel( moriturus );
			//DestroyImmediate( moriturus );
		}
		mbDirty = true;
		mSelectedGroup = null;
	}
	

	//================================================================================================
	//
	//   SELECTION METHODS
	//
	//================================================================================================

	//------------------------------------------------------------------------------------------------
	// UnselectAll
	// Unselects all panels
	//------------------------------------------------------------------------------------------------		
	void UnselectAll () 
	{
		mPrimarySelectedPanel = null;
		foreach( EditorPanel panel in maPanels )
		{
			panel.mbSelected = false;
		}
		RefreshSelectedGroup();
		
		mModifyingDuplicateOffset = false;
	}
	
	//------------------------------------------------------------------------------------------------
	// Select
	// Selects one panel 
	//------------------------------------------------------------------------------------------------		
	void Select( EditorPanel panelToSelect ) 
	{
		if( mPrimarySelectedPanel != panelToSelect )
		{
			GUI.FocusControl( "" );
		}
		mPrimarySelectedPanel = panelToSelect;
		foreach( EditorPanel otherpanel in maPanels )
		{
			otherpanel.mbSelected = ( otherpanel.miUniqueID == panelToSelect.miUniqueID );
		}
		ExtendSelectionToGroups();
		RefreshSelectedGroup();
		
		GUI.BringWindowToFront( panelToSelect.miUniqueID );
		mModifyingDuplicateOffset = false;
	}
	
	//------------------------------------------------------------------------------------------------
	// AddToSelection
	// Selects one panel, but doesn't unselect currently selected (ctrl-click)
	//------------------------------------------------------------------------------------------------		
	void AddToSelection( EditorPanel panelToSelect ) 
	{
		mPrimarySelectedPanel = null;
		foreach( EditorPanel otherpanel in maPanels )
		{
			otherpanel.mbSelected |= ( otherpanel == panelToSelect );
		}
		ExtendSelectionToGroups();
		RefreshSelectedGroup();
		
		GUI.BringWindowToFront( panelToSelect.miUniqueID );
		mModifyingDuplicateOffset = false;
	}
	
	//------------------------------------------------------------------------------------------------
	// SelectInside
	// Select the panels (partially) inside the selection rectangle
	//------------------------------------------------------------------------------------------------		
	void SelectInside( Rect rectangle ) 
	{
		mPrimarySelectedPanel = null;
		if( rectangle.width < 0 )
		{
			rectangle.width = -rectangle.width;
			rectangle.x -= rectangle.width;
		}
		if( rectangle.height < 0 )
		{
			rectangle.height = -rectangle.height;
			rectangle.y -= rectangle.height;
		}		
		foreach( EditorPanel panel in maPanels )
		{
			if(   panel.mWindowRect.xMax > rectangle.xMin 
			   && panel.mWindowRect.xMin < rectangle.xMax
			   && panel.mWindowRect.yMax > rectangle.yMin
			   && panel.mWindowRect.yMin < rectangle.yMax )
			{
			 	panel.mbSelected = true;
				GUI.BringWindowToFront( panel.miUniqueID );
			}
			else
			{
				panel.mbSelected = false;
			}
		}
		ExtendSelectionToGroups();
		RefreshSelectedGroup();
		
		mModifyingDuplicateOffset = false;
	}
	
	//------------------------------------------------------------------------------------------------
	// SelectInsideAndAlreadySelected
	// Select the panels (partially) inside the selection rectangle, but doesn't unselect currently selected (ctrl-click)
	//------------------------------------------------------------------------------------------------		
	void SelectInsideAndAlreadySelected( Rect rectangle ) 
	{
		mPrimarySelectedPanel = null;
		if( rectangle.width < 0 )
		{
			rectangle.width = -rectangle.width;
			rectangle.x -= rectangle.width;
		}
		if( rectangle.height < 0 )
		{
			rectangle.height = -rectangle.height;
			rectangle.y -= rectangle.height;
		}		
		foreach( EditorPanel panel in maPanels )
		{
			if(   panel.mWindowRect.xMax > rectangle.xMin 
			   && panel.mWindowRect.xMin < rectangle.xMax
			   && panel.mWindowRect.yMax > rectangle.yMin
			   && panel.mWindowRect.yMin < rectangle.yMax )
			{
			 	panel.mbSelected = true;
				GUI.BringWindowToFront( panel.miUniqueID );
			}
		}
		ExtendSelectionToGroups();
		RefreshSelectedGroup();
		
		mModifyingDuplicateOffset = false;
	}	
	
	//------------------------------------------------------------------------------------------------
	// BringSelectedToFront
	// Displays the selected panels in front of the unselected
	//------------------------------------------------------------------------------------------------		
	void BringSelectedToFront() 
	{
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.mbSelected )
			{
				GUI.BringWindowToFront( panel.miUniqueID );
			}
		}
	}
	
	//================================================================================================
	//
	//   GROUPING METHODS
	//
	//================================================================================================

	//------------------------------------------------------------------------------------------------
	// GroupPanels
	// Creates a new group containing all selected panels
	//------------------------------------------------------------------------------------------------		
	void GroupPanels () 
	{
		if( maGroups == null )
		{
			maGroups = new Group[0];	
		}
		
		Group newGroup = new Group();
		newGroup.maPanels = new EditorPanel[miSelectedPanels];
		int i = 0;
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.mbSelected )
			{
				newGroup.maPanels[i] = panel;
				i++;
			}
		}
		

		Array.Resize( ref maGroups, maGroups.Length + 1 );
		newGroup.miSize = newGroup.maPanels.Length;
		maGroups[maGroups.Length-1] = newGroup;
		
		//DELETEME ?
		/*
		newGroup.maPanelIDs = new int[newGroup.maPanels.Length];
		for( int j = 0; j < newGroup.maPanels.Length; j++ )
		{
			newGroup.maPanelIDs[j] = newGroup.maPanels[j].miUniqueID;
		}
		*/
		
		mSelectedGroup = newGroup;
		mbDirty = true;
	}	
	
	//------------------------------------------------------------------------------------------------
	// UngroupPanels
	// Breaks the currently selected group
	//------------------------------------------------------------------------------------------------		
	void UngroupPanels () 
	{
		// To be sure
		RefreshSelectedGroup();
		
		// Find and destroy selected groups	
		int iRemoveIndex = Array.IndexOf( maGroups, mSelectedGroup );
		if( iRemoveIndex == -1 )
		{
			return;
		}			
		
		for( int i = iRemoveIndex; i < maGroups.Length-1; i++ )
		{
			maGroups[i]	= maGroups[i+1];
		}
		Array.Resize( ref maGroups, maGroups.Length - 1 );
		mSelectedGroup = null;
		mbDirty = true;
	}
	
	//------------------------------------------------------------------------------------------------
	// RefreshSelectedGroup
	// Sets mSelectedGroup to the group - if any - that contains exactly the selected panels.
	// Should be called at every selection change.
	//------------------------------------------------------------------------------------------------		
	void RefreshSelectedGroup() 
	{
		if( maGroups == null )
		{
			maGroups = new Group[0];	
		}
		
		miSelectedPanels = 0;
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.mbSelected )
			{
				miSelectedPanels++;
			}
		}
		
		if( miSelectedPanels > 1 )
		{
			foreach( Group groope in maGroups )  // We write "groope" because "group" is a keyword
			{
				if( IsGroupExactlySelected( groope )  )
				{
					mSelectedGroup = groope;
					return;
				}	
			}
		}			
		mSelectedGroup = null;
	}
	
	//------------------------------------------------------------------------------------------------
	// ExtendSelectionToGroups()
	// Selects entirely all partially selected groups
	//------------------------------------------------------------------------------------------------		
	void ExtendSelectionToGroups() 
	{
		if( maGroups == null )
		{
			maGroups = new Group[0];	
		}
		
		foreach( Group groope in maGroups )  // We write "groope" because "group" is a keyword
		{
			int iSelectedMembers = 0;
			foreach( EditorPanel panel in groope.maPanels )
			{
				if( panel != null && panel.mbSelected )
				{
					iSelectedMembers++;
				}
			}	
			
			if( iSelectedMembers > 0 && iSelectedMembers < groope.maPanels.Length )
			{
				// Group partially selected
				foreach( EditorPanel panel in groope.maPanels )
				{
					panel.mbSelected = true;
				}				
			}
			
		}
	}
	
		
	//------------------------------------------------------------------------------------------------
	// IsGroupExactlySelected
	// Checks whether the group consists exactly of the selected panels
	//------------------------------------------------------------------------------------------------		
	bool IsGroupExactlySelected( Group groope ) 
	{
		if( miSelectedPanels != groope.maPanels.Length )
		{
			//Can't be this group
			return false;
		}
		
		foreach( EditorPanel panel in groope.maPanels )
		{
			if( !panel.mbSelected )
			{
				//Unselected panel in group
				return false;
			}
		}
		
		// Now we have the correct number of selected panels, AND all panels in the group are selected, thus none outside are selected.
		return true;
	}
	
	
	//------------------------------------------------------------------------------------------------
	// BreakGroupsWithPanel
	// Breaks all groups which contain a specific panel
	//------------------------------------------------------------------------------------------------		
	void BreakGroupsWithPanel( EditorPanel panel ) 
	{
		for( int i = 0; i < maGroups.Length; i++ )
		{
			Group groope = maGroups[i];
			if( Array.IndexOf( groope.maPanels, panel ) != -1 )
			{
				Debug.Log("Breaking group");
				
				// Shift following elements one step backwards
				for( int j = i; j < maGroups.Length-1; j++ )
				{
					maGroups[j]	= maGroups[j+1];
				}
				
				// Shorten array
				Array.Resize( ref maGroups, maGroups.Length - 1 );				
			}
			else
			{
				Debug.Log("Not in group");	
			}
		}
	}

	//================================================================================================
	//
	//   Find and Replace methods
	//
	//================================================================================================

	//------------------------------------------------------------------------------------------------
	// GroupPanels
	// Creates a new group containing all selected panels
	//------------------------------------------------------------------------------------------------		
	void FindAndReplace( string strFind, string strReplaceOrNull, bool bSelectionOnly  ) 
	{
		foreach( EditorPanel panel in maPanels )
		{
			if( !bSelectionOnly || panel.mbSelected )
			{
				panel.mbSelected = panel.FindAndReplace( strFind, strReplaceOrNull );
			}
		}
		
		// The whole group system is based on the assumption that a group is never ever partally selected, so...
		ExtendSelectionToGroups();
		RefreshSelectedGroup();
	}
		
	//================================================================================================
	//
	//   ALLIGNMENT METHODS
	//
	//================================================================================================

	//------------------------------------------------------------------------------------------------
	// AllignSelectedLeft
	// Alligns left borders of selected panels to a common vertical line
	//------------------------------------------------------------------------------------------------		
	void AllignSelectedLeft () 
	{
		// Gather data
		float fSumOfX = 0.0f;
		int iSelectedPanels = 0;
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.mbSelected  )
			{
				iSelectedPanels++;
				fSumOfX += panel.mWindowRect.x;
			}
		}
		
		// Method has no sense for zero or one panel
		if( iSelectedPanels <= 1 )
		{
			return;
		}
		
		// Allign panels
		float fNewX = Mathf.Round( fSumOfX / iSelectedPanels );
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.mbSelected  )
			{
				panel.mWindowRect.x = fNewX;
			}
		}
		mbDirty = true;
	}

	//------------------------------------------------------------------------------------------------
	// AllignSelectedTop
	// Alligns upper borders of selected panels to a common horizontal line
	//------------------------------------------------------------------------------------------------		
	void AllignSelectedTop () 
	{
		// Gather data
		float fSumOfY = 0.0f;
		int iSelectedPanels = 0;
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.mbSelected  )
			{
				iSelectedPanels++;
				fSumOfY += panel.mWindowRect.y;
			}
		}
		
		// Method has no sense for zero or one panel
		if( iSelectedPanels <= 1 )
		{
			return;
		}
		
		// Allign panels
		float fNewY = Mathf.Round( fSumOfY / iSelectedPanels );
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.mbSelected  )
			{
				panel.mWindowRect.y = fNewY;
			}
		}
		mbDirty = true;
	}	
	
	//------------------------------------------------------------------------------------------------
	// AllignSelectedRight
	// Alligns right borders of selected panels to a common vertical line
	//------------------------------------------------------------------------------------------------		
	void AllignSelectedRight () 
	{
		// Gather data
		float fSumOfX = 0.0f;
		int iSelectedPanels = 0;
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.mbSelected  )
			{
				iSelectedPanels++;
				fSumOfX += panel.mWindowRect.x + panel.mWindowRect.width;
			}
		}
		
		// Method has no sense for zero or one panel
		if( iSelectedPanels <= 1 )
		{
			return;
		}
		
		// Allign panels
		float fNewX = Mathf.Round( fSumOfX / iSelectedPanels );
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.mbSelected  )
			{
				panel.mWindowRect.x = fNewX - panel.mWindowRect.width;
			}
		}
		mbDirty = true;
	}

	//------------------------------------------------------------------------------------------------
	// AllignSelectedBottom
	// Alligns lower borders of selected panels to a common horizontal line
	//------------------------------------------------------------------------------------------------		
	void AllignSelectedBottom () 
	{
		// Gather data
		float fSumOfY = 0.0f;
		int iSelectedPanels = 0;
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.mbSelected  )
			{
				iSelectedPanels++;
				fSumOfY += panel.mWindowRect.y + panel.mWindowRect.height;
			}
		}
		
		// Method has no sense for zero or one panel
		if( iSelectedPanels <= 1 )
		{
			return;
		}
		
		// Allign panels
		float fNewY = Mathf.Round( fSumOfY / iSelectedPanels );
		foreach( EditorPanel panel in maPanels )
		{
			if( panel.mbSelected  )
			{
				panel.mWindowRect.y = fNewY - panel.mWindowRect.height;
			}
		}
		mbDirty = true;
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
	
	
	//------------------------------------------------------------------------------------------------
	// DebugDrawRectangle
	//------------------------------------------------------------------------------------------------		
	void DebugDrawRectangle ( Rect rectangle ) 
	{
		if( mMinimapTexture == null )
		{
			mMinimapTexture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
			mMinimapTexture.SetPixel( 0, 0, new Color( 1.0f, 1.0f, 1.0f, 0.1f ) );
			mMinimapTexture.Apply();	
		}		
		GUI.DrawTexture( rectangle, mMinimapTexture );
	}
	

	
	//================================================================================================
	//
	//   READ AND WRITE
	//
	//================================================================================================
	
	//------------------------------------------------------------------------------------------------
	// Write
	// Saves the graph to an XML file
	//------------------------------------------------------------------------------------------------		
	void Write( string strPath )
	{
		ReinitializeSlotIDs();
		
		try
		{
			// Prepare the writer
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = ("    ");
			mReader = null;
			try
			{
				mWriter = XmlWriter.Create( strPath, settings);
			}
			catch( ArgumentNullException  e )
			{
				Debug.LogError("Error: Writing path invalid.");
				throw( e );
				
			}			
			
			// <graph>
			mWriter.WriteStartElement("graph");	
			
			foreach( EditorPanel panel in maPanels )
			{
				panel.Write( mWriter );
			}
			
			if( maGroups.Length > 0 )
			{
				// <grouplist groups = "7">
				mWriter.WriteStartElement("grouplist");	
				mWriter.WriteAttributeString( "groups", "" + maGroups.Length );
				
				foreach( Group groope  in maGroups )
				{
					// <group panels = "4">
					mWriter.WriteStartElement("group");
					mWriter.WriteAttributeString( "panels", "" + groope.maPanels.Length );
					
					foreach( EditorPanel panel  in groope.maPanels )
					{
						//<PanelID>24</PanelID>
						mWriter.WriteStartElement("PanelID");
						mWriter.WriteValue( panel.miUniqueID );	
						mWriter.WriteEndElement();
					}
					
					// </group>
					mWriter.WriteEndElement();
				}
				
				// </grouplist>
				mWriter.WriteEndElement();
			}
			
			// </graph>
			mWriter.WriteEndElement();
			
			
			
			// Finish the writer
			mWriter.Flush();
		}
		catch( Exception  e )
		{
			Debug.LogError( "Error while writing graph file "+strPath+": \n"+e);
			
		}
		finally
		{
			if( mWriter != null && mWriter.WriteState != WriteState.Closed )
			{
				mWriter.Close();
				
				AssetDatabase.Refresh();
			}
		}
	}

	//------------------------------------------------------------------------------------------------
	// Read
	// Loads the graph from an XML file
	//------------------------------------------------------------------------------------------------	
	void Read( string strPath )
	{
		// Destroy current level content
		ClearAll();
		ReadAndAdd( strPath, false );
	}
	
	//------------------------------------------------------------------------------------------------
	// ReadAndAdd
	// Loads the graph from an XML file and adds it to the current graph
	//------------------------------------------------------------------------------------------------	
	void ReadAndAdd( string strPath, bool bSelectNew )
	{
		try
		{
			// Prepare the reader
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreWhitespace = true;
			mReader  = XmlReader.Create( strPath, settings);
			mWriter = null;	
			
			UnselectAll();
			
			
			if( !mReader.IsEmptyElement )
			{
				// Put old panel list aside 
				// (In order not to mix up panel IDs)
				EditorPanel[] aOldPanels = maPanels;
				maPanels = new EditorPanel[0];
				
				// <graph>
				mReader.ReadStartElement("graph");
				
				// Read panels
				while( mReader.NodeType == XmlNodeType.Element && mReader.Name == "panel" )
				{	
					EditorPanel newPanel = new EditorPanel(); //ScriptableObject.CreateInstance( typeof( EditorPanel ) ) as EditorPanel;
					newPanel.Read( mReader );
					AddPanel( newPanel );
					miNextUnusedSlotID = Mathf.Max( miNextUnusedSlotID, newPanel.miUniqueID+1 );
				}
				

				foreach( EditorPanel panel in maPanels )
				{
					panel.PostReadInitialization();
				} 
				
				if( bSelectNew )
				{

					foreach( EditorPanel panel in maPanels )
					{
						AddToSelection( panel );
					} 
				}
				
				foreach( EditorPanel oldPanel in aOldPanels )
				{
					AddPanel( oldPanel );
				} 				
					
					
				if( mReader.NodeType == XmlNodeType.Element && mReader.Name == "grouplist" )
				{
					// <grouplist groups = "7">
					int iNumberOfGroups = System.Convert.ToInt32( mReader["groups"] );
					mReader.ReadStartElement("grouplist");	
					ArrayList aGroups = new ArrayList( maGroups );
					
					for( int i = 0; i < iNumberOfGroups; i++ )
					{
						Group newGroup = new Group();
						
						// <group panels = "4">
						int iNumberOfPanels = System.Convert.ToInt32( mReader["panels"] );
						newGroup.maPanels = new EditorPanel[iNumberOfPanels];
						//newGroup.maPanelIDs = new int[iNumberOfPanels];//DELETEME
						mReader.ReadStartElement("group");
						
						for( int j = 0; j < iNumberOfPanels; j++ )
						{
							//<PanelID>24</PanelID>
							mReader.ReadStartElement("PanelID");
							int iID = mReader.ReadContentAsInt();
							newGroup.maPanels[j] = GetPanelWithID( iID ); 
							//newGroup.maPanelIDs[j] = iID;//DELETEME
							mReader.ReadEndElement();
						}
						
						aGroups.Add( newGroup );
						
						// </group>
						mReader.ReadEndElement();
					}
					
					// </grouplist>
					mReader.ReadEndElement();
					
					maGroups = aGroups.ToArray( typeof( Group ) ) as Group[];
				}
			
				// </graph>
				mReader.ReadEndElement();
			}
			else
			{
				// <graph/>
				mReader.ReadStartElement("graph");						
			}
			
			//RegisterAllPanelSetWindows();
			
		} // Gotta catch 'em all !
		catch( SecurityException  e )
		{
			Debug.LogError( "Error: Not allowed to read diagram file "+strPath );
			Debug.LogError( ""+e );
		}
		catch( FileNotFoundException  e )
		{
			Debug.LogError( "Error: Unable to find diagram file "+strPath );
			Debug.LogError( ""+e );				
		}
		catch( UriFormatException  e )
		{
			Debug.LogError( "Error: Incorrect URI format of diagram file "+strPath );
			Debug.LogError( ""+e );				
			
		}		
		catch( Exception  e )
		{
			Debug.LogError( "Error while reading diagram file "+strPath+": \n"+e);
		}
		finally
		{
			if( mReader != null && mReader.ReadState != ReadState.Closed )
			{
				// Finish the reader
				mReader.Close();
				//Debug.Log("File read from "+strPath );	
			}
		}		
		
		ReinitializeSlotIDs();

	}
	
	public void ReinitializeSlotIDs()
	{
		miNextUnusedSlotID = START_SLOT_IDS;
		foreach( EditorPanel panel in maPanels )
		{
			foreach( EditorSlot slot in panel.maSlots )
			{
				slot.miUniqueID = miNextUnusedSlotID++;
			}			
		}	
		foreach( EditorLink link in maLinks )
		{
			link.MemorizeSlotIDs();
		}
	}
	
	private bool IsPrime( int iNumber ) 
	{
		// 0 and 1 are no primes
		if( iNumber <= 1 )
		{
			return false;	
		}		
		
		// Is iNumber even ?
		if( iNumber%2 == 0 )
		{
			return ( iNumber == 2 );	
		}		
		
		// Test all odd factors up to the square root of iNumber
		int iSqrt = (int)Mathf.Floor( Mathf.Sqrt( (float)iNumber ) );
		for ( int i = 3; i <= iSqrt; i += 2 )
		{
			if( iNumber%i == 0 )
			{
				// We found a factor
				return false;
			}
		}
		
		// No factor found, must be prime
		return true;
	}

	private void ApplyIdentityTransform()
	{
		GUI.matrix = Matrix4x4.identity; 
	}	

	
	public void GoMad()
	{
		mbGoneMad = true;
		mfLastMadnessUpdateTime = (float)EditorApplication.timeSinceStartup;
		foreach( EditorPanel panel in maPanels )
		{
			panel.GoMad(); 
		}
		
	}
	
	private void MadnessUpdate()
	{
		if( maPanels.Length == 0 )
		{
			// Madness cleansed
			mbGoneMad = false;
			return;
		}
		
		float fTime = (float)EditorApplication.timeSinceStartup;
		float fDeltaTime = fTime - mfLastMadnessUpdateTime;
		mfLastMadnessUpdateTime = fTime;	
		foreach( EditorPanel panel in maPanels )
		{
			panel.MadnessUpdate( fDeltaTime ); 
		}
		
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
	
	

	public Color GetOctarine() 
	{
		return mOctarine;	
	}

	public Color GetColourOutOfSpace() 
	{
		return mTheColourOutOfSpace;	
	}
	
	public void OnDisable() 
	{
		//Debug.Log("OnDisable");
		
		int iMaxgroupSize = 0;
		if( maGroups == null )
		{
			maGroups = new Group[0];	
		}
		
		foreach( Group groope in maGroups  )
		{
			iMaxgroupSize = Mathf.Max( iMaxgroupSize, groope.maPanels.Length );
		}
			
			
		// "Save" group contents to the ID arrays
		for( int i = 0;  i < maGroups.Length; i++ )
		{
			Group groope = maGroups[i];
			groope.miSize = groope.maPanels.Length;
			groope.maPanelIDs = new int[groope.maPanels.Length];
			for( int j = 0; j < groope.maPanels.Length; j++ )
			{
				groope.maPanelIDs[j] = groope.maPanels[j].miUniqueID;
			}
		}
	
		ReinitializeSlotIDs();		
	}

	public void OnEnable() 
	{
		//Debug.Log("OnEnable");
		
		if( maPanels == null )
		{
			maPanels = new EditorPanel[0];	
		}		
		if( maLinks == null )
		{
			maLinks = new EditorLink[0];	
		}
		
		for( int i = 0; i < maPanels.Length; i++ )
		{
			EditorPanel panel = maPanels[i];
				
			// Repair parent pointers
			foreach( EditorSlot slot in panel.maSlots )
			{
				slot.RepairBrokenParentPointers( panel );
				slot.ForgetAllLinks();
			}	
			
			// Assign correct ID
			panel.miUniqueID = START_PANELS_IDS + i;
		}
		foreach( EditorLink link in maLinks )
		{
			link.Reconnect( this );	
		}
		
		// Restitute Group contents from ID arrays
		if( maGroups != null )
		{
			for( int i = 0;  i < maGroups.Length; i++ )
			{
				bool bBadGroup = false;
				Group groope = maGroups[i];
				if( groope != null )
				{
				
					groope.maPanels = new EditorPanel[groope.miSize];
					for( int j = 0; j < groope.miSize; j++ )
					{
						int ID = groope.maPanelIDs[j];
						if( ID != 0 )
						{
							EditorPanel panel = GetPanelWithID( ID );
							groope.maPanels[j] = panel;
						}
						else
						{
							bBadGroup = true;	
						}
					}
				}
				else
				{
					bBadGroup = true;	
				}
				
				if( bBadGroup )
				{
					// Delete group from array
					for( int j = i;  j < maGroups.Length-1; j++ )
					{	
						maGroups[j] = maGroups[j+1];
					}
					Array.Resize<Group>( ref maGroups, maGroups.Length-1 );
				}
			}
		}
	}
	
	string DebugGroupInfo()
	{
		string strOutput = "";
		if( maGroups != null )
		{
			for( int i = 0;  i < maGroups.Length; i++ )
			{
				strOutput += "[";
				
				Group groope = maGroups[i];
				if( groope == null )
				{
					strOutput += "null";	
				}
				else
				{
					for( int j = 0; j < groope.miSize; j++ )
					{
						if( j > 0 ) { strOutput += ", "; }
						
						strOutput += groope.maPanelIDs[j];
					}
				}
				strOutput += "]";
			}
		}		
		return strOutput;
	}
}





              
