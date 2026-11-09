using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;

namespace MSLLogicGatesSampleClient
{
    public class CircSimBitMapCacheSystem
    {
        private bool mIsInit = false;

        ~CircSimBitMapCacheSystem()
        {
            if (TheCache != null)
            {
                foreach (Bitmap B in TheCache.Values)
                    B.Dispose();
            }
        }

        public Dictionary<string, Bitmap> TheCache = new Dictionary<string, Bitmap>();

        public string StartPath = Environment.CurrentDirectory + @"\GraphicElements\";

        public bool IsInited
        {
            get { return mIsInit; }
        }

        public void InitializeTheCache()
        {
            try
            {
                string[] BMPFiles = Directory.GetFiles(StartPath, "*.bmp");

                foreach (string B in BMPFiles)
                {
                    TheCache[Path.GetFileNameWithoutExtension(B)] = (Bitmap)Bitmap.FromFile(B);
                }

                mIsInit = true;
            }

            catch
            {

            }

        } //Initialize the Cache

    } //Bitmap Cache System

} //namespace
