using System;

namespace Tutorial4___3D_Object_Selection
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Tutorial4 game = new Tutorial4())
            {
                game.Run();
            }
        }
    }
#endif
}

