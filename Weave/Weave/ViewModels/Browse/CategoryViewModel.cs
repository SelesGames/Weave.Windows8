using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;

namespace Weave.ViewModels.Browse
{
    public class CategoryViewModel : BindableBase
    {
        public CategoryInfo Info { get; set; }
        private int _newCount;
        public int NewCount
        {
            get { return _newCount; }
            set { SetProperty(ref _newCount, value); }
        }

        private String _displayName;
        public String DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

    } // end of class
}
