using System;
using System.Drawing;

namespace cymatics
{
    class BitmapUpdateEventArgs : EventArgs
    {

        public BitmapUpdateEventArgs(Bitmap img)
        {
            this.image = img;
        }

        public Bitmap image { get; private set; }
    }
}
