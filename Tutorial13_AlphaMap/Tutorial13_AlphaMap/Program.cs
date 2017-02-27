using System;

namespace Tutorial13_AlphaMap
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ShaderTutorial game = new ShaderTutorial())
            {
                game.Run();
            }
        }
    }
}

