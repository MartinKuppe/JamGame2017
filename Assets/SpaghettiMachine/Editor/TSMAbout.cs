using UnityEditor;
using UnityEngine;
using System.Collections;
using Spaghetti;


public class TSMAbout : EditorWindow  
{
	private Texture mImage;
	private GUISkin mGUISkin;
	
	[MenuItem ("The Spaghetti Machine/Help/About...", false, 6)]
	//------------------------------------------------------------------------------------------------
	// Init
	// called when clicking on the menu entry
	//------------------------------------------------------------------------------------------------
	static void Init() 
	{
		TSMAbout window = EditorWindow.GetWindow( typeof( TSMAbout ), true, "About The Spaghetti Machine", true ) as TSMAbout;	
		window.mImage = Resources.Load( "SMAbout" ) as Texture;
		window.minSize = new Vector2( window.mImage.width, window.mImage.height );
		window.maxSize = new Vector2( window.mImage.width, window.mImage.height );
		window.mGUISkin  = EditorGUIUtility.Load("SpaghettiMachine/WoodSkin/WoodSkin.guiskin") as GUISkin;
	}
	
	void OnGUI() 
	{	
		GUILayout.Label(mImage);
		GUI.color = Color.black;
		GUI.Label( new Rect( 50, 370, 100, 100 ), SpaghettiMachineEditor.mstrVersion, mGUISkin.FindStyle("AboutVersion" ) );
	}
}
