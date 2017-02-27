using System;

namespace Tutorial11_PostProcess
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

