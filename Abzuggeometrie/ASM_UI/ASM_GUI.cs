//==============================================================================
//
//        Filename: ASM_GUI.cs
//
//        Created by: CENIT AG (Thomas Wassel)
//              Version: NX NX 9.0.3.4
//              Date: 30-10-2014  (Format: mm-dd-yyyy)
//              Time: 08:30 (Format: hh-mm)
//
//==============================================================================

//------------------------------------------------------------------------------
//These imports are needed for the following template code
//------------------------------------------------------------------------------
using System;
using NXOpen;
using NXOpen.BlockStyler;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using NXOpen.Assemblies;
using NXOpen.UF;
using Assembly = System.Reflection.Assembly;
using NXOpen.Features;

//------------------------------------------------------------------------------
//Represents Block Styler application class
//------------------------------------------------------------------------------

namespace Daimler.NX.BoolGeometrie
{
	public class ASM_GUI

	{
	private const string RniPartType = "RNI_PARTTYPE";

	//class members
	public static Session theSession = null;

	public static NXObject nxobject = null;

	public static UI theUI = null;
	private string theDlxFileName;
	private string ownDllLocation;

	private NXOpen.BlockStyler.BlockDialog theDialog;
	private NXOpen.BlockStyler.Group ASEGUI;// Block type: Group
	private NXOpen.BlockStyler.Group InputBodies;// Block type: Group
	private NXOpen.BlockStyler.SelectObject selectionBody;// Block type: Selection
	private NXOpen.BlockStyler.Group TargetPart;// Block type: Group
	private NXOpen.BlockStyler.SelectPartFromList selectPart;// Block type: Select Part



	//------------------------------------------------------------------------------
	//Bit Option for Property: SnapPointTypesEnabled
	//------------------------------------------------------------------------------
	public static readonly int              SnapPointTypesEnabled_UserDefined = (1 << 0);
	public static readonly int                 SnapPointTypesEnabled_Inferred = (1 << 1);
	public static readonly int           SnapPointTypesEnabled_ScreenPosition = (1 << 2);
	public static readonly int                 SnapPointTypesEnabled_EndPoint = (1 << 3);
	public static readonly int                 SnapPointTypesEnabled_MidPoint = (1 << 4);
	public static readonly int             SnapPointTypesEnabled_ControlPoint = (1 << 5);
	public static readonly int             SnapPointTypesEnabled_Intersection = (1 << 6);
	public static readonly int                SnapPointTypesEnabled_ArcCenter = (1 << 7);
	public static readonly int            SnapPointTypesEnabled_QuadrantPoint = (1 << 8);
	public static readonly int            SnapPointTypesEnabled_ExistingPoint = (1 << 9);
	public static readonly int             SnapPointTypesEnabled_PointonCurve = (1 <<10);
	public static readonly int           SnapPointTypesEnabled_PointonSurface = (1 <<11);
	public static readonly int         SnapPointTypesEnabled_PointConstructor = (1 <<12);
	public static readonly int     SnapPointTypesEnabled_TwocurveIntersection = (1 <<13);
	public static readonly int             SnapPointTypesEnabled_TangentPoint = (1 <<14);
	public static readonly int                    SnapPointTypesEnabled_Poles = (1 <<15);
	public static readonly int         SnapPointTypesEnabled_BoundedGridPoint = (1 <<16);
	//------------------------------------------------------------------------------
	//Bit Option for Property: SnapPointTypesOnByDefault
	//------------------------------------------------------------------------------
	public static readonly int             SnapPointTypesOnByDefault_EndPoint = (1 << 3);
	public static readonly int             SnapPointTypesOnByDefault_MidPoint = (1 << 4);
	public static readonly int         SnapPointTypesOnByDefault_ControlPoint = (1 << 5);
	public static readonly int         SnapPointTypesOnByDefault_Intersection = (1 << 6);
	public static readonly int            SnapPointTypesOnByDefault_ArcCenter = (1 << 7);
	public static readonly int        SnapPointTypesOnByDefault_QuadrantPoint = (1 << 8);
	public static readonly int        SnapPointTypesOnByDefault_ExistingPoint = (1 << 9);
	public static readonly int         SnapPointTypesOnByDefault_PointonCurve = (1 <<10);
	public static readonly int       SnapPointTypesOnByDefault_PointonSurface = (1 <<11);
	public static readonly int     SnapPointTypesOnByDefault_PointConstructor = (1 <<12);
	public static readonly int     SnapPointTypesOnByDefault_BoundedGridPoint = (1 <<16);
	
	//------------------------------------------------------------------------------
	//Constructor for NX Styler class
	//------------------------------------------------------------------------------
  
	public ASM_GUI(Session session)
	{
		try
		{
			// Get session
			theSession = session;
			Part workPart = theSession.Parts.Work;
			Part displayPart = theSession.Parts.Display;
						
			// Get the ui
			theUI = UI.GetUI();
			// Get executing assembly
			Assembly assembly = Assembly.GetExecutingAssembly();
			// save own dll location
			ownDllLocation = Path.GetDirectoryName(assembly.Location);

			// Get manifest resource names
			string[] names = assembly.GetManifestResourceNames();

			// Get dlx stream 
			Stream dlxStream = assembly.GetManifestResourceStream(names[6]);

			// Get temp path + structureCreation.dlx
			string fileFullPath = Path.GetTempPath() + "ASE_GUI.dlx";

			// store this stream  to file --> temporary and this will immediately destroyed in dialogShown_cb
			if (null != dlxStream)
			{
				// Create a FileStream object to write a stream to a file
				using (FileStream fileStream = File.Create(fileFullPath, (int)dlxStream.Length))
				{
					// Fill the bytes[] array with the stream data
					byte[] bytesInStream = new byte[dlxStream.Length];
					dlxStream.Read(bytesInStream, 0, (int)bytesInStream.Length);

					// Use FileStream object to write to the specified file
					fileStream.Write(bytesInStream, 0, bytesInStream.Length);
				}
			}

			theDlxFileName = fileFullPath;
			
			//theDlxFileName = @"C:\Users\wassel\Documents\Visual Studio 2013\Projects\Abzuggeometrie\Abzuggeometrie\Resources\ASE_GUI.dlx";
			theDialog = theUI.CreateDialog(theDlxFileName);
			theDialog.AddApplyHandler(new NXOpen.BlockStyler.BlockDialog.Apply(apply_cb));
			theDialog.AddOkHandler(new NXOpen.BlockStyler.BlockDialog.Ok(ok_cb));
			theDialog.AddUpdateHandler(new NXOpen.BlockStyler.BlockDialog.Update(update_cb));
			theDialog.AddCancelHandler(new NXOpen.BlockStyler.BlockDialog.Cancel(cancel_cb));
			theDialog.AddInitializeHandler(new NXOpen.BlockStyler.BlockDialog.Initialize(initialize_cb));
			theDialog.AddFocusNotifyHandler(new NXOpen.BlockStyler.BlockDialog.FocusNotify(focusNotify_cb));
			theDialog.AddKeyboardFocusNotifyHandler(new NXOpen.BlockStyler.BlockDialog.KeyboardFocusNotify(keyboardFocusNotify_cb));
			theDialog.AddEnableOKButtonHandler(new NXOpen.BlockStyler.BlockDialog.EnableOKButton(enableOKButton_cb));
			theDialog.AddDialogShownHandler(new NXOpen.BlockStyler.BlockDialog.DialogShown(dialogShown_cb));
		}
		catch (Exception ex)
		{
			//---- Enter your exception handling code here -----
			throw ex;
		}
	}
	
	//------------------------------------------------------------------------------
	//This method shows the dialog on the screen
	//------------------------------------------------------------------------------
	public NXOpen.UIStyler.DialogResponse Show()
	{
		try
		{
			theDialog.Show();
		}
		catch (Exception ex)
		{
			//---- Enter your exception handling code here -----
			theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}
		return 0;
	}
	
	//------------------------------------------------------------------------------
	//Method Name: Dispose
	//------------------------------------------------------------------------------
	public void Dispose()
	{
		if(theDialog != null)
		{
			theDialog.Dispose();
			theDialog = null;
		}
	}
		
	//------------------------------------------------------------------------------
	//---------------------Block UI Styler Callback Functions--------------------------
	//------------------------------------------------------------------------------
	
	//------------------------------------------------------------------------------
	//Callback Name: initialize_cb
	//------------------------------------------------------------------------------
	public void initialize_cb()
	{
		try
		{
			ASEGUI = (NXOpen.BlockStyler.Group)theDialog.TopBlock.FindBlock("ASEGUI");
			InputBodies = (NXOpen.BlockStyler.Group)theDialog.TopBlock.FindBlock("InputBodies");
			selectionBody = (NXOpen.BlockStyler.SelectObject)theDialog.TopBlock.FindBlock("selectionBody");
			TargetPart = (NXOpen.BlockStyler.Group)theDialog.TopBlock.FindBlock("TargetPart");
			selectPart = (NXOpen.BlockStyler.SelectPartFromList)theDialog.TopBlock.FindBlock("selectPart");
			
		}
		catch (Exception ex)
		{
			//---- Enter your exception handling code here -----
			theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}
	}
	
	//------------------------------------------------------------------------------
	//Callback Name: dialogShown_cb
	//This callback is executed just before the dialog launch. Thus any value set 
	//here will take precedence and dialog will be launched showing that value. 
	//------------------------------------------------------------------------------
	public void dialogShown_cb()
	{
		try
		{
			// set empty value
			//File.Delete(theDlxFileName);
			// set maximum scope to "Entire Assembly" for comoonent selection (export)
			//selectionBody.MaximumScopeAsString = "Entire Assembly";
			
		}
		catch (Exception ex)
		{
			//---- Enter your exception handling code here -----
			theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}
	}
	
	//------------------------------------------------------------------------------
	//Callback Name: apply_cb
	//------------------------------------------------------------------------------
	public int apply_cb()
	{
		int errorCode = 0;
		
		try
		{
			// Aufruf der Start Methode
			myStartMethode();
		}
		catch (Exception ex)
		{
			errorCode = 1;
			theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error,ex.ToString());
		}
		
		return errorCode;
	}
	
	//------------------------------------------------------------------------------
	//Callback Name: update_cb
	//------------------------------------------------------------------------------
	public int update_cb( NXOpen.BlockStyler.UIBlock block)
	{
		try
		{
			if(block == selectionBody)
			{
				// UI Selection (Abzugskörper) in Liste speichern
				List<Body> selectionInput = new List<Body>();
				int e = 0;
				if (selectionBody.GetSelectedObjects().Count() != 0)
				{
					for (e = 0; e <= selectionBody.GetSelectedObjects().Count() -1; e++)
					selectionInput.Add((Body)theUI.SelectionManager.GetSelectedTaggedObject(e));
				}
					ComponentStatics.GetInputComponent(selectionInput);
			}
			else if(block == selectPart)
			{
				// UI Selection (TargetPart) in Liste speichern
				List<Component> selectionTarget = new List<Component>();
				int i = 0;

				if (selectPart.GetSelectedObjects().Count() != 0)
				{
					for (i = 0; i <= selectPart.GetSelectedObjects().Count() -1; i++)
					selectionTarget.Add((NXOpen.Assemblies.Component)theUI.SelectionManager.GetSelectedTaggedObject(i));
				}
					ComponentStatics.GetTargetComponent(selectionTarget);
				}
					   
		}
		catch (Exception ex)
		{
			  theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}
		return 0;
	}

	 
	//------------------------------------------------------------------------------
	//Callback Name: ok_cb
	//------------------------------------------------------------------------------
	public int ok_cb()
	{
		int errorCode = 0;
		try
		{
			// Aufruf der MainMethode
			myStartMethode();
		}
		catch (Exception ex)
		{
			
			errorCode = 1;
			theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}
		return errorCode;
	}
	
	//------------------------------------------------------------------------------
	//Callback Name: cancel_cb
	//------------------------------------------------------------------------------
	public int cancel_cb()
	{
		try
		{
			
		}
		catch (Exception ex)
		{
			
			theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}
		return 0;
	}
	
	//------------------------------------------------------------------------------
	//Callback Name: focusNotify_cb
	//This callback is executed when any block (except the ones which receive keyboard entry such as Integer block) receives focus.
	//------------------------------------------------------------------------------
	public void focusNotify_cb(NXOpen.BlockStyler.UIBlock block, bool focus)
	{
		try
		{
		   
		}
		catch (Exception ex)
		{
			
			theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}
	}
	
	//------------------------------------------------------------------------------
	//Callback Name: keyboardFocusNotify_cb
	//This callback is executed when block which can receive keyboard entry, receives the focus.
	//------------------------------------------------------------------------------
	public void keyboardFocusNotify_cb(NXOpen.BlockStyler.UIBlock block, bool focus)
	{
		try
		{
		   
		}
		catch (Exception ex)
		{
			
			theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}
	}
	
	//------------------------------------------------------------------------------
	//Callback Name: enableOKButton_cb
	//This callback allows the dialog to enable/disable the OK and Apply button.
	//------------------------------------------------------------------------------
	public bool enableOKButton_cb()
	{
		bool enableOkButton = true;
		try
		{
			
			
		}
		catch (Exception ex)
		{
			
			theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}
		return enableOkButton; 
	}
	
	//------------------------------------------------------------------------------
	//Function Name: GetBlockProperties
	//Returns the propertylist of the specified BlockID
	//------------------------------------------------------------------------------
	public PropertyList GetBlockProperties(string blockID)
	{
		PropertyList plist =null;
		try
		{
			plist = theDialog.GetBlockProperties(blockID);
		}
		catch (Exception ex)
		{
			
			theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
		}
		return plist;
	}

	#region


	//=================================================================================
	//==================StartmyMain  Methode===========================================
	//Methode zum Abrufen der Selektion_List Aktivieren/Deaktivieren der Zielparts; 
	//kopieren mit Link ; Boolean Operation !
	//==================================================================================

	/// <summary>startet weitere Methoden in chronologischer Folge .</summary>
	/// <example>SetPartWork 
	/// legt Status "aktiv" auf das Part.<c>SetPartWork </c>
	/// SetGroupActiv
	/// legt Status "aktiv" auf das Referenz Seti m Part.<c>SetGroupActiv</c>
	/// SetWaveLink 
	/// kopiert das Solid der Abzugskörper in das ZielPart.<c>SetWaveLink</c>
	/// SetUnite
	/// generiert die Volumenoperation "Subtract".<c>SetUnite</c>
	/// </example>
	/// <returns>void.</returns>
	public static void myStartMethode()
	{
		int i = 0;
		int e = 0;
		try
		{
			
			Part workPart = theSession.Parts.Work;
			Part displayPart = theSession.Parts.Display;


			if ( ComponentStatics.getInput != null && ComponentStatics.getTarget != null)
			{
				foreach (Component TargetPart in ComponentStatics.getTarget)
				{
					ComponentStatics.SetPartWork(e, theSession);
					ComponentStatics.SetGroupActiv(true, theSession, "_External_References", "_Externe_Referenzen");

					i = 0;
					foreach (Body linkBody in ComponentStatics.getInput)
					{
						SetWaveLink(i);
						ComponentStatics.SetGroupActiv(false, theSession, "_External_References", "_Externe_Referenzen");
						ComponentStatics.SetGroupActiv(true, theSession, "_Single_Part_mech_Machining", "_ET_mech_Bearbeitung");
						SetUnite(i, e);
						ComponentStatics.SetGroupActiv(false, theSession, "_Single_Part_mech_Machining", "_ET_mech_Bearbeitung");
						ComponentStatics.SetGroupActiv(true, theSession, "_External_References", "_Externe_Referenzen");
						i++;
					}
					ComponentStatics.SetGroupActiv(false, theSession, "_Single_Part_mech_Machining", "_ET_mech_Bearbeitung");
					ComponentStatics.SetGroupActiv(false, theSession, "_External_References", "_Externe_Referenzen");

					e++;
				}

			}
			else
			{
				string msg = "Kein Ziel_Part oder Link Body ausgewählt.";
				theUI.NXMessageBox.Show("Note", NXMessageBox.DialogType.Information, msg);
			
			}

			NXOpen.Assemblies.Component nullAssemblies_Component = null;
			PartLoadStatus partLoadStatus2;
			theSession.Parts.SetWorkComponent(nullAssemblies_Component, out partLoadStatus2);


		}

		catch (Exception)
		{
			string message = "Select Object: Input Bodies  " + ComponentStatics.getInput.Count() + 
				"Select Object: Target Parts  \n" + ComponentStatics.getTarget.Count();
			theUI.NXMessageBox.Show("Auswahl NULL", NXMessageBox.DialogType.Warning, message);

		}

	}

	//===================================================================================================
	//===================================Ende StatmyMain=================================================
	//===================================================================================================

		
	//===================================================================================================
	//================== Boolean_Operation mit "WaveLink" Bodies ===================================
	//===================================================================================================

	/// <summary>Get selected Bodies greate a WaveLink to Target_Part .</summary>
	/// <param name="i">Counter of Remove Body.</param>
	/// <param name="e">Counter of Target component.</param>
	/// </param>
	/// <returns>void.</returns>
	public static void SetUnite(int i, int e)
	{

		Part workPart = theSession.Parts.Work;
		Part displayPart = theSession.Parts.Display;
		workPart = theSession.Parts.Work;
		//int targetObjects = ComponentStatics.getTarget.Count;

		// ----------------------------------------------
		//   Menu: Insert->Combine->Subtract...
		// ----------------------------------------------
		NXOpen.Session.UndoMarkId markId3;
		markId3 = theSession.SetUndoMark(NXOpen.Session.MarkVisibility.Visible, "Start");

		NXOpen.Features.BooleanFeature nullFeatures_BooleanFeature = null;

		if (!workPart.Preferences.Modeling.GetHistoryMode())
		{
			throw new Exception("Create or edit of a Feature was recorded in History Mode but playback is in History-Free Mode.");
		}

		NXOpen.Features.BooleanBuilder booleanBuilder1;
		booleanBuilder1 = workPart.Features.CreateBooleanBuilderUsingCollector(nullFeatures_BooleanFeature);

		ScCollector scCollector1;
		scCollector1 = booleanBuilder1.ToolBodyCollector;

		NXOpen.GeometricUtilities.BooleanRegionSelect booleanRegionSelect1;
		booleanRegionSelect1 = booleanBuilder1.BooleanRegionSelect;
		booleanBuilder1.Tolerance = 0.001;
		booleanBuilder1.Operation = NXOpen.Features.Feature.BooleanType.Subtract;
		theSession.SetUndoMarkName(markId3, "Subtract Dialog");


		// string der ObjektReferenz ableiten (Ziel Part´s)
		string str11 = "COMPONENT " + ComponentStatics.getTarget[e].OwningComponent.Parent.DisplayName + " 1";
		string str22 = "COMPONENT " + ComponentStatics.getTarget[e].OwningComponent.DisplayName + " 1";
		string str33 = "COMPONENT " + ComponentStatics.getTarget[e].DisplayName + " 1";

		//Übergabe der ObjektReferenz (Ziel Part´s)
		NXOpen.Assemblies.Component component11 = (NXOpen.Assemblies.Component)displayPart.ComponentAssembly.RootComponent.FindObject(str11);
		NXOpen.Assemblies.Component component22 = (NXOpen.Assemblies.Component)component11.FindObject(str22);
		NXOpen.Assemblies.Component component33 = (NXOpen.Assemblies.Component)component22.FindObject(str33);


		Feature[] featureTargetSet = new Feature[1];
		Feature[] featureTarget = new Feature[1];
		featureTargetSet = theSession.Parts.Work.Features.GetFeatures();
		string str_TargetSet = "";
		//aus der aktuellen Objektreferenz das Solid Feature ermitteln.(Ziel Part´s)
		foreach (Feature myNXSet in featureTargetSet)
		{
			if (myNXSet.FeatureType == "EXTRUDE")
			{
				str_TargetSet = myNXSet.JournalIdentifier;
			}
		}
		
		Body bodyTarget = (Body)workPart.Bodies.FindObject(str_TargetSet);
		bool added1;
		added1 = booleanBuilder1.Targets.Add(bodyTarget);
		TaggedObject[] targets1 = new TaggedObject[1];
		targets1[0] = bodyTarget;
		booleanRegionSelect1.AssignTargets(targets1);
		
		ScCollector scCollector2;
		scCollector2 = workPart.ScCollectors.CreateCollector();
		
		Feature[] featureBool = new Feature[1];
		featureBool = theSession.Parts.Work.Features.GetFeatures();
		string str_bool = "";
		//aus der aktuellen Objektreferenz das Solid Feature ermitteln.("WaveLink" Body)
		foreach (Feature myNXGroup in featureBool)
		{
			myNXGroup.GetFeatureName();

			if (myNXGroup.Name.ToString() == "_OUT_ABZUGSKOERPER_" + i.ToString())
			{
				str_bool = myNXGroup.JournalIdentifier;
			}
		}

		if (ComponentStatics.getInput[i].IsOccurrence == true)
		{

			string str1 = "";
			string str2 = "";
			string str3 = "";
			string str4 = "";
			string str5 = "";
			int check = 0;

			NXOpen.Assemblies.Component component1, component2, component3, component4;

			//	string der ObjektReferenz ableiten (Ziel Part´s)
			//	Übergabe der ObjektReferenz (Ziel Part´s)
			if (check == 0 && null != ComponentStatics.getInput[i].OwningComponent && ComponentStatics.getInput[i].OwningComponent.DisplayName != displayPart.JournalIdentifier)
			{
				str1 = ComponentStatics.getInput[i].Prototype.JournalIdentifier;
			}
			else if (check == 0)
			{
				string message = "Keine Body Selection vorhanden. \nCheck and try again.";
				UI.GetUI().NXMessageBox.Show("Input Selection", NXMessageBox.DialogType.Information, message);
			}
			if (check == 0 && null != ComponentStatics.getInput[i].OwningComponent.OwningComponent && ComponentStatics.getInput[i].OwningComponent.OwningComponent.DisplayName != displayPart.JournalIdentifier)
			{
				str2 = "COMPONENT " + ComponentStatics.getInput[i].OwningComponent.DisplayName + " 1";
			}
			else if (check == 0)
			{
				str2 = "COMPONENT " + ComponentStatics.getInput[i].OwningComponent.DisplayName + " 1";
				component1 = (NXOpen.Assemblies.Component)displayPart.ComponentAssembly.RootComponent.FindObject(str2);
				Body body = (Body)component1.FindObject("PROTO#.Bodies|" + str1);
				check = 1;
			}
			if (check == 0 && null != ComponentStatics.getInput[i].OwningComponent.OwningComponent.OwningComponent && ComponentStatics.getInput[i].OwningComponent.OwningComponent.OwningComponent.DisplayName != displayPart.JournalIdentifier)
			{
				str3 = "COMPONENT " + ComponentStatics.getInput[i].OwningComponent.Parent.DisplayName + " 1";
			}
			else if (check == 0)
			{
				str3 = "COMPONENT " + ComponentStatics.getInput[i].OwningComponent.Parent.DisplayName + " 1";
				component1 = (NXOpen.Assemblies.Component)displayPart.ComponentAssembly.RootComponent.FindObject(str3);
				component2 = (NXOpen.Assemblies.Component)component1.FindObject(str2);
				Body body = (Body)component2.FindObject("PROTO#.Bodies|" + str1);
				check = 1;
			}
			if (check == 0 && null != ComponentStatics.getInput[i].OwningComponent.OwningComponent.OwningComponent.OwningComponent && ComponentStatics.getInput[i].OwningComponent.OwningComponent.OwningComponent.OwningComponent.DisplayName != displayPart.JournalIdentifier)
			{
				str4 = "COMPONENT " + ComponentStatics.getInput[i].OwningComponent.Parent.Parent.DisplayName + " 1";
			}
			else if (check == 0)
			{
				str4 = "COMPONENT " + ComponentStatics.getInput[i].OwningComponent.Parent.Parent.DisplayName + " 1";
				component1 = (NXOpen.Assemblies.Component)displayPart.ComponentAssembly.RootComponent.FindObject(str4);
				component2 = (NXOpen.Assemblies.Component)component1.FindObject(str3);
				component3 = (NXOpen.Assemblies.Component)component2.FindObject(str2);
				Body body = (Body)component3.FindObject("PROTO#.Bodies|" + str1);
				check = 1;
			}

			if (check == 0 && null != ComponentStatics.getInput[i].OwningComponent.OwningComponent.OwningComponent.OwningComponent.OwningComponent && ComponentStatics.getInput[i].OwningComponent.OwningComponent.OwningComponent.OwningComponent.OwningComponent.DisplayName != displayPart.JournalIdentifier)
			{
				str5 = "COMPONENT " + ComponentStatics.getInput[i].OwningComponent.Parent.Parent.Parent.DisplayName + " 1";
				component1 = (NXOpen.Assemblies.Component)displayPart.ComponentAssembly.RootComponent.FindObject(str5);
				component2 = (NXOpen.Assemblies.Component)component1.FindObject(str4);
				component3 = (NXOpen.Assemblies.Component)component2.FindObject(str3);
				component4 = (NXOpen.Assemblies.Component)component2.FindObject(str2);
				Body body = (Body)component4.FindObject("PROTO#.Bodies|" + str1);
				check = 1;
			}

			try
			{
				//Übergabe der Solid Objekte für die Volumenoperation
				NXOpen.Features.Feature[] LinkBodyfeatur = new NXOpen.Features.Feature[1];
				LinkBodyfeatur[0] = (NXOpen.Features.ExtractFace)theSession.Parts.Work.Features.FindObject(str_bool);

				BodyFeatureRule bodyFeatureRule1;
				bodyFeatureRule1 = workPart.ScRuleFactory.CreateRuleBodyFeature(LinkBodyfeatur, false, component33);

				SelectionIntentRule[] rules1 = new SelectionIntentRule[1];
				rules1[0] = bodyFeatureRule1;
				scCollector2.ReplaceRules(rules1, false);

				booleanBuilder1.ToolBodyCollector = scCollector2;

				TaggedObject[] targets2 = new TaggedObject[1];
				targets2[0] = bodyTarget;
				booleanRegionSelect1.AssignTargets(targets2);

				//generieren der Subtract Operation und Anpassen des Objekt Namen.
				NXObject nxobjectBool;
				nxobjectBool = booleanBuilder1.Commit();
				nxobjectBool.SetName("Subtract_OUT_ABZUGSKOERPER_" + i.ToString());
				booleanBuilder1.Destroy();
				theSession.CleanUpFacetedFacesAndEdges();
			}
			catch (Exception)
			{

				string message = "Der Abzugskörper durchdringt das Ziel Part nicht: \n" ;
				theUI.NXMessageBox.Show("Subtract Operation Error", NXMessageBox.DialogType.Warning, message);
				
			}
		}

	}
	//===================================================================================================
	//===================================Ende SetUnite =================================================
	//===================================================================================================

	//===================================================================================================
	//================== InputBody kopieren mit Link "WaveLink" =========================================
	//===================================================================================================

	/// <summary>Get selected Bodies greate a WaveLink to Target_Part .</summary>
	/// <param name="i">Counter of component.</param>
	/// </param>
	/// <returns>void.</returns>
	public static void SetWaveLink(int i)
	{

		Part workPart = theSession.Parts.Work;
		Part displayPart = theSession.Parts.Display;

		NXOpen.Session.UndoMarkId markId;
		markId = theSession.SetUndoMark(NXOpen.Session.MarkVisibility.Visible, "Start");

		NXOpen.Features.Feature nullFeatures_Feature = null;

		if (!workPart.Preferences.Modeling.GetHistoryMode())
		{
			throw new Exception("Create or edit of a Feature was recorded in History Mode but playback is in History-Free Mode.");
		}
				
		NXOpen.Features.WaveLinkBuilder waveLinkBuilder = workPart.BaseFeatures.CreateWaveLinkBuilder(nullFeatures_Feature);
		NXOpen.Features.WaveDatumBuilder waveDatumBuilder = waveLinkBuilder.WaveDatumBuilder;
		NXOpen.Features.CompositeCurveBuilder compositeCurveBuilder = waveLinkBuilder.CompositeCurveBuilder;
		NXOpen.Features.WaveSketchBuilder waveSketchBuilder = waveLinkBuilder.WaveSketchBuilder;
		NXOpen.Features.WaveRoutingBuilder waveRoutingBuilder = waveLinkBuilder.WaveRoutingBuilder;
		NXOpen.Features.WavePointBuilder wavePointBuilder = waveLinkBuilder.WavePointBuilder;
		NXOpen.Features.ExtractFaceBuilder extractFaceBuilder = waveLinkBuilder.ExtractFaceBuilder;
		NXOpen.Features.MirrorBodyBuilder mirrorBodyBuilder = waveLinkBuilder.MirrorBodyBuilder;
		extractFaceBuilder.FaceOption = NXOpen.Features.ExtractFaceBuilder.FaceOptionType.FaceChain;
		waveLinkBuilder.Type = NXOpen.Features.WaveLinkBuilder.Types.BodyLink;
		extractFaceBuilder.FaceOption = NXOpen.Features.ExtractFaceBuilder.FaceOptionType.FaceChain;
		extractFaceBuilder.AngleTolerance = 45.0;
		waveDatumBuilder.DisplayScale = 2.0;
		extractFaceBuilder.ParentPart = NXOpen.Features.ExtractFaceBuilder.ParentPartType.OtherPart;
		mirrorBodyBuilder.ParentPartType = NXOpen.Features.MirrorBodyBuilder.ParentPart.OtherPart;
		theSession.SetUndoMarkName(markId, "WAVE Geometry Linker Dialog");
		compositeCurveBuilder.Section.DistanceTolerance = 0.001;
		compositeCurveBuilder.Section.ChainingTolerance = 0.00095;
		extractFaceBuilder.Associative = true;
		extractFaceBuilder.MakePositionIndependent = false;
		extractFaceBuilder.FixAtCurrentTimestamp = false;
		extractFaceBuilder.HideOriginal = false;
		extractFaceBuilder.InheritDisplayProperties = false;
				
		ScCollector scCollector = extractFaceBuilder.ExtractBodyCollector;
		extractFaceBuilder.CopyThreads = true;
		extractFaceBuilder.FeatureOption = NXOpen.Features.ExtractFaceBuilder.FeatureOptionType.OneFeatureForAllBodies;

					 
		if (ComponentStatics.getInput[i].IsOccurrence == true)
		{
			
			string str1 = "";  
			string str2 = "";
			string str3 = ""; 
			string str4 = "";
			string str5 = "";
			
			int check = 0;

			NXOpen.Assemblies.Component component1, component2, component3, component4; 


			if (check == 0 && null != ComponentStatics.getInput[i].OwningComponent && ComponentStatics.getInput[i].OwningComponent.DisplayName != displayPart.JournalIdentifier)
			{
				str1 = ComponentStatics.getInput[i].Prototype.JournalIdentifier;
			}
			else if (check == 0)
			{
				string message = "Keine Body Selection vorhanden. \nCheck and try again.";
				UI.GetUI().NXMessageBox.Show("Input Selection", NXMessageBox.DialogType.Information, message);
			}
			if (check == 0 && null != ComponentStatics.getInput[i].OwningComponent.OwningComponent && ComponentStatics.getInput[i].OwningComponent.OwningComponent.DisplayName != displayPart.JournalIdentifier)
			{
				str2 = "COMPONENT " + ComponentStatics.getInput[i].OwningComponent.DisplayName + " 1";
			}
			else if (check == 0)
			{
				str2 = "COMPONENT " + ComponentStatics.getInput[i].OwningComponent.DisplayName + " 1";
				component1 = (NXOpen.Assemblies.Component)displayPart.ComponentAssembly.RootComponent.FindObject(str2);
				Body body = (Body)component1.FindObject("PROTO#.Bodies|" + str1);
				check = 1;
			}
			if (check == 0 && null != ComponentStatics.getInput[i].OwningComponent.OwningComponent.OwningComponent && ComponentStatics.getInput[i].OwningComponent.OwningComponent.OwningComponent.DisplayName != displayPart.JournalIdentifier)
			{
				str3 = "COMPONENT " + ComponentStatics.getInput[i].OwningComponent.Parent.DisplayName + " 1";
			}
			else if (check == 0)
			{
				str3 = "COMPONENT " + ComponentStatics.getInput[i].OwningComponent.Parent.DisplayName + " 1";
				component1 = (NXOpen.Assemblies.Component)displayPart.ComponentAssembly.RootComponent.FindObject(str3);
				component2 = (NXOpen.Assemblies.Component)component1.FindObject(str2);
				Body body = (Body)component2.FindObject("PROTO#.Bodies|" + str1);
				check = 1;
			}
			if (check == 0 && null != ComponentStatics.getInput[i].OwningComponent.OwningComponent.OwningComponent.OwningComponent && ComponentStatics.getInput[i].OwningComponent.OwningComponent.OwningComponent.OwningComponent.DisplayName != displayPart.JournalIdentifier)
			{
				str4 = "COMPONENT " + ComponentStatics.getInput[i].OwningComponent.Parent.Parent.DisplayName + " 1";
			}
			else if (check == 0)
			{
				str4 = "COMPONENT " + ComponentStatics.getInput[i].OwningComponent.Parent.Parent.DisplayName + " 1";
				component1 = (NXOpen.Assemblies.Component)displayPart.ComponentAssembly.RootComponent.FindObject(str4);
				component2 = (NXOpen.Assemblies.Component)component1.FindObject(str3);
				component3 = (NXOpen.Assemblies.Component)component2.FindObject(str2);
				Body body = (Body)component3.FindObject("PROTO#.Bodies|" + str1);
				check = 1;
			}

			if (check ==0 && null != ComponentStatics.getInput[i].OwningComponent.OwningComponent.OwningComponent.OwningComponent.OwningComponent && ComponentStatics.getInput[i].OwningComponent.OwningComponent.OwningComponent.OwningComponent.OwningComponent.DisplayName != displayPart.JournalIdentifier)
			{
				str5 = "COMPONENT " + ComponentStatics.getInput[i].OwningComponent.Parent.Parent.Parent.DisplayName + " 1";
				component1 = (NXOpen.Assemblies.Component)displayPart.ComponentAssembly.RootComponent.FindObject(str5);
				component2 = (NXOpen.Assemblies.Component)component1.FindObject(str4);
				component3 = (NXOpen.Assemblies.Component)component2.FindObject(str3);
				component4 = (NXOpen.Assemblies.Component)component2.FindObject(str2);
				Body body = (Body)component4.FindObject("PROTO#.Bodies|" + str1);
				check = 1;
			}
			
			Body[] bodies = new Body[1];
			bodies[0] = ComponentStatics.getInput[i];

			BodyDumbRule bodyDumbRule = workPart.ScRuleFactory.CreateRuleBodyDumb(bodies, true);

			SelectionIntentRule[] rules1 = new SelectionIntentRule[1];
			rules1[0] = bodyDumbRule;
			scCollector.ReplaceRules(rules1, false);

			NXObject nxobject_1;
			nxobject_1 = waveLinkBuilder.Commit();
			nxobject_1.SetName("_OUT_ABZUGSKOERPER_" + i.ToString());

			waveLinkBuilder.Destroy();

			theSession.CleanUpFacetedFacesAndEdges();
											
		}
		
	}
		//===================================================================================================
		//===================================Ende WaveLink =================================================
		//===================================================================================================
#endregion
	}
	   
}

