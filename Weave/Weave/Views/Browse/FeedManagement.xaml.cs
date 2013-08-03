using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Weave.ViewModels.Browse;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Weave.Views.Browse
{
    public sealed partial class FeedManagement : UserControl
    {
        public FeedManagement()
        {
            this.InitializeComponent();
        }

        private void GrdVwCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FeedManagementViewModel vm = this.DataContext as FeedManagementViewModel;
            if (vm != null)
            {
                if (e.AddedItems.Count > 0)
                {
                    String category = e.AddedItems[0] as String;
                    vm.SelectCategory(category);
                }
                else vm.SelectCategory(null);
            }
        }

        public void ClearSelection()
        {
            GrdVwCategories.SelectedItem = null;
        }

    } // end of class
}
