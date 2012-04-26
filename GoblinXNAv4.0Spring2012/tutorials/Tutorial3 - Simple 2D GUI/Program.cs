using System;

namespace Tutorial3___Simple_2D_GUI
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Tutorial3 game = new Tutorial3())
            {
                game.Run();
            }
        }
    }
#endif
}

