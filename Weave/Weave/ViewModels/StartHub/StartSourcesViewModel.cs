using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.ViewModels.StartHub
{
    public class StartSourcesViewModel : StartItemBase
    {
        public struct SourceListing
        {
            public String Display { get; set; }
            public String Key { get; set; }
        }

        private ObservableCollection<SourceListing> _items = new ObservableCollection<SourceListing>();
        public ObservableCollection<SourceListing> Items
        {
            get { return _items; }
        }

    } // end of class
}
