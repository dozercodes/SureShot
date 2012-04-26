using System;

namespace Tutorial5___Simple_Physics
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Tutorial5 game = new Tutorial5())
            {
                game.Run();
            }
        }
    }
#endif
}

