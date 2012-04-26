using System;

namespace ARDominos
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (DominoGame game = new DominoGame())
            {
                game.Run();
            }
        }
    }
}

