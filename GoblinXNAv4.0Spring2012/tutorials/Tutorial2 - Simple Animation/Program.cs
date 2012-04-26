using System;

namespace Tutorial2___Simple_Animation
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Tutorial2 game = new Tutorial2())
            {
                game.Run();
            }
        }
    }
#endif
}

