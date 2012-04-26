using System;

namespace Tutorial9___Advanced_Features
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Tutorial9 game = new Tutorial9())
            {
                game.Run();
            }
        }
    }
#endif
}

