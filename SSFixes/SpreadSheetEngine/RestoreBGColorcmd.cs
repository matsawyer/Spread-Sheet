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
    //Class to restore the background color of a cell
    public class RestoreBGColorcmd : IUndoRedocmd
    {
        private int mycellcolor;
        private string mycellname;

        public RestoreBGColorcmd(int cellcolor, string cellname)
        {
            mycellcolor = cellcolor;
            mycellname = cellname;
        }
        public IUndoRedocmd Exec(Spreadsheet sheet)
        {
            Cell cell = sheet.get_cell(mycellname);
            int oldcellcolor = cell.BackGroundColor;
            cell.BackGroundColor = mycellcolor;
            return new RestoreBGColorcmd(oldcellcolor, mycellname);
        }
    }
}
