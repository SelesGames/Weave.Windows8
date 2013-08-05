using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;

namespace Weave.ViewModels.Browse
{
    public class SpacerViewModel : BindableBase
    {
        private int _height;
        public int SpacerHeight
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }
    }
}
