using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Weave.Common
{
    public static class ThreadHelper
    {
        public static void CheckBeginInvokeOnUI(Action action)
        {
            CoreDispatcher dispatcher = Window.Current.Dispatcher;
            if (dispatcher.HasThreadAccess) action();
            else dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }
    }
}
