//Matt Sawyer
//11252935
//CptS 322 - Summer 2015
//HW 7 More Spreadsheet stuff



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ArithmeticEngine
{
    public class ArithmeticExpression
    {
        #region Super(base)class Node and it's sub(derived)classes

        //private abstract node class the rest of the node 
        //types will inherit left and right children from
        private abstract class Node
        {
            public Node L, R;

        }


        // Node representation for binary operators within the Expression Tree
        private class OPNode : Node
        {
            public char Operand;

            public OPNode()
            {
            }

            public OPNode(char op)
            {
                Operand = op;
            }
        }

        // Node representing expression variables
        private class VariableNode : Node
        {
            public string Name;

            public VariableNode()
            {
            }

            public VariableNode(string name)
            {
                Name = name;
            }
        }

        // Node for the constant numerical values within the expression
        private class ConstantNode : Node
        {
            public double val;

            public ConstantNode()
            {
            }

            public ConstantNode(double doublenum)
            {
                val = doublenum;
            }
        }

        #endregion



        private Node RootTreeNode; //root node of expression tree
        private string exprstr;
        public readonly static char[] PossibleOperands = { '+', '-', '*', '/', '^' };
        private Dictionary<string, double> DictionaryVariables; //variable dictionary declartion


        ////getter and setter for root node
        //private Node RootTN
        //{
        //    set
        //    {
        //        this.RootTreeNode = value;
        //    }
        //    get
        //    {
        //        return this.RootTreeNode;
        //    }
        //}

        //getter and setter for properties of the expression string


        public string ExpressionString
        {
            get
            {
                return exprstr;
            }
            set
            {
                //When set, expressionstring is set, dictionary is cleared, and tree recompiled
                exprstr = value;
                DictionaryVariables.Clear(); 
                RootTreeNode = Compile(exprstr);
            }
        }

    #region ArithmeticExpression Constructors

        public ArithmeticExpression()
        {
            DictionaryVariables = new Dictionary<string, double>();
        }

        public ArithmeticExpression(string EXSTR)
        {
            string result = "";
            if (EXSTR.Contains(' '))
            {
                foreach (char e in EXSTR)
                {
                    if (e != ' ')
                    {
                        result += e;
                    }
                }
            }
            else
            {
                result = EXSTR;
            }
            exprstr = result;
            DictionaryVariables = new Dictionary<string, double>();
            RootTreeNode = Compile(exprstr);
        }


    #endregion  

        //Compile will use the the input expression string to attemp to create an arithmetic expression
        //Returns either null if input empty or a branch node representative of input expression
        private Node Compile(string inputexpression)
        {
            if (string.IsNullOrEmpty(inputexpression))
            {
                return null;
            }


            //Detects left paranthesis, begins looking for right paranthesis
            if (inputexpression[0] == '(')
            {
                // Counter to keep track of paranthesis
                int parcounter = 0;
                for (int i = 0; i < inputexpression.Length; i++)
                {
                    if (inputexpression[i] == '(')
                    {
                        parcounter++; //Increments at left paranthesis
                    }
                    else if (inputexpression[i] == ')')
                    {
                        parcounter--;//Decrements at right paranthesis

                        if (parcounter == 0)//Counter at zero means left and right paranthesis have matched
                        {
                            if (inputexpression.Length - 1 != i)
                            {
                                break; // if we are not at string end, we continue compilation
                            }
                            else
                            {
                                // if we are at end of expression, we compile between the paranthesis
                                return Compile(inputexpression.Substring(1, inputexpression.Length - 2));
                            }
                        }
                    }
                }
            }

            char[] operArr = ArithmeticExpression.PossibleOperands;
            foreach (char operand in operArr)
            {
                // Compile the expression based on the current operation.
                // Only return subtree if non-null.
                Node oNode = Compile(inputexpression, operand);
                if (oNode != null)
                    return oNode;
            }

            //if we get here, our leaf is either a Constant or a Variable.
            double dubblenum;
            if (double.TryParse(inputexpression, out dubblenum))
            {
                return new ConstantNode(dubblenum);
            }
            else
            {
                // Initializes the variable in the dictionary
                DictionaryVariables[inputexpression] = 0;
                return new VariableNode(inputexpression);
            }
        }

        //This Compile function serves as the recursive call to compile, we overload it with an operand argument
        //this function will take in an expression and operand to return an a branch node of the expression
        private Node Compile(string expression, char operation)
        {
            bool flag = false;
            int i = expression.Length - 1;
            int parcounter = 0;
            bool rightsided = false;

            if (operation == '^')//Exponent operator has highest precedence
            {
                rightsided = true;
                i = 0;
            }


            while (!flag)
            {
                if (expression[i] == '(')
                {
                    if (rightsided)
                        parcounter--;//decrements left paranthesis if right sided
                    else
                        parcounter++;//increments left paranthesis if left sided
                }
                else if (expression[i] == ')')
                {
                    if (rightsided)
                        parcounter++;//increments right paranthesis if right sided
                    else
                        parcounter--;//decrements right paranthesis if left sided
                }

                if (parcounter == 0 && expression[i] == operation) // if we are inbetween paranthesis, evaluate expression
                {
                    // Create and return a subtree with the current op (as an OPNode) being the root, and the 
                    // left and right expressions (as their own compiled subtrees) being the Left and Right children.
                    OPNode OPnod = new OPNode(operation);
                    OPnod.L = Compile(expression.Substring(0, i));
                    OPnod.R = Compile(expression.Substring(i + 1));
                    return OPnod;
                }

                if (rightsided)
                {
                    if (i == expression.Length - 1)
                        flag = true; //ends expression reading loop if we have hit the right end of the expression
                    i++;//otherwise keeps reads next part of the expression to the right side
                }
                else
                {
                    if (i == 0)
                        flag = true; //ends expression reading loop if we were at the left end of expression
                    i--; //otherwise reads the next part of the expression on the left
                }
            }
            return null;
        }

        //public function call to the private Evaluate which works through the
        //compiled expression in order to return an evaluation of the root node
        public double Evaluation()
        {
            //RootTreeNode = Compile(expressionString);
            return Evaluate(RootTreeNode);
        }

        //Evaluate function will take an input node argument and evaluate it
       private double Evaluate(Node NodeArg)
        {
            ConstantNode ConstaNode = NodeArg as ConstantNode;
            if (ConstaNode != null)
            {
                return ConstaNode.val;
            }

            VariableNode VarNode = NodeArg as VariableNode;
            if (VarNode != null)
            {
                return DictionaryVariables[VarNode.Name];
            }
            // If OPNode, recursively evaluate Left and Right subtrees and perform operation on them.
            OPNode OperaNode = NodeArg as OPNode;
            if (OperaNode != null)
            {
                switch (OperaNode.Operand) //determines what to do with expression based on operation read
                {
                    case '^':
                        return Math.Pow(Evaluate(OperaNode.L), Evaluate(OperaNode.R)); //needed math library to work out exponentials
                    case '+':
                        return Evaluate(OperaNode.L) + Evaluate(OperaNode.R);
                    case '-':
                        return Evaluate(OperaNode.L) - Evaluate(OperaNode.R);
                    case '*':
                        return Evaluate(OperaNode.L) * Evaluate(OperaNode.R);
                    case '/':
                        return Evaluate(OperaNode.L) / Evaluate(OperaNode.R);

                }
            }
            return 0;
        }
        //This function is used to set variable values in the expression
        public void SetVariable(string VariableName, double VarValue)
        {
            DictionaryVariables[VariableName] = VarValue;
        }

        //Utility function to gather all expression variables
        //Variables returned as an array of strings
        public string[] GetVariables()
        {
            return DictionaryVariables.Keys.ToArray();
        }

    }
}
