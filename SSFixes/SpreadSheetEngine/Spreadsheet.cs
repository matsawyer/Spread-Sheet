//Matt Sawyer
//11252935
//CptS 322 - Summer 2015
//HW 8


using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;
using ArithmeticEngine;


namespace SpreadSheetEngine
{
    //The Spreadsheet class, uses the abstract cell class to create cell spreadsheet
    //The constructor will allocate a 2D array of cells
    public class Spreadsheet
    {
        //private List<Spreadsheet> _sheets = new List<Spreadsheet>();
       // public Spreadsheet _sheets = new Spreadsheet(50,26);
      //  private int _activeSheetIndex = 0;
        public UndoRedoSystem UndoRedo = new UndoRedoSystem();

        #region mCell inheriting base class

        //this class inherits from cell allowing us to access protected elements
        //With this inheriting class, we can set the value property of cells.
        private class mCell : Cell
        {
            //constructor calls the base class constructor
            public mCell(int row_index, int column_index)
                : base(row_index, column_index)
            {
            }
            //this is how we'll set the value property from the spreadsheet
            public void SetValue(string new_value)
            {
                privValue = new_value;
            }
        }

        #endregion
        public UndoRedoSystem undoredo = new UndoRedoSystem();
        private Cell[,] mCellsArrayArray;
        //dictionary object, keys are the cells and the values are a list of cells the kill is dependent on
        private Dictionary<string, HashSet<string>> dependency;
        //Spreadsheet Cell change event
        public event PropertyChangedEventHandler CellPropertyChanged;

        //row property of spreadsheet
        public int RowCount
        {
            get
            { return mCellsArrayArray.GetLength(0); }
        }
        
        //column property of spreadsheet
        public int ColumnCount
        {
           get
           { return mCellsArrayArray.GetLength(1); }
        }

        //Spreadsheet constructor creates object according to size constraints
        //This will also subscribe cells to property change events
        public Spreadsheet(int RowCount, int ColumnCount)
        {
            mCellsArrayArray = new Cell[RowCount, ColumnCount];
            dependency = new Dictionary<string, HashSet<string>>();
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    Cell CurrentCell = new mCell(i, j);
                    CurrentCell.PropertyChanged += AlertPropertyChange;
                    mCellsArrayArray[i,j] = CurrentCell;
                }
            }
        }

        //Circular Reference check, returns true if CircularReference is present, false otherwise.
        public bool OhNoCircularReference(string startingcell, string currentcell)
        {
            //Should we encounter our starting cell while traversing, we have found a circular reference.
            if (startingcell == currentcell)
            {
                return true;
            }
            //Cell is not subscribed to a dependent, thus there is no circle reference.
            if (!dependency.ContainsKey(currentcell))
            {
                return false;
            }
            // Recursive call checks within the subscribed cells for circular references
            foreach (string dependent in dependency[currentcell])
            {
                if (OhNoCircularReference(startingcell, dependent))
                {
                    return true;
                }
            }

            return false;
        }
        //Saves the currently active sheet as an XML file
        public void Save(XmlWriter writer)
        {
            writer.WriteStartElement("Spreadsheet");

            var savedcells =
                from Cell cell in mCellsArrayArray
                where !cell.DefaultCellCheck
                select cell;

            foreach (Cell cell in savedcells)
            {
                writer.WriteStartElement("Cell");
                writer.WriteAttributeString("Name", cell.Name);
                writer.WriteElementString("Text", cell.Text);
                writer.WriteElementString("BackGroundColor", cell.BackGroundColor.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }


        //Loads spreadsheet instance from XML file selected by user
        public void Load(XElement spreadsheetElement)
        {
            if ("Spreadsheet" != spreadsheetElement.Name)
            {
                return;
            }

            foreach (XElement child in spreadsheetElement.Elements("Cell"))
            {
                Cell cellda = get_cell(child.Attribute("Name").Value);

                //skips past the empty cells.
                if (cellda == null)
                { 
                    continue; 
                }

                // Load and set text.
                var textElement = child.Element("Text");
                if (textElement != null)
                {
                    cellda.Text = textElement.Value;
                }

                //Load and set background color.
                var bgElement = child.Element("BackGroundColor");
                if (bgElement != null)
                {
                    cellda.BackGroundColor = int.Parse(bgElement.Value);
                }
            }
        }
        /// Clears spreadsheet of data
        public void Terminate_Sheet()
        {
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    if (!mCellsArrayArray[i, j].DefaultCellCheck)
                    {
                        mCellsArrayArray[i, j].Terminate();
                    }
                }
            }
        }

        //Using the location string parameter of the Cell,
        //we attempt to discover the cell location 
        //For example, the string C2 would be begin
        //the operation on the cell at (2,1)
        private void EvaluateCell(string location)
        {
            EvaluateCell(get_cell(location));
        }

        //function overload uses row and col indices to determine cell value
        private void EvaluateCell(int row, int col)
        {
            EvaluateCell(get_cell(row, col));
        }

        /// Uses cell indices to determine the cell value.
        private void EvaluateCell(Cell argcell)
        {
            mCell lleCym = argcell as mCell;
            if (string.IsNullOrEmpty(lleCym.Text))
            {
                lleCym.SetValue("");
               CellPropertyChanged(argcell, new PropertyChangedEventArgs("Value"));
            }
            else if (lleCym.Text[0] == '=' && lleCym.Text.Length > 1)
            {
                bool boo = false;
                string expressionString = lleCym.Text.Substring(1);
                ArithmeticExpression ArithExp = new ArithmeticExpression(expressionString);

                // Gets expression variables
                string[] ExpressionVariables = ArithExp.GetVariables();

                // Sets each variable in expression
                foreach (string variableName in ExpressionVariables)
                {
                    // Checks for self references
                    if (variableName == lleCym.Name)
                    {
                        Errorchecks(lleCym, variableName, "Self reference");
                        boo = true;
                        break;
                    }
                    // Check if the cell represented by a variable is valid
                    if (get_cell(variableName) == null)
                    {
                        Errorchecks(lleCym, variableName, "Bad reference");
                        boo = true;
                        break;
                    }
           
                    SetExpressionVariable(ArithExp, variableName);

                    // Check if there's a circular reference from this variable.
                    if (OhNoCircularReference(variableName, lleCym.Name))
                    {
                        Errorchecks(lleCym, variableName, "Circular reference");
                        boo = true;
                        break;
                    }
                }
                if (boo) return;
                // Sets expression evaluation value and pushes out propertychange event.
                lleCym.SetValue(ArithExp.Evaluation().ToString());
                CellPropertyChanged(argcell, new PropertyChangedEventArgs("Value"));
            }
            else
            {
                lleCym.SetValue(lleCym.Text);
                CellPropertyChanged(argcell, new PropertyChangedEventArgs("Value"));
            }
            if (dependency.ContainsKey(lleCym.Name))
            {
                foreach (string name in dependency[lleCym.Name])
                {
                    EvaluateCell(name);
                }
            }
        }

        //displays the located error if there was one during the evaluation
       // private void Errorchecks(mCell CheckedCell, string varname, string inputstr)
        private void Errorchecks(mCell CheckedCell, string varname, string errorstr)
        {
            CheckedCell.SetValue("!(" + errorstr + ")");

            CellPropertyChanged(CheckedCell as Cell, new PropertyChangedEventArgs("Value"));
        }

        //getter and setter for element of get_cell function
        public Cell rCelly { get; set; }

        //function returns mCell object from the input row and column indices
        public Cell get_cell(int ro, int co)
        {
            return mCellsArrayArray[ro, co];
        }


        //Overload of the getcell function, using the column letter and row number string and assuming
        //valid location, this function will return the cell that was found at that spot.
        public Cell get_cell(string spot)
        {
            char letter = spot[0];
            if (!Char.IsLetter(letter))
            {
                //first character was not a letter, thus null was returned
                return null;
            }

            int num;
            if (!int.TryParse(spot.Substring(1), out num))
            {
                //No numbers found in the rest of string to give us a row and column for the cell
                //so we return null.
                return null;
            }
            //Cell result
            Cell Rcelly;
            try
            {
               // Rcelly = get_cell(num - 1, letter - 'A');
                Rcelly = get_cell(num - 1, (int)letter - 'A');
            }
            catch (Exception)
            {
                //error caught, null returned
                return null;
            }
            //location was valid, cell result is returned
            return Rcelly;
        }



       
        /// Function to set expression variable retrieved from spreadsheet
        /// First parameter, exp, is the expression we are manipulating.
        /// Second parameter is the cell of the expression variable we will set
        private void SetExpressionVariable(ArithmeticExpression ex, string Name_ExpVariable)
        {
            Cell expressionVariableCell = get_cell(Name_ExpVariable);
            double value;
            if (string.IsNullOrEmpty(expressionVariableCell.Value))
            {
                ex.SetVariable(expressionVariableCell.Name, 0);
            }
            else if (!double.TryParse(expressionVariableCell.Value, out value))
            {
                ex.SetVariable(Name_ExpVariable, 0);
            }
            else
            {
                ex.SetVariable(Name_ExpVariable, value);
            }
        }



        //track the dependencies of the tracked cell and it's referenced variables
        private void TrackDependents(string TrackedCell, string[] variables)
        {
            foreach (string var in variables)
            {
                if (!dependency.ContainsKey(var))
                {
                    // Build dictionary entry for this variable name.
                    dependency[var] = new HashSet<string>();
                }

                // Add this cell name to dependencies for this variable name.
                dependency[var].Add(TrackedCell);
            }
        }



        // Frees the cell, making it independent of others,
        // I was thinking of calling it july4th
        private void INDEPENDENCEDAY(string cell)
        {         
            List<string> keysToFreedom = new List<string>();
            foreach (string key in dependency.Keys)
            {
                if (dependency[key].Contains(cell))
                {
                    keysToFreedom.Add(key);
                }
            }
            foreach (string key in keysToFreedom)
            {
                HashSet<string> set = dependency[key];

                if (set.Contains(cell))
                {   //if we're modifying a spot with a cell, cell is dropped
                    set.Remove(cell); 
                }
            }
        }

        public class mEventArgs : EventArgs
        {
            public int row;
            public int col;
        }

        //Property changed event handler, first parameter sender is object 
        public void AlertPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Text")
            {
                mCell mycell = sender as mCell;

                //declaring independence on our cell
                INDEPENDENCEDAY(mycell.Name);

                //looks for expression in the cell
                if ((mycell.Text.Length > 1) && (mycell.Text != "") && (mycell.Text[0] == '='))
                {
                    ArithmeticExpression exp = new ArithmeticExpression(mycell.Text.Substring(1));
                    //well, that cell's independence didn't last long
                    TrackDependents(mycell.Name, exp.GetVariables());
                }
                EvaluateCell(sender as Cell);
            }
            else if (e.PropertyName == "BackGroundColor")
            {           
                CellPropertyChanged(sender, new PropertyChangedEventArgs("BackGroundColor"));
                //CellPropertyChanged(sender, new PropertyChangedEventArgs("BGColor"));
            }
           
        }

    }
    
}
