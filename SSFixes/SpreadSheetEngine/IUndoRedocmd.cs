//Matt Sawyer
//11252935
//CptS 322 - Summer 2015
//HW 8


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadSheetEngine
{
    //IUndoRedocmd interface, all members are implicitly abstract 
    //thus members have no implementation
    public interface IUndoRedocmd
    {
        IUndoRedocmd Exec(Spreadsheet msheet);
    }

    //All of the functions that work our Undoings and Redoings
    public class UndoRedoCollection
    {
        public string ActionDescription;
        private IUndoRedocmd[] commandz;
        public UndoRedoCollection()
        {
        }
        public UndoRedoCollection(IUndoRedocmd[] cmds, string description)
        {
            commandz = cmds;
            ActionDescription = description;
        }
        public UndoRedoCollection(List<IUndoRedocmd> cmds, string description)
        {
            commandz = cmds.ToArray();
            ActionDescription = description;
        }
        //executes each of the listed command actions
        public UndoRedoCollection Exec(Spreadsheet sheet)
        {
            List<IUndoRedocmd> cmdList = new List<IUndoRedocmd>();

            foreach (IUndoRedocmd cmd in commandz)
            {
                cmdList.Add(cmd.Exec(sheet));
            }
            return new UndoRedoCollection(cmdList.ToArray(), this.ActionDescription);
        }
    }

    //We use stacks to keep track of our current amount of doings.
    public class UndoRedoSystem
    {   
        private Stack<UndoRedoCollection> UNDO = new Stack<UndoRedoCollection>();
        private Stack<UndoRedoCollection> REDO = new Stack<UndoRedoCollection>();

        //Enables undo 
        public bool CanUndo 
        {
            get { return UNDO.Count != 0; }
        }
        //Enables redo
        public bool CanRedo
        {
            get { return REDO.Count != 0; }
        }
        //Describes the action that clicking the undo button will commit
        public string UndoDescription
        {
            get
            {
                if (CanUndo) return UNDO.Peek().ActionDescription;
                return "";
            }
        }
        //Describes the action that clicking the redo button will commit
        public string RedoDescription
        {
            get
            {
                if (CanRedo) return REDO.Peek().ActionDescription;
                return "";
            }
        }
        //Pushes the next action call onto the undo stack.
        public void pushUndos(UndoRedoCollection undos)
        {
            UNDO.Push(undos);
            REDO.Clear();
        }
        // Undoes last operation
        public void Undo(Spreadsheet sheet)
        {
            UndoRedoCollection actions = UNDO.Pop();
            REDO.Push(actions.Exec(sheet));
        }
        //Redoes last operation
        public void Redo(Spreadsheet sheet)
        {
            UndoRedoCollection actions = REDO.Pop();
            UNDO.Push(actions.Exec(sheet));
        }
        //Strikes down the stacks, no undo or redo
        public void Clear()
        {
            UNDO.Clear();
            REDO.Clear();
        }
    }



}
