using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Controls;
using System.Diagnostics;

namespace WillDevicesSampleApp
{
  /// <summary>
  /// Provides application-specific behavior to supplement the default Application class.
  /// </summary>
  sealed partial class App : Application
  {
        static readonly string license = "eyJhbGciOiJSUzUxMiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJMTVMiLCJleHAiOjE2ODg5Njg5NzAsImlhdCI6MTY4MTEwNjU3MCwic2VhdHMiOjAsInJpZ2h0cyI6WyJDRExfQUNDRVNTIiwiQ0RMX0xJVkVfU1RSRUFNSU5HIiwiQ0RMX1RISVJEUEFSVFlfUEVOUyIsIkNETF9QSFVfMTExIiwiQ0RMX09FTV9NT05UQkxBTkMiLCJDREwyX0VOVU1fVVNCIiwiQ0RMMl9FTlVNX0JMRSIsIkNETDJfRU5VTV9XQUMiLCJDREwyX0VOVU1fU1lTIiwiQ0RMMl9CQVNJQyIsIkNETDJfU0VSVklDRV9SZWFsVGltZUluayIsIkNETDJfU0VSVklDRV9EaXNjcmV0ZURpc3BsYXkiLCJDREwyX1NFUlZJQ0VfRGVza3RvcERpc3BsYXkiLCJDREwyX1NFUlZJQ0VfRmlsZVRyYW5zZmVyIiwiQ0RMMl9TRVJWSUNFX0VuY3J5cHRpb24iXSwiZGV2aWNlcyI6WyJXQUNPTV9TTUFSVFBBRCIsIldBQ09NX1NUVSIsIldBQ09NX0RSSVZFUiJdLCJ0eXBlIjoiZXZhbCIsImxpY19uYW1lIjoiV2Fjb21fSW5rX1NES19mb3JfZGV2aWNlcyIsIndhY29tX2lkIjoiMjk4YThmZmMwZmJkNDgxMGJhM2ZmOTY3MDBhYzQ3NzkiLCJsaWNfdWlkIjoiYThiZGU2MDUtNjBkMi00MWMyLWExZDQtNzk0OTNjN2ZkYjVkIiwiYXBwc193aW5kb3dzIjpbXSwiYXBwc19pb3MiOltdLCJhcHBzX2FuZHJvaWQiOltdLCJtYWNoaW5lX2lkcyI6W119.WdSmHtUSk13PxxFZe8iSXxDZlu39s6GhUY1fGpxgoKG0W0kQT1wnzQkN-dINh9rWI1KpYjPuV8OpDJGSqJQqETmQzRsZMjBK9m5zuAjC_UUTm-5dOR8bHP0eRZx1kBVco1QKDi1KnDD7HVImiaXGiwswVR4b9aG5v7cJLIaUwWOARj0vVDYpZ9xlMgblvuKOEPHrLzUpurWHTcaOJzCA3JxdGb220qBQnYZUBNN_NzzK8BNsZ06A-rlCTlO9cZR-SoHjMxNSxHgte4sS5vy7X6opYWAUxrjFZhY_VpHEJySgsLC2gLa1EV3g1ZmohN1gjze3GLwKwErWWgM5zK9FSQ";
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
      Wacom.Licensing.LicenseManager.Instance.SetLicense(license);
            Debug.WriteLine(31 / 2);
      //this.InitializeComponent();
      //this.OnSuspending += OnSuspending;
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="e">Details about the launch request and process.</param>
    protected void OnStartup(object sender, StartupEventArgs e)
    {
      Frame rootFrame = MainWindow.Content as Frame;

      // Do not repeat app initialization when the Window already has content,
      // just ensure that the window is active
      if (rootFrame == null)
      {
        // Create a Frame to act as the navigation context and navigate to the first page
        rootFrame = new Frame();

        rootFrame.NavigationFailed += OnNavigationFailed;

        //if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
        //{
        //  //TODO: Load state from previously suspended application
        //}

        // Place the frame in the current Window
        MainWindow.Content = rootFrame;
      }

      //if (e.PrelaunchActivated == false)
      {
        if (rootFrame.Content == null)
        {
          // When the navigation stack isn't restored navigate to the first page,
          // configuring the new page by passing required information as a navigation
          // parameter
          rootFrame.Navigate(typeof(MainPage), e.Args);
        }
        // Ensure the current window is active
        MainWindow.Activate();
      }
    }

    /// <summary>
    /// Invoked when Navigation to a certain page fails
    /// </summary>
    /// <param name="sender">The Frame which failed navigation</param>
    /// <param name="e">Details about the navigation failure</param>
    void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
      throw new Exception("Failed to load Page " + e.Uri);
    }

#if false
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
      if (AppObjects.Instance.Device != null)
      {
        AppObjects.Instance.Device.Dispose();
        AppObjects.Instance.Device = null;
      }

      deferral.Complete();
    } 
#endif
  }
}
