using System.Text;

namespace MSLLogicGatesAPI
{
    public abstract class BasicComponent
    {
        //A generic component to place on the game board. Any components to be placed on the board must inherit from this one.

        //For visual representation, we need coordinates:
        private int x;
        private int y;

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }

        public bool IsSelected = false;

        //Rotation of the object
        private ObjRotationDirs objRotation = ObjRotationDirs.ObjRot00;
        public ObjRotationDirs ObjRotation { get => objRotation; set => objRotation = value; }

        public short ObjRotationDegrees
        {
            get
            {
                switch (ObjRotation)
                {
                    case ObjRotationDirs.ObjRot00:
                        return 0;
                    case ObjRotationDirs.ObjRot90:
                        return 90;
                    case ObjRotationDirs.ObjRot180:
                        return 180;
                    case ObjRotationDirs.ObjRot270:
                        return 270;
                    default:
                        return 0;
                }
            }

            set
            {
                if (value > 360) value = (short)(value - 360);

                if (value <= 0 || value >= 271)
                    ObjRotation = ObjRotationDirs.ObjRot00;
                if (value >= 1 && value <= 90)
                    ObjRotation = ObjRotationDirs.ObjRot90;
                if (value >= 91 && value <= 180)
                    ObjRotation = ObjRotationDirs.ObjRot180;
                if (value >= 181 && value <= 270)
                    ObjRotation = ObjRotationDirs.ObjRot270;
            }
        }

        public virtual short ItemTypeIndex
        {
            //As a general rule, any element placed into the Gallery should have its own ItemTypeIndex. A single ItemTypeIndex could reprepesent multiple types of components,
            //if they are all selected from a single Gallery icon. And, a single C# object could have several values for ItemTypeIndex, if it adds multiple items to the Gallery.
            get
            {
                return -1;
            }
        }

        public virtual string ItemTypeName
        {
            //Up to one name per Gallery item, please. Almost just like ItemTypeIndex. Except the same name could be used for multiple Indexes, when that makes sense.
            get
            {
                return "Basic Component";
            }
        }

        public virtual string GetImage()
        {
            return "White";
        }

        public virtual string HoverDescription
        {
            get
            {
                return "Basic Generic Component";
            }
        }

      
        public string SaveLine()
        {
            return "Obj|" + ItemTypeIndex + "|" + x + "|" + y + "|" + ObjRotation.ToString() + "|" + AuxDataSaveLine().Replace("|", "┆");
        }

        public void LoadLine(string LineToLoadData)
        {
            try
            {
                string[] LoadParts = LineToLoadData.Split('|');

                //Ignore LoadParts[0] and LoadParts[1]

                if (int.TryParse(LoadParts[2], out int lx)) x = lx;
                if (int.TryParse(LoadParts[3], out int ly)) y = ly;

                if (Enum.TryParse<ObjRotationDirs>(LoadParts[4], out ObjRotationDirs lrot))
                    ObjRotation = lrot;

                LoadAuxData(LoadParts[5].Replace("┆", "|"));
            }
            catch { }
        }

        protected virtual string AuxDataSaveLine()
        {
            return "";
        }

        protected virtual void LoadAuxData(string AuxData) { }
        //LoadAuxData does nothing by default. This will come in handy for several advanced objects.

        public virtual bool VisualStatus()
        {
            // Some objects show visual reflection of power or other status, especially for puzzle goals.
            // even if it has no Output leads. This is where to access that status.
            return false;
        }

        public virtual short Height
        {
            get { return 22; } //default size for everything is 22, for some reason.
        }

        public virtual short Width
        {
            get { return 22; }
        }

        public int X2
        {
            get
            {
                if (ObjRotation == ObjRotationDirs.ObjRot00
                    || ObjRotation == ObjRotationDirs.ObjRot180)
                    return X + Width;
                else //if this rotated, its apparent width becomes its height
                    return X + Height;
            }
        }

        public int Y2
        {
            get
            {
                if (ObjRotation == ObjRotationDirs.ObjRot00
                    || ObjRotation == ObjRotationDirs.ObjRot180)
                    return Y + Height;
                else //if this rotated, its apparent height becomes its width
                    return Y + Width;
            }
        }

        public void FindCenter(ref int CentX, ref int CentY)
        {
            if (ObjRotation == ObjRotationDirs.ObjRot00
                    || ObjRotation == ObjRotationDirs.ObjRot180)
            {
                CentX = X + (Width / 2); //we can ignore remainders here, if we must.
                CentY = Y + (Height / 2);
            }
            else
            {   //if this rotated, its apparent height becomes its width
                CentX = X + (Height / 2);
                CentY = Y + (Width / 2);
            }
        }

        protected void AdjCoordsforRotation(ref int KX, ref int KY)
        {

            switch (ObjRotation)
            {
                case ObjRotationDirs.ObjRot00:
                    break;
                case ObjRotationDirs.ObjRot90:
                    int KY2 = KY;
                    KY = KX;
                    KX = Height - KY2 - 1;
                    break;
                case ObjRotationDirs.ObjRot180:
                    if (Width > Height)
                    {
                        KX = KX - (Width - Height);
                        KY = KY + (Width - Height);
                    }

                    KX = Height - KX - 1;
                    KY = Width - KY;

                    if (Height > Width)
                    {
                        KX = KX - (Height - Width);
                        KY = KY + (Height - Width);
                    }

                    break;
                case ObjRotationDirs.ObjRot270:
                    int KX2 = KX;
                    KX = KY;
                    KY = KX2;

                    KY = Width - KY;
                    break;
            }
        } //AdjCoords for Rots

        public virtual void AfterClockTick() { }
        //Called each time the "clock" ticks a step on the game board or circuit board.
        //Mostly applies to logic objects, but it can also apply to any basic objects that move over time.

    } //Basic Component

    public class ShapeObject : CanBeGrappledWith
    {
        public ShapeObject()
        {
            ColorSetting = NonLogicColors.NonLogRed;
            ShapeSetting = NonLogicShapes.NonLogCircle;
        }

        public ShapeObject(NonLogicShapes NLShape)
        {
            ColorSetting = NonLogicColors.NonLogRed;
            ShapeSetting = NLShape;
        }

        public ShapeObject(NonLogicColors NLColor)
        {
            ColorSetting = NLColor;
            ShapeSetting = NonLogicShapes.NonLogCircle;
        }

        public ShapeObject(NonLogicColors NLColor, NonLogicShapes NLShape)
        {
            ColorSetting = NLColor;
            ShapeSetting = NLShape;
        }

        public override short ItemTypeIndex
        {
            get
            {
                return 1;
            }
        }

        public override string ItemTypeName
        {
            get
            {
                return "Shape";
            }
        }

        public override string GetImage()
        {
            string O = "{colormask}:";
            O += ColorSetting.ToString().Replace("NonLog", "");
            O += "|NL";
            O += ShapeSetting.ToString().Replace("NonLog", "");

            return O;
        }

        public override string HoverDescription
        {
            get
            {
                string S = ShapeSetting.ToString().Replace("NonLog", "");
                if (S == "PlusSign") S = "Plus Sign";
                if (S == "DataCard") S = "Data Card";
                if (S == "CDDVD") S = "CD/DVD";

                string O = "Shape\nShape: " + S
                  + "\nColor: "
                  + ColorSetting.ToString().Replace("NonLog", "")
                  + "\nStatus: ";

                if (IsGrabbed) O += "Grabbed"; else O += "Free";

                return O;
            }
        } //HoverDescrip

        protected override string AuxDataSaveLine()
        {
            return ShapeSetting.ToString().Replace("NonLog", "") + "," + ColorSetting.ToString().Replace("NonLog", "");
        } //SaveLine

        protected override void LoadAuxData(string AuxData)
        {
            if (AuxData.Contains(","))
            {
                string[] SA;
                char[] CA = { ',' };
                SA = AuxData.Split(CA);
                InsertAuxParam(SA[0].Trim());
                InsertAuxParam(SA[1].Trim());
            }
            else InsertAuxParam(AuxData.Trim());
        } //LoadAuxData

        private void InsertAuxParam(string S)
        {
            NonLogicColors NLC = ColorSetting;
            NonLogicShapes NLS = ShapeSetting;

            if (S.Trim() != "")
            {
                try
                {
                    NLC = (NonLogicColors)Enum.Parse(typeof(NonLogicColors), "NonLog" + S, true);
                }
                catch
                {
                    NLC = ColorSetting;
                }

                try
                {
                    NLS = (NonLogicShapes)Enum.Parse(typeof(NonLogicShapes), "NonLog" + S, true);
                }
                catch
                {
                    NLS = ShapeSetting;
                }
            } //S != ""

            ColorSetting = NLC;
            ShapeSetting = NLS;
        } //priv. InsertAuxData

    } //NonLogic Shape

} //namespace
