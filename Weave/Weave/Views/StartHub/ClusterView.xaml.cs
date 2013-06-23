using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Weave.ViewModels.StartHub;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Weave.Views.StartHub
{
    public sealed partial class ClusterView : UserControl
    {
        public ClusterView()
        {
            this.InitializeComponent();
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image image = sender as Image;
            if (image != null && image.Opacity < 1)
            {
                image.Opacity = 1;
            }
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Image image = sender as Image;
            if (image != null && image.Source is BitmapImage)
            {
                BitmapImage bmp = (BitmapImage)image.Source;
                if (bmp.PixelHeight != 0) image.Opacity = 1;
            }
        }

        private void Header_Click(object sender, RoutedEventArgs e)
        {
            StartItemBase vm = this.DataContext as StartItemBase;
            if (vm != null) vm.OnHeaderClick();
        }

        private void VariableGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartItemBase vm = this.DataContext as StartItemBase;
            if (vm != null) vm.OnItemClick(e.ClickedItem);
        }

        private void BtnRemoveCluster_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                StartClusterViewModel cluster = button.DataContext as StartClusterViewModel;
                if (cluster != null)
                {
                    Weave.Common.ClusterHelper.RemoveCluster(cluster);
                }
            }
        }

    } // end of class
}
