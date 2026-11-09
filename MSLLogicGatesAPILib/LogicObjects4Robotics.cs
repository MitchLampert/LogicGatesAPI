using System.IO;
using System.Text;

namespace MSLLogicGatesAPI
{
    public class Antenna : LogicObject, IWireless
    {
        //IsHot will be used to see if object is sending a signal.

        //IsRec will be used if recieving one:
        private bool IsRec;

        public Antenna()
        {
            NumIns = 1;
            NumOuts = 1;
            IsHot = false;
            IsRec = false;
        } //initializer

        public override void Input(short InNum, bool Value)
        {
            if (Value)
            {
                IsHot = true;
                IsRec = true;
            }
            else
                IsHot = false;
            //NOT setting IsRec on the else statement!
        } //Input

        public override bool Output(short OutNum)
        {
            return IsRec;
        }

        public bool WirelessSignal
        {
            get
            {
                return IsHot;
                //For Get, see if broadcasting, so we use IsHot
            }
            set
            {
                IsRec = value;
                //For Set, we are setting the Rec'd flag, so use IsRec.
            }
        } //Wireless Signal

        public override void Reset()
        {
            IsHot = false;
            IsRec = false;
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
                return 19;
            }
        }

        override public string ItemTypeName
        {
            get
            {
                return "Antenna";
            }
        }

        public override string GetImage()
        {
            string S;

            if (IsHot) S = "AntennaSend";
            else
            {
                if (IsRec) S = "AntennaRec";
                else S = "Antenna";
            }

            return S;
        } //Get Image

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            if (IONum == 1)
            {
                if (IsIn)
                {
                    OX = 15;
                    OY = 20;
                }
                else
                {
                    OX = 4;
                    OY = 22;
                }
            } //IONum = 1
            else
            {
                OX = -1;
                OY = -1;
            }
        } //GetLeadCoords

        public override string HoverDescription
        {
            get
            {
                string S = "Antenna\nStatus: ";

                if (IsHot) S += "Sending";
                else
                {
                    if (IsRec) S += "Receiving";
                    else S += "Off";
                }

                return S;
            }
        } //HoverDescrip

        public override bool VisualStatus()
        {
            return IsRec;
        }

    } //Antenna for wireless signals.

    public class Grabber : LogicObject, IReadsGrabbableObjects
    {
        private class NullGrabbedObj : CanBeGrappledWith { }

        private CanBeGrappledWith NLGrabbed = new NullGrabbedObj();

        public Grabber()
        {
            NumIns = 1;
            NumOuts = 1;
        }

        private bool IsReady = false;

        //The grabber could find itself in situations
        //where it falls into an infinite loop. See FastInverter obj. for more
        //details on how this counter handles situation.
        //Example: In-Room sensor is connected to grabber. (Sensors do not detect
        //      grabbed objects.) If the only object in room that sensor is
        //      detecting is overlapping the claw, then circuit could fall into
        //      a loop: Grabber grabs obj, so sensor no longer sees it, so it
        //      deactivates the grabber, which lets go of the obj., which the
        //      sensor now sees, so the grabber grabs it.... ad infinitum.
        //This will help us prevent that from happening in the HasDelay function:
        private byte HitCount = 0;

        //IsHot used to determine if we have grabbed an object.

        public override void Input(short InNum, bool Value)
        {
            IsReady = Value;
            if (IsReady == false)
            {  //make sure we let go, if power is cut:
                if (IsHot) NLGrabbed.IsGrabbed = false;
                IsHot = false;
            }
        }

        public void ReadGrabbableObjects(List<CanBeGrappledWith> ObjLocs)
        {

            if (IsReady)
            {
                foreach (CanBeGrappledWith L in ObjLocs)
                {
                    if (IsHot == false && L.IsGrabbed == false &&
                        ((L.X >= X && L.X <= X + 22) ||
                        (L.X + 22 >= X && L.X + 22 <= X + 22)) &&
                        ((L.Y >= Y && L.Y <= Y + 22) ||
                        (L.Y + 22 >= Y && L.Y + 22 <= Y + 22)))
                    {
                        CanBeGrappledWith L2 = L;
                        IsHot = true;
                        NLGrabbed = L2; //can't ref. foreach var.
                        NLGrabbed.IsGrabbed = true;
                    } //IsHot = False and all that other stuff
                } //for each
            } //IsReady
            else
            {
                if (IsHot) NLGrabbed.IsGrabbed = false;
                IsHot = false;
            } //else not IsReady

        } //read grabbable objects

        public override void Reset()
        {
            NLGrabbed.IsGrabbed = false;
            IsHot = false;
            IsReady = false;
            HitCount = 0;
        }

        public override bool HasDelay
        {
            get
            {
                HitCount++;

                //See notes. This helps remove possible infinite loops:
                if (HitCount >= 3)
                    return true;
                else
                    return false;
            }
        }

        public override void AfterClockTick()
        {
            if (IsReady)
            {
                if (IsHot)
                {
                    NLGrabbed.X = X + 10;
                    NLGrabbed.Y = Y + 10;
                } //IsHot
            } //IsReady
            else
            {
                if (IsHot) NLGrabbed.IsGrabbed = false;
                IsHot = false;
            } //else IsReady
        } //AfterPowerTick

        public override short ItemTypeIndex
        {
            get
            {
                return 20;
            }
        }

        public override string ItemTypeName
        {
            get
            {
                return "Grabber";
            }
        }

        public override string GetImage()
        {
            string S;

            if (IsReady)
            {
                if (IsHot) S = "Claw3";
                else S = "Claw2";
            }
            else
                S = "Claw";

            return S;
        }

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            OX = -1;
            OY = -1;

            if (IONum == 1)
            {
                if (IsIn)
                {
                    OX = 1;
                    OY = 1;
                }
                else
                {
                    OX = 8;
                    OY = 0;
                }
            }
        } //GetLeadCoords

        public override string HoverDescription
        {
            get
            {
                string S = "Grabber\nStatus: ";

                if (IsReady)
                {
                    if (IsHot) S += "HOT! Grabbing Object: " + NLGrabbed.ItemTypeName;
                    else S += "Warm. Ready to grab something.";
                }
                else
                    S += "Cold";

                return S;
            }
        }

    } //Grabber

    public class ChipIO : LogicObject
    {
        //used inside Chip Templates to mark inputs and outputs of that chip.
        private bool mIsInput = true;
        //Can either be a single Input or a single Output

        //NOTE: mIsInput and IsInput are from OUTSIDE the chip's perspective.
        //Inside is going to be the opposite. A Chip Input will need an output
        //lead connector. An Output will need an input one.

        private short chipLeadNumber = 1;
        public short ChipLeadNumber { get => chipLeadNumber; set => chipLeadNumber = value; }

        public ChipIO()
        {
            mIsInput = true;
            NumIns = 0;
            NumOuts = 1;
        }

        public ChipIO(bool IsIn)
        {
            mIsInput = IsIn;

            if (!IsIn)
            {
                NumIns = 1;
                NumOuts = 0;
            }
            else
            {
                NumIns = 0;
                NumOuts = 1;
            }
        }

        public ChipIO(short LNum)
        {
            mIsInput = true;
            NumIns = 0;
            NumOuts = 1;
            ChipLeadNumber = LNum;
        }

        public ChipIO(bool IsIn, short LNum)
        {
            mIsInput = IsIn;

            if (!IsIn)
            {
                NumIns = 1;
                NumOuts = 0;
            }
            else
            {
                NumIns = 0;
                NumOuts = 1;
            }
            ChipLeadNumber = LNum;
        }

        public bool IsInput
        {
            get
            {
                return mIsInput;
            }
            set
            {
                mIsInput = value;

                if (!mIsInput)
                {
                    NumIns = 1;
                    NumOuts = 0;
                }
                else
                {
                    NumIns = 0;
                    NumOuts = 1;
                }
            }
        } //IsInput prop.

        public override void Input(short InNum, bool Value)
        {
            if (!mIsInput && InNum == 1) IsHot = Value;
        }

        public override bool Output(short OutNum)
        {
            if (mIsInput && OutNum == 1)
                return IsHot;
            else
                return false;
        }

        public override short ItemTypeIndex
        {
            get
            {
                if (mIsInput)
                    return 23;
                else
                    return 24;
            }
        }

        public override string ItemTypeName
        {
            get
            {
                if (mIsInput)
                    return "Chip Input Lead";
                else
                    return "Chip Output Lead";
            }
        }

        public override string GetImage()
        {
            string R = "Chip";

            if (IsInput) R += "I"; else R += "O";

            if (IsHot) R = "H" + R;

            return R;
        }

        public override bool HasDelay
        {
            get
            {
                return false;
            }
        }

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            OX = -1;
            OY = -1;
            if (IONum == 1)
            {
                OX = 10;
                if (!IsIn) OY = 21; else OY = 20;
            }
        } //get "knobbies"

        public override string HoverDescription
        {
            get
            {
                StringBuilder S = new StringBuilder("Chip ");

                if (IsInput) S.Append("Input"); else S.Append("Output");

                S.Append("\nStatus: ");

                if (IsHot) S.Append("HOT!"); else S.Append("Cold");

                S.Append("\nChip Lead Number: " + ChipLeadNumber);

                return S.ToString();
            }
        } //Hover descrip

        protected override string AuxDataSaveLine()
        {
            return ChipLeadNumber.ToString();
        }

        protected override void LoadAuxData(string AuxData)
        {
            try
            {
                ChipLeadNumber = short.Parse(AuxData);
            }
            catch
            {
                ChipLeadNumber = 1;
            }
        }

        public override bool VisualStatus()
        {
            return IsHot;
        }

        public void EmbeddedInput(bool Value)
        {
            //This is called from a chip shell to set the input, since it
            //can not do it from the Input function.
            if (IsInput) IsHot = Value;
        }

       
    } //Chip I/O

    public abstract class InternalCircuit : LogicObject, IReadsGrabbableObjects
    {
        protected LogicBoardCircuit privC = new LogicBoardCircuit();

        public InternalCircuit()
        {
            privC.IsEmbeddedCircuit = true;
        }

        public string InternalBoardTitle
        {
            get
            {
                return privC.CircuitBoardTitle;
            }
            set
            {
                privC.CircuitBoardTitle = value;
            }
        }

        public override void Reset()
        {
            privC.ResetPower();
        }

        public override bool HasDelay
        {
            get
            {
                return false;
            }
        }

        public override void AfterClockTick()
        {
            privC.PowerFlow();
            privC.AfterClockTick();
        }

        public void ReadGrabbableObjects(List<CanBeGrappledWith> NonLogObjs)
        {
            foreach (BasicComponent O in privC.TheComponents)
            {
                if (O is IReadsGrabbableObjects)
                {
                    //Before we read obj, rotate items inside of it according to rotation of this circuit:
                    ObjRotationDirs OldRot = InternalRotation(O);

                    O.X = X2 - O.Width;
                    O.Y = Y2 - O.Height;

                    IReadsGrabbableObjects I = (IReadsGrabbableObjects)O;
                    I.ReadGrabbableObjects(NonLogObjs);

                    //restore original rotation:
                    O.ObjRotation = OldRot;
                }
            }
        } //read non logs

        public virtual bool AddComponent(BasicComponent BasicObj)
        {
            BasicObj.X = X; //Component will be at same spot, in robot...
            BasicObj.Y = Y; //...as the robot is in the room.
            return privC.AddObject(BasicObj);
        }

        public virtual bool AddComponent(short ObjIndex)
        {
            return privC.AddObject(ObjIndex, X, Y);
        }

        public virtual bool RemoveComponent(int ObjID)
        {
            return privC.RemoveObject(ObjID);
        }


        //We are NOT using AuxData to store embedded circuits, because we need a LOT more space than what it provides.
        //Instead. Internal Circuits will be handled with special code to load and save all of its content, by the larger save/load functions of the game board.

        public virtual string SaveInternalContent(int EmbedLevel)
        {
            return privC.SaveGameBoardContent(EmbedLevel);
        }

        public virtual string LoadInternalContent(string TheContent)
        {
            return privC.LoadAllGameBoardContent(TheContent);
        }

        protected ObjRotationDirs InternalRotation(BasicComponent O)
        {
            ObjRotationDirs OldRot = O.ObjRotation;

            //Add rotation to given object by how much robot is rotated, if any:
            switch (ObjRotation) //rotation of this robot
            {
                case ObjRotationDirs.ObjRot90:
                    O.ObjRotationDegrees = (short)(O.ObjRotationDegrees + 90);
                    break;
                case ObjRotationDirs.ObjRot180:
                    O.ObjRotationDegrees = (short)(O.ObjRotationDegrees + 180);
                    break;
                case ObjRotationDirs.ObjRot270:
                    O.ObjRotationDegrees = (short)(O.ObjRotationDegrees + 270);
                    break;
            } //switch ObjRotation of robot

            return OldRot;
        }

    } //Internal Circuit object

    public class ChipShell : InternalCircuit
    {
        //Chips contain their own, internal circuits! W00t!

        private short mChipNumber = 0;
        public short ChipNumber { get { return mChipNumber; } set { mChipNumber = value; } }

        private const short MaxLeads = 16;

        private const int ChipInTIndexID = 23;
        private const int ChipOutIndexID = 24;

        private string DefaultFileExtention = "";
        private string RecentFilePath = ""; //This will NOT be saved with the file. This just holds the data temporarily.

        public ChipShell()
        {
            NumIns = 0;
            NumOuts = 0;
            privC.CircuitType = CircuitTypes.CircTypeChip;
        }

        public ChipShell(short ChipNum)
        {
            NumIns = 0;
            NumOuts = 0;
            privC.CircuitType = CircuitTypes.CircTypeChip;
            ChipNumber = ChipNum;
        }

        //Number of inputs and outputs depends on number of ChipIO parts inside of it.

        private Dictionary<short, byte> LeadStatus = new Dictionary<short, byte>();
        //short key = lead number (1 to 16, for now. May go higher, later.)
        //byte value = staus: 
        //            0 = nothing
        //            1 = Cold Input
        //            2 = Cold Output
        //            3 = Hot Input
        //            4 = Hot Output

        public void ReCountIOLeads()
        {
            NumIns = 0;
            NumOuts = 0;

            LeadStatus = new Dictionary<short, byte>();

            foreach (BasicComponent O in privC.TheComponents)
            {
                //new code:
                if (O is ChipIO)
                {
                    short CS = 0;
                    ChipIO C = (ChipIO)O;
                    if (C.IsInput)
                    {
                        NumIns++;
                        LeadStatus[C.ChipLeadNumber] = 1;
                        CS = C.ChipLeadNumber;
                    }
                    else
                    {
                        NumOuts++;
                        CS = (short)((MaxLeads + 1) - C.ChipLeadNumber);
                        LeadStatus[CS] = 2;
                    }
                    if (C.VisualStatus()) LeadStatus[CS] += 2;
                }
            } //for each object

            for (short I = 1; I <= MaxLeads; I++)
            {
                if (!LeadStatus.ContainsKey(I))
                    LeadStatus[I] = 0;
            }
        }// recount leads

        public override void Input(short InNum, bool Value)
        {
            foreach (BasicComponent O in privC.TheComponents)
            {
                if (O.ItemTypeIndex == ChipInTIndexID) //Chip Input Lead
                {
                    ChipIO C = (ChipIO)O;
                    if (C.ChipLeadNumber == InNum)
                        C.EmbeddedInput(Value);
                }
            } //O
        } //input

        public override bool Output(short OutNum)
        {
            bool R = false;

            foreach (BasicComponent O in privC.TheComponents)
            {
                if (O.ItemTypeIndex == ChipOutIndexID) //Chip Output Lead
                {
                    ChipIO C = (ChipIO)O;
                    if (C.ChipLeadNumber == OutNum)
                        R = C.VisualStatus();
                }
            } //O

            return R;
        } //output

        public override bool AddComponent(BasicComponent BasicObj)
        {
            bool R;
            R = base.AddComponent(BasicObj);
            if (R)
            {
                if (BasicObj.ItemTypeIndex == ChipInTIndexID) NumIns++;
                if (BasicObj.ItemTypeIndex == ChipOutIndexID) NumOuts++;
            }
            return R;
        }

        public override bool AddComponent(short ObjIndex)
        {
            bool R;
            R = base.AddComponent(ObjIndex);
            if (R)
            {
                if (ObjIndex == ChipInTIndexID) NumIns++;
                if (ObjIndex == ChipOutIndexID) NumOuts++;
            }
            return R;
        }

        public override bool RemoveComponent(int ObjID)
        {
            bool R;
            R = base.RemoveComponent(ObjID);
            if (R)
            {
                BasicComponent O = privC.TheComponents[ObjID];
                if (O.ItemTypeIndex == ChipInTIndexID) NumIns--;
                if (O.ItemTypeIndex == ChipOutIndexID) NumOuts--;
            }
            return R;
        }

        
        public override short ItemTypeIndex => 25;

        public override string ItemTypeName => "Chip";

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            OX = -1; OY = -1;

            if ((IONum < 0) || (IONum > MaxLeads) ||
                 (IsIn && IONum > NumIns) ||
                 (!IsIn && IONum > NumOuts))
                return;

            ReCountIOLeads();

            //If Output, then lead will be on opposite site.
            if (!IsIn) IONum = (short)((MaxLeads + 1) - IONum);

            switch (LeadStatus[IONum])
            {
                case 0:
                    return;
                case 1:
                case 3:
                    if (IsIn) OX = 2;
                    else return;
                    break;
                case 2:
                case 4:
                    if (!IsIn) OX = 1;
                    else return;
                    break;
                default:
                    return;
            } //switch leadstatus

            //For leads on the opposite side:
            if (IONum > 8) OX = 25 - OX;

            if (OX != -1)
            {
                switch (IONum)
                {
                    case 1:
                    case 16:
                        OY = 3;
                        break;
                    case 2:
                    case 15:
                        OY = 13;
                        break;
                    case 3:
                    case 14:
                        OY = 23;
                        break;
                    case 4:
                    case 13:
                        OY = 33;
                        break;
                    case 5:
                    case 12:
                        OY = 43;
                        break;
                    case 6:
                    case 11:
                        OY = 53;
                        break;
                    case 7:
                    case 10:
                        OY = 63;
                        break;
                    case 8:
                    case 9:
                        OY = 73;
                        break;
                    default:
                        OY = -1;
                        break;
                } //switch IONum
            } //if X != -1

        } //Get Lead coords.

        public override string GetImage()
        {
            StringBuilder TheImage = new StringBuilder("ChipShell");

            ReCountIOLeads();

            for (short I = 1; I <= MaxLeads; I++)
            {
                string img = "";
                switch (LeadStatus[I])
                {
                    case 1:
                        img = "|{colormask}:Black|ChipShellIn";
                        break;
                    case 2:
                        img = "|{colormask}:Black|ChipShellOut";
                        break;
                    case 3:
                        img = "|{colormask}:Red|ChipShellIn";
                        break;
                    case 4:
                        img = "|{colormask}:Red|ChipShellOut";
                        break;
                } //Leadstatus

                if (img != "")
                {
                    if (I > 8) TheImage.Append("|{reverseit}:");

                    TheImage.Append(img);

                    if (I > 8)
                        TheImage.Append(17 - I);
                    else
                        TheImage.Append(I);
                }
            } //for I = 1 to MaxLeads

            return TheImage.ToString();
        }

        public override string HoverDescription
        {
            get
            {
                return "Chip\nName: " + privC.CircuitBoardTitle +
                    "\nID Number: " + ChipNumber +
                    "\nNumber of Inputs: " + NumIns +
                    "\nNumber of Outputs: " + NumOuts;
            }
        }

        
        public override short Height
        {
            get
            {
                return 75;
            }
        }

        public override short Width
        {
            get
            {
                return 25;
            }
        }

        protected override string AuxDataSaveLine()
        {
            return ChipNumber.ToString();
        }

        protected override void LoadAuxData(string AuxData)
        {
            try
            {
                ChipNumber = short.Parse(AuxData);
            }
            catch { }
        }

        public override string LoadInternalContent(string TheContent)
        {
            string S = base.LoadInternalContent(TheContent);
            ReCountIOLeads();
            return S;
        }

    } //Chip Shell 

} //namespace