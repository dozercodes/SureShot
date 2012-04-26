using System;

namespace Tutorial6___Simple_Particle_Systems
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Tutorial6 game = new Tutorial6())
            {
                game.Run();
            }
        }
    }
#endif
}

