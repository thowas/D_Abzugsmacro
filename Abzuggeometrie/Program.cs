//==============================================================================
//
//        Filename: Program.cs
//
//        Created by: CENIT AG (Thomas Wassel)
//              Version: NX NX 9.0.3.4
//              Date: 30-10-2014  (Format: mm-dd-yyyy)
//              Time: 08:30 (Format: hh-mm)
//
//==============================================================================

using System;
using NXOpen;

namespace Daimler.NX.BoolGeometrie
{
    /// <summary>class Program</summary>
    public class Program
    {
        /// <summary>Main method </summary>
        /// <returns></returns>
        public static void Main()
        {
            ASM_GUI theASM_gui = null;
          
            try
            {
               
                Session theSession = Session.GetSession();
                Part workPart = theSession.Parts.Work;
                
                if (null != workPart)
                {
                    
                    theASM_gui = new ASM_GUI(theSession);
                    theASM_gui.Show();
                }
                else
                {
                    UI.GetUI().NXMessageBox.Show("Main", NXMessageBox.DialogType.Information, "Current work part is empty. Please create/open one and try again.");
                }
            }
            catch (Exception ex)
            {
                UI.GetUI().NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
            }
            finally
            {
                if (theASM_gui != null)
                    theASM_gui.Dispose();
                    theASM_gui = null;
            }

        }

        public static int GetUnloadOption(string arg)
        {
            //return System.Convert.ToInt32(Session.LibraryUnloadOption.Explicitly);
            return System.Convert.ToInt32(Session.LibraryUnloadOption.Immediately);
            //return System.Convert.ToInt32(Session.LibraryUnloadOption.AtTermination);
        }

        public static void UnloadLibrary(string arg)
        {
            try
            {
            }
            catch (Exception ex)
            {
                UI.GetUI().NXMessageBox.Show("Main Function", NXMessageBox.DialogType.Error, ex.ToString());
            }
        }

    }
}
