//#define VR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Tutorial13___Stereo_Rendering___PhoneLib;

namespace Tutorial13___Stereo_Rendering___Phone
{
    public partial class GamePage : PhoneApplicationPage
    {
        ContentManager contentManager;
        GameTimer timer;

        Tutorial13_Phone tutorial13;

#if !VR
        // For rendering the XAML onto a texture
        UIElementRenderer elementRenderer;
#endif

        public GamePage()
        {
            InitializeComponent();

            // Get the content manager from the application
            contentManager = (Application.Current as App).Content;

            // Create a timer for this page
            timer = new GameTimer();
            timer.UpdateInterval = TimeSpan.FromTicks(333333);
            timer.Update += OnUpdate;
            timer.Draw += OnDraw;

            tutorial13 = new Tutorial13_Phone();

#if !VR
            LayoutUpdated += new EventHandler(GamePage_LayoutUpdated);
#endif
        }

#if !VR
        void GamePage_LayoutUpdated(object sender, EventArgs e)
        {
            // Create the UIElementRenderer to draw the XAML page to a texture.

            // Check for 0 because when we navigate away the LayoutUpdate event
            // is raised but ActualWidth and ActualHeight will be 0 in that case.
            if (ActualWidth > 0 && ActualHeight > 0 && elementRenderer == null)
            {
                elementRenderer = new UIElementRenderer(this, (int)640, (int)480);
            }
        }
#endif

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the sharing mode of the graphics device to turn on XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);

#if VR
            tutorial13.Initialize(SharedGraphicsDeviceManager.Current, contentManager, null);
#else
            tutorial13.Initialize(SharedGraphicsDeviceManager.Current, contentManager, viewfinderBrush);
#endif

            // Start the timer
            timer.Start();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Stop the timer
            timer.Stop();

            // Set the sharing mode of the graphics device to turn off XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);

            tutorial13.Dispose();

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Allows the page to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            tutorial13.Update(e.ElapsedTime, this.IsEnabled);
        }

        /// <summary>
        /// Allows the page to draw itself.
        /// </summary>
        private void OnDraw(object sender, GameTimerEventArgs e)
        {
#if !VR
            // Render the Silverlight controls using the UIElementRenderer
            elementRenderer.Render();

            if (tutorial13.VideoBackground == null)
                tutorial13.VideoBackground = elementRenderer.Texture;
#endif
            
            tutorial13.Draw(e.ElapsedTime);
        }
    }
}