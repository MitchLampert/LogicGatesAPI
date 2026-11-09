
namespace MSLLogicGatesAPI
{
    public class ToggleSwitch : LogicObject, IUserSwitchable
    {
        /*
        A Toggle Switch is really a user-pushable button. By default, its value is always
        true. But, can be turned off by the user.
        Logically, it has no inputs that can be attached to circuits, but its value
        can be inputted by the user. */
        private bool InitHot; //Saves the initial state of the spot.

        public ToggleSwitch()
        {
            IsHot = true;
            InitHot = true;
            NumIns = 0;
            NumOuts = 1;
        } //constructor

        public ToggleSwitch(bool InitSet)
        {   //Optionally, you can set the initial value of the spot.
            IsHot = InitSet;
            InitHot = InitSet;
            NumIns = 0;
            NumOuts = 1;
        } //construct. w/ param.

        public override void Input(short InNum, bool Value)
        {
            //This object has no logical inputs, so we do nothing here.
            //User setting is done through the IUserSwitchable interface.
        }

        public void UserSetsSwitch(bool Value)
        {
            /*
        Sets the value of the switch. This CAN NOT be done via the Input
        function, because the switch is not supposed to have any inputs
        that can be used by a circuit. Forcing its value to be set in a 
        different manner, like this, forces the interface to allow only
        the user to determine its value.  It can not be set by internal logic. */
            IsHot = Value;
        }

        public bool CurrentSwitchSetting()
        {
            return IsHot;
        }

        protected override string AuxDataSaveLine()
        {
            if (IsHot) return "1"; else return "0";
        }

        protected override void LoadAuxData(string AuxData)
        {
            if (AuxData.Trim() == "1")
            {
                IsHot = true;
                InitHot = true;
            }
            else
            {
                IsHot = false;
                InitHot = false;
            }
        } //LoadAuxData

        public override void Reset()
        {//Resets the switch to its initial value.
            IsHot = InitHot;
        }

        public override bool HasDelay
        {
            get
            { //Toggle switches do not have a delay. The instant is value is changed by
                //the user, that change goes right to the next object.
                return false;
            }
        }

        public override short ItemTypeIndex
        {
            get
            {
                return 14;
            }
        }

        public override string ItemTypeName
        {
            get
            {
                return "Toggle Switch";
            }
        }

        public override string GetImage()
        {
            string S;

            if (IsHot)
                S = "HSwitch";
            else
                S = "Switch";

            return S;
        }

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            if (!IsIn && IONum == 1)
            {
                OX = 10;
                OY = 0;
            }
            else
            {
                OX = -1;
                OY = -1;
            }
        } //Get Lead Coords

        public override string HoverDescription
        {
            get
            {
                string S = "Toggle Switch\nStatus: ";
                if (IsHot) S += "HOT!"; else S += "Cold";
                return S;
            }
        }

        public int SwitchCoordX
        {
            get
            {
                //The switch will be centered on the middle of the bottom row.
                int CX = 0; int CY = 0;
                FindCenter(ref CX, ref CY);

                return CX;
            }
        }

        public int SwitchCoordY
        {
            get
            {
                return Y + Height;
            }
        }

    } //Toggle Switch

    public class Indicator : LogicObject
    {
        //An indicator has no logical output, but has one input.

        //Its "output" is actually something to be seen only by the user. It has
        //no output that can be used internally by the circuit.

        public Indicator()
        {
            NumIns = 1;
            NumOuts = 0;
        }

        public override bool Output(short OutNum)
        {
            //Since the indicator has no logical output, we do nothing here.
            return false; //compiler forces us to return something.
        }

        public override bool VisualStatus()
        {
            /*
        This allows the "output" to be seen by the user. It CAN NOT be done with the
        Output function, because this is not supposed to have any logical output.
        By forcing the status to be seen in this different way, it enforces that
        only the user can see it, not the circuit.
             */
            return IsHot;
        }

        public override bool HasDelay
        {
            get
            {
                //Indicators won't have a delay, for now. I might change this.
                return false;
            }
        }

        public override short ItemTypeIndex
        {
            get
            {
                return 15;
            }
        }

        public override string ItemTypeName
        {
            get
            {
                return "Indicator";
            }
        }

        public override string GetImage()
        {
            string O = "IndLED";
            if (IsHot) O = "H" + O;
            return O;
        } //GetImage

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            if (IsIn && IONum == 1)
            {
                OX = 10;
                OY = 20;
            }
            else
            {
                OX = -1;
                OY = -1;
            }
        } //Get Lead Coords

        public override string HoverDescription
        {
            get
            {
                string S = ItemTypeName + "\nStatus: ";

                if (IsHot) S += "HOT!"; else S += "Cold";

                return S;
            }
        }

    } //Indicators

    public class InLineSwitch : LogicObject, IUserSwitchable
    {
        private bool mIsClosed = true;

        private bool InitClosed = true; //Saves the initial state.

        public InLineSwitch()
        {
            //By default, the switch is closed.
            mIsClosed = true;
            InitClosed = true;
            NumIns = 1;
            NumOuts = 1;
        }

        public InLineSwitch(bool InitSet)
        {
            //Optionally, you can set the initial value of the spot.
            mIsClosed = false;
            InitClosed = false;
            NumIns = 1;
            NumOuts = 1;
        }

        public override void Input(short InNum, bool Value)
        {
            if (mIsClosed)
                IsHot = Value;
            else
                IsHot = false;
        }

        public override void Reset()
        {
            mIsClosed = InitClosed;
            IsHot = false;
        }

        public void UserSetsSwitch(bool Value)
        {
            mIsClosed = Value;
            if (mIsClosed == false) IsHot = false;
        }

        public bool CurrentSwitchSetting()
        {
            return mIsClosed;
        }

        public int SwitchCoordX
        {
            get
            {
                //Switch, for this, will be in the exact center, for now.
                int CX = 0; int CY = 0;
                FindCenter(ref CX, ref CY);
                return CX;
            }
        }

        public int SwitchCoordY
        {
            get
            {
                int CX = 0; int CY = 0;
                FindCenter(ref CX, ref CY);
                return CY;
            }
        }

        public override bool VisualStatus()
        {
            return mIsClosed;
        }

        public override bool HasDelay
        {
            get
            {
                return false;
            }
        }

        public override short ItemTypeIndex
        {
            get
            {
                return 16;
            }
        }

        public override string ItemTypeName
        {
            get
            {
                return "In-Line Switch";
            }
        }

        public override string GetImage()
        {
            string R;
            if (mIsClosed)
            {
                if (IsHot)
                    R = "HCircSwitch";
                else
                    R = "CircSwitch";
            }
            else
                R = "CircSwitchO";

            return R;
        }

        public override string HoverDescription
        {
            get
            {
                string R = "In-Line Switch\nStatus: ";
                if (mIsClosed)
                {
                    if (IsHot)
                        R += "Closed, HOT!";
                    else
                        R += "Closed, Cold";
                }
                else
                    R += "Open, Cold";

                return R;
            }
        }

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            OX = -1;
            OY = -1;

            if (IONum == 1)
            {
                if (IsIn)
                {
                    OX = 13;
                    OY = 20;
                } //IsIn
                else
                {
                    OX = 12;
                    OY = 0;
                }
            } //IONum = 1
        } //get leads

        protected override string AuxDataSaveLine()
        {
            if (mIsClosed) return "1"; else return "0";
        }

        protected override void LoadAuxData(string AuxData)
        {
            if (AuxData.Trim() == "1")
            {
                mIsClosed = true;
                InitClosed = true;
            }
            else
            {
                mIsClosed = false;
                InitClosed = false;
            }
        } //LoadAuxData

    }//In-Line Switch

    public class RandomBit : LogicObject
    {
        /* A RandomBit has no inputs and 1 output. However, the value of its output
           is random. It will spit either True or False, roughly 50% of the time for each. */

        private Random R = new Random((int)DateTime.Now.Ticks & int.MaxValue);

        public RandomBit()
        {
            NumIns = 0;
            NumOuts = 1;

            //Initial value is randomly determined, too:
            AfterClockTick(); //and this the best way to do it!
        }

        public override void AfterClockTick()
        {
            bool O; //for returning Output
            int I; //for getting random number

            I = R.Next(1, 1000);

            //Return a value based on random number.
            if (I <= 500)
                O = false;
            else
                O = true;

            IsHot = O;

        }//AfterPowerTick

        public override void Reset()
        {
            AfterClockTick(); //Random after reset, too.
        }

        public override bool HasDelay
        {
            /* Random bits should probably have a delay, since that will prevent it
            from getting checked too often by other functions. But, I really
            don't think it matters much, here. */
            get
            {
                return true;
            }
        }

        public override void Input(short InNum, bool Value)
        {
            //Do nothing here. Prevents interferance from logic circuits
        }

        public override short ItemTypeIndex
        {
            get
            {
                return 17;
            }
        }

        public override string ItemTypeName
        {
            get
            {
                return "Randomizer";
            }

        }

        public override string GetImage()
        {
            string S;

            if (IsHot) S = "HRando"; else S = "ORando";

            return S;
        }

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            if (!IsIn && IONum == 1)
            {
                OX = 10;
                OY = 0;
            }
            else
            {
                OX = -1;
                OY = -1;
            }
        }//GetLeadCoords

        public override string HoverDescription
        {
            get
            {
                string S = "Randomizer\nStatus: ";
                if (IsHot) S += "HOT!"; else S += "Cold";
                return S;
            }
        }

    } //Random Bit

    public class BigPushButton : LogicObject, ISearchesThroughComponents
    {
        /* Activates when almost any type of component in the playfield (with a small number of exceptions) touches it. NOT us humans!
         * This will usually be a Robot or a Shape. But, most things will suffice. */

        public override void Input(short InNum, bool Value)
        {
            //Do nothing here.
        }

        public override short ItemTypeIndex
        {
            get { return 18; }
        }

        public override string ItemTypeName
        {
            get { return "Push Button"; }
        }

        public override bool HasDelay
        { get { return true; } }

        public override string GetImage()
        {
            if (IsHot) return "HButton";
            else return "PushButton";
        }

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            if (!IsIn && IONum == 1)
            {
                OX = 20;
                OY = 10;
            }
            else
            {
                OX = -1;
                OY = -1;
            }
        }

        public void SearchThroughComponents(List<BasicComponent> ThePileOfComponents)
        {
            int[] PleaseIgnore = { 18, 23, 24 };
            //We are ignoring the following: Floating Images, Text, Chip IO, other Push Buttons

            IsHot = false;

            foreach (BasicComponent C in ThePileOfComponents)
            {
                if (PleaseIgnore.Contains(C.ItemTypeIndex)) continue;

                //See of object is overlapping this one.
                //Test 1
                if (((C.X >= X && C.X <= X2) || (C.X2 >= X && C.X2 <= X2)) &&
                    ((C.Y >= Y && C.Y <= Y2) || (C.Y2 >= Y && C.Y2 <= Y2)))
                    IsHot = true;

                //Test 2
                if (((X >= C.X && X <= C.X2) || (X2 >= C.X && X2 <= C.X2)) &&
                    ((Y >= C.Y && Y <= C.Y2) || (Y2 >= C.Y && Y2 <= C.Y2)))
                    IsHot = true;

            } //For Each component
        } //Search Through Components

        public override string HoverDescription
        {
            get
            {
                string S;
                S = "Push Button\nStatus: ";
                if (IsHot) S += "HOT!";
                else S += "Cold";

                return S;
            }
        }
    } //Push Button
} //namespace