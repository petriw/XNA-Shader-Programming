using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace NoiseGPU
{
    public class PerlinNoise
    {
        GraphicsDevice device;

        // permutation table
        static int[] permutation = new int[256];

        // gradients for 3d noise
        static float[,] gradients =  
        {
            {1,1,0},
            {-1,1,0},
            {1,-1,0},
            {-1,-1,0},
            {1,0,1},
            {-1,0,1},
            {1,0,-1},
            {-1,0,-1}, 
            {0,1,1},
            {0,-1,1},
            {0,1,-1},
            {0,-1,-1},
            {1,1,0},
            {0,-1,1},
            {-1,1,0},
            {0,-1,-1}
        };

        public void InitNoiseFunctions(int seed, GraphicsDevice device)
        {
            this.device = device;

            Random rand = new Random(seed);

            // Reset
            for (int i = 0; i < permutation.Length; i++)
            {
                permutation[i] = -1;
            }

            // Generate random numbers
            for (int i = 0; i < permutation.Length; i++)
            {
                while (true)
                {
                    int iP = rand.Next() % permutation.Length;
                    if (permutation[iP] == -1)
                    {
                        permutation[iP] = i;
                        break;
                    }
                }
            }
        }

        int perm2d(int i)
        {
            return permutation[i % 256];
        }

        public Texture2D GeneratePermTexture2d()
        {
            Texture2D permTexture2d = new Texture2D(device, 256, 256, true, SurfaceFormat.Color);
            Color[] data = new Color[256 * 256];
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    int A = perm2d(x) + y;
                    int AA = perm2d(A);
                    int AB = perm2d(A + 1);
                    int B = perm2d(x + 1) + y;
                    int BA = perm2d(B);
                    int BB = perm2d(B + 1);
                    data[x + (y * 256)] = new Color((byte)(AA), (byte)(AB),
                                                    (byte)(BA), (byte)(BB));
                }
            }
            permTexture2d.SetData<Color>(data);
            return permTexture2d;
        }

        public Texture2D GeneratePermGradTexture()
        {
            Texture2D permGradTexture = new Texture2D(device, 256, 1, true, SurfaceFormat.NormalizedByte4);
            NormalizedByte4[] data = new NormalizedByte4[256 * 1];
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 1; y++)
                {
                    data[x + (y * 256)] = new NormalizedByte4(gradients[permutation[x] % 16, 0], gradients[permutation[x] % 16, 1], gradients[permutation[x] % 16, 2], 1);
                }
            }
            permGradTexture.SetData<NormalizedByte4>(data);
            return permGradTexture;
        }

        public PerlinNoise()
        {
        }
    }
}
