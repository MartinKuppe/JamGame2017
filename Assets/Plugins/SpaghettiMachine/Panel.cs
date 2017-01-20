using UnityEngine;
using System.Xml; 
using System.Collections;

namespace Spaghetti
{
	public class Panel
	{
		public Rect mWindowRect;
		
		private string mstrType = "EditorPanel"; 
		public Slot[] maSlots;	
		private Slot[] maInputSlots;
		private Slot[] maOutputSlots;		
		public int miUniqueID;
		
		private bool mbActive = false;
		private SpaghettiMachineCore mSpaghettiMachine;
		private GameObject mMaster;
		
		public Texture2D mImage;
		
		public static int WINDOW_MARGE_TOP = 20;
		public static int WINDOW_MARGE_LEFT = 3;
		public static int WINDOW_MARGE_RIGHT = 3;	
		public static int WINDOW_MARGE_BOTTOM = 3;
		public static float HEADER_HEIGHT = 31.0f;	
		public static float SLOT_HEIGHT = 17.0f;		
	
		private Hashtable mCustomVariables;
		private string mstrVariablesDisplayString = "";
	
		

		
		// ---------------------- Initialization ----------------------------------------------------
		
		public Panel( SpaghettiMachineCore machine )
		{
			mSpaghettiMachine = machine;
			mMaster = null;
		}
		
		public void Read( XmlReader reader, Graph graph )
		{
			// <panel>
			mstrType = reader["type"];			
				
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
			int iSlots   = System.Convert.ToInt32( reader["number"] );		
			reader.ReadStartElement("slots");	
			int iIndex = 0;
			maSlots = new Slot[iSlots];
			ArrayList aInputSlots = new ArrayList();
			ArrayList aOutputSlots = new ArrayList();
			
			// Read slots
			while( reader.NodeType == XmlNodeType.Element && reader.Name == "slot" )
			{	
				Slot newSlot   = new Slot( this, iIndex, reader["label"], reader["plug"], reader["content"], reader["color"], reader["multiple"], reader["default"], reader["enum"], reader["plug_x"], reader["plug_y"] );
				newSlot.Read( reader, graph );
				maSlots[iIndex] = newSlot;
				iIndex++;
				switch( newSlot.GetPlugType() )
				{
				case PlugType.None:
					//NOP
					break;
					
				case PlugType.Input:
					aInputSlots.Add( newSlot );
					break;
				
				case PlugType.Output:
					aOutputSlots.Add( newSlot );
					break;
				
				case PlugType.InOut:
					aInputSlots.Add( newSlot );
					aOutputSlots.Add( newSlot );
					break;
				}
			}
			maInputSlots = aInputSlots.ToArray( typeof( Slot ) ) as Slot[];
			maOutputSlots = aOutputSlots.ToArray( typeof( Slot ) ) as Slot[];
			
			// </slots>		
			reader.ReadEndElement();	
			
			// </panel>		
			reader.ReadEndElement();	
		}
		
		
		public void PostReadInitialization( Graph graph   )
		{
			foreach( Slot slot in maSlots )
			{
				slot.PostReadInitialization( graph );
			}
			if( mMaster == null )
			{
				SetMaster( mSpaghettiMachine.gameObject );
			}
		}
		

		public SpaghettiMachineCore GetSpaghettiMachine()
		{
			return mSpaghettiMachine;	
		}
		
		public void SetMaster( GameObject master )
		{
			mMaster = master;
			foreach( Slot slot in maSlots )
			{
				slot.SetMaster( master );
			}

		}
	
		private void SetActive( bool bActive  )
		{	
			if( mbActive == bActive )
			{
				return;
			}
			mbActive = bActive;		
			switch( mSpaghettiMachine.mMachineType )
			{
			case MachineType.StateMachine :
				if( bActive )
				{
					// State machines can only have zero or one active panels
					if( mSpaghettiMachine.GetLastActivatedPanel() != null )
					{
						mSpaghettiMachine.GetLastActivatedPanel().Deactivate();
					}
					mMaster.SendMessage( "OnPanelActivated", this, SendMessageOptions.DontRequireReceiver );
					mSpaghettiMachine.SetLastActivatedPanel( this );
					
				}
				else
				{
					mMaster.gameObject.SendMessage( "OnPanelDeactivated", this, SendMessageOptions.DontRequireReceiver );
				}				
				break;
				
			case MachineType.MultistateMachine :
				if( bActive )
				{
					// Multistate machines can have multiple active panels
					mMaster.SendMessage( "OnPanelActivated", this, SendMessageOptions.DontRequireReceiver );
					mSpaghettiMachine.SetLastActivatedPanel( this );
				}
				else
				{
					mMaster.gameObject.SendMessage( "OnPanelDeactivated", this, SendMessageOptions.DontRequireReceiver );
				}				
				break;				
				
			case MachineType.TriggerMachine :
				if( bActive )
				{
					// In trigger machines, panels are activated only for a short moment. 
					// This moment is already over :
					mbActive = false;
					
					// The only purpose of activation is this message :
					mMaster.SendMessage( "OnPanelActivated", this, SendMessageOptions.DontRequireReceiver );
					mSpaghettiMachine.SetLastActivatedPanel( this );
				}
				else
				{
					Debug.LogWarning("Trying to deactivate panel in trigger machine, this has no sense.");
				}				
				break;
			}
			
			mSpaghettiMachine.SendMessage( "OnActivePanelsChanged" );
			mSpaghettiMachine.RefreshDebugger();
		}
		
		public void UpdateDebugger()
		{
			foreach( Slot slot in maSlots )
			{
				slot.UpdateDebugger();
			}
		}
		
		
		
		//================================================================================================
		//
		//   ACCESS TO TYPE, SLOTS ETC.
		//
		//================================================================================================
		
		//------------------------------------------------------------------------------------------------
		// GetType
		//------------------------------------------------------------------------------------------------		
		public string GetPanelType()  
		{
			return mstrType;
		}
		//------------------------------------------------------------------------------------------------
		// GetSlots
		//------------------------------------------------------------------------------------------------			
		public Slot[] GetSlots()
		{	
			return maSlots;
		}
		
		//------------------------------------------------------------------------------------------------
		// GetInputSlots
		//------------------------------------------------------------------------------------------------			
		public Slot[] GetInputSlots()
		{	
			return maInputSlots;
		}
		
		//------------------------------------------------------------------------------------------------
		// GetOutputSlots
		//------------------------------------------------------------------------------------------------			
		public Slot[] GetOutputSlots()
		{	
			return maOutputSlots;
		}		
				
		//------------------------------------------------------------------------------------------------
		// GetNumberOfSlots
		//------------------------------------------------------------------------------------------------			
		public int GetNumberOfSlots()
		{	
			return maSlots.Length;
		}
		
		//------------------------------------------------------------------------------------------------
		// GetNumberOfInputSlots
		//------------------------------------------------------------------------------------------------			
		public int GetNumberOfInputSlots()
		{	
			return maInputSlots.Length;
		}
		
		//------------------------------------------------------------------------------------------------
		// GetNumberOfOutputSlots
		//------------------------------------------------------------------------------------------------			
		public int GetNumberOfOutputSlots()
		{	
			return maOutputSlots.Length;
		}		
	
		//------------------------------------------------------------------------------------------------
		// GetSlot
		//------------------------------------------------------------------------------------------------	
		public Slot GetSlot( int i )  
		{	
			if( i < 0 || i >= maSlots.Length )
			{
				Debug.LogWarning("GetSlot: Index "+i+" out of range, panel has actually "+maSlots.Length+"slots. Returned null." );				
				return null;
			}
			return maSlots[i];
		}
		

		//------------------------------------------------------------------------------------------------
		// FindSlot
		//------------------------------------------------------------------------------------------------		
		public Slot FindSlot( string strLabel )  
		{	
			foreach( Slot slot in maSlots )
			{
				if( slot.GetLabel() == strLabel )
				{
					return slot;
				}
			}
			Debug.LogWarning("Panel "+mstrType+" Couldn't find slot "+strLabel );
			return null;
		}
		
		//------------------------------------------------------------------------------------------------
		// FindInputSlot
		//------------------------------------------------------------------------------------------------		
		public Slot FindInputSlot( string strLabel )  
		{	
			foreach( Slot slot in maInputSlots )
			{
				if( slot.GetLabel() == strLabel )
				{
					return slot;
				}
			}
			Debug.LogWarning("Panel "+mstrType+" Couldn't find input slot "+strLabel );
			return null;
		}
		
		//------------------------------------------------------------------------------------------------
		// FindOutputSlot
		//------------------------------------------------------------------------------------------------		
		public Slot FindOutputSlot( string strLabel )  
		{	
			foreach( Slot slot in maOutputSlots )
			{
				if( slot.GetLabel() == strLabel )
				{
					return slot;
				}
			}
			Debug.LogWarning("Panel "+mstrType+" Couldn't find output slot "+strLabel );
			return null;
		}
	
		//------------------------------------------------------------------------------------------------
		// FindOutputSlot
		//------------------------------------------------------------------------------------------------		
		public Slot FindSlotByID( int ID )  
		{	
			foreach( Slot slot in maSlots )
			{
				if( slot.GetID() == ID )
				{
					return slot;
				}
			}
			return null;
		}
		
		//------------------------------------------------------------------------------------------------
		// GetMaster
		//------------------------------------------------------------------------------------------------		
		public GameObject GetMaster()  
		{
			return mMaster;
		}
			
		
		//================================================================================================
		//
		//   ACCESS TO "ACTIVE" FLAG
		//
		//   For example for state machines.
		//
		//================================================================================================
		
		//------------------------------------------------------------------------------------------------
		// Activate
		// If the machine's flag "mbMultiplePanelsCanBeActive" isn't set, this will automatically
		// deactivate whatever other panel may be active.
		//------------------------------------------------------------------------------------------------			
		public void Activate()
		{
			SetActive( true );	
		}
		
		//------------------------------------------------------------------------------------------------
		// Deactivate
		//------------------------------------------------------------------------------------------------		
		public void Deactivate()
		{
			SetActive( false );	
		}
	
		//------------------------------------------------------------------------------------------------
		// IsActive
		//------------------------------------------------------------------------------------------------		
		public bool IsActive() 
		{	
			return mbActive;
		}
		
		//================================================================================================
		//
		//   INPUT POTENTIALS
		//
		//   For example for logic gates
		//
		//================================================================================================	
			                        
		//------------------------------------------------------------------------------------------------
		// GetInputPotentials
		//------------------------------------------------------------------------------------------------		
		public float[] GetInputPotentials() 
		{	
			float[] aArray = new float[ maInputSlots.Length ];
			for( int i = 0; i < maInputSlots.Length; i++ )
			{
				aArray[i] = maInputSlots[i].GetPotential();
			}
			return aArray;	
		}
		//------------------------------------------------------------------------------------------------
		// GetSumOfInputPotentials
		//------------------------------------------------------------------------------------------------		
		public float GetSumOfInputPotentials() 
		{	
			float fSum = 0.0f;
			foreach( Slot slot in maInputSlots )
			{
				fSum += slot.GetPotential();
			}
			return fSum;	
		}
		
		//================================================================================================
		//
		//   VARIABLES
		//
		//   A variable is a pair name/value which can be attached to the panel at runtime.
		//   Exemple: An integer counting how many times a panel has been activated
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
	
	}
}
