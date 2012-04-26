using System;

namespace Tutorial17___Interpolation
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Tutorial17 game = new Tutorial17())
            {
                game.Run();
            }
        }
    }
#endif
}

