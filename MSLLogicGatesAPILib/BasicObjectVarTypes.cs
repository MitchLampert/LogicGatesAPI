using System;
using System.Collections.Generic;
using System.Text;

namespace MSLLogicGatesAPI
{
    public enum ObjRotationDirs
    {
        ObjRot00, /* midnight */
        ObjRot90, /* 3 o'clock */
        ObjRot180, /* 6 o'clock */
        ObjRot270, /* 9 o'clock */
    }

    public enum NonLogicColors
    {
        NonLogRed,
        NonLogOrange,
        NonLogYellow,
        NonLogGreen,
        NonLogBlue,
        NonLogBlack,
        NonLogGray,
        NonLogWhite
    }

    public enum NonLogicShapes
    {
        NonLogCircle,
        NonLogSquare,
        NonLogRectangle,
        NonLogTriangle,
        NonLogHexagon,
        NonLogOctagon,
        NonLogTrapezoid,
        NonLogOval
    }

    public enum GateTypes
    {
        // For the LogicGate object, these are the various types of gates available.
        NotGate,
        OrGate,
        AndGate,
        XorGate,
        NorGate,
        NandGate,
        XNorGate
    }

    public abstract class CanBeGrappledWith : BasicComponent
    {
        // Most objects that are non-logical will probably inherit from this.

        private bool isGrabbed = false;
        public bool IsGrabbed { get => isGrabbed; set => isGrabbed = value; }

        private NonLogicColors colorSetting;
        private NonLogicShapes shapeSetting;
        public virtual NonLogicColors ColorSetting { get => colorSetting; set => colorSetting = value; }
        public virtual NonLogicShapes ShapeSetting { get => shapeSetting; set => shapeSetting = value; }
    } //Can be gappled with

    public interface IReadsGrabbableObjects
    {
        void ReadGrabbableObjects(List<CanBeGrappledWith> GabbableObjects);
    }

    public interface IUserSwitchable
    {
        //User can phyically click on object to switch its state.

        void UserSetsSwitch(bool Value);
        bool CurrentSwitchSetting();
        int SwitchCoordX { get; } //X and Y coordinates for center of user-clickable area of switch. (size of clickable area is standard across all such objects)
        int SwitchCoordY { get; }
    }

    public interface ISearchesThroughComponents
    {
        void SearchThroughComponents(List<BasicComponent> ThePileOfComponents);
    }

    public interface IWireless
    {
        bool WirelessSignal { get; set; }
    }
} //namespace
