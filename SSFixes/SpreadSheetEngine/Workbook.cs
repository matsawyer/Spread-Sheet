//Matt Sawyer
//11252935
//CptS 322 - Summer 2015
//HW 8


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace SpreadSheetEngine
{
    public class Workbook
    {

        private List<Spreadsheet> SpreadsheetList = new List<Spreadsheet>();

        private int sheetIndex = 0;

        public UndoRedoSystem UndoRedo = new UndoRedoSystem();
      
        public event PropertyChangedEventHandler WBSheetChanged;

        //Get current sheet being worked on
        public Spreadsheet CurrentSheet
        {
            get
            {
                return SpreadsheetList[sheetIndex];
            }
        }

        //Workbook constructor builds a new 10x10 workbook
        public Workbook()
            : this(10, 10)
        {
        }

        //Creates a workbook sheet instance of the input size.
        public Workbook(int rows, int cols)
        {
            AddSheet(new Spreadsheet(rows, cols));
        }
        //lays down another workbooksheet
        public void AddSheet(Spreadsheet sheet)
        {
            SpreadsheetList.Add(sheet);
            sheet.CellPropertyChanged+= OnSpreadsheetCellChanged;
        }

        //Saves current sheet as an XML file at user-picked destination
        public bool Save(Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.NewLineChars = "\r\n";
            settings.NewLineOnAttributes = false;
            settings.Indent = true;

            //Uses XmlWriter to write our settings to the stream
            XmlWriter writer = XmlWriter.Create(stream, settings);
            if (writer == null)
            {
                return false;
            }
            writer.WriteStartElement("Workbook");
            foreach (Spreadsheet spread in SpreadsheetList)
            {
                spread.Save(writer);
            }
            writer.WriteEndElement();
            writer.Close();
            return true;
        }

        //Loads an XML file from  user selected destination source.
        public bool Load(Stream stream)
        {
            XDocument doc = null;

            try
            {
                doc = XDocument.Load(stream);
            }
            catch (Exception)
            {
                return false;
            }

            if (doc == null)
            {
                return false;
            }
            //Trashes current sheet to make room for laoded sheet
            SpreadsheetList[0].Terminate_Sheet();
            XElement root = doc.Root;
            foreach (XElement child in root.Elements("Spreadsheet"))
            {
                SpreadsheetList[0].Load(child);
            }
            // Clear undo/ redo stacks.
            UndoRedo.Clear();

            return true;
        }

        //Handler for spreadsheet changes
        public void OnSpreadsheetCellChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                WBSheetChanged(sender, e);
            }
            else if (e.PropertyName == "BackGroundColor")
            {
                WBSheetChanged(sender, e);
            }
        }

        

    }
}
