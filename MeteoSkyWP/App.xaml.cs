using MeteoSkyWP.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace MeteoSkyWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        private TransitionCollection transitions;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active.
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page.
                rootFrame = new Frame();

                // Associate the frame with a SuspensionManager key.
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                // TODO: Change this value to a cache size that is appropriate for your application.
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate.
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        // Something went wrong restoring state.
                        // Assume there is no state and continue.
                    }
                }

                // Place the frame in the current Window.
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter.
                if (!string.IsNullOrEmpty(e.Arguments))
                {
                    var targetUrl = e.Arguments;
                    if (targetUrl == "CurrentLocation")
                        targetUrl = string.Empty;

                    if (!rootFrame.Navigate(typeof(ForecastPage), targetUrl))
                    {
                        throw new Exception("Failed to create initial page");
                    }
                }
                else
                {
                    if (!rootFrame.Navigate(typeof(RootPage), e.Arguments))
                    {
                        throw new Exception("Failed to create initial page");
                    }
                }
            }
            else if (!string.IsNullOrEmpty(e.Arguments))
            {
                var targetUrl = e.Arguments;
                if (targetUrl == "CurrentLocation")
                    targetUrl = string.Empty;

                rootFrame.Navigate(typeof(ForecastPage), targetUrl);
            }

            // Create background task
            var taskRegistered = false;
            var taskName = "MeteoSkyUpdateTilesBackgroundTask";

            foreach (var task in Windows.ApplicationModel.Background.BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    taskRegistered = true;
                    break;
                }
            }

            if (!taskRegistered )
            {
                var access = await Windows.ApplicationModel.Background.BackgroundExecutionManager.RequestAccessAsync();

                if (access == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity)
                {
                    var builder = new BackgroundTaskBuilder();

                    builder.Name = taskName;
                    builder.TaskEntryPoint = "MeteoSkyWPruntimeComponent.UpdateForecastBackgroundTask";
                    builder.SetTrigger(new TimeTrigger(60, false));
                    builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));

                    BackgroundTaskRegistration task = builder.Register();
                }
            }

            // Ensure the current window is active.
            Window.Current.Activate();
        }

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();

            // Check whether there is an action to perform as the result 
            // of a new tile being pinned.
            if (OnNewTilePinned != null)
            {
                OnNewTilePinned();
                OnNewTilePinned = null;
            }

            deferral.Complete();
        }

        /// <summary>
        /// Gets or sets the action to be performed as the result of a new tile being pinned.
        /// </summary>
        public static Action OnNewTilePinned { get; set; }
    }
}
