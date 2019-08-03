using System;
using System.Collections.Generic;
using System.Text;

namespace GalaSoft.MvvmLight.Threading
{
    class DispatcherHelper
    {
        internal static void CheckBeginInvokeOnUI(Action p)
        {
            p();
        }
    }
}
