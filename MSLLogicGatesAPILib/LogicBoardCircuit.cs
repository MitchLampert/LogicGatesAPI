using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Reflection;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

/*
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or distribute this software, either in source code form or as a compiled binary, for any purpose, commercial or non-commercial, and by any means.
In jurisdictions that recognize copyright laws, the author or authors of this software dedicate any and all copyright interest in the software to the public domain. We make this dedication for the benefit of the public at large and to the detriment of our heirs and successors. We intend this dedication to be an overt act of relinquishment in perpetuity of all present and future rights to this software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
//Original Author: Mitchell Scott Lampert


namespace MSLLogicGatesAPI
{
    public class LogicBoardCircuit
    {
        #region Variables and Properties
       
        //Also avoid using these directly. Build it from GetStuffToGrab function.
        private List<CanBeGrappledWith>? GrappleGroup = null;
       
        //Basic board components:
        public List<BasicComponent> TheComponents = new List<BasicComponent>();
        public List<Wire> Wires = new List<Wire>();
        
        //Properties of circuit:
        private CircuitTypes mCircuitType = CircuitTypes.CircTypeStandard;
        private string mCircuitBoardTitle = ""; 
        private bool mIsEmbeddedCircuit = false;

        public CircuitTypes CircuitType { get { return mCircuitType; } set { mCircuitType = value; } }
        public string CircuitBoardTitle
        {
            get { return mCircuitBoardTitle; }

            set
            {
                mCircuitBoardTitle = value;
            }
        }

        public bool IsEmbeddedCircuit { get { return mIsEmbeddedCircuit; } set { mIsEmbeddedCircuit = value; } }

        public string CircuitTypeString
        {
            get
            {
                switch (mCircuitType)
                {
                    case CircuitTypes.CircTypeStandard:
                        return "Standard Game Board";
                    case CircuitTypes.CircTypeChip:
                        return "Integrated Chip";
                    case CircuitTypes.CircTypeRoboShell:
                        return "Robot Design Internals";
                    case CircuitTypes.CircTypeMaster:
                        return "Master Game Board";
                    default:
                        return "Unknown";
                }
            }
        }

        //Set artificial upper limit to number of components.
        private int MaxNumComps = 0;
        //Zero or lower means no limit. Above zero would be a limit.
        public int MaxComponents { get { return MaxNumComps; } set { MaxNumComps = value; } }

        private int MaxEmbed = 0; //Zero or lower means NO limit. Above zero means that limit.
        public int MaxEmbeddedCircuits { get { return MaxEmbed; } set { MaxEmbed = value; } }

        //Antenna related stuff:
        private bool AntPower = false; //Antenna power!
        public bool WirelessSignal { get { return AntPower; } }

        private const string CBIdentifier = "MSL[BPMILS]*5";

        #endregion

        #region Setting and Collecting Functions

        public LogicBoardCircuit()
        {

        } //Constructor

        public LogicBoardCircuit(CircuitTypes CircType)
        {
            mCircuitType = CircType;
        }


        private List<CanBeGrappledWith> GetStuffToGrab(bool IsASensor)
        {
            if (GrappleGroup != null)
                return GrappleGroup;

            GrappleGroup = new List<CanBeGrappledWith>();
            foreach (BasicComponent O in TheComponents)
            {
                if (O is CanBeGrappledWith)
                    GrappleGroup.Add((CanBeGrappledWith)O);
            } //for each component


            return GrappleGroup;
        } //form nonlog. group

        #endregion

        #region Basic Adding and Removing Stuff

        private short NextChipLeadToUse(bool IsIn)
        {
            //When we add chip I/O parts to the board, we must use the next
            //available lead number.
            short K = 0;
            short I = 0;

            List<short> Ks = new List<short>();

            foreach (BasicComponent O in TheComponents)
            {
                if (O is ChipIO)
                {
                    ChipIO C = (ChipIO)O;
                    if (C.IsInput == IsIn)
                        Ks.Add(C.ChipLeadNumber);
                }
            }

            do
            {
                I++;
                if (!Ks.Contains(I)) K = I;

                if (I >= 32760) K = -1;

            } while (K == 0);
            return K;
        }

        private short NextChipNumber()
        {
            //This is so that we can display the ID number of chip, for easy
            //reference.

            short K = 0;
            short I = 0;

            List<short> Ks = new List<short>();

            foreach (BasicComponent O in TheComponents)
            {
                if (O is ChipShell)
                {
                    ChipShell C = (ChipShell)O;
                    Ks.Add(C.ChipNumber);
                }
            }

            do
            {
                I++;
                if (!Ks.Contains(I)) K = I;

                if (I >= 32760) K = -1;

            } while (K == 0);
            return K;
        }

        public static BasicComponent? ProduceComponentOfItemTypeIndex(short ItemID)
        {
            //This static function only produces components independent of anything on the board; which is fine for MOST types.
            //Some types require settings specific to the board. For that, there is the ProduceComponentForThisCircuit function.

            BasicComponent? C = null;

            switch (ItemID)
            {
                case 1:
                    C = new ShapeObject(); break;
                case 2:
                    C = new HotPoint(); break;
                case 3:
                    C = new LogicGate(GateTypes.NotGate); break;
                case 4:
                    C = new LogicGate(GateTypes.AndGate); break;
                case 5:
                    C = new LogicGate(GateTypes.OrGate); break;
                case 6:
                    C = new LogicGate(GateTypes.XorGate); break;
                case 7:
                    C = new LogicGate(GateTypes.NorGate); break;
                case 8:
                    C = new LogicGate(GateTypes.NandGate); break;
                case 9:
                    C = new LogicGate(GateTypes.XNorGate); break;
                case 10:
                    C = new Node(2); break;
                case 11:
                    C = new Node(3); break;
                case 12:
                    C = new Node(4); break;
                case 13:
                    C = new FlipFlop(); break;
                case 14:
                    C = new ToggleSwitch(); break;
                case 15:
                    C = new Indicator(); break;
                case 16:
                    C = new InLineSwitch(); break;
                case 17:
                    C = new RandomBit(); break;
                case 18:
                    C = new BigPushButton(); break;
                case 19:
                    C = new Antenna(); break;
                case 20:
                    C = new Grabber(); break;
                case 21:
                    C = new TouchSensor(); break;
                case 22:
                    C = new ClosestDirectionalSensor(); break;
                case 23:
                    C = new ChipIO(true); break; //Note: Chips and their parts should be constructed with parameters specific to this circuit.
                case 24:
                    C = new ChipIO(false); break;
                case 25:
                    C = new ChipShell(); break;
            } //Case ItemTypeIndex

            return C;
        } //Produce Component of given ItemTypeIndex

        private BasicComponent? ProduceComponentForThisCircuit(short ItemID)
        {
            //Some components, such as chip parts and door triggers, require initiation that is specific to this circuit.
            BasicComponent? C = null;

            switch (ItemID)
            {
                case 23:
                    C = new ChipIO(true, NextChipLeadToUse(true)); break;
                case 24:
                    C = new ChipIO(false, NextChipLeadToUse(false)); break;
                case 25:
                    C = new ChipShell(NextChipNumber()); break;
                default:
                    C = ProduceComponentOfItemTypeIndex(ItemID); break;
            }

            return C;
        }

        public static bool IsTypeIndexAllowedInCircType(CircuitTypes CType, short ItemID)
        {
            //We are defining these things at the game board level, instead of as properties of the components, because it seems easier to implement this way, for now.
            bool R = true;

            int[] NotInStandard = new int[] { 23, 24 }; //ChipIOs
            int[] NotInRoboShell = new int[] { 23, 24 }; //ChipIO, Door Trigger, Signal Lock, embedded RoboShells, Target Systems
            int[] NotInChips = new int[] { 19, 20 }; //Door Trigger, Signal Lock, RoboShell, Thrusters, Bumpers, Claw, Antenna, Target Systems

            switch (CType)
            {
                case CircuitTypes.CircTypeStandard:
                    if (NotInStandard.Contains(ItemID)) R = false;
                    break;
                case CircuitTypes.CircTypeRoboShell:
                    if (NotInRoboShell.Contains(ItemID)) R = false;
                    break;
                case CircuitTypes.CircTypeChip:
                    if (NotInChips.Contains(ItemID)) R = false;
                    break;
            }

            return R;
        }

        public static string ItemTypeNameOfItemTypeIndex(short ItemID)
        {
            BasicComponent? C = ProduceComponentOfItemTypeIndex(ItemID);
            if (C == null)
                return string.Empty;
            else
                return C.ItemTypeName;
        }

        private bool IsObjectRestrictedFromAdding(short IndexID)
        {
            //NOTE: No logic in here for open source version.
            return false;
        } //Is Object Restricted From Being Added?

        public bool AddObject(short IndexID, int X, int Y)

        {
            if (MaxNumComps > 0 && TheComponents.Count >= MaxNumComps)
                return false; //Make sure we don't go over global limit, if set.

            if (!IsTypeIndexAllowedInCircType(mCircuitType, IndexID))
                return false;
            
            if (IsObjectRestrictedFromAdding(IndexID)) return false;

            BasicComponent? O = ProduceComponentForThisCircuit(IndexID);
            if (O == null) return false;

            if (O != null)
            {
                O.X = X;
                O.Y = Y;
                TheComponents.Add(O);
            }
            return true;
        } //Add Object Main Function

        public bool AddObject(BasicComponent Obj)
        {
            if (MaxNumComps > 0 && TheComponents.Count >= MaxNumComps)
                return false; //Make sure we don't go over limit, if set.

            if (!IsTypeIndexAllowedInCircType(mCircuitType, Obj.ItemTypeIndex)) return false;

            if (IsObjectRestrictedFromAdding(Obj.ItemTypeIndex)) return false;

            TheComponents.Add(Obj);

            return true;
        } //Add object directly

        public bool AddObject(short IndexID, int X, int Y, ObjRotationDirs ObjRot)
        {
            bool B = AddObject(IndexID, X, Y);

            //Rotate object after creation, in a separate function; because some components, such as Thrusters and Bumpers, have their own default orientation we want to preserve by default.
            if (B)
                TheComponents[TheComponents.Count - 1].ObjRotation = ObjRot;

            return B;
        }

        public bool AddObject(short IndexID)
        {
            return AddObject(IndexID, 10, 10);
        }

        public bool RemoveObject(int ObjID)
        {
            /*
            This removes an object from components collection, as well as any wires or bars connected to it.

            There are going to be problems if you remove an commponent from the collection
            without removing any wires and bars connected to it, as well.

            ObjID = the ID of the object we are moving.

            Note that when an object is removed, the IDs of objects above it will decrement.
            So, those IDs will have to be decremented in any wires attached to them.  */

            //Open source version does not have any rules for not allowing items to be removed.

            //Remove object from collection.
            TheComponents.RemoveAt(ObjID);

            //Check wires and bars. Remove any connected to the object.
            //Also decrement any IDs in them that are connected to higher IDs.
            List<ConnectingPiece> CSToRem = new List<ConnectingPiece>();
            foreach (ConnectingPiece P in Wires)
            {
                if (P.End1ID == ObjID || P.End2ID == ObjID) CSToRem.Add(P);
                if (P.End1ID > ObjID) P.End1ID--;
                if (P.End2ID > ObjID) P.End2ID--;
            }
            foreach (ConnectingPiece P in CSToRem)
            {
                if (P is Wire) Wires.Remove((Wire)P);
            } //wires and bars

            return true;
        } //Remove Object

        public bool AddWireBetweenObjects(int Obj1, short Out1, int Obj2, short In2)
        {
            //Only do this if both objIDs are LogicObjects:
            if (!(TheComponents[Obj1] is LogicObject) ||
                !(TheComponents[Obj2] is LogicObject))
            {
                return false;
            }

            Wire W = new Wire();

            LogicObject O;
            int X = 0;
            int Y = 0;

            //make sure only one wire per lead:
            foreach (Wire I in Wires)
            {
                if ((I.End1ID == Obj1 && I.End1Out == Out1) ||
                    (I.End2ID == Obj2 && I.End2In == In2))
                {
                    return false;
                }
            } //for each wire

            //No wire found on this lead:
            W.End1ID = Obj1;
            W.End1Out = Out1;
            W.End2ID = Obj2;
            W.End2In = In2;

            O = (LogicObject)TheComponents[Obj1];
            O.GetIOLeadCoords(Out1, false, ref X, ref Y);
            W.X1 = X;
            W.Y1 = Y;

            O = (LogicObject)TheComponents[Obj2];
            O.GetIOLeadCoords(In2, true, ref X, ref Y);
            W.X2 = X;
            W.Y2 = Y;

            //Add the wire to circuit's collection of wires:
            Wires.Add(W);

            return true;

        } //add wire

        public bool RemoveWire(Wire W)
        {
            //Open Source version does not have rules for preventing wires from being removed.

            Wires.Remove(W);
            return true;
        }

        public bool RemoveWire(int WireID)
        {
            if (WireID < 0 || WireID > Wires.Count - 1) return true;
            return RemoveWire(Wires[WireID]);
        }

        #endregion

        #region PowerFlow and Thrust

        public void PowerFlow()
        {
            //Step 050: assume antenna power is off, and null out special collections.
            AntPower = false;
            GrappleGroup = null;

            //Step 100: Initial power for wires
            foreach (Wire W in Wires)
            {
                if (TheComponents[W.End1ID] is LogicObject)
                {
                    LogicObject L = (LogicObject)TheComponents[W.End1ID];
                    W.IsHot = L.Output(W.End1Out);
                }
            } //Wires

            //Step 200: set power to each item:
            for (int J = 0; J <= TheComponents.Count - 1; J++)
                SetPowerOfObj(J);

            //Step 300: Feed antenna power to wireless objects:
            if (!IsEmbeddedCircuit)
            {
                foreach (BasicComponent B in TheComponents)
                {
                    if (B is IWireless)
                    {
                        IWireless T = (IWireless)B;
                        T.WirelessSignal = AntPower;
                    }
                }
            }

            bool HasNewPowers = false;

            //Step 400: Power wires again, and repower objects where HasDelay is false; Until there are no more new things to power.
            do
            {
                HasNewPowers = false;

                //410: Power wires
                foreach (Wire W in Wires)
                {
                    if (TheComponents[W.End1ID] is LogicObject)
                    {
                        LogicObject L = (LogicObject)TheComponents[W.End1ID];
                        W.IsHot = L.Output(W.End1Out);
                    }
                } //Wires

                //420: repower non-delayed objs:
                int I = -1;
                foreach (BasicComponent BC in TheComponents)
                {
                    I++;
                    if (BC is LogicObject)
                    {
                        LogicObject L = (LogicObject)BC;
                        if (L.HasDelay == false)
                        {
                            string PrevS = L.OutputsAsString();
                            SetPowerOfObj(I);
                            if (L.OutputsAsString() != PrevS) HasNewPowers = true;
                        } //HasDelay = false
                    } //is Logic Object
                } //for each BC
            }
            while (HasNewPowers);

            //Step 600 - repaint everything, handled in GUI.
            //Step 700 - located in AfterClockTick func.

            //Thruster movement covered in separate function call.

        }//PowerFlow!

        private void SetPowerOfObj(int ID)
        {
            //Skip this, if it is not a LogicObject
            if (!(TheComponents[ID] is LogicObject)) return;

            LogicObject O = (LogicObject)TheComponents[ID];

            List<Wire> CWires = new List<Wire>();

            bool F;

            //Step 210: Collect all wires connected to inputs of obj:
            foreach (Wire W in Wires)
                if (W.End2ID == ID) CWires.Add(W);

            if (O.NumInputs > 0)
            {
                //Step 220: Power object according to heat of wires:
                for (short I = 1; I <= O.NumInputs; I++)
                {
                    F = false;
                    foreach (Wire W in CWires)
                    {
                        if (W.End2In == I)
                        {  //If wire is connected to this input, set it according to heat:
                            F = true;
                            O.Input(I, W.IsHot);
                        }
                    }
                    //Set to false, if no wire is connected:
                    if (F == false) O.Input(I, false);
                } //for I
            } //NumInputs > 0

            //Step 230: Implement special wireless things:
            if (O is IWireless)
            {
                IWireless A = (IWireless)O;
                if (A.WirelessSignal) AntPower = true;
            }

            //Step 240: Implement special Grabbing feeders:
            if (O is IReadsGrabbableObjects && !IsEmbeddedCircuit)
            {
                bool IsASensor = (O is Sensor);
                IReadsGrabbableObjects R = (IReadsGrabbableObjects)O;
                R.ReadGrabbableObjects(GetStuffToGrab(IsASensor));
            }

            if (O is ISearchesThroughComponents && !IsEmbeddedCircuit)
            {
                ISearchesThroughComponents R = (ISearchesThroughComponents)O;
                R.SearchThroughComponents(TheComponents);
            }

        } //SetPower of Obj

        public void AfterClockTick()
        {
            //This is considered step 700 in Powerflow process:
            foreach (BasicComponent O in TheComponents)
                if (O is LogicObject)
                {
                    LogicObject L = (LogicObject)O;
                    L.AfterClockTick();
                }

        } //after a clock tick of power flow

        public void ResetPower()
        {
            //Resets the power of all objects and wires to their default settings.

            foreach (Wire W in Wires)
                W.IsHot = false;

            foreach (BasicComponent O in TheComponents)
                if (O is LogicObject)
                {
                    LogicObject L = (LogicObject)O;
                    L.Reset();
                }

            AntPower = false;
        } //reset power

        public void AdjustConnectionsForObj(int ObjID)
        {
            int I = -1;
            foreach (Wire W in Wires)
            {
                I++;
                if (W.End1ID == ObjID || W.End2ID == ObjID)
                    AdjustWireCoords(I);
            }
        }

        private void AdjustWireCoords(int WID)
        {
            Wire W = Wires[WID];

            if (!(TheComponents[W.End1ID] is LogicObject) ||
                !(TheComponents[W.End2ID] is LogicObject))
                return;

            LogicObject O1 = (LogicObject)TheComponents[W.End1ID];
            LogicObject O2 = (LogicObject)TheComponents[W.End2ID];

            O1.GetIOLeadCoords(W.End1Out, false, ref W.X1, ref W.Y1);
            O2.GetIOLeadCoords(W.End2In, true, ref W.X2, ref W.Y2);
            FindBendCoords(W.ConnBend, W.X1, W.X2, W.Y1, W.Y2, ref W.XB, ref W.YB);

        } //Adj wire coords

        private void FindBendCoords(ConnectorBends Bend, int X1, int X2, int Y1, int Y2, ref int XB, ref int YB)
        {
            int LX; int RX; int UY; int DY;

            bool BL = false;

            if (X1 < X2) { LX = X1; RX = X2; BL = true; }
            else { LX = X2; RX = X1; }

            if (Y1 < Y2) { UY = Y1; DY = Y2; BL = !BL; }
            else { UY = Y2; DY = Y1; }

            switch (Bend)
            {
                case ConnectorBends.connbendNone:
                    XB = LX + ((RX - LX) / 2);
                    YB = UY + ((DY - UY) / 2);
                    break;
                case ConnectorBends.connbendUp:
                    if (BL) XB = LX; else XB = RX;
                    YB = UY;
                    break;
                case ConnectorBends.connbendDown:
                    if (!BL) XB = LX; else XB = RX;
                    YB = DY;
                    break;
            }
        } //find bend coords

        #endregion

        #region Saving and Loading

        public string SaveAllGameBoardContent()
        {
            return SaveGameBoardContent(0);
        }

        public string SaveGameBoardContent(int EmbedLevel)
        {
            StringBuilder Text = new StringBuilder();

            //EmbedLevel: 0 is assumed to be the parent circuit or main game board. Any numbers above that are levels of embedded circuits, either in chips or robots, etc.

            if (MaxEmbed > 0 && EmbedLevel > MaxEmbed)
            {
                Text.AppendLine("ERROR|Max embedded circuit level exceeded.");
                return "";
            }

            //Returns all content of this circuit as a string.
            //(Relies on GUI to actually save it as a file.)

            Text.AppendLine("Template|" + CircuitType.ToString());
            Text.AppendLine("Title|" + CircuitBoardTitle.Replace("|", "┆"));

            foreach (BasicComponent O in TheComponents)
                Text.AppendLine(O.SaveLine());

            foreach (ConnectingPiece C in Wires)
                Text.AppendLine(C.SaveLine());

            //Embedded Circuits are going to be sooo confusing! Oy vey!
            int I = -1;
            foreach (BasicComponent O in TheComponents)
            {
                I++;
                if (O is InternalCircuit)
                {
                    InternalCircuit IC = (InternalCircuit)O;
                    Text.Append(ConstructEmbeddedSaveLine(IC, EmbedLevel, I));
                }
            }

            return Text.ToString();
        } //Save Game Board Content

        private string ConstructEmbeddedSaveLine(InternalCircuit IC, int EmbedLevel, int ItemID)
        {
            StringBuilder Text = new StringBuilder();
            Text.AppendLine("EMBED|" + EmbedLevel + "|" + ItemID);
            Text.Append(IC.SaveInternalContent(EmbedLevel + 1));
            Text.AppendLine("EndEMBED┆|" + EmbedLevel + "|" + ItemID);
            return Text.ToString();
        }

        private struct ContentSection
        {
            public string ContentType;
            public StringBuilder TheContent;
            public int EmbedIntoID;
        }

        private List<ContentSection> BreakApartContentToSections(string TheContent)
        {
            List<ContentSection> ContentSections = new List<ContentSection>();

            //Sections: One line sections for basic properties, Components, Connectors
            //Multiline sections for: Walls, Notes, Embedded Circuits

            List<string> TheLines = TheContent.Replace("\r", "").Split('\n').ToList<string>();

            ContentSection CurrentSection = new ContentSection();
            CurrentSection.TheContent = new StringBuilder();

            string ScanMode = "";
            string EndEmbedToken = "";

            foreach (string L in TheLines)
            {
                try
                {
                    string LL = L.Trim().ToLower();
                    switch (ScanMode)
                    {
                        case "embed":
                            if (LL == EndEmbedToken)
                            {
                                ScanMode = "";
                                ContentSections.Add(CurrentSection);
                            }
                            else
                                CurrentSection.TheContent.AppendLine(L);
                            break;
                        default:
                            CurrentSection = new ContentSection();
                            CurrentSection.TheContent = new StringBuilder();

                            if (LL.StartsWith("walls|"))
                            {
                                ScanMode = "walls";
                                CurrentSection.ContentType = "Walls";
                            }

                            else if (LL.StartsWith("notes|"))
                            {
                                ScanMode = "notes";
                                CurrentSection.ContentType = "Notes";
                            }

                            else if (LL.StartsWith("embed|"))
                            {
                                ScanMode = "embed";
                                string[] LS = LL.Split('|');
                                EndEmbedToken = "endembed┆|" + LS[1] + "|" + LS[2];

                                CurrentSection.ContentType = "Embedded";
                                CurrentSection.EmbedIntoID = int.Parse(LS[2]);
                            }

                            else if (string.IsNullOrWhiteSpace(LL))
                            {
                                //Do Nothing
                            }

                            else
                            {
                                CurrentSection.ContentType = "Line";
                                CurrentSection.TheContent.Append(L);
                                ContentSections.Add(CurrentSection);
                            }

                            break;
                    } //Switch Scan Mode
                }
                catch (Exception ex)
                {
                    CurrentSection = new ContentSection();
                    CurrentSection.TheContent = new StringBuilder();

                    CurrentSection.ContentType = "Error";
                    CurrentSection.TheContent.AppendLine(ex.Message);
                    CurrentSection.TheContent.AppendLine(L);
                }
            } //For each Line


            return ContentSections;
        } //Beaking apart text content into sections


        public string LoadAllGameBoardContent(string TheContent)
        {
            //Clear out most of the game board variables at the top:
            TheComponents = new List<BasicComponent>();
            Wires = new List<Wire>();
            CircuitBoardTitle = "";
            CircuitType = CircuitTypes.CircTypeStandard; //default to this. could change later when loading a file below
            AntPower = false;

            //Do NOT reset IsEmbeddedCircuit

            List<ContentSection> ContentSections = BreakApartContentToSections(TheContent);

            return LoadFromContentSections(ContentSections, false);
        } //Load Game Board Content

        private string LoadFromContentSections(List<ContentSection> TheSections, bool AdjNewIDs)
        {
            StringBuilder TheReport = new StringBuilder();

            int FirstNewID = TheComponents.Count;

            foreach (ContentSection Section in TheSections)
            {
                try
                {
                    switch (Section.ContentType)
                    {
                        case "Error":
                            TheReport.AppendLine(Section.TheContent.ToString());
                            break;
                        case "Embedded":
                            int ICID = Section.EmbedIntoID;

                            if (AdjNewIDs) ICID = FirstNewID + ICID;

                            if (TheComponents[ICID] is InternalCircuit)
                            {
                                InternalCircuit IC = (InternalCircuit)TheComponents[ICID];
                                string ICR = IC.LoadInternalContent(Section.TheContent.ToString());
                                if (!string.IsNullOrEmpty(ICR)) TheReport.AppendLine(ICR);
                                AdjustConnectionsForObj(ICID);
                            }
                            else
                                TheReport.AppendLine("Does not contain internal circuit: ID#" + ICID);

                            break;
                        case "Line":
                            string[] LP = Section.TheContent.ToString().Split('|');

                            switch (LP[0].ToLower())
                            {
                                case "template":
                                    LP[1] = LP[1].ToLower();
                                    if (LP[1].Contains("roboshell")) CircuitType = CircuitTypes.CircTypeRoboShell;
                                    if (LP[1].Contains("chip")) CircuitType = CircuitTypes.CircTypeChip;
                                    if (LP[1].Contains("master")) CircuitType = CircuitTypes.CircTypeMaster;
                                    //"standard" is default, and already set above.
                                    break;
                                case "title":
                                    CircuitBoardTitle = LP[1].Replace("┆", "|");
                                    break;
                                case "obj":
                                    short NewItemID = short.Parse(LP[1]);

                                    bool A = AddObject(NewItemID);
                                    if (A)
                                    {
                                        BasicComponent NBC = TheComponents[TheComponents.Count - 1];
                                        NBC.LoadLine(Section.TheContent.ToString());
                                        NBC.IsSelected = AdjNewIDs;
                                    }
                                    else
                                        TheReport.Append("Denied: " + Section.TheContent);
                                    break;
                                case "w":
                                    int W1 = int.Parse(LP[1]);
                                    int W2 = int.Parse(LP[2]);
                                    short WO = short.Parse(LP[3]);
                                    short WI = short.Parse(LP[4]);

                                    //Make sure wires are pointing to right objects, if they need to be adjusted:
                                    if (AdjNewIDs)
                                    {
                                        W1 = FirstNewID + W1;
                                        W2 = FirstNewID + W2;
                                    }

                                    bool WA = AddWireBetweenObjects(W1, WO, W2, WI);

                                    if (WA)
                                    {
                                        ConnectorBends WB = ConnectorBends.connbendNone;
                                        LP[5] = LP[5].ToLower();
                                        if (LP[5].Contains("up")) WB = ConnectorBends.connbendUp;
                                        if (LP[5].Contains("down")) WB = ConnectorBends.connbendDown;

                                        Wire NW = Wires[Wires.Count - 1];
                                        NW.ConnBend = WB;

                                        if (AdjNewIDs) NW.IsSelected = true;

                                        AdjustWireCoords(Wires.Count - 1);
                                    }

                                    if (!WA) TheReport.Append("Denied: " + Section.TheContent);

                                    break;
                            } //First part of string line

                            break;
                        default:
                            TheReport.AppendLine("Unknown Content Type: " + Section.ContentType);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    TheReport.AppendLine(ex.ToString());
                }
            } //For each section

            return TheReport.ToString();
        } //Load from content sections

        public string LoadEmbeddedCircuitContent(int CompIndex, string TheContent)
        {
            string R = "";

            if (TheComponents[CompIndex] is InternalCircuit)
            {
                InternalCircuit IC = (InternalCircuit)TheComponents[CompIndex];
                R = IC.LoadInternalContent(TheContent);
            }
            else
                R = "This component does not contain an embedded circuit.";

            return R;
        } //Load embedded circuit content

        public string GetAllPublicMemberValues(int ItemID)
        {
            BasicComponent obj = TheComponents[ItemID];

            Type type = obj.GetType();

            StringBuilder Values = new StringBuilder();

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in properties)
            {
                Values.AppendLine(prop.Name + ": " + prop.GetValue(obj));
            }

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                Values.AppendLine(field.Name + ": " + field.GetValue(obj));
            }

            return Values.ToString();
        } //Get All Public Members

        #endregion

        #region Selection-Related Stuff

        public void ClearSelections()
        {
            foreach (BasicComponent O in TheComponents)
                O.IsSelected = false;

            foreach (Wire W in Wires)
                W.IsSelected = false;

        } //clear select

        public void SelectAll()
        {
            foreach (BasicComponent O in TheComponents)
                O.IsSelected = true;

            foreach (Wire W in Wires)
                W.IsSelected = true;
        }

        public void SelectOnlyWiress()
        {
            foreach (BasicComponent O in TheComponents)
                O.IsSelected = false;

            foreach (Wire W in Wires)
                W.IsSelected = true;
        }

        private bool TestWithinBox(int BX1, int BY1, int BX2, int BY2, int OX1, int OY1, int OX2, int OY2)
        {
            bool R = false;

            if (((OX1 >= BX1 && OX1 <= BX2) || (OX2 >= BX1 && OX2 <= BX2)) &&
                 ((OY1 >= BY1 && OY1 <= BY2) || (OY2 >= BY1 && OY2 <= BY2))
               ) R = true;

            return R;
        }

        public bool SelectWithinBox(int X1, int Y1, int X2, int Y2)
        {
            bool R = false; //Returns True if anything was in box. False if empty.

            int ID = -1;
            foreach (BasicComponent O in TheComponents)
            {
                ID++;

                //Test if each object is in the selection Box.
                //Also: If in Play Mode, make sure object is not in a No Click Zone:

                O.IsSelected = TestWithinBox(X1, Y1, X2, Y2, O.X, O.Y, O.X2, O.Y2);

                if (O.IsSelected) R = true;
            }

            //Select all wires and bars that are ONLY connected to selected objects at BOTH ends:
            foreach (Wire W in Wires)
            {
                bool Side1Sel = TheComponents[W.End1ID].IsSelected;
                bool Side2Sel = TheComponents[W.End2ID].IsSelected;

                W.IsSelected = Side1Sel && Side2Sel;
            }
            return R;
        } //Select within Box

        public bool RemoveSelectedParts()
        {
            //Removes selected objects. This will also remove anything connected to them.
            List<int> L = new List<int>();
            int I = -1;

            bool R = true;

            foreach (BasicComponent O in TheComponents)
            {
                I++;
                if (O.IsSelected) L.Add(I);
            }
            L.Sort();
            L.Reverse(); //Must remove items in order of highest index to lowest
            foreach (int O in L)
            {
                bool R2 = RemoveObject(O); //This will also remove anything attached to the object. This will NOT remove parts that are restricted.
                if (!R2) R = false;
            }

            return R;
        } //Remove selected parts

        public bool RemoveSelectedWires()
        {
            int I = -1;
            bool R = true;

            //Removes only wires that are set as "selected".

            List<int> WIDs = new List<int>();

            foreach (Wire W in Wires)
            {
                I++;
                if (W.IsSelected) WIDs.Add(I);
            }

            WIDs.Reverse();

            foreach (int J in WIDs)
            {
                bool R2 = RemoveWire(J); //This will NOT remove wires that are restricted.
                if (!R2) R = false;
            }


            return R;
        } //remove selected connections

        public int CountSelectedComponents()
        {
            int I = 0;
            foreach (BasicComponent C in TheComponents)
            {
                if (C.IsSelected) I++;
            }
            return I;
        }

        public int CountSelectedWires()
        {
            int I = 0;

            foreach (Wire W in Wires)
            {
                if (W.IsSelected) I++;
            }

            return I;
        }

        public int ConnectingHitTest(int X, int Y)
        {
            int R = -1;
            int I = -1;
            bool B = false;


            foreach (ConnectingPiece C in Wires)
            {
                I++;
                B = C.HitTest(X, Y);
                if (B) R = I;
            }

            return R;
        }

        public string ClipboardContentOfSelection()
        {
            StringBuilder Text = new StringBuilder();

            Text.AppendLine(CBIdentifier + "|");

            //Key was the ID in the ComponentBoardCircuit. Value is the new ID going in. Used for matching ends of connectors.
            Dictionary<int, int> NewCompIDs = new Dictionary<int, int>();

            int CurrID = -1;
            int NewID = -1;

            foreach (BasicComponent O in TheComponents)
            {
                CurrID++;
                if (O.IsSelected)
                {
                    NewID++;
                    NewCompIDs.Add(CurrID, NewID);
                    Text.AppendLine(O.SaveLine());
                }
            }

            //If no components selected, return nothing:
            if (NewID == -1) return "";

            foreach (ConnectingPiece C in Wires)
                if (C.IsSelected)
                    Text.AppendLine(C.SaveLineAltEnds(NewCompIDs[C.End1ID], NewCompIDs[C.End2ID]));

            //Some selected stuff may have internal circuits, too! Yeesh!
            CurrID = -1;
            foreach (BasicComponent O in TheComponents)
            {
                CurrID++;
                if (O.IsSelected && O is InternalCircuit)
                {
                    InternalCircuit IC = (InternalCircuit)O;
                    Text.Append(ConstructEmbeddedSaveLine(IC, 0, NewCompIDs[CurrID]));
                }
            }

            return Text.ToString();
        } //Clipboard Content of Selectd Stuff

        public bool PasteFromClipboardContent(string TheContent)
        {
            if (!TheContent.StartsWith(CBIdentifier + "|")) return false;
            List<ContentSection> ContentSections = BreakApartContentToSections(TheContent);

            string S = LoadFromContentSections(ContentSections, true);

            if (string.IsNullOrEmpty(S)) return true; else return false;
        }

        #endregion

    } //Logic Board Circ
} //namespace
