

namespace MSLLogicGatesAPI
{
    public enum CircuitTypes
    {
        CircTypeStandard,
        CircTypeRoboShell,
        CircTypeChip,
        CircTypeMaster //All component types allowed, no matter what!
    }

    public enum CircPowerModes
    {
        CPowerOn,
        CPowerOff
    }

    public enum CircConstructModes
    {
        CMNormal,
        CMSoldering, /* Wiring */
        CMCBars,
        CMWallSetting,
        CMDoorSetting,
        CMNoClickSetting,
        CMGoalAreaSetting
    }

    public enum CircSpeeds
    {
        CSpeedPainfulSlow = -2,
        CSpeedSlow,
        CSpeedNormal,
        CSpeedFast,
        CSpeedHyperFast
    }

} //namespace