//Matt Sawyer
//11252935
//CptS 322 - Summer 2015
//HW 8


using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadSheetEngine
{


    #region Cell abstract class

    public abstract class Cell : INotifyPropertyChanged
    {
        //Cell class fields
        private readonly int RowInx;
        private readonly int ColumnInx;
        private readonly string cellName = "";
        protected string tText = "";
        protected string privValue = "";
        protected int BGroundColor = -1;

        //delegate is an object that calls the method
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        //properties of Cell class
                 #region Cell properties

        public bool DefaultCellCheck
        {
            get
            {
                if (string.IsNullOrEmpty(Text) && BackGroundColor == -1)
                {
                    return true;
                }

                return false;
            }
        }

        //this is which row the cell is on
        public int RowIndex
        {
            get
            {
                return RowInx;
            }
        }


        //This gets which column the cell is on
        public int ColumnIndex
        {
            get
            {
                return ColumnInx;
            }
        }

        //Name of cell
        public string Name
        {
            get
            {
                return cellName;
            }

        }

        // Background color of this cell.
        public int BackGroundColor
        {
            get
            { 
                return BGroundColor;
            }

            set
            {
                // If our color changed, set it and fire property changed event.
                if (value != BGroundColor)
                {
                    BGroundColor = value;
                  //  PropertyChanged(this, new PropertyChangedEventArgs("BackColor"));
                    PropertyChanged(this, new PropertyChangedEventArgs("BackGroundColor"));
                }
            }
        }

        //Text property is a getter and setter for member variable
        public string Text
        {
            get
            { return tText; }

            set
            {
                if (value != tText)
                {
                    tText = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                }
            }
        }

        //Value property is a string that represents the "evaluated" value.
        public string Value
        {
            get     //Value is getter only
            {
                return privValue;
            }
        }

                #endregion

        #region Construction workers

        public Cell()
        {
        }

        //Row, column, and name of cells will be set through this constructor
        public Cell(int row_index, int column_index)
        {
            RowInx = row_index;
            ColumnInx = column_index;
            cellName += Convert.ToChar('A' + column_index);
            cellName += (row_index+1).ToString();
        }

        #endregion
        //deletes the cell content
        public void Terminate()
        {
            Text = "";
            BackGroundColor = -1;
        }
    }
        

    #endregion


}