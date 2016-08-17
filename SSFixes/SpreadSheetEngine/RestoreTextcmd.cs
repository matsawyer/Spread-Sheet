//Matt Sawyer
//11252935
//CptS 322 - Summer 2015
//HW 8



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadSheetEngine.Undo
{
    // RestoreTextCommand class as implemented during lecture on 6/30
    public class RestoreTextcmd : IUndoRedocmd
    {
        private string mycelltxt, mycellname;
        public RestoreTextcmd(string CellText, string cellName)
        {
            mycelltxt = CellText;
            mycellname = cellName;
        }
        public IUndoRedocmd Exec(Spreadsheet msheet)
        {
            Cell cell = msheet.get_cell(mycellname);
            string oldtxt = "";
            oldtxt = cell.Text;
            cell.Text = mycelltxt;
            return new RestoreTextcmd(oldtxt, mycellname);
        }
    }
}

