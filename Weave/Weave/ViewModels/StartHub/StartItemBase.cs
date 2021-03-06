﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Common;

namespace Weave.ViewModels.StartHub
{
    public class StartItemBase : BindableBase
    {
        public virtual void OnHeaderClick()
        {
            throw new NotImplementedException();
        }

        public virtual void OnItemClick(object item)
        {
            throw new NotImplementedException();
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

    } // end of class
}
