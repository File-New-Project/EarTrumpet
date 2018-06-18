using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EarTrumpet.Extensions;

namespace EarTrumpet.DataModel.Internal
{
    class LogarithmicVolume
    {
        private const float curveFactor = 6.908f;
        // Logical is the thing we display.
        // Linear is what we give the OS
        public static float displayToVolume(float vol) // Assumes Vol is a [0,1] and translates to [0,1] after logical conversion
        {
            return ((float)(Math.Exp(curveFactor * vol) / Math.Exp(curveFactor))).Bound(0, 1f);
            //return ((float)(Math.Pow(vol, 4))).Bound(0, 1f);

        }

        public static float volumeToDisplay(float vol) // Assumes Vol is a [0,1] and translates to [0,1] after logical conversion
        {
            return ((float)(Math.Log(vol * Math.Exp(curveFactor)) / curveFactor)).Bound(0, 1f);
            //return ((float)Math.Pow(vol, 0.25)).Bound(0, 1f);
        }
    }
}
