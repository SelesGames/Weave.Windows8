using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;

namespace Weave.ViewModels.Browse
{
    public class ManagedFeedViewModel : BindableBase
    {
        private bool _isAdded;
        public bool IsAdded
        {
            get { return _isAdded; }
            set { SetProperty(ref _isAdded, value); }
        }

        private Feed _feed;
        public Feed Feed
        {
            get { return _feed; }
            set { SetProperty(ref _feed, value); }
        }

    } // end of class
}
