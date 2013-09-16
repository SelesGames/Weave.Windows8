using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using Weave.Common;
using Weave.FeedLibrary;
using Weave.ViewModels;
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

        private ExpandedLibrary _feedLibrary;
        private Dictionary<String, List<Feed>> _categoryFeedMap = new Dictionary<string,List<Feed>>();

        public FirstLaunchView()
        {
            this.InitializeComponent();
        }

        private void AddCategoryModel(String imageName, String categoryName)
        {
            CategoryModel model = new CategoryModel();
            model.Name = categoryName;
            if (_categoryFeedMap.ContainsKey(categoryName)) model.SourceCount = _categoryFeedMap[categoryName].Count;
            model.UnselectedImage = String.Format(ImagePathFormat, imageName); ;
            model.SelectedImage = String.Format(ImagePathFormat, imageName + "_Selected");
            _items.Add(model);
        }

        private async void InitCategories()
        {
            PrgRngLoading.IsActive = true;
            _feedLibrary = new ExpandedLibrary(ViewModels.Browse.FeedManagementViewModel.FeedsUrl + "?xsf=" + (new Random()).Next(123, 978));
            List<Feed> feeds = await _feedLibrary.Feeds.Value;
            String key;
            if (feeds != null)
            {
                foreach (Feed f in feeds)
                {
                    key = f.Category == null ? "" : f.Category;
                    if (!_categoryFeedMap.ContainsKey(key)) _categoryFeedMap[key] = new List<Feed>();
                    _categoryFeedMap[key].Add(f);
                }
            }

            AddCategoryModel("Business", "Business");
            AddCategoryModel("Gaming", "Gaming");
            AddCategoryModel("Microsoft", "Microsoft");
            AddCategoryModel("ScienceAndAstronomy", "Science & Astronomy");
            AddCategoryModel("Sports", "Sports");
            AddCategoryModel("Technology", "Technology");
            AddCategoryModel("USNews", "U.S. News");
            AddCategoryModel("WorldNews", "World News");

            PrgRngLoading.IsActive = false;

            GrdVwCategories.ItemsSource = _items;

            GrdVwCategories.SelectedItems.Add(_items[2]);
            GrdVwCategories.SelectedItems.Add(_items[5]);
            GrdVwCategories.SelectedItems.Add(_items[7]);
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0) InitCategories();
        }
    }
}
