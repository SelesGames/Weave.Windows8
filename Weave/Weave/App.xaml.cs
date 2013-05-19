using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Weave
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        /// <summary>
        /// Helper funciton to find an element in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of element to find.</typeparam>
        /// <param name="element">The root element to start looking from.</param>
        /// <returns>The found element, otherwise null.</returns>
        public static T FindSimpleVisualChild<T>(DependencyObject element) where T : class
        {
            while (element != null)
            {

                if (element is T)
                    return element as T;

                element = VisualTreeHelper.GetChild(element, 0);
            }

            return null;
        }

        /// <summary>
        /// Helper function to find the visual state of and element.
        /// </summary>
        /// <param name="element">The element to search through.</param>
        /// <param name="name">The name of the visual state group.</param>
        /// <returns></returns>
        public static VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
                return null;

            IList<VisualStateGroup> groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup group in groups)
                if (group.Name == name)
                    return group;

            return null;
        }

        /// Listen for change of the dependency property  
        public static void RegisterForNotification(string propertyName, FrameworkElement element, Object defaultValue, PropertyChangedCallback callback)
        {

            //Bind to a depedency property  
            Binding b = new Binding();
            b.Path = new PropertyPath(propertyName);
            b.Source = element;
            var prop = DependencyProperty.RegisterAttached(
                "ListenAttached" + propertyName,
                typeof(object),
                typeof(UserControl),
                new PropertyMetadata(defaultValue, callback));

            element.SetBinding(prop, b);
        }

        public static void LogError(String message, Exception e = null)
        {
            // add logging logic
        }

    } // end of class
}
