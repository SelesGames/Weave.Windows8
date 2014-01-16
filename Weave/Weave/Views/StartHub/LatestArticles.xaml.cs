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
    public sealed partial class LatestArticles : UserControl
    {
        // special event for testing to select hero image
        public static event Action<StartNewsItemContainer> HeroSelected;

        public LatestArticles()
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

        private async void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            StartNewsItemContainer item = ((FrameworkElement)sender).DataContext as StartNewsItemContainer;
            if (item != null)
            {
                Windows.UI.Popups.PopupMenu contextMenu = new Windows.UI.Popups.PopupMenu();
                contextMenu.Commands.Add(new Windows.UI.Popups.UICommand("Select", null, "Select"));

                //GeneralTransform buttonTransform = BtnAccount.TransformToVisual(null);
                //Point point = buttonTransform.TransformPoint(new Point(-29, 0));
                Point point = e.GetPosition(null);
                Rect position = new Rect(point, new Size());

                Windows.UI.Popups.IUICommand response = await contextMenu.ShowForSelectionAsync(position, Windows.UI.Popups.Placement.Default);
                if (response != null)
                {
                    if (HeroSelected != null) HeroSelected(item);
                }
            }
        }


        public int ItemWidth { get; set; }
        //public int ItemWidth
        //{
        //    get { return (int)GetValue(ItemWidthProperty); }
        //    set { SetValue(ItemWidthProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for ItemWidth.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ItemWidthProperty =
        //    DependencyProperty.Register("ItemWidth", typeof(int), typeof(LatestArticles), new PropertyMetadata(0));


        //public int ItemHeight
        //{
        //    get { return (int)GetValue(ItemHeightProperty); }
        //    set { SetValue(ItemHeightProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for ItemHeight.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ItemHeightProperty =
        //    DependencyProperty.Register("ItemHeight", typeof(int), typeof(LatestArticles), new PropertyMetadata(0));

        public int ItemHeight { get; set; }


    } // end of class
}
