using System;

namespace Tutorial8___Optical_Marker_Tracking
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Tutorial8 game = new Tutorial8())
            {
                game.Run();
            }
        }
    }
#endif
}

