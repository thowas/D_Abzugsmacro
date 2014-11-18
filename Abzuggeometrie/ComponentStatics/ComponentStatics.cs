//==============================================================================
//
//        Filename: ComponentStatics.cs
//
//        Created by: CENIT AG (Thomas Wassel)
//              Version: NX 9.0.3.4
//              Date: 30-10-2014  (Format: mm-dd-yyyy)
//              Time: 08:30 (Format: hh-mm)
//
//==============================================================================

using System;
using System.Collections.Generic;
using NXOpen;
using NXOpen.Features;
using NXOpen.Assemblies;
using NXOpen.Positioning;

namespace Daimler.NX.BoolGeometrie
{
   
   /// <summary>Component statics class </summary>
   public static class ComponentStatics
   {

	   public static List<Component> getTarget = null;
	   public static List<Body> getInput = null;

	   #region static Methode
	   //========================================================================================
	  //================== Start GetRootComponent ===============================================
	  //========================================================================================
	  /// <summary>Get root component of a component.</summary>
	  /// <param name="component">Component to get root component for.</param>
	  /// <returns>Root component of component.</returns>
	  public static Component GetRootComponent(Component component)
	  {
		 Component rootComponent = null;

		 // for the fathers (beginning at the passed component)
		 Component currentFather = component;
		 while (null != currentFather)
		 {
			// remember current father as root component
			rootComponent = currentFather;

			// next father is current parent
			currentFather = GetParentComponent(currentFather);
		 }

		 return rootComponent;
	  }


	  /// <summary>Get parent component of a component.</summary>
	  /// <param name="component">Component to get parent component for.</param>
	  /// <returns>Parent component of component.</returns>
	  public static Component GetParentComponent(Component component)
	  {
		 // get parent of component
		 Component parentComponent = component.Parent;

		 return parentComponent;
	  }
	  //========================================================================================
	  //================== Ende GetRootComponent ===============================================
	  //========================================================================================



	  //=====================================================================================
	  //=================== Liste der Ziel-Parts ============================================
	  //=====================================================================================

	  /// <summary>Get all selected component to modify.</summary>
	  /// <param name="inputList">The List of component.</param>
	  /// </param>
	  /// <returns>The List of the component.</returns>
	  public static List<Component> GetTargetComponent(List<Component> targetList)
	  {
		  // List der Parts die bearbeitete werden.
		 getTarget = new List<Component>(targetList);
					
		  return getTarget;
	  }

	  //========================================================================================
	  //================== Ende GetTargetComponent =============================================
	  //========================================================================================
	   

	  //======================================================================================
	  //=================== Liste der Abzugs_Bodies ==========================================
	  //======================================================================================

	  /// <summary>Get all selected component to copy.</summary>
	  /// <param name="inputList">The List of component.</param>
	  /// </param>
	  /// <returns>The List of the component.</returns>
	  public static List<Body> GetInputComponent(List<Body> inputList)
	  {
		  getInput = new List<Body>(inputList);

		  return getInput;
	  }
	  //========================================================================================
	  //================== Ende GetInputComponent ==============================================
	  //========================================================================================


	  //========================================================================================
	  //================== Selektiertes Part "InWork" setzen ===================================
	  //========================================================================================
	  /// <summary>Get selected component to InWork Status.</summary>
	  /// <param name="i">Counter of component.</param>
	  /// </param>
	  /// <returns>selected component.</returns>
	  public static Component SetPartWork(int i,Session theSession)
	  {
		  Part workPart = theSession.Parts.Work;
		  Part displayPart = theSession.Parts.Display;
		  workPart = theSession.Parts.Work;

		  NXOpen.Assemblies.Component component = (NXOpen.Assemblies.Component)getTarget[i];
		  PartLoadStatus partLoadStatus;
		  theSession.Parts.SetWorkComponent(component, out partLoadStatus);

		  workPart = theSession.Parts.Work;

		 
		  return component;
	  }
	  //========================================================================================
	  //================== Ende SetPartWork ==================================================
	  //========================================================================================

	  //========================================================================================
	  //================== Feature_Groupe "_External_References" aktiv/inaktiv setzen ==========
	  //========================================================================================
	  /// <summary>Get selected component to InWork Status.</summary>
	  /// <param name="TF">Status aktiv/deaktiv.</param>
	  /// <param name="theSession">current NX Document.</param>
	  /// <param name="feature_1">Name des ReferenzSet für Remove Geometrie.</param>
	  /// <param name="feature_2">Name des ReferenzSet für BoolOperation .</param>
	  /// </param>
	  /// <returns>void</returns>
	  public static void SetGroupActiv(bool TF, Session theSession,string feature_1,string feature_2)
	  {
		  
		  Part workPart = theSession.Parts.Work;
		  Part displayPart = theSession.Parts.Display;
		  workPart = theSession.Parts.Work;

		  NXOpen.Session.UndoMarkId markId;
		  markId = theSession.SetUndoMark(NXOpen.Session.MarkVisibility.Visible, "Set Active Group Feature");

		  //NXOpen.Features.FeatureGroup featureGroup = (NXOpen.Features.FeatureGroup)workPart.Features  .FindObject("FEATURE_SET(8)");
		  Feature[] featureGroup = new Feature[1];          
		  featureGroup = theSession.Parts.Work.Features.GetFeatures();


		  foreach ( Feature myNXGroup in featureGroup)
		  {
			  myNXGroup.GetFeatureName();

			  if (myNXGroup.Name.ToString() == feature_1 || myNXGroup.Name.ToString() == feature_2)
			  {
				  myNXGroup.SetGroupActive(TF);
			  }
			 
		  }
						   
	  }
	   //========================================================================================
	   //================== Ende SetGroupActiv ==================================================
	   //========================================================================================

   }
	   #endregion
}
