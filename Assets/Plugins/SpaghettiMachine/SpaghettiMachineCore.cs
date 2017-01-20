using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace Spaghetti
{
	public enum MachineType { StateMachine, MultistateMachine, TriggerMachine };
	

	public class SpaghettiMachineCore : MonoBehaviour 
	{	
		public MachineType mMachineType = MachineType.StateMachine;
				
		[HideInInspector]
		public bool mbDebuggerIsRunning = false;
		
		[HideInInspector]
		public bool showActivePanels = true;
		[HideInInspector]
		public bool showCustomVariables = true;
		[HideInInspector]
		public bool showPotentials = false;	
		
		protected Graph mGraph;
		protected Panel[] maPanels;

		private Panel mLastActivatedPanel;
		
		protected ArrayList maLinks;	
		
		
		private ISpaghettiDebugger mDebugger = null;
		private Panel[]   maActivePannelsCache = null;
		
		//================================================================================================
		//
		//   LINKS
		//
		//================================================================================================		
		
		public void AddLink( Slot slot1, Slot slot2 )
		{
			Link newLink = new Link( slot1, slot2 ); 		
			maLinks.Add( newLink );
		}
		
		public ArrayList GetLinks()
		{
			return maLinks;
		}
		

		
		//================================================================================================
		//
		//   INTERNAL METHODS
		//
		//================================================================================================
				
		//------------------------------------------------------------------------------------------------
		// GetLastActivatedPanel
		//------------------------------------------------------------------------------------------------	
		public Panel GetLastActivatedPanel()  
		{
			return mLastActivatedPanel;
		}
		
		//------------------------------------------------------------------------------------------------
		// SetLastActivatedPanel
		//------------------------------------------------------------------------------------------------	
		public void SetLastActivatedPanel( Panel panel  )  
		{
			mLastActivatedPanel = panel;
		}

		//------------------------------------------------------------------------------------------------
		// OnActivePanelsChanged
		//------------------------------------------------------------------------------------------------	
		
		private void OnActivePanelsChanged()
		{
			maActivePannelsCache = null;	
		}
	
		//================================================================================================
		//
		//   ACCESS TO PANELS
		//
		//================================================================================================		
	
		//------------------------------------------------------------------------------------------------
		// GetNumberOfPanels
		//------------------------------------------------------------------------------------------------		
		public int GetNumberOfPanels()  
		{	
			return (maPanels != null ) ? maPanels.Length : -1;
		}	
		
		//------------------------------------------------------------------------------------------------
		// GetPanel
		//------------------------------------------------------------------------------------------------		
		public Panel GetPanel( int i )  
		{	
			if( i < 0 || i >= maPanels.Length )
			{
				return null;
			}
			return maPanels[i] as Panel;
		}
		
		//------------------------------------------------------------------------------------------------
		// GetPanels
		//------------------------------------------------------------------------------------------------		
		public Panel[] GetPanels()  
		{	
			return maPanels;
		}	
	
		//------------------------------------------------------------------------------------------------
		// FindPanelByType
		//------------------------------------------------------------------------------------------------		
		public Panel FindPanelByType( string strType )  
		{	
			foreach( Panel panel in maPanels )
			{
				if( panel.GetPanelType() == strType )
				{
					return panel;
				}
			}
			return null;
		}

		//------------------------------------------------------------------------------------------------
		// FindPanelsByType
		//------------------------------------------------------------------------------------------------		
		public Panel[] FindPanelsByType( string strType )  
		{	
			ArrayList aPanels = new ArrayList();
			foreach( Panel panel in maPanels )
			{
				if( panel.GetPanelType() == strType )
				{
					aPanels.Add( panel );
				}
			}
			return aPanels.ToArray( typeof( Panel ) ) as Panel[];
		}
		
		//------------------------------------------------------------------------------------------------
		// SetPanels
		//------------------------------------------------------------------------------------------------		
		public void SetPanels( ArrayList panels )  
		{				
			maPanels = panels.ToArray( typeof( Panel ) ) as Panel[];
			RefreshDebugger();
		}			
		
		//------------------------------------------------------------------------------------------------
		// SetDebugger
		//------------------------------------------------------------------------------------------------		
		public void SetDebugger( ISpaghettiDebugger debugger )  
		{	
			mDebugger = debugger;
		}	
		
		//------------------------------------------------------------------------------------------------
		// GetDebugger
		//------------------------------------------------------------------------------------------------		
		public ISpaghettiDebugger GetDebugger()  
		{	
			return mDebugger;
		}		
		
		//================================================================================================
		//
		//   ACTIVATION MECHANISM
		//
		//================================================================================================
		
		//------------------------------------------------------------------------------------------------
		// GetActivePanel
		//------------------------------------------------------------------------------------------------	

		public Panel GetActivePanel()  
		{
			if( mMachineType != MachineType.StateMachine )
			{
				Debug.LogWarning("GetActivePanel called for non-StateMachine");
			}
			return mLastActivatedPanel;
		}	
		
		//------------------------------------------------------------------------------------------------
		// GetActivePanel
		//------------------------------------------------------------------------------------------------	

		public Panel[] GetActivePanels()  
		{
			if( maActivePannelsCache == null )
			{
				ArrayList aActivePanels = new ArrayList();
				foreach( Panel panel in maPanels )
				{
					if( panel.IsActive() )
					{
						aActivePanels.Add( panel );
					}
				}
				maActivePannelsCache = aActivePanels.ToArray( typeof( Panel ) ) as Panel[];
			}
				
			return maActivePannelsCache;
		}		
	
		//================================================================================================
		//
		//   DEBUGGER
		//
		//================================================================================================
		
		//------------------------------------------------------------------------------------------------
		// RefreshDebugger
		//------------------------------------------------------------------------------------------------	

		public void RefreshDebugger()  
		{	
			if( mDebugger != null )
			{
				mDebugger.Refresh();	
			}
		}
	}
}
	
