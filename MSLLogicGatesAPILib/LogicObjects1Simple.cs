using System.Text;

namespace MSLLogicGatesAPI
{
    public abstract class LogicObject : BasicComponent
    {
        // A generic logic object.
        // All of the objects that use logic, which should be most of them, will inherit from this.

        // All logic objects can be Hot or Cold. This status is usually determined by the
        //public Output function. But, the IsHot variable can be used internally to test for that.
        protected bool IsHot = false;

        /* Each object can have (almost) any number of input and output leads. These properties can be
        accessed by public NumInputs and NumOutputs properties. But, these two vars can
        be used internally. */
        protected short NumIns = 1; //By default, there will only one input and one output.
        protected short NumOuts = 1;

        public virtual void Input(short InNum, bool Value)
        {
            /* Sets a value for an input on the object.
                InNum = The Input number to set (by default, this is 1, so it is ignored here.)
                Value = The value given to the input. 
                       True = Energy is flowing in. False = Energy NOT flowing in. */

            //By default, the Value sets the IsHot var. This behavior could change with
            //different objects.
            IsHot = Value;
        }//Input

        public virtual bool Output(short OutNum)
        {
            /* Reads the value for an output of an object.
               OutNum = The Output number to get. (by deault, there is only one, so
                        this is ignored, here.) */
            //By default, it simply returns the value of IsHot. This behavior could change
            //in different objects, though.
            return IsHot;
        }//Output

        /* Note: the Input and Output functions should ONLY be used when there is a
           logical input or output that can be utilized by the circuit. If an object
           does something that does not affect the circuit's logic, that should be
           implemented with different functions. */

        public short NumInputs
        {
            //Returns the number of Inputs the object has.
            get
            {
                return NumIns;
            }
        }

        public short NumOutputs
        {
            //Returns the number of Onputs the object has.
            get
            {
                return NumOuts;
            }
        }

        public string OutputsAsString()
        {
            StringBuilder S = new StringBuilder("");
            for (short I = 1; I == NumOuts; I++)
            {
                if (Output(I)) S.Append("1"); else S.Append("0");
            }
            return S.ToString();
        }

        public virtual void Reset()
        {
            /* This is used to reset the status of the object to all its default values.
            In this default case, it merely sets IsHot to False. But, this behavior could change
            in different types of objects. */
            IsHot = false;
        }

        public virtual bool HasDelay
        {
            /*  Certain objects have a 'delay' when used in a circuit. Or gates, for example,
                are commonly used to build Delay Circuits. Other objects, such as Nodes, must
                deliver their values to the next object right away, so there is no delay.
                This property lets the circuit know if the object has a delay, or not. */
            get
            {
                return false;
                // By default, we are setting it to False, for now.
            }
        }

        public override short ItemTypeIndex
        {
            get
            {
                return 0;
            }
        }

        public override string ItemTypeName
        {
            get
            {
                return "Logic Object";
            }
        }

        public override string HoverDescription
        {
            get
            {
                return "Generic Logic Object";
            }
        }

        public override bool VisualStatus()
        {
            return IsHot;
        }

        protected virtual void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            //Get coordinates of I/O leads BEFORE rotation.

            // IONum = Input or output number to get.
            // IsIn = true if Input lead, False if output lead.
            // OX = returns X coord.
            // OY = returns Y coord.
            // Returns -1 if invalid.
            if (IONum == 1)
            {
                OX = X;
                OY = Y;
            }
            else
            {
                OX = -1;
                OY = -1;
            }
        }

        public void GetIOLeadCoords(short IONum, bool IsIn, ref int LX, ref int LY)
        {
            //Get coordinates of I/O leads AFTER taking rotation into consideration.

            int KX = 0, KY = 0;
            GetInitialLeadCoords(IONum, IsIn, ref KX, ref KY);

            if (KX < 0 || KY < 0)
            {
                LX = -1;
                LY = -1;
                return;
            }

            AdjCoordsforRotation(ref KX, ref KY);

            LX = X + KX;
            LY = Y + KY;
        } //Get leads coords, after Rotation


    } //Generic Logic Object

    public class HotPoint : LogicObject
    {
        //Connects an Input into something that is always Hot. An alternative to using Not Gates, that does not have any Input leads.
        public HotPoint()
        {
            NumIns = 0;
            NumOuts = 1;
            IsHot = true;
        }

        public override short ItemTypeIndex
        {
            get
            {
                return 2;
            }
        }

        public override string ItemTypeName
        {
            get
            {
                return "Hot Point";
            }
        }

        public override string GetImage()
        {
            return "HotSpot";
        }

        public override string HoverDescription
        {
            get
            {
                return "Hot Point";
            }
        }

        public override short Height
        {
            get
            {
                return 10;

            }
        }

        public override short Width
        {
            get
            {
                return 5;
            }
        }

        public override void Input(short InNum, bool Value)
        {
            //Does Nothing, here.
        }

        public override bool Output(short OutNum)
        {
            return true; //Always Hot!
        }

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            OX = -1;
            OY = -1;

            if (!IsIn && IONum == 1)
            {
                OX = 2;
                OY = 0;
            } //Not IsIn

        } //Get Lead Coords

    } //Hot Point

    public class LogicGate : LogicObject
    {
        // This class obviously implements logic gates.

        // The type of Gate for each instance. Set externally by the GateType property.
        private GateTypes mGateType;

        // Since some gates can have 2 inputs, we need an extra variable to hold that
        // second input's status. The protected IsHot var. provides the first input's status.
        private bool IsHot2 = false;

        public GateTypes GateType
        {
            get
            {
                return mGateType;
            }
            set
            {
                mGateType = value;

                //All gates have only 1 output.
                NumOuts = 1;

                //Most gates have 2 inputs. Not gates only have 1.
                if (value == GateTypes.NotGate)
                {
                    NumIns = 1; // Not gates only have 1 input.
                    IsHot2 = true; // By default, the IsHot2 is always True for Not gates.
                }
                else
                {
                    NumIns = 2; // Other gates have 2 inputs.
                    IsHot2 = false; // By default, the IsHot2 is False for most gates.
                } //else
            } // set
        } // GateType

        public LogicGate()
        {
            //Default contructor

            //The default gate type is a Not Gate, so we will set the vars. apropriately.
            mGateType = GateTypes.NotGate;
            NumIns = 1;
            NumOuts = 1;
            IsHot2 = true;
        }

        public LogicGate(GateTypes GType)
        {
            //Constructor with GateType parameter.

            // Optionally, the GateType can be set after creation of the object, too.
            // GType = The type of gate to make this into.

            GateType = GType;
        } //constructor

        public override void Input(short InNum, bool Value)
        {
            //This merely sets the status of each input.
            if (InNum == 1)
                IsHot = Value; //IsHot is the status of the first input.
            else
                IsHot2 = Value; //IsHot2 is for the second one.
        }

        public override bool Output(short OutNum)
        {
            //Gets the output of the gate, based on what type it was.
            bool O; //This will be for the return value.

            switch (mGateType)
            {
                case GateTypes.NotGate:
                    O = !IsHot; //Returns the opposite of the first Input.
                    IsHot2 = true; //The second should always be True for Not gates.
                    break;
                //Other gates use logical operators between their input values.
                case GateTypes.OrGate:
                    O = IsHot || IsHot2; //or: O = IsHot | IsHot2;
                    break;
                case GateTypes.AndGate:
                    O = IsHot && IsHot2; //or: O = IsHot & IsHot2;
                    break;
                case GateTypes.XorGate:
                    O = IsHot ^ IsHot2;
                    break;
                case GateTypes.NorGate:
                    O = !(IsHot || IsHot2);
                    break;
                case GateTypes.NandGate:
                    O = !(IsHot && IsHot2);
                    break;
                case GateTypes.XNorGate:
                    O = !(IsHot ^ IsHot2);
                    break;
                default:
                    O = false;
                    break;
            } //switch

            return O;
        } //output

        public override void Reset()
        { //Resets the internal logic values to their default setting.
            if (mGateType == GateTypes.NotGate)
            {
                IsHot = false;
                IsHot2 = true; //Remember, if this is a Not gate, IsHot2 should be True.
            }
            else
            {
                IsHot = false;
                IsHot2 = false;
            }
        }

        public override bool HasDelay
        {
            get
            { //Logic gates have a delay in them! 
                return true;
            }
        }

        public override short ItemTypeIndex
        {
            get
            {
                short O;
                switch (mGateType)
                {
                    case GateTypes.NotGate:
                        O = 3;
                        break;
                    case GateTypes.AndGate:
                        O = 4;
                        break;
                    case GateTypes.OrGate:
                        O = 5;
                        break;
                    case GateTypes.XorGate:
                        O = 6;
                        break;
                    case GateTypes.NorGate:
                        O = 7;
                        break;
                    case GateTypes.NandGate:
                        O = 8;
                        break;
                    case GateTypes.XNorGate:
                        O = 9;
                        break;
                    default:
                        O = 3;
                        break;
                } //switch
                return O;
            } //get
        } //ItemTypeIndex

        public override string ItemTypeName
        {
            get
            {
                string O;
                switch (mGateType)
                {
                    case GateTypes.NotGate:
                        O = "Not Gate";
                        break;
                    case GateTypes.OrGate:
                        O = "Or Gate";
                        break;
                    case GateTypes.AndGate:
                        O = "And Gate";
                        break;
                    case GateTypes.XorGate:
                        O = "Xor Gate";
                        break;
                    case GateTypes.NorGate:
                        O = "Nor Gate";
                        break;
                    case GateTypes.NandGate:
                        O = "NAnd Gate";
                        break;
                    case GateTypes.XNorGate:
                        O = "XNor Gate";
                        break;
                    default:
                        O = "Unknown";
                        break;
                } //switch
                return O;
            } //get
        } //ItemTypeName

        public override string GetImage()
        {
            string O = "";
            switch (mGateType)
            {
                case GateTypes.NotGate:
                    O = "Not";
                    break;
                case GateTypes.OrGate:
                    O = "Or";
                    break;
                case GateTypes.AndGate:
                    O = "And";
                    break;
                case GateTypes.XorGate:
                    O = "Xor";
                    break;
                case GateTypes.NorGate:
                    O = "NOr";
                    break;
                case GateTypes.NandGate:
                    O = "NAnd";
                    break;
                case GateTypes.XNorGate:
                    O = "XNor";
                    break;
            } //switch

            if (Output(1))
                O = "H" + O;

            return O;
        } //GetImage

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            OX = -1;
            OY = -1;
            if (mGateType == GateTypes.NotGate)
            {
                if (IONum == 1)
                {
                    if (IsIn)
                    {
                        OX = 10;
                        OY = 20;
                    } //IsIn
                    else
                    {
                        OX = 10;
                        OY = 0;
                    } //IsIn
                } //IONum = 1
            } //GateType = NotGate
            else
            {
                if (IsIn & IONum <= 2 & IONum > 0)
                {
                    if (IONum == 1)
                    {
                        OX = 6;
                        OY = 20;
                    } //IONum = 1
                    else
                    {
                        OX = 14;
                        OY = 20;
                    } //IONum = 2
                } //IsIn
                if (!IsIn & IONum == 1)
                {
                    OX = 11;
                    OY = 0;
                } //Not IsIn
            } //GateType <> NotGate

        } //GetLeadCoords

        public override string HoverDescription
        {
            get
            {
                string R = ItemTypeName;

                R += " \nStatus: ";

                if (Output(1))
                    R += "HOT!";
                else
                    R += "Cold";

                return R;
            } //get
        } //Hover Description

        public override bool VisualStatus()
        {
            if (mGateType == GateTypes.OrGate |
                mGateType == GateTypes.AndGate |
                mGateType == GateTypes.XorGate)
                return Output(1);
            else
                return !Output(1);
        }

    } //Logic Gate

    public class Node : LogicObject
    {
        /* Implements a node object, which distributes a signal to two or more other objects.
    A Node only has one input, but can have (almost) any number of outputs. Though, the UI will likely only support up to 4. */

        /*  Default behavior of the LogicObject base class is otherwise sufficent for this
            object. 
            For example, it doesn't matter which OutNum is used in the Output function,
            the signal will be the same as IsHot. */

        public Node(short NOuts)
        {
            //The number of outputs can optionally be set upon creation.
            NumOuts = NOuts;
            NumIns = 1;
        }

        public Node()
        {
            //By default, there are only 2 outputs.
            NumOuts = 2;
            NumIns = 1;
        }

        public override bool HasDelay
        {
            //Nodes must not have a delay!
            get
            {
                return false;
            }
        }

        public override short ItemTypeIndex
        {
            get
            {
                short O;

                switch (NumOuts)
                {
                    case 2:
                        O = 10;
                        break;
                    case 3:
                        O = 11;
                        break;
                    default:
                        O = 12; //4-Way
                        break;
                }

                return O;
            }
        }

        public override string ItemTypeName
        {
            get
            {
                return "Node " + NumOuts;
            }
        }

        public override string GetImage()
        {
            string S;

            switch (NumOuts)
            {
                case 2:
                    S = "Node";
                    break;
                case 3:
                    S = "Node3";
                    break;
                default:
                    S = "Node4";
                    break;
            }

            if (IsHot) S = "H" + S;

            return S;
        }

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            OX = -1;
            OY = -1;

            if (IsIn && IONum == 1)
            {
                OX = 10;
                OY = 11;
            }
            if (!IsIn && IONum <= NumOuts && IONum > 0)
            {
                switch (IONum)
                {
                    case 1:
                        OX = 10;
                        OY = 0;
                        break;
                    case 2:
                        OX = 10;
                        OY = 22;
                        break;
                    case 3:
                        OX = 0;
                        OY = 11;
                        break;
                    case 4:
                        OX = 22;
                        OY = 11;
                        break;
                } //switch
            } //Not IsIn

        } //Get Lead Coords

        public override string HoverDescription
        {
            get
            {
                string S;
                S = "Node (" + NumOuts + "-Way)\nStatus: ";
                if (IsHot) S += "HOT!";
                else S += "Cold";

                return S;
            }
        }

    } //Node

    public class FlipFlop : LogicObject
    {
        //A Flip-Flop class, which is like a logic-powered switch

        //For sake of code brevity, later on, we are using two vars to store which
        //side is flipped, even though, logically, one would be sufficient.
        private bool FSide1 = true;
        private bool FSide2 = false;

        //In an effort to make desired functionality self-contained in this
        //object, we are going to need a couple of more variables.
        private bool OldF1 = true;
        private bool SetOldF1 = false;

        public FlipFlop()
        {   //A Flipflop has 2 inputs and 2 outputs.
            NumIns = 2;
            NumOuts = 2;
        }

        public override void Input(short InNum, bool Value)
        {

            if (!SetOldF1)
            {
                OldF1 = FSide1;
            }

            if (InNum == 1)
                FSide1 = Value;
            else FSide2 = Value;

            if (SetOldF1)
            {
                if ((FSide1 && FSide2) || (!FSide1 && !FSide2))
                {
                    FSide1 = OldF1;
                    FSide2 = !OldF1;
                }
            }
            else
            {
                SetOldF1 = true;
            }

            IsHot = FSide2;

        } //Input

        public override void AfterClockTick()
        {
            SetOldF1 = false;
        }

        public override bool Output(short OutNum)
        {
            bool O; //'This will be for the return value.

            if (OutNum == 1) O = FSide1;
            else O = FSide2;

            return O;
        } //Output

        public override void Reset()
        {
            FSide1 = true;
            FSide2 = false;
            IsHot = false;
        }

        public override bool HasDelay
        {
            //Not sure if FFs need a delay or not. Might change this in future.
            //Delay = True ensures no irreconsilable FFs.
            //Delay = False behaves like RO and RB games.
            get
            {
                return true;
            }
        }

        public override short ItemTypeIndex
        {
            get
            {
                return 13;
            }
        }

        override public string ItemTypeName
        {
            get
            {
                return "Flip-Flop";
            }
        }

        public override string GetImage()
        {
            string S;

            if (!IsHot) S = "FlipFlop1";
            else S = "FlipFlop2";

            return S;
        }

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            OX = -1;
            OY = -1;

            if (IONum > 0 && IONum <= 2)
            {
                if (IsIn)
                {
                    if (IONum == 1)
                    {
                        OX = 6;
                        OY = 20;
                    }
                    else
                    {
                        OX = 15;
                        OY = 20;
                    }
                }
                else
                {
                    if (IONum == 1)
                    {
                        OX = 5;
                        OY = 0;
                    }
                    else
                    {
                        OX = 16;
                        OY = 0;
                    }
                } //IsIn
            } //IONum > 0 and IONum <=2

        } //GetLeadCoords

        public override string HoverDescription
        {
            get
            {
                string S;

                S = "Flip-Flop\nStatus: ";
                if (FSide1)
                    S += "Left Side Hot";
                else
                    S += "Right Side Hot";

                return S;
            }
        }

        protected override string AuxDataSaveLine()
        {
            string S;

            if (FSide1) S = "0";
            else S = "1";

            return S;
        }

        protected override void LoadAuxData(string AuxData)
        {
            if (AuxData.Trim() == "1")
            {
                FSide1 = false;
                FSide2 = true;
                IsHot = true;
            }
            else Reset();
        }

    } //Flip-Flop


} //namespace