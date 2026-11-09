using MSLLogicGatesAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

/*
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or distribute this software, either in source code form or as a compiled binary, for any purpose, commercial or non-commercial, and by any means.
In jurisdictions that recognize copyright laws, the author or authors of this software dedicate any and all copyright interest in the software to the public domain. We make this dedication for the benefit of the public at large and to the detriment of our heirs and successors. We intend this dedication to be an overt act of relinquishment in perpetuity of all present and future rights to this software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
//Original Author: Mitchell Scott Lampert


namespace MSLLogicGatesSampleClient
{
    /// <summary>
    /// Interaction logic for LogicBoardCtrl.xaml
    /// </summary>
    public partial class LogicBoardCtrl : UserControl
    {
        #region Variables and Properties

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private LogicBoardCircuit TheMainCircuit = new LogicBoardCircuit();

        private DispatcherTimer PowerClock = new System.Windows.Threading.DispatcherTimer();

        private CircSimBitMapCacheSystem BitMapCache = new CircSimBitMapCacheSystem();

        private CircPowerModes mCircPowerMode = CircPowerModes.CPowerOff;

        #endregion

        #region Setting Up and Initializing Stuff

        public LogicBoardCtrl()
        {
            InitializeComponent();
            PowerClock.Interval = TimeSpan.FromMilliseconds(110);
            PowerClock.Tick += PowerClock_Tick;
        } //Constructor

        public void InitializeBitMapCache()
        {
            BitMapCache = new CircSimBitMapCacheSystem();
            BitMapCache.InitializeTheCache();
        }

        private void ClearGameBoard()
        {
            PowerMode = CircPowerModes.CPowerOff;
            TheMainCircuit = new LogicBoardCircuit();
            
            //Don't "redraw it all". This should be faster:
            using (Bitmap MainBmp = new Bitmap((int)PnlMainBorder.ActualWidth, (int)PnlMainBorder.ActualHeight))
            using (Graphics G = Graphics.FromImage(MainBmp))
            {
                G.Clear(System.Drawing.Color.White);
                DrawBitmapToMainImage(MainBmp);
            }
        } //ClearGameBoard

        #endregion

        #region Loading Stuff

        public string LoadAllGameBoardContent(string TheContent)
        {
            ClearGameBoard();
            string R = TheMainCircuit.LoadAllGameBoardContent(TheContent);
            RedrawTheWholeGameBoard();

            return R;
        } //LoadAllGameBoardContent

        #endregion

        #region Power Flow Stuff!!

        public void PowerFlowStep()
        {
            TheMainCircuit.PowerFlow();
            RedrawTheWholeGameBoard();
            TheMainCircuit.AfterClockTick();
        }

        public CircPowerModes PowerMode
        {
            get { return mCircPowerMode; }
            set
            {
                mCircPowerMode = value;
                if (PowerMode == CircPowerModes.CPowerOn)
                    PowerClock.Start();
                else
                    PowerClock.Stop();
            }
        }

        public void TogglePowerFlow()
        {
            if (mCircPowerMode == CircPowerModes.CPowerOn)
                PowerMode = CircPowerModes.CPowerOff;
            else
                PowerMode = CircPowerModes.CPowerOn;
        }

        #endregion

        #region Drawing Everything!

        private void DrawBitmapToMainImage(Bitmap B)
        {
            IntPtr hBitmap = B.GetHbitmap();

            try
            {
                PnlMainImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Something Weird Happened!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                //This helps prevent memory error crashes:
                DeleteObject(hBitmap);
            }
        }

        private void RedrawComponent(int ID, Graphics G)
        {
            const string ColorMaskInd = "{colormask}:";
            const string ReversingInd = "{reverseit}:";

            BasicComponent O = TheMainCircuit.TheComponents[ID];

            string[] BMPOverlays = O.GetImage().Split('|');

            bool ColorOverlay = false;
            System.Drawing.Color OverlayColor = System.Drawing.Color.Black;

            bool IsReversing = false;

            //Clickable areas of user switches:
            if (O.IsSelected && O is IUserSwitchable)
            {
                IUserSwitchable US = (IUserSwitchable)O;
                G.FillEllipse(System.Drawing.Brushes.Aqua, US.SwitchCoordX - 5, US.SwitchCoordY - 5, 10, 10);
            }

            foreach (string OImage in BMPOverlays)
            {
                if (OImage != "" && !OImage.Contains(':'))
                {
                    using (Bitmap B = (Bitmap)BitMapCache.TheCache[OImage].Clone())
                    {
                        B.MakeTransparent(System.Drawing.Color.White);

                        if (IsReversing)
                            B.RotateFlip(RotateFlipType.RotateNoneFlipX);

                        switch (O.ObjRotation)
                        {
                            case ObjRotationDirs.ObjRot90:
                                B.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;
                            case ObjRotationDirs.ObjRot180:
                                B.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                break;
                            case ObjRotationDirs.ObjRot270:
                                B.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                        }


                        if (ColorOverlay)
                        {
                            ImageAttributes imgattr = new ImageAttributes();
                            ColorMap[] clrmap = new ColorMap[1] { new ColorMap() };

                            clrmap[0].OldColor = System.Drawing.Color.Gray;
                            clrmap[0].NewColor = OverlayColor;

                            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(O.X, O.Y, O.Width, O.Height);

                            imgattr.SetRemapTable(clrmap);
                            G.DrawImage(B, rect, 0, 0, rect.Width, rect.Height, GraphicsUnit.Pixel, imgattr);

                            imgattr.Dispose();
                        }
                        else
                        {
                            G.DrawImage(B, O.X, O.Y);
                        }

                        ColorOverlay = false;

                    } //Using Bitmap B (Here, we are disposing a "clone".)

                } //If OImage not null


                if (OImage.Contains(ColorMaskInd))
                {
                    string SColor = OImage.Replace(ColorMaskInd, "").ToLower();

                    OverlayColor = GetOverlayColorFromStr(SColor);

                    ColorOverlay = true;
                } //Color Mask

                if (OImage.Contains(ReversingInd)) IsReversing = true;

            } //for each string in overlays

        } //Redraw Component

        private System.Drawing.Color GetOverlayColorFromStr(string SColor)
        {
            System.Drawing.Color OverlayColor = new System.Drawing.Color();

            switch (SColor)
            {
                case "red":
                    OverlayColor = System.Drawing.Color.Red;
                    break;
                case "orange":
                    OverlayColor = System.Drawing.Color.Orange;
                    break;
                case "yellow":
                    OverlayColor = System.Drawing.Color.Yellow;
                    break;
                case "green":
                    OverlayColor = System.Drawing.Color.Green;
                    break;
                case "blue":
                    OverlayColor = System.Drawing.Color.Blue;
                    break;
                case "black":
                    OverlayColor = System.Drawing.Color.Black;
                    break;
                case "gray":
                    OverlayColor = System.Drawing.Color.LightGray;
                    break;
                case "white":
                    OverlayColor = System.Drawing.Color.White;
                    break;
                default:
                    OverlayColor = System.Drawing.Color.Black;
                    break;
            }

            return OverlayColor;
        } //Get overlay color

        private void RedrawWires(Graphics G)
        {
            foreach (Wire W in TheMainCircuit.Wires)
            {
                if (W.IsSelected)
                {
                    using (System.Drawing.Pen PSW = new System.Drawing.Pen(System.Drawing.Brushes.Yellow, 3))
                    {
                        if (W.ConnBend == ConnectorBends.connbendNone)
                        {
                            G.DrawLine(PSW, W.X1, W.Y1, W.X2, W.Y2);
                        }
                        else
                        {
                            G.DrawLine(PSW, W.X1, W.Y1, W.XB, W.YB);
                            G.DrawLine(PSW, W.X2, W.Y2, W.XB, W.YB);
                        }
                    }
                } //Is Selected

                if (W.IsHot)
                    if (W.ConnBend == ConnectorBends.connbendNone)
                    {
                        G.DrawLine(Pens.Red, W.X1, W.Y1, W.X2, W.Y2);
                    }
                    else
                    {
                        G.DrawLine(Pens.Red, W.X1, W.Y1, W.XB, W.YB);
                        G.DrawLine(Pens.Red, W.X2, W.Y2, W.XB, W.YB);
                    }
                else //if not hot:
                {
                    if (W.ConnBend == ConnectorBends.connbendNone)
                    {
                        G.DrawLine(Pens.Black, W.X1, W.Y1, W.X2, W.Y2);
                    }
                    else
                    {
                        G.DrawLine(Pens.Black, W.X1, W.Y1, W.XB, W.YB);
                        G.DrawLine(Pens.Black, W.X2, W.Y2, W.XB, W.YB);
                    }
                } //Is Hot



            } //For each connecting piece

        } //Draw wires


        public void RedrawTheWholeGameBoard()
        {
            short I = -1;
            using (Bitmap MainBmp = new Bitmap((int)PnlMainBorder.ActualWidth, (int)PnlMainBorder.ActualHeight))
            using (Graphics G = Graphics.FromImage(MainBmp))
            {
                G.Clear(System.Drawing.Color.White);

                foreach (BasicComponent O in TheMainCircuit.TheComponents)
                {
                    I++;
                    RedrawComponent(I, G);
                }

                RedrawWires(G);

                DrawBitmapToMainImage(MainBmp);

            } //Using Graphics elements - All will be disposed here!

        } //REDRAW IT ALL!!

        #endregion

        #region Event Handlers of Child Controls

        private void PowerClock_Tick(object? sender, EventArgs e)
        {
            PowerFlowStep();
        } //The Main Power Clock Timer Tick

        #endregion

    } //Control Class
} //namespace
