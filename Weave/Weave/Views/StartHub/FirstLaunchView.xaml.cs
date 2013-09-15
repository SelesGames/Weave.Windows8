using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Weave.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Weave.Views.StartHub
{
    public sealed partial class FirstLaunchView : UserControl
    {
        private class CategoryModel : BindableBase
        {
            private String _name;
            public String Name
            {
                get { return _name; }
                set { SetProperty(ref _name, value); }
            }

            private int _souceCount;
            public int SourceCount
            {
                get { return _souceCount; }
                set { SetProperty(ref _souceCount, value); }
            }

            private String _selectedImage;
            public String SelectedImage
            {
                get { return _selectedImage; }
                set { SetProperty(ref _selectedImage, value); }
            }

            private String _unselectedImage;
            public String UnselectedImage
            {
                get { return _unselectedImage; }
                set { SetProperty(ref _unselectedImage, value);}
            }

            private bool _isSelected;
            public bool IsSelected
            {
                get { return _isSelected; }
                set { SetProperty(ref _isSelected, value); }
            }
        }

        private const String ImagePathFormat = "/Assets/FirstLaunch/{0}.png";

        private ObservableCollection<CategoryModel> _items = new ObservableCollection<CategoryModel>();

        public event Action<object> Completed;

        public FirstLaunchView()
        {
            this.InitializeComponent();

            InitCategories();
            GrdVwCategories.ItemsSource = _items;

            GrdVwCategories.SelectedItems.Add(_items[2]);
            GrdVwCategories.SelectedItems.Add(_items[5]);
            GrdVwCategories.SelectedItems.Add(_items[7]);
        }

        private void InitCategories()
        {
            CategoryModel model = new CategoryModel();
            String imageName = "Business";
            model.Name = "Business";
            model.SourceCount = 12;
            model.UnselectedImage = String.Format(ImagePathFormat, imageName); ;
            model.SelectedImage = String.Format(ImagePathFormat, imageName + "_Selected");
            _items.Add(model);

            model = new CategoryModel();
            imageName = "Gaming";
            model.Name = "Gaming";
            model.SourceCount = 6;
            model.UnselectedImage = String.Format(ImagePathFormat, imageName); ;
            model.SelectedImage = String.Format(ImagePathFormat, imageName + "_Selected");
            _items.Add(model);

            model = new CategoryModel();
            imageName = "Microsoft";
            model.Name = "Microsoft";
            model.SourceCount = 7;
            model.UnselectedImage = String.Format(ImagePathFormat, imageName); ;
            model.SelectedImage = String.Format(ImagePathFormat, imageName + "_Selected");
            _items.Add(model);

            model = new CategoryModel();
            imageName = "ScienceAndAstronomy";
            model.Name = "Science & Astronomy";
            model.SourceCount = 4;
            model.UnselectedImage = String.Format(ImagePathFormat, imageName); ;
            model.SelectedImage = String.Format(ImagePathFormat, imageName + "_Selected");
            _items.Add(model);

            model = new CategoryModel();
            imageName = "Sports";
            model.Name = "Sports";
            model.SourceCount = 9;
            model.UnselectedImage = String.Format(ImagePathFormat, imageName); ;
            model.SelectedImage = String.Format(ImagePathFormat, imageName + "_Selected");
            _items.Add(model);

            model = new CategoryModel();
            imageName = "Technology";
            model.Name = "Technology";
            model.SourceCount = 14;
            model.UnselectedImage = String.Format(ImagePathFormat, imageName); ;
            model.SelectedImage = String.Format(ImagePathFormat, imageName + "_Selected");
            _items.Add(model);

            model = new CategoryModel();
            imageName = "USNews";
            model.Name = "U.S. News";
            model.SourceCount = 6;
            model.UnselectedImage = String.Format(ImagePathFormat, imageName); ;
            model.SelectedImage = String.Format(ImagePathFormat, imageName + "_Selected");
            _items.Add(model);

            model = new CategoryModel();
            imageName = "WorldNews";
            model.Name = "World News";
            model.SourceCount = 19;
            model.UnselectedImage = String.Format(ImagePathFormat, imageName); ;
            model.SelectedImage = String.Format(ImagePathFormat, imageName + "_Selected");
            _items.Add(model);
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (Completed != null) Completed(this);
        }

        private void GrdVwCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (CategoryModel model in e.AddedItems)
            {
                model.IsSelected = true;
            }
            foreach (CategoryModel model in e.RemovedItems)
            {
                model.IsSelected = false;
            }

            if (GrdVwCategories.SelectedItems.Count > 0)
            {
                BtnStart.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                BtnStart.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void GrdVwCategories_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GrdVwCategories.SelectedItems.Contains(e.ClickedItem))
            {
                GrdVwCategories.SelectedItems.Remove(e.ClickedItem);
            }
            else
            {
                GrdVwCategories.SelectedItems.Add(e.ClickedItem);
            }
        }
    }
}
