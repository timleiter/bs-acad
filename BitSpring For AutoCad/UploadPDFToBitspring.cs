using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Publishing;

namespace BitSpring_For_AutoCad
{
    public class UploadPDFToBitspring
    {
        private String mPdfString;

        public void UploadToBitSpring()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed;
            if (doc != null)
            {
                ed = doc.Editor;
                ed.WriteMessage("\nConverting the document to PDF");
            }

            //Convert the current active document to PDF
            ConvertToPDF();
        }

        void ConvertToPDF()
        {
            short bgPlot = (short)Application.GetSystemVariable("BACKGROUNDPLOT");

            Application.SetSystemVariable("BACKGROUNDPLOT", 0);

            Document doc = Application.DocumentManager.MdiActiveDocument;
            
            using (DsdEntryCollection dsdDwgFiles = new DsdEntryCollection())
            {
                Database acCurDb = doc.Database;

                // Get the layout dictionary of the current database
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    DBDictionary lays =
                        acTrans.GetObject(acCurDb.LayoutDictionaryId,
                            OpenMode.ForRead) as DBDictionary;

                    // Step through and list each named layout and Model
                    foreach (DBDictionaryEntry item in lays)
                    {
                        //Create a DsdEntry for each layout
                        using (DsdEntry dsdDwgFile1 = new DsdEntry())
                        {
                            //Set the file name and layout
                            dsdDwgFile1.DwgName = doc.Name;
                            dsdDwgFile1.Layout = item.Key;
                            dsdDwgFile1.Title = item.Key;

                            // Set the page setup override
                            dsdDwgFile1.Nps = "";
                            dsdDwgFile1.NpsSourceDwg = "";

                            dsdDwgFiles.Add(dsdDwgFile1);
                        }
                    }

                    // Abort the changes to the database
                    acTrans.Abort();
                }                

                // Set the properties for the DSD file and then write it out
                using (DsdData dsdFileData = new DsdData())
                {
                    // Setup the temp path variables
                    String tempPath = System.IO.Path.GetTempPath() + "Hightail\\";
                    String tempFileName = tempPath + System.IO.Path.GetFileNameWithoutExtension(doc.Name);
                    mPdfString = tempFileName + ".pdf";

                    // Ensure the path exists
                    if (!System.IO.Directory.Exists(tempPath))
                    {
                        System.IO.Directory.CreateDirectory(tempPath);
                    }

                    // Set the target information for publishing
                    dsdFileData.DestinationName = mPdfString;
                    dsdFileData.ProjectPath = tempPath;
                    dsdFileData.SheetType = SheetType.MultiPdf;

                    // Set the drawings that should be added to the publication
                    dsdFileData.SetDsdEntryCollection(dsdDwgFiles);

                    // Set the general publishing properties
                    dsdFileData.LogFilePath = tempFileName + "-Log.txt";

                    // Create the DSD file
                    dsdFileData.WriteDsd(tempFileName + ".dsd");

                    try
                    {
                        // Publish the specified drawing files in the DSD file, and
                        // honor the behavior of the BACKGROUNDPLOT system variable

                        using (DsdData dsdDataFile = new DsdData())
                        {
                            dsdDataFile.ReadDsd(tempFileName + ".dsd");

                            // Get the DWG to PDF.pc3 and use it as a 
                            // device override for all the layouts
                            PlotConfig acPlCfg = PlotConfigManager.SetCurrentConfig("DWG to PDF.PC3");

                            Application.Publisher.EndPublish += new Autodesk.AutoCAD.Publishing.EndPublishEventHandler(Publisher_EndPublish);

                            Application.Publisher.PublishExecute(dsdDataFile, acPlCfg);
                        }
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception es)
                    {
                        System.Windows.Forms.MessageBox.Show(es.Message);
                    }
                }
            }

            //reset the background plot value
            Application.SetSystemVariable("BACKGROUNDPLOT", bgPlot);
        }

        private void Publisher_EndPublish(object sender, PublishEventArgs e)
        {
            // Now that the PDF is written we need to upload that to the server, carry on processing that upload here.
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed;
            
            if (doc != null)
            {
                ed = doc.Editor;
                ed.WriteMessage("\nUploading to a new BitSpring Space.");
            }

            //Upload the file to BitSpring

            if (doc != null)
            {
                ed = doc.Editor;
                ed.WriteMessage("\nOpening BitSpring space in your default browser.");
            }

            //Open the space in a browser

            if (doc != null)
            {
                ed = doc.Editor;
                ed.WriteMessage("\nPlease check your default browser for you BitSpring space.");
            }
        }
    }
}
