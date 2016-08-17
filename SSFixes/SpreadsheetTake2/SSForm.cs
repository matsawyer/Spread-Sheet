//Matt Sawyer
//11252935
//CptS 322 - Summer 2015
//HW 7 More Spreadsheet stuff

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpreadSheetEngine;
using SpreadSheetEngine.Undo;
using System.Xml;
using System.Xml.Linq;


namespace SpreadsheetTake2
{

    public partial class SSForm : Form
    {
        // Spreadsheet mSheet = new Spreadsheet(50,26); // spreadsheet object to for the main form code.
        //   public UndoRedoSystem UndoRedo = new UndoRedoSystem();
        public Workbook book = new Workbook(50, 26);
            
             
        public SSForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Here, we renew subscriptions to various events
            dataGridView1.CellBeginEdit += dGV1CellBeginEdit;
            dataGridView1.CellEndEdit += dGV1CellEndEdit;

            //mSheet.CellPropertyChanged += OnSpreadSheetChanged; 
            book.WBSheetChanged += OnSpreadSheetChanged;

            dataGridView1.Columns.Clear(); //empties grid before we set up the new grid
            const string alphabet = " ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            //creates columms, one for each letter of the alphabet
            for (int i = 1; i < alphabet.Length; i++)
            {
                string header = "this";
                //string text = "that";
                this.dataGridView1.Columns.Add(header, Convert.ToString(alphabet[i]));
            }

            //creates 50 rows 
            dataGridView1.Rows.Add(50);

            //for each row, the header value will be set to the row number as a string
            //for (int j = 1; j < dataGridView1.Rows.Count; j++)
            //{


            //    DataGridViewRowHeaderCell headercell = dataGridView1.Rows[j].HeaderCell;
            //    headercell.Value = (j + 1).ToString();
            //    headercell.Value = (j).ToString();
            //    dataGridView1.Rows[j].HeaderCell = headercell;
            //    dataGridView1.Rows[j + 1].HeaderCell = headercell;
            //}
            int Row_Num = 1;
            foreach (DataGridViewRow DataGridRow in dataGridView1.Rows)
            {
                DataGridRow.HeaderCell.Value = Convert.ToString(Row_Num++);
            }


            //https://msdn.microsoft.com/en-us/library/ms158604(v=vs.110).aspx found this auto resize command on MSDN
            dataGridView1.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
        }




        void dGV1CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // Get the row and column indices of the dataGridView cell that started being edited.
            int row = e.RowIndex ;
            int col = e.ColumnIndex;
            //Cell currentcell = mSheet.get_cell(col, row+1);//speadsheet cell being edited
            Cell currentcell = book.CurrentSheet.get_cell(row, col);//speadsheet cell being edited
            dataGridView1.Rows[row].Cells[col].Value = currentcell.Text;
        }


        void dGV1CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //row and column indices of the selected cell
            int row = e.RowIndex;
            int col = e.ColumnIndex;
            string updatedtxt;//String allocated for new text from datagrid

            //Cell Sheetcell = mSheet.get_cell(col, row+1); //speadsheet cell being edited
            //Cell Sheetcell = book.ActiveSheet.get_cell(col, row);
            Cell Sheetcell = book.CurrentSheet.get_cell(row, col);

            try 
            { 
                updatedtxt = dataGridView1.Rows[row].Cells[col].Value.ToString(); 
            }
            catch (NullReferenceException) 
            { 
                updatedtxt = ""; 
            }

            IUndoRedocmd[] undos = new IUndoRedocmd[1];

            undos[0] = new RestoreTextcmd(Sheetcell.Text, Sheetcell.Name);


            Sheetcell.Text = updatedtxt;//Sets text in the spreadsheet cell

            book.UndoRedo.pushUndos(new UndoRedoCollection(undos, "change in cell text"));
            dataGridView1.Rows[row].Cells[col].Value = Sheetcell.Value;

            UpdateToolStripMenu();
        }

        private void UpdateToolStripMenu()
        {
            ToolStripMenuItem group = menuStrip1.Items[1] as ToolStripMenuItem;
            foreach (ToolStripItem item in group.DropDownItems)
            {
                if (item.Text.Substring(0, 4) == "Undo")
                {
                    // item.Enabled = mSheet.urdo.CanUndo;
                    //item.Text = "Undo " + mSheet.urdo.UndoDescription;
                    item.Enabled = book.UndoRedo.CanUndo;
                    item.Text = "Undo " + book.UndoRedo.UndoDescription;
                }
                else if (item.Text.Substring(0, 4) == "Redo")
                {
                    // item.Enabled = mSheet.urdo.CanRedo;
                    // item.Text = "Redo " + mSheet.urdo.RedoDescription;
                    item.Enabled = book.UndoRedo.CanRedo;
                    item.Text = "Redo" + book.UndoRedo.RedoDescription;
                }
            }
        }

        //pickCellBackgroundColorToolStripMenuItem function affiliate
        private void pickCellBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Our undos for each action.
            List<IUndoRedocmd> undos = new List<IUndoRedocmd>();

            // If they've chosen a color...
            ColorDialog cellcolor = new ColorDialog();
            if (cellcolor.ShowDialog() == DialogResult.OK)
            {
      
                int colorchoice = cellcolor.Color.ToArgb();

               foreach (DataGridViewCell myDataGridCell in dataGridView1.SelectedCells)
                {
                    //  Cell mySheetCell = mSheet.get_cell(myDataGridCell.ColumnIndex, myDataGridCell.RowIndex);
                    //Cell mySheetCell = book.ActiveSheet.get_cell(myDataGridCell.ColumnIndex, myDataGridCell.RowIndex);
                   Cell mySheetCell = book.CurrentSheet.get_cell(myDataGridCell.RowIndex, myDataGridCell.ColumnIndex);
                    undos.Add(new RestoreBGColorcmd(mySheetCell.BackGroundColor, mySheetCell.Name));
                    mySheetCell.BackGroundColor = colorchoice;
                }

                // Add the undos to our undo/redo system.
                // mSheet.urdo.applyUndos(new UndoRedoCollection(undos, "changing cell background color"));
                book.UndoRedo.pushUndos(new UndoRedoCollection(undos, "change in cell background color"));
                UpdateToolStripMenu();
            }
        }

        

        private void OnSpreadSheetChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                Cell currentcell = sender as Cell;
                if (currentcell != null)
                {
                    int row = currentcell.RowIndex;
                    int col = currentcell.ColumnIndex;
                    string value = currentcell.Value;
                    dataGridView1.Rows[row].Cells[col].Value = value;
                }
            }
            else if (e.PropertyName == "BackGroundColor")
            {
                Cell currentcell = sender as Cell;
                if (currentcell != null)
                {
                    int row = currentcell.RowIndex;
                    int col = currentcell.ColumnIndex;
                    int color = currentcell.BackGroundColor;
                    //requested property implementation for cell color
                    dataGridView1.Rows[row].Cells[col].Style.BackColor = Color.FromArgb(color);
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //function associated with clicking on a cell
        }

        //void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        //{
        //   // this.dataGridView1.Rows[e.RowIndex].Cells[0].Value = (e.RowIndex + 1).ToString();
        //    this.dataGridView1.Rows[e.RowIndex].Cells[0].Value = (e.RowIndex).ToString();
        //}

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //mSheet.urdo.Undo(mSheet);
            book.UndoRedo.Undo(book.CurrentSheet);
            UpdateToolStripMenu();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //mSheet.urdo.Redo(mSheet);
            book.UndoRedo.Redo(book.CurrentSheet);
            UpdateToolStripMenu();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML files (*.xml)|*.xml";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stream stream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write);
                book.Save(stream);
                //mSheet.Save(stream);
                stream.Dispose();
            }

        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stream stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                book.Load(stream);
                //mSheet.Load(stream);
                stream.Dispose();
            }

            UpdateToolStripMenu();
        }
    }
}
