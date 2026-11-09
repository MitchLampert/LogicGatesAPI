
namespace MSLLogicGatesAPI
{
    public enum ConnectorBends
    {
        connbendNone,
        connbendUp,
        connbendDown
    }

    public abstract class ConnectingPiece
    {
        //The IDs of objects are assigned by the LogicCircuit class.
        private int end1ID; //ID of first object to connect to.
        private int end2ID; //ID of second object to connect to.
        public int End1ID { get => end1ID; set => end1ID = value; }
        public int End2ID { get => end2ID; set => end2ID = value; }

        //Coords. for each end of connector, in visual thing:
        public int X1;
        public int X2;
        public int Y1;
        public int Y2;

        private ConnectorBends connBend = ConnectorBends.connbendNone;
        public ConnectorBends ConnBend { get => connBend; set => connBend = value; }

        public int XB = -1; //point where wire or bar bend occurs, if there is one.
        public int YB = -1;

        public bool IsSelected = false;

        public virtual string SaveLine()
        {
            return "";
        }

        //This is used for when we need to save a copy of this connector, but with adjusted values for the IDs it is connecting to,
        //such as when copied to the clipboard. Instead of making a whole new copy of the element, we can just do this.
        public virtual string SaveLineAltEnds(int E1, int E2)
        {
            return "";
        }

        public bool HitTest(int X, int Y)
        {
            //Given X and Y coords, see if hit this connector.
            //(Takes bending into account.)
            bool R = false;
            bool P = false;

            int HY; //higher Y
            int LY; //lower Y
            int RX; //right X
            int LX; //left X

            bool Y1IsH; //Y1 is higher than Y2

            //Left and Right X
            if (X1 < X2)
            {
                LX = X1;
                RX = X2;
            }
            else
            {
                LX = X2;
                RX = X1;
            }
            //Top and bottom Y
            if (Y1 < Y2)
            {
                HY = Y1;
                LY = Y2;
                Y1IsH = false;
            }
            else
            {
                HY = Y2;
                LY = Y1;
                Y1IsH = true;
            }

            //Hit Test for vertical line:
            if (X1 == X2)
            {
                P = true;
                if ((X == X1) && (Y >= HY) && (Y <= LY))
                    R = true;
            }

            //Hit Test for horizontal line:
            if (Y1 == Y2 && P == false)
            {
                P = true;
                if ((Y == Y1) && (X >= LX) && (X <= (RX - 1)))
                    R = true;
            }

            //Hit Test for diagonal or bent lines
            if (P == false)
            {
                switch (ConnBend)
                {   //Hit Test for Diagonal lines:
                    case ConnectorBends.connbendNone:
                        for (int I = LX; I <= RX; I++)
                            for (int J = HY; J <= LY; J++)
                                if (I == X && J == Y)
                                    R = true;
                        break;
                    //Hit Test for Bent Lines:
                    case ConnectorBends.connbendUp:
                        //Along top:
                        if (Y == HY && X >= LX && X <= RX)
                            R = true;
                        //Along the side:
                        if (Y1IsH)
                        {
                            if ((X == (X1 - 1)) && Y <= LY && Y >= HY)
                                R = true;
                        }
                        else
                        {
                            if ((X == (X2 - 1)) && Y <= LY && Y >= HY)
                                R = true;
                        }
                        break;
                    case ConnectorBends.connbendDown:
                        //Along top:
                        if (Y == LY && X >= LX && X <= RX)
                            R = true;
                        //Along the side:
                        if (Y1IsH)
                        {
                            if ((X == (X2 - 1)) && Y <= LY && Y >= HY)
                                R = true;
                        }
                        else
                        {
                            if ((X == (X1 - 1)) && Y <= LY && Y >= HY)
                                R = true;
                        }
                        break;
                } //switch wirebend
            } //P = false

            return R;
        } //HitTest
    } //connecting piece

    public class Wire : ConnectingPiece
    {
        //A Wire class stores a few more simple values for what wire ends are
        //connected to.

        private short end1Out; //Which output of obj. 1 to connect to.
        private short end2In; //Which input of obj. 2 to connect to.
        public short End1Out { get => end1Out; set => end1Out = value; }
        public short End2In { get => end2In; set => end2In = value; }

        public bool IsHot = false; //Determines if this wire has power or not.

        public override string SaveLine()
        {
            return SaveLineAltEnds(End1ID, End2ID);
        }

        public override string SaveLineAltEnds(int E1, int E2)
        {
            return "W|" + E1 + "|" + E2 + "|" + End1Out + "|"
                    + End2In + "|" + ConnBend.ToString();
        }
    } //Wire

} //namespace