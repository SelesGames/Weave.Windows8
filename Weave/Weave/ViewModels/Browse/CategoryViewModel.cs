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
        public enum CategoryType { Specific, Latest, All, Favorites, Other };

        private CategoryType _type = CategoryType.Specific;
        public CategoryType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        private CategoryInfo _info;
        public CategoryInfo Info
        {
            get { return _info; }
            set { SetProperty(ref _info, value); }
        }

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

        private bool _requiresRefresh;
        public bool RequiresRefresh
        {
            get { return _requiresRefresh; }
            set { SetProperty(ref _requiresRefresh, value); }
        }

    } // end of class
}
