using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Weave.Common;
using Weave.ViewModels;
using Weave.ViewModels.Browse;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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

        public void ClearData()
        {
            FeedManagementViewModel vm = this.DataContext as FeedManagementViewModel;
            if (vm != null)
            {
                vm.SelectCategory(null);
            }
            GrdVwCategories.SelectedItem = null;
            TxtBxInput.Text = "";
            TxtBlkInputOverlay.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Rect bounds = DisplayUtilities.GetPopupElementRect(button);
                PopupAddToCategory.HorizontalOffset = bounds.Left;
                PopupAddToCategory.VerticalOffset = bounds.Top + bounds.Height;
                List<String> availableCategories = UserHelper.Instance.GetAvailableCategories();
                LstBxAvailableCategories.ItemsSource = availableCategories;
                PopupAddToCategory.Tag = button.DataContext;
                PopupAddToCategory.IsOpen = true;
            }
        }

        private void PopupAddToCategory_Closed(object sender, object e)
        {
            PopupAddToCategory.Tag = null;
        }

        private void PopupAddToCategory_Opened(object sender, object e)
        {
            double height = FlyoutContent.ActualHeight;

            SbAddToCategoryPopIn.Begin();
        }

        private void LstBxAvailableCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                String category = e.AddedItems[0] as String;

                FeedManagementViewModel vm = this.DataContext as FeedManagementViewModel;
                if (vm != null)
                {
                    vm.AddFeedToCategory(category, PopupAddToCategory.Tag as FeedItemViewModel);
                }

                LstBxAvailableCategories.SelectedItem = null;
                PopupAddToCategory.IsOpen = false;
            }
        }

        private void TxtBxInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            String text = TxtBxInput.Text;
            if (String.IsNullOrEmpty(text))
            {
                TxtBlkInputOverlay.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                TxtBlkInputOverlay.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void TxtNewCategory_TextChanged(object sender, TextChangedEventArgs e)
        {
            String text = TxtBxNewCategory.Text;
            if (String.IsNullOrEmpty(text))
            {
                TxtBlkNewCategoryOverlay.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                TxtBlkNewCategoryOverlay.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private async void TxtBxInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                TextBox textbox = sender as TextBox;
                if (textbox != null && !String.IsNullOrEmpty(textbox.Text))
                {
                    FeedManagementViewModel vm = this.DataContext as FeedManagementViewModel;
                    if (vm != null)
                    {
                        GrdVwCategories.SelectedItem = null;
                        e.Handled = true;
                        await vm.Search(textbox.Text);
                    }
                }
            }
        }

        private void FlyoutContent_Loaded(object sender, RoutedEventArgs e)
        {
            double bottom = PopupAddToCategory.VerticalOffset + FlyoutContent.ActualHeight;
            if (bottom > Window.Current.Bounds.Bottom)
            {
                PopupAddToCategory.VerticalOffset -= (bottom - Window.Current.Bounds.Bottom);
            }
        }

        private void BtnAddNewCategory_Click(object sender, RoutedEventArgs e)
        {
            FeedManagementViewModel vm = this.DataContext as FeedManagementViewModel;
            String category = TxtBxNewCategory.Text;
            if (vm != null && !String.IsNullOrEmpty(category))
            {
                vm.AddFeedToCategory(category, PopupAddToCategory.Tag as FeedItemViewModel);
            }

            LstBxAvailableCategories.SelectedItem = null;
            PopupAddToCategory.IsOpen = false;
        }

    } // end of class
}
