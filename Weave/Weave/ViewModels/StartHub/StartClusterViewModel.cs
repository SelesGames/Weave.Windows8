using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;

namespace Weave.ViewModels.StartHub
{
    public class StartClusterViewModel : BindableBase
    {
        public String Header { get; set; }

        private ObservableCollection<StartNewsItem> _items = new ObservableCollection<StartNewsItem>();
        public ObservableCollection<StartNewsItem> Items
        {
            get { return _items; }
        }

    } // end of class
}
