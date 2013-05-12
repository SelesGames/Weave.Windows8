using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Weave.Common
{
    public class VariableGridView : GridView
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var viewModel = item as IResizable;

            if (viewModel != null)
            {
                element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, viewModel.WidthSpan);
                element.SetValue(VariableSizedWrapGrid.RowSpanProperty, viewModel.HeightSpan);
            }

            base.PrepareContainerForItemOverride(element, item);
        }
    } // end of class
}
