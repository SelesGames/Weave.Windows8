﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.ViewModels;
using Weave.ViewModels.Browse;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Weave.Common
{
    public class NavigationItemStyleSelector : StyleSelector
    {
        public Style FeedStyle { get; set; }
        public Style CoreCategoryStyle { get; set; }
        public Style CategoryStyle { get; set; }
        public Style AddStyle { get; set; }
        public Style SpacerStyle { get; set; }

        protected override Windows.UI.Xaml.Style SelectStyleCore(object item, Windows.UI.Xaml.DependencyObject container)
        {
            if (item != null)
            {
                if (item is FeedItemViewModel) return FeedStyle;
                else if (item is CategoryViewModel)
                {
                    CategoryViewModel vm = (CategoryViewModel)item;
                    if (vm.Type == CategoryViewModel.CategoryType.Other) return SpacerStyle;
                    else
                    {
                        if (vm.Type == CategoryViewModel.CategoryType.Specific) return CategoryStyle;
                        else return CoreCategoryStyle;
                    }
                }
                else if (item is FeedManagementViewModel) return AddStyle;
                else if (item is SpacerViewModel) return SpacerStyle;
            }
            return base.SelectStyleCore(item, container);
        }
    }

    public class TextSeparatorStyleSelector : StyleSelector
    {
        public Style SeparatorStyle { get; set; }
        public Style TextStyle { get; set; }

        protected override Windows.UI.Xaml.Style SelectStyleCore(object item, Windows.UI.Xaml.DependencyObject container)
        {
            if (item != null && item is String)
            {
                String str = (String)item;
                if (str.Equals("-")) return SeparatorStyle;
                else return TextStyle;
            }
            return base.SelectStyleCore(item, container);
        }
    }
}
