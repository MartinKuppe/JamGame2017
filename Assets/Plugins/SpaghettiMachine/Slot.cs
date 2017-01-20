using System; 
using System.Xml; 
using UnityEngine;
using System.Collections;

namespace Spaghetti
{
	public enum PlugType { None, Input, Output, InOut };
	public enum ContentType { None, String, Text, Float, Int, Bool, Master, GameObject, More, Enum, Vector3, Curve, Color };
	

	public class Signal
	{
		public Panel mSender;
		public Panel mReceiver;
		public Slot  mSendingSlot;
		public string mstrMessage;
		
		public Signal( Panel sender, Panel receiver, Slot sendingslot, string strMessage )
		{
			mSender = sender;
			mReceiver = receiver;
			mstrMessage = strMessage;
			mSendingSlot = sendingslot;
		}
	}
	
	
	
	public class Slot
	{
		public Panel mParent;
		public string mstrLabel  = "<label>";
		public PlugType mPlugType = PlugType.None;
		public ContentType mContentType = ContentType.None;	
		public Color mColor = Color.white;
		private ArrayList maConnectedSlots;	
		public int miIndex;
	
		public string mstrDataText = "";
		public float mfDataFloat   = 0.0f;
		public int miDataInt   = 0;
		public GameObject mDataGameObject = null;	
		public Vector3    mvDataVector = Vector3.zero;
		public Color	mDataColor = Color.white;
		public AnimationCurve mDataCurve = null;
		
		public bool mbMultipleAllowed = true;
		public int miUniqueID = -1;
		
		private ArrayList maComesFromIDs;	
		private ArrayList maGoesToIDs;	
		
		private string mstrColorName;
		
		private SpaghettiMachineCore mSpaghettiMachine;
		private GameObject mMaster;	
		public bool mbMore = false;
		
		private float mfPotential = 0.0f;
		
		private int miSetPotentialRecursionCounter = 0; 
		
		private Hashtable mCustomVariables;
		private string mstrVariablesDisplayString = "";
		
		public Vector2 mvPlugPosition;
		public Vector2 mvNormedPlugPosition;
		public bool mbCustomPosition = false;	
		public Vector2 mvCustomPlugDirection = Vector2.zero;		

		private static float SPARKLE_TIME = 0.2f;
		
		private float mfSparklingTimer = 0.0f;
		private bool mbSparklingYellow = true;
		
		public string[] maEnumValues;
		
		// ---------------------- Initialization ----------------------------------------------------
		
		public Slot( Panel panel, 
		            int iIndex, 
		            string strLabel, 
		            string strPlugType, 
		            string strContentType, 
		            string strColor, 
		            string strMultiple, 
		            string strDefault,
		            string strEnumValues,
		            string strNormedPlugX,
	                string strNormedPlugY )
		{
			mParent = panel;
			mstrLabel = strLabel;
			mstrColorName = strColor;
			mSpaghettiMachine = mParent.GetSpaghettiMachine();
			
			if( strPlugType == null )
			{
				strPlugType = "none";	
			}			
			if( strContentType == null )
			{
				strContentType = "none";	
			}
			
			switch( strPlugType.ToLower() )
			{
			case "none" :
				mPlugType = PlugType.None; 
				mvPlugPosition = new Vector2( mParent.mWindowRect.width - 4.0f, Panel.HEADER_HEIGHT + iIndex * Panel.SLOT_HEIGHT - 3.0f );	// Only used to determine y position of slot
				break;			
				
			case "input" : 
				mPlugType = PlugType.Input;
				mvPlugPosition = new Vector2( 3.0f, Panel.HEADER_HEIGHT + iIndex * Panel.SLOT_HEIGHT - 3.0f );
				break;
				
			case "output" : 
				mPlugType = PlugType.Output; 	
				mvPlugPosition = new Vector2( mParent.mWindowRect.width - 4.0f, Panel.HEADER_HEIGHT + iIndex * Panel.SLOT_HEIGHT - 3.0f );
				break;
				
			case "inout" : 
				mPlugType = PlugType.InOut; 	
				mvPlugPosition = new Vector2( mParent.mWindowRect.width - 4.0f, Panel.HEADER_HEIGHT + iIndex * Panel.SLOT_HEIGHT - 3.0f );
				break;	
				
			default :
				Debug.LogWarning("Unknown plug type "+strPlugType );
				break;
			}

			//Debug.Log("content = "+ strContentType);
			
			switch( strContentType.ToLower() )
			{
			case "none" : 
				mContentType = ContentType.None; 		
				break;
				
			case "string" : 
				mContentType = ContentType.String; 		
				break;
				
			case "text"  :
				mContentType = ContentType.Text; 
				break;	
	
			case "float" : 
				mContentType = ContentType.Float; 
				break;	
				
			case "int"  :
				mContentType = ContentType.Int; 
				break;	
				
				
			case "bool" : 
				mContentType = ContentType.Bool; 
				break;	
				
			case "master" : 
				mContentType = ContentType.Master;				
				break;					

			case "gameobject" : 
				mContentType = ContentType.GameObject;				
				break;
				
			case "more"  :
				mContentType = ContentType.More; 
				break;				
				
			case "enum"  :
				mContentType = ContentType.Enum; 
				strEnumValues = strEnumValues.Replace( " ", "" );
				maEnumValues = strEnumValues.Split( new Char[] {','} );				
				break;
				
			case "vector3" : 				
				mContentType = ContentType.Vector3;				
				break;
				
			case "color" : 				
				mContentType = ContentType.Color;				
				break;				
				
			case "curve" : 
				mContentType = ContentType.Curve;
				mDataCurve = AnimationCurve.Linear( 0.0f, 0.0f, 1.0f, 1.0f );
				break;								
				
			default :
				Debug.LogWarning("Unknown content type "+strContentType );
				break;
			}		
			
			mbMultipleAllowed = ( strMultiple != null && strMultiple.ToLower() == "true" );
			

			if( strColor != null )
			{
				switch( strColor.ToLower() )
				{
				case "white" : 	mColor = Color.white; 						break;
				case "red"  :	mColor = Color.red; 						break;
				case "green"  :	mColor = Color.green; 						break;
				case "blue"  :	mColor = Color.blue; 						break;	
				case "yellow" : mColor = Color.yellow; 						break;
				case "cyan"  :	mColor = Color.cyan; 						break;
				case "grey"  :	mColor = Color.grey; 						break;	
				case "black"  :	mColor = Color.black; 						break;
				case "magenta": mColor = Color.magenta; 					break;
				case "orange" : mColor = new Color(0.96f, 0.58f, 0.11f); 	break;	
				case "brown"  :	mColor = new Color(0.72f, 0.46f, 0.18f); 	break;					
				}
			}
			
			if( strNormedPlugX != null )
			{
				float fBaseX = (float)Panel.WINDOW_MARGE_LEFT; 
				float fWidth = mParent.mWindowRect.width - (float)Panel.WINDOW_MARGE_LEFT - Panel.WINDOW_MARGE_RIGHT - 1;
				float fLambda = (float)System.Convert.ToDouble( strNormedPlugX );
				
				//Debug.Log("mvNormedPlugPosition.x = "+mvNormedPlugPosition.x );
				
				mvPlugPosition.x =  Mathf.Round( fBaseX +  fLambda * fWidth ); 
				if( mvPlugPosition.x < 5.0f +Panel.WINDOW_MARGE_LEFT )
				{
					mvCustomPlugDirection += new Vector2( -1.0f, 0.0f ); 
				}
				else if( mvPlugPosition.x > mParent.mWindowRect.width - 5.0f - Panel.WINDOW_MARGE_RIGHT )
				{
					mvCustomPlugDirection += new Vector2( 1.0f, 0.0f ); 
				}
				mbCustomPosition = true;
			}
			
			if( strNormedPlugY != null )
			{
				float fBaseY = (float)Panel.WINDOW_MARGE_TOP;
				float fHeight = mParent.mWindowRect.height - Panel.WINDOW_MARGE_TOP -  Panel.WINDOW_MARGE_BOTTOM;
				float fLambda = (float)System.Convert.ToDouble( strNormedPlugY );
				
				//Debug.Log("mvNormedPlugPosition.y = "+mvNormedPlugPosition.y );
					
				mvPlugPosition.y = Mathf.Round( fBaseY + fLambda * fHeight );
				if( mvPlugPosition.y < 5.0f + Panel.WINDOW_MARGE_TOP )
				{
					mvCustomPlugDirection += new Vector2( 0.0f, -1.0f ); 
				}
				else if( mvPlugPosition.y > mParent.mWindowRect.height - 5.0f - Panel.WINDOW_MARGE_BOTTOM )
				{
					mvCustomPlugDirection += new Vector2( 0.0f, 1.0f ); 
				}			
				mbCustomPosition = true;
			} 
	
			maConnectedSlots = new ArrayList();
			
			miIndex = iIndex;
		}
		
	
		
		
		public void Read( XmlReader reader, Graph graph )
		{
			// <slot>
			reader.ReadStartElement("slot");
			
			//<ID>24</ID>
			reader.ReadStartElement("ID");
			miUniqueID = reader.ReadContentAsInt();	
			reader.ReadEndElement();		
	
			// Read  connections
			maComesFromIDs = new ArrayList();
			while( reader.NodeType == XmlNodeType.Element && reader.Name == "input" )
			{		
				// <input ID = "23">	
				maComesFromIDs.Add( System.Convert.ToInt32( reader["ID"] ) );
				reader.ReadStartElement("input");	
			}
			
			maGoesToIDs = new ArrayList();		
			while( reader.NodeType == XmlNodeType.Element && reader.Name == "output" )
			{		
				// <output ID = "17">	
				maGoesToIDs.Add( System.Convert.ToInt32( reader["ID"] ) );
				reader.ReadStartElement("output");	
			}		
	
			// <data_string> (optional)
			if( reader.NodeType == XmlNodeType.Element && reader.Name == "data_string" )
			{
				reader.ReadStartElement("data_string");	
				mstrDataText = reader.ReadContentAsString();	
				reader.ReadEndElement();
			}
			
			// <data_float> (optional)
			if( reader.NodeType == XmlNodeType.Element && reader.Name == "data_float" )
			{
				reader.ReadStartElement("data_float");	
				mfDataFloat = reader.ReadContentAsFloat();	
				reader.ReadEndElement();
			}
			
			// <data_int> (optional)	
			if( reader.NodeType == XmlNodeType.Element && reader.Name == "data_int" )
			{
				reader.ReadStartElement("data_int");	
				miDataInt = reader.ReadContentAsInt();	
				reader.ReadEndElement();
			}
				
			// <more/> (optional)	
			if( reader.NodeType == XmlNodeType.Element && reader.Name == "more" )
			{		
				reader.ReadStartElement("more");
				mbMore = ( reader.ReadContentAsString() == "true" );
				reader.ReadEndElement();
			}
			
			// <data_vector> (optional)	
			if( reader.NodeType == XmlNodeType.Element && reader.Name == "data_vector" )
			{
				reader.ReadStartElement("data_vector");	
				mvDataVector = StringToVector( reader.ReadContentAsString() ); 
				reader.ReadEndElement();
			}			
	
			// <data_curve> (optional)	
			if( reader.NodeType == XmlNodeType.Element && reader.Name == "data_curve" )
			{
				reader.ReadStartElement("data_curve");	
				mDataCurve = StringToCurve( reader.ReadContentAsString() ); 
				reader.ReadEndElement();
			}
			
			// <data_color> (optional)	
			if( reader.NodeType == XmlNodeType.Element && reader.Name == "data_color" )
			{
				reader.ReadStartElement("data_color");	
				mDataColor = StringToColor( reader.ReadContentAsString() ); 
				reader.ReadEndElement();
			}				
			
			// </slot>		
			reader.ReadEndElement();	
		}			
		
		public void PostReadInitialization( Graph graph )
		{
			maConnectedSlots = new ArrayList();
			foreach( int ID in maComesFromIDs )
			{
				Slot otherslot   = graph.FindSlotByID( ID );
				maConnectedSlots.Add( otherslot );
			}
			foreach( int ID in maGoesToIDs )
			{
				Slot otherslot = graph.FindSlotByID( ID );
				maConnectedSlots.Add( otherslot );
				mParent.GetSpaghettiMachine().AddLink( this, otherslot );
			}
			
			if( mContentType == ContentType.Master && mstrDataText != "" )
			{
				GameObject gmaster = GameObject.Find( mstrDataText );
				if( gmaster != null )
				{
					mParent.SetMaster( gmaster );
				}
				else
				{
					Debug.LogWarning("Master \""+mstrDataText+"\" not found.");	
				}
			}
			if( mContentType == ContentType.GameObject && mstrDataText != "" )
			{
				mDataGameObject = GameObject.Find( mstrDataText );
				if( mDataGameObject == null )
				{
					Debug.LogWarning("GameObject \""+mstrDataText+"\" not found.");	
				}
			}			
			
		}
		
		public void SetMachine( SpaghettiMachineCore machine )
		{
			mSpaghettiMachine = machine;
			if( mMaster == null )
			{
				mMaster = machine.gameObject;	
			}
		}
		
		public void SetMaster( GameObject master )
		{
			mMaster = master;
		}	
		
		public int GetID()
		{
			return miUniqueID;	
		}
		
		// ---------------------- Acces ----------------------------------------------------
			
		public Panel GetPanel()   
		{	
			return mParent;
		}	
		
		public int GetNumberOfConnectedSlots()   
		{	
			return maConnectedSlots.Count;
		}	
		
		public Slot GetConnectedSlot( int i )   
		{	
			if( i < 0 || i >= maConnectedSlots.Count )
			{
				return null;
			}
			return maConnectedSlots[i] as Slot;
		}		
		
		public Slot GetConnectedSlot()   
		{	
			return GetConnectedSlot( 0 );
		}
		
	
		
		public Slot[] GetConnectedSlots()   
		{	
			return maConnectedSlots.ToArray( typeof(Slot) ) as Slot[];
		}	

		public Panel GetConnectedPanel( int i )   
		{	
			Slot connectedslot = GetConnectedSlot( i );
			return ( connectedslot != null ) ? connectedslot.GetPanel() : null;
		}
		
		public Panel GetConnectedPanel()   
		{	
			return GetConnectedPanel(0);
		}
	
		public string GetLabel()   
		{	
			return mstrLabel;
		}	
		
		public PlugType GetPlugType()   
		{	
			return mPlugType;
		}
		
		public ContentType GetContentType()   
		{	
			return mContentType;
		}		
		
		public string GetColorName()   
		{	
			return mstrColorName;
		}	
		
		public int GetSlotIndex()   
		{	
			return miIndex;
		}	
		
		public Slot Below()   
		{	
			if( miIndex+1 < mParent.GetNumberOfSlots() )
			{
				return mParent.GetSlot( miIndex+1 );
			}
			else
			{
				return null;	
			}
		}	
		
		public Slot Above()   
		{	
			if( miIndex-1 >= 0 )
			{
				return mParent.GetSlot( miIndex-1 );
			}
			else
			{
				return null;	
			}
		}		
		
		public string GetDataString()   
		{	
			return mstrDataText;
		}	
	
		public float GetDataFloat()   
		{	
			return mfDataFloat;
		}	
		
		public int GetDataInt()   
		{	
			return miDataInt;
		}
		
		public bool GetDataBool()   
		{	
			return (miDataInt != 0);
		}

		public GameObject GetDataGameObject()   
		{	
			return mDataGameObject;
		}
		
		public AnimationCurve GetDataCurve()   
		{	
			return mDataCurve;
		}

		public Vector3 GetDataVector3()   
		{	
			return mvDataVector;
		}

		public void SetContentType( ContentType newvalue )   
		{	
			mContentType = newvalue;
		}		

		public void SetDataString( string newvalue )   
		{	
			mstrDataText = newvalue;
		}	
	
		public void SetDataFloat( float newvalue )   
		{	
			mfDataFloat = newvalue;
		}	
		
		public void SetDataInt( int newvalue )   
		{	
			miDataInt = newvalue;
		}
		
		public void SetDataBool( bool newvalue )   
		{	
			miDataInt = newvalue ? 1 : 0;
		}

		public void SetDataGameObject( GameObject newvalue )   
		{	
			mDataGameObject = newvalue;
		}
		
		public void SetDataCurve( AnimationCurve newvalue )   
		{	
			mDataCurve = newvalue;
		}

		public void SetDataVector3( Vector3 newvalue )   
		{	
			mvDataVector = newvalue;
		}
		
		public void SetDataColor( Color newvalue )   
		{	
			mDataColor = newvalue;
		}		
		
		
		public bool IsMultiplug()   
		{	
			return mbMultipleAllowed;
		}	
		
		//================================================================================================
		//
		//   MANAGEMENT OF "ACTIVE" FLAG
		//
		//   For example for state machines or triggers
		//
		//================================================================================================
		
		//------------------------------------------------------------------------------------------------
		// ActivateConnected
		//------------------------------------------------------------------------------------------------			
		public void ActivateConnected()  
		{	
			if( mSpaghettiMachine.mMachineType == MachineType.StateMachine && maConnectedSlots.Count > 1 )
			{
				Debug.LogWarning("ActivateConnected tries to activate multiple panels for a state machine.");	
			}
			
			if( mPlugType == PlugType.Input )
			{
				Debug.LogWarning("ActivateConnected called for input plug, abandonned.");
				return;
			}
			
			foreach( Slot otherslot in maConnectedSlots )
			{
				if( otherslot.mPlugType == PlugType.Output )
				{
					Debug.LogWarning("ActivateConnected tried to send to output plug, ignored.");
				}
				else
				{
					otherslot.mParent.Activate();
				}
			}
			
			Sparkle();
		}
		
		//================================================================================================
		//
		//   SENDING SIGNALS
		//
		//   Signals are strings sent through connections. Think "message". Can have various uses.
		//
		//================================================================================================
		
		//------------------------------------------------------------------------------------------------
		// SendSignalToConnected
		//------------------------------------------------------------------------------------------------			
		public void SendSignalToConnected( string strSignal )  
		{	
			
			if( mPlugType == PlugType.Input )
			{
				Debug.LogWarning("SendSignalToConnected called for input plug, abandonned.");
				return;
			}
			
			foreach( Slot otherslot in maConnectedSlots )
			{
				if( otherslot.mPlugType == PlugType.Output )
				{
					Debug.LogWarning("SendSignalToConnected tried to send signal to output plug, ignored.");
				}
				else
				{
					mMaster.SendMessage( "OnPanelReceivesSignal", new Signal( mParent, otherslot.mParent, this, strSignal), SendMessageOptions.DontRequireReceiver ); 
				}
			}
			
			Sparkle();
		}
		
		//================================================================================================
		//
		//   POTENTIALS
		//
		//   A "potential" is a value continuously transmitted via connections. Think "current".
	
		//   When you change the potential on an output node, this automatically sets the same potential
		//   on all connected input nodes.
		//   Can be used for electrical or logical circuits, neural networks and many other. 	
		//
		//================================================================================================
		
		
		public float GetPotential()
		{
			return mfPotential;
		}
		
		//------------------------------------------------------------------------------------------------
		// SetPotential 
		//------------------------------------------------------------------------------------------------			
		public void SetPotential( float fValue )  
		{
			// Anti-infinite-recursion measure
			if( miSetPotentialRecursionCounter > 10 )
			{
				Debug.LogWarning("SetPotential called recursively on the same slot ten times - abandonning.");
				return;
			}
			
			
			if( mPlugType == PlugType.Input )
			{
				Debug.LogWarning("SetPotential called for input plug, abandonned.");
				return;
			}
			if( mfPotential == fValue )
			{
				// Nothing changes
				return;
			}
			
			miSetPotentialRecursionCounter++;		
			
			mfPotential = fValue;			
			
			foreach( Slot otherslot in maConnectedSlots )
			{
				if( otherslot.mPlugType == PlugType.Output )
				{
					Debug.LogWarning("SetPotential tried to send potential to output plug, ignored.");
				}
				else
				{
					otherslot.mfPotential = mfPotential;
					otherslot.mParent.GetMaster().SendMessage( "OnInputPotentialChanged", otherslot.mParent, SendMessageOptions.DontRequireReceiver ); 	
				}
			}
			
			miSetPotentialRecursionCounter--;
			
			Sparkle();
		}
		
		//================================================================================================
		//
		//   VARIABLES
		//
		//   A variable is a pair name/value which can be attached to the slot at runtime.
		//   Exemple: In a dialog structure with possible loops, one can assign a boolean 
		//   to each answer slot indicating whether the answer has already been given.
		//
		//================================================================================================
		
		
		public object GetVariable( string strName )
		{
			if( mCustomVariables != null && mCustomVariables.Contains( strName ) )
			{
				return mCustomVariables[strName];
			}
			else
			{			
				return null;
			}			
		}	
		
		public void SetVariable( string strName, object oValue )
		{
			if( mCustomVariables == null )
			{
				mCustomVariables = new Hashtable();	
			}
			if( mCustomVariables.Contains( strName ) )
			{
				mCustomVariables[strName] = oValue;
			}
			else
			{
				mCustomVariables.Add( strName, oValue );
			}
			UpdateVariablesOutput();
			mSpaghettiMachine.gameObject.SendMessage("RefreshDebugger");
		}
		
		private void UpdateVariablesOutput()
		{
			mstrVariablesDisplayString = "";
			bool bFirstLine = true;
			foreach(DictionaryEntry entry in mCustomVariables)
			{
				if( !bFirstLine )
				{
					mstrVariablesDisplayString += ", ";	
				}
				mstrVariablesDisplayString += entry.Key.ToString() + " = " + entry.Value.ToString();
			    bFirstLine = false;
			}
		}
		
		public string GetVariablesDisplayString()
		{
			return mstrVariablesDisplayString;
		}
		
		public Hashtable GetVariablesAsHashtable()
		{
			return 	mCustomVariables;
		}
		
		
		public Vector2 GetGlobalPlugPosition() 
		{
			Vector2 vPlugPosition = mvPlugPosition;
			vPlugPosition.y = Mathf.Min( vPlugPosition.y, mParent.mWindowRect.height - 4);
			return new Vector2( mParent.mWindowRect.x,  mParent.mWindowRect.y ) + vPlugPosition;
		}
		
		public Vector2 GetLocalPlugPosition() 
		{
			Vector2 vPlugPosition = mvPlugPosition;
			vPlugPosition.y = Mathf.Min( vPlugPosition.y, mParent.mWindowRect.height - 4);
			return  vPlugPosition;
		}	
	
		public float GetDistanceToWindowTop() 
		{
			return mvPlugPosition.y;
		}
	
		public float GetDistanceToWindowBottom() 
		{
			return mParent.mWindowRect.height - mvPlugPosition.y;
		}	
		
		public Color GetPlugColor()
		{
			/*
			if(  HasOctarinePlug() )
			{
				return SpaghettiMachineEditor.GetInstance().GetOctarine();
			}
			else if(  HasColourOutOfSpacePlug() )
			{
				return SpaghettiMachineEditor.GetInstance().GetColourOutOfSpace();
			}
			else  
			{
				return mColor;
			}
			*/
			return mColor;
		}
		
		
		private void Sparkle()
		{
			mfSparklingTimer = SPARKLE_TIME;
			mSpaghettiMachine.RefreshDebugger();
		}
		
		public void UpdateDebugger()
		{
			if( mfSparklingTimer > 0.0f )
			{
				mfSparklingTimer -= Time.deltaTime;
				mSpaghettiMachine.gameObject.SendMessage("RefreshDebugger");
			}
		}	
		
		public bool IsSparkling()
		{
			return( mfSparklingTimer > 0.0f );
		}		
		
		public Color GetSparklingColor()
		{
			mbSparklingYellow = !mbSparklingYellow;
			return mbSparklingYellow ? Color.yellow : Color.black;
		}
		
		public bool HasOctarinePlug()
		{
			return ( mstrColorName == "octarine" );
		}
		
		public bool HasColourOutOfSpacePlug()
		{
			return ( mstrColorName == "thecolouroutofspace" );
		}
		
		private Vector3 StringToVector( string strInput )
		{
			if( strInput == "" || strInput == null )
			{
				return Vector3.zero;
			}
			
			char[] aSeparator = { "|"[0] };
			
			string[] aSubStrings = strInput.Split( aSeparator );
			
			if( aSubStrings.Length == 3 )
			{
				return new Vector3( Convert.ToSingle( aSubStrings[0] ), Convert.ToSingle( aSubStrings[1] ), Convert.ToSingle( aSubStrings[2] ) );
			}
			else
			{
				Debug.LogError( "Unable to convert string \""+strInput+"\" to Vector3.\nString must consist of three floats separated by \"|\", e.g. \"3.92|2.03|7.99\"" );
				return Vector3.zero;
			}
		}	
		
		private Color StringToColor( string strInput )
		{
			if( strInput == "" || strInput == null )
			{
				return Color.white;
			}
			
			char[] aSeparator = { "|"[0] };
			
			string[] aSubStrings = strInput.Split( aSeparator );
			
			if( aSubStrings.Length == 4 )
			{
				return new Color( Convert.ToSingle( aSubStrings[0] ), Convert.ToSingle( aSubStrings[1] ), Convert.ToSingle( aSubStrings[2] ), Convert.ToSingle( aSubStrings[3] ) );
			}
			else if( aSubStrings.Length == 3 )
			{
				return new Color( Convert.ToSingle( aSubStrings[0] ), Convert.ToSingle( aSubStrings[1] ), Convert.ToSingle( aSubStrings[2] ) );
			}
			else
			{
				Debug.LogError( "Unable to convert string \""+strInput+"\" to Color.\nString must consist of three or four floats separated by \"|\", e.g. \"0.3|0.8|1.0\"" );
				return Color.cyan;
			}
		}
		
		private AnimationCurve StringToCurve( string strInput )
		{
			AnimationCurve curve = new AnimationCurve();
		
			char[] aSeparator1 = { "|"[0] };
			char[] aSeparator2 = { ":"[0] };
			
			string[] aSubStrings = strInput.Split( aSeparator1 );
			
			// Get pre-wrap mode
			switch( aSubStrings[0] )
			{
			case "clamp":
				curve.preWrapMode = WrapMode.Clamp;
				break;
				
			case "clampforever":
				curve.postWrapMode = WrapMode.ClampForever;
				break;
				
			case "loop":
				curve.preWrapMode = WrapMode.Loop;
				break;
				
			case "pingpong":
				curve.preWrapMode = WrapMode.PingPong;
				break;			
				
			default:
				Debug.LogError("Unhandled wrap mode "+aSubStrings[0] );
				curve.preWrapMode = WrapMode.Clamp;
				break;			
			}
			
			for( int i = 1; i < aSubStrings.Length-1; i++ )
			{
				string[] aSubSubStrings	= aSubStrings[i].Split( aSeparator2 );
				
				float fTime = Convert.ToSingle( aSubSubStrings[0] );
				float fValue = Convert.ToSingle( aSubSubStrings[1] ); 
				float fInTangent = Convert.ToSingle( aSubSubStrings[2] ); 
				float fOutTangent = Convert.ToSingle( aSubSubStrings[3] ); 					
				Keyframe key = new Keyframe( fTime, fValue, fInTangent, fOutTangent );
				
				curve.AddKey( key );
			}
			
			
			// Get post-wrap mode		
			switch( aSubStrings[ aSubStrings.Length-1 ] )
			{
			case "clamp":
				curve.postWrapMode = WrapMode.Clamp;
				break;
			
			case "clampforever":
				curve.postWrapMode = WrapMode.ClampForever;
				break;
			
				
			case "loop":
				curve.postWrapMode = WrapMode.Loop;
				break;
				
			case "pingpong":
				curve.postWrapMode = WrapMode.PingPong;
				break;			
				
			default:
				Debug.LogError("Unhandled wrap mode "+aSubStrings[ aSubStrings.Length-1 ] );
				curve.postWrapMode = WrapMode.Clamp;
				break;			
			}	
			
			return curve;
		}
			
	}
	

	
}
		
	
