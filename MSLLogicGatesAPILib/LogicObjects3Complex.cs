using System.Collections;
using System.Text;

namespace MSLLogicGatesAPI
{
    public abstract class Sensor : LogicObject, IReadsGrabbableObjects
    {
        //This class will be inherited by other sensor types.

        //true = detects a shape, false = detects a color
        private bool mDetectsShape;
        public virtual bool DetectsShape
        {
            get { return mDetectsShape; }
            set { mDetectsShape = value; }
        }

        public NonLogicColors ColorSetting;
        public NonLogicShapes ShapeSetting;

        public Sensor()
        {
            NumIns = 0;
            NumOuts = 1;
            DetectsShape = true;
            ShapeSetting = NonLogicShapes.NonLogCircle;
            ColorSetting = NonLogicColors.NonLogRed;
        }

        public Sensor(NonLogicColors NLColor)
        {
            NumIns = 0;
            NumOuts = 1;
            DetectsShape = false;
            ShapeSetting = NonLogicShapes.NonLogCircle;
            ColorSetting = NLColor;
        }

        public Sensor(NonLogicShapes NLShape)
        {
            NumIns = 0;
            NumOuts = 1;
            DetectsShape = true;
            ShapeSetting = NLShape;
            ColorSetting = NonLogicColors.NonLogRed;
        }

        public override void Input(short InNum, bool Value)
        { /* do nothing */ }

        protected virtual void InitializeFeed()
        {
            IsHot = false;
        }

        public void ReadGrabbableObjects(List<CanBeGrappledWith> ObjLocs)
        {
            bool F;
            InitializeFeed();
            foreach (CanBeGrappledWith N in ObjLocs)
            {
                F = false;
                if (DetectsShape)
                {
                    if (N.ShapeSetting == ShapeSetting) F = true;
                }
                else
                {
                    if (N.ColorSetting == ColorSetting) F = true;
                }

                if (N.IsGrabbed) F = false;

                if (F) ShapeFeedTest(N.X, N.Y);
            } //for each
        } //read non logs

        protected virtual void ShapeFeedTest(int SX, int SY)
        {
            //Different senors inherited from this, will do different things here!
            IsHot = false;
        }

        public override bool HasDelay
        {
            get
            {
                return false;
            }
        }

        public override short Height
        {
            get
            {
                return 40;
            }
        }

        public override short Width
        {
            get
            {
                return 40;
            }
        }

        protected string GetOverlayInfo()
        {
            //Note: we have to do a composite of several bitmaps, in the GetImage function.
            //So, a pipe (|) will separate the different files. This function provides
            //much of the common overlay code.
            string S;
            if (DetectsShape) S = "Sense" + ShapeSetting.ToString().Replace("NonLog", "");
            else S = "{colormask}:" + ColorSetting.ToString().Replace("NonLog", "")
                + "|SenseColor";

            return S;
        }

        protected string HoverSenseInfo()
        {
            //common function for use in hover descriptions of sensors.
            string S = "Detecting: ";

            if (DetectsShape)
            {
                string SS = ShapeSetting.ToString().Replace("NonLog", "");
                if (SS == "PlusSign") SS = "Plus Sign";
                if (SS == "DataCard") SS = "Data Card";
                if (SS == "CDDVD") SS = "CD/DVD";

                S += "Shape: " + SS;
            }
            else
                S += "Color: " + ColorSetting.ToString().Replace("NonLog", "");

            return S;
        }

        protected override string AuxDataSaveLine()
        {
            string S = "";

            if (DetectsShape)
                S = ShapeSetting.ToString().Replace("NonLog", "");
            else
                S = ColorSetting.ToString().Replace("NonLog", "");

            return S;
        }

        protected override void LoadAuxData(string AuxData)
        {
            string S = AuxData.Trim();

            NonLogicColors NLC = ColorSetting;
            NonLogicShapes NLS = ShapeSetting;

            if (S.Trim() != "")
            {
                try
                {
                    NLC = (NonLogicColors)Enum.Parse(typeof(NonLogicColors), "NonLog"
                        + S, true);
                    DetectsShape = false;
                }
                catch
                {
                    NLC = ColorSetting;
                }

                try
                {
                    NLS = (NonLogicShapes)Enum.Parse(typeof(NonLogicShapes), "NonLog"
                        + S, true);
                    DetectsShape = true;
                }
                catch
                {
                    NLS = ShapeSetting;
                }
            } //S != ""

            ColorSetting = NLC;
            ShapeSetting = NLS;
        }

        public override bool VisualStatus()
        {
            return IsHot;
        }

    } //Generic Sensor class

    public class TouchSensor : Sensor
    {
        protected override void ShapeFeedTest(int SX, int SY)
        {
            if (SX + 22 >= X && SX <= X + 40 &&
                SY + 22 >= Y && SY <= Y + 40)
                IsHot = true;
        }

        public override string GetImage()
        {
            string S = "SensorTouchStem";

            if (IsHot) S = "H" + S;

            S = GetOverlayInfo() + "|" + S;

            return S;
        }

        public override short ItemTypeIndex
        {
            get
            {
                return 21;
            }
        }

        public override string ItemTypeName
        {
            get
            {
                return "Touch Sensor";
            }
        }

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            OX = -1;
            OY = -1;

            if (IsIn == false && IONum == 1)
            {
                OX = 40;
                OY = 19;
            }
        }

        public override string HoverDescription
        {
            get
            {
                string S;

                S = "Touch Sensor\n" + HoverSenseInfo() + "\nStatus: ";

                if (IsHot) S += "HOT!"; else S += "Cold";

                return S;
            }
        }

    } //Touch Sensor

    public class ClosestDirectionalSensor : Sensor
    {
        //Points in the direction of only the closest detected object.

        private bool[] IsHotDir = new bool[3];
        /*
    For Compass (directional) sensors:
       Up = IsHot       = Out1
       Dn = IsHotDir(0) = Out3
       Rt = IsHotDir(1) = Out2
       Lt = IsHotDir(2) = Out4         
        */

        private int CloserX;
        private int CloserY;

        public ClosestDirectionalSensor()
        {
            NumOuts = 4;
        }

        protected override void InitializeFeed()
        {
            IsHot = false;
            IsHotDir[0] = false;
            IsHotDir[1] = false;
            IsHotDir[2] = false;
            CloserX = 999999999;
            CloserY = 999999999;
        }

        protected override void ShapeFeedTest(int SX, int SY)
        {
            int XD = Math.Abs((X + 11) - SX);
            int YD = Math.Abs((Y + 11) - SY);
            bool F = false;

            if (XD < CloserX || YD < CloserY)
            {
                CloserX = XD;
                CloserY = YD;
                F = true;
                //cancel last one:
                IsHot = false;
                IsHotDir[0] = false;
                IsHotDir[1] = false;
                IsHotDir[2] = false;
            } //XD < Closer

            if (F)
            {
                if (SX > X + 22) IsHotDir[1] = true;
                if (SX < X) IsHotDir[2] = true;
                if (SY > Y + 22) IsHotDir[0] = true;
                if (SY < Y) IsHot = true;

                //Adjust for rotation of obj:
                bool[] OutDir = AdjustOutsForRotations();
                IsHot = OutDir[0];
                IsHotDir[1] = OutDir[1];
                IsHotDir[0] = OutDir[2];
                IsHotDir[2] = OutDir[3];
            } //F
        } //ShapeFeedTest

        private bool[] AdjustOutsForRotations()
        {
            bool[] OutDir = new bool[4];
            switch (ObjRotation)
            {
                case ObjRotationDirs.ObjRot00:
                    OutDir[0] = IsHot;
                    OutDir[1] = IsHotDir[1];
                    OutDir[2] = IsHotDir[0];
                    OutDir[3] = IsHotDir[2];
                    break;
                case ObjRotationDirs.ObjRot90:
                    OutDir[0] = IsHotDir[1];
                    OutDir[1] = IsHotDir[0];
                    OutDir[2] = IsHotDir[2];
                    OutDir[3] = IsHot;
                    break;
                case ObjRotationDirs.ObjRot180:
                    OutDir[0] = IsHotDir[0];
                    OutDir[1] = IsHotDir[2];
                    OutDir[2] = IsHot;
                    OutDir[3] = IsHotDir[1];
                    break;
                case ObjRotationDirs.ObjRot270:
                    OutDir[0] = IsHotDir[2];
                    OutDir[1] = IsHot;
                    OutDir[2] = IsHotDir[1];
                    OutDir[3] = IsHotDir[0];
                    break;
            }
            return OutDir;
        }

        public override bool Output(short OutNum)
        {
            bool O;
            switch (OutNum)
            {
                case 1:
                    O = IsHot; break;
                case 2:
                    O = IsHotDir[1]; break;
                case 3:
                    O = IsHotDir[0]; break;
                case 4:
                    O = IsHotDir[2]; break;
                default:
                    O = IsHot; break;
            } //switch

            return O;
        }

        public override void Reset()
        {
            IsHot = false;
            IsHotDir[0] = false;
            IsHotDir[1] = false;
            IsHotDir[2] = false;
        }

        public override string GetImage()
        {
            string S = "SensorDir1Stem|";

            if (IsHot) S += "HSensorDir1StemU|";
            if (IsHotDir[1]) S += "HSensorDir1StemR|";
            if (IsHotDir[0]) S += "HSensorDir1StemD|";
            if (IsHotDir[2]) S += "HSensorDir1StemL|";

            S += GetOverlayInfo();

            return S;
        }

        public override short ItemTypeIndex
        {
            get
            {
                return 22;
            }
        }

        public override string ItemTypeName
        {
            get
            {
                return "Directional Sensor";
            }
        }

        protected override void GetInitialLeadCoords(short IONum, bool IsIn, ref int OX, ref int OY)
        {
            OX = -1;
            OY = -1;

            if (IsIn == false)
            {
                switch (IONum)
                {
                    case 1:
                        OX = 19;
                        OY = 0;
                        break;
                    case 2:
                        OX = 40;
                        OY = 19;
                        break;
                    case 3:
                        OX = 20;
                        OY = 40;
                        break;
                    case 4:
                        OX = 0;
                        OY = 19;
                        break;
                } //switch IONum
            } //IsIn = False
        } //get leads

        public override string HoverDescription
        {
            get
            {
                string S;
                string P = "Cold";

                S = "Directional Sensor\n" + HoverSenseInfo() + "\nStatus: ";

                if (IsHot) P = "Pointing Up";
                if (IsHotDir[0]) P = "Pointing Down";
                if (IsHotDir[1]) P = "Pointing Right";
                if (IsHotDir[2]) P = "Pointing Left";

                S += P;

                return S;
            }
        }

    } //Dir. Closest Sensor

} //namespace