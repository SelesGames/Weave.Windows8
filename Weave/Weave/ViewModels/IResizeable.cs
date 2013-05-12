using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.ViewModels
{
    public interface IResizable
    {
        int WidthSpan { get; set; }
        int HeightSpan { get; set; }

    } // end of interface
}
