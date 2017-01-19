﻿using GlmNet;
using System;

namespace Framework.OGL
{

    /* Perlin noise class.  ( by Tom Nuydens (tom@delphi3d.net) )
 * Converted to C# by Mattias Fagerlund, Mattias.Fagerlund@cortego.se
 
  ******************************************************************************
 
  I used the following references for my implementation:
    http://students.vassar.edu/mazucker/code/perlin-noise-math-faq.html
    Darwin Peachey's chapter in "Texturing & Modeling: A Procedural Approach"
  Another good resource is
    http://freespace.virgin.net/hugo.elias/models/m_perlin.htm
 
  ******************************************************************************
 
  This class generates 3D Perlin noise. The demo that comes with this is 2D, but
  uses the 3rd dimension to create animated noise. The noise does not tile,
  although it could be made to do so with a few small modifications to the
  algorithm.
 
  Perlin noise can be used as a starting point for all kinds of things,
  including terrain generation, cloud rendering, procedural textures, and more.
  Most of these techniques involve rendering multiple "octaves" of noise. This
  means you generate multiple noise values for every pixel (each with different
  X, Y and/or Z coordinates), and then sum them. There's an example of this in
  the accompanying demo.
*/

    public class PerlinNoise
    {
        private const int GradientSizeTable = 256;
        private readonly Random _random;
        private readonly float[] _gradients = new float[GradientSizeTable * 3];
        /* Borrowed from Darwyn Peachey (see references above).
           The gradient table is indexed with an XYZ triplet, which is first turned
           into a single random index using a lookup in this table. The table simply
           contains all numbers in [0..255] in random order. */
        private readonly byte[] _perm = new byte[] {
              225,155,210,108,175,199,221,144,203,116, 70,213, 69,158, 33,252,
                5, 82,173,133,222,139,174, 27,  9, 71, 90,246, 75,130, 91,191,
              169,138,  2,151,194,235, 81,  7, 25,113,228,159,205,253,134,142,
              248, 65,224,217, 22,121,229, 63, 89,103, 96,104,156, 17,201,129,
               36,  8,165,110,237,117,231, 56,132,211,152, 20,181,111,239,218,
              170,163, 51,172,157, 47, 80,212,176,250, 87, 49, 99,242,136,189,
              162,115, 44, 43,124, 94,150, 16,141,247, 32, 10,198,223,255, 72,
               53,131, 84, 57,220,197, 58, 50,208, 11,241, 28,  3,192, 62,202,
               18,215,153, 24, 76, 41, 15,179, 39, 46, 55,  6,128,167, 23,188,
              106, 34,187,140,164, 73,112,182,244,195,227, 13, 35, 77,196,185,
               26,200,226,119, 31,123,168,125,249, 68,183,230,177,135,160,180,
               12,  1,243,148,102,166, 38,238,251, 37,240,126, 64, 74,161, 40,
              184,149,171,178,101, 66, 29, 59,146, 61,254,107, 42, 86,154,  4,
              236,232,120, 21,233,209, 45, 98,193,114, 78, 19,206, 14,118,127,
               48, 79,147, 85, 30,207,219, 54, 88,234,190,122, 95, 67,143,109,
              137,214,145, 93, 92,100,245,  0,216,186, 60, 83,105, 97,204, 52};

        public PerlinNoise(int seed)
        {
            _random = new Random(seed);
            initGradients();
        }

        public vec3 ComputeCurl(float x, float y, float z, float epsilon)
        {
            float n1, n2, a, b;
            vec3 curl = new vec3();

            n1 = Noise(x, y + epsilon, z);
            n2 = Noise(x, y - epsilon, z);
            a = (n1 - n2) / (2 * epsilon);

            n1 = Noise(x, y, z + epsilon);
            n2 = Noise(x, y, z - epsilon);
            b = (n1 - n2) / (2 * epsilon);

            curl.x = a - b;

            n1 = Noise(x, y, z + epsilon);
            n2 = Noise(x, y, z - epsilon);
            a = (n1 - n2) / (2 * epsilon);

            n1 = Noise(x + epsilon, y, z);
            n2 = Noise(x - epsilon, y, z);
            b = (n1 - n2) / (2 * epsilon);

            curl.y = a - b;

            n1 = Noise(x + epsilon, y, z);
            n2 = Noise(x - epsilon, y, z);
            a = (n1 - n2) / (2 * epsilon);

            n1 = Noise(x, y + epsilon, z);
            n2 = Noise(x, y - epsilon, z);
            b = (n1 - n2) / (2 * epsilon);

            curl.z = a - b;

            return curl;
        }

        public float Noise(float x, float y, float z)
        {
            /* The main noise function. Looks up the pseudorandom gradients at the nearest
               lattice points, dots them with the input vector, and interpolates the
               results to produce a single output value in [0, 1] range. */

            int ix = (int)Math.Floor(x);
            float fx0 = x - ix;
            float fx1 = fx0 - 1;
            float wx = smooth(fx0);

            int iy = (int)Math.Floor(y);
            float fy0 = y - iy;
            float fy1 = fy0 - 1;
            float wy = smooth(fy0);

            int iz = (int)Math.Floor(z);
            float fz0 = z - iz;
            float fz1 = fz0 - 1;
            float wz = smooth(fz0);

            float vx0 = lattice(ix, iy, iz, fx0, fy0, fz0);
            float vx1 = lattice(ix + 1, iy, iz, fx1, fy0, fz0);
            float vy0 = lerp(wx, vx0, vx1);

            vx0 = lattice(ix, iy + 1, iz, fx0, fy1, fz0);
            vx1 = lattice(ix + 1, iy + 1, iz, fx1, fy1, fz0);
            float vy1 = lerp(wx, vx0, vx1);

            float vz0 = lerp(wy, vy0, vy1);

            vx0 = lattice(ix, iy, iz + 1, fx0, fy0, fz1);
            vx1 = lattice(ix + 1, iy, iz + 1, fx1, fy0, fz1);
            vy0 = lerp(wx, vx0, vx1);

            vx0 = lattice(ix, iy + 1, iz + 1, fx0, fy1, fz1);
            vx1 = lattice(ix + 1, iy + 1, iz + 1, fx1, fy1, fz1);
            vy1 = lerp(wx, vx0, vx1);

            float vz1 = lerp(wy, vy0, vy1);
            return lerp(wz, vz0, vz1);
        }

        private void initGradients()
        {
            for (int i = 0; i < GradientSizeTable; i++)
            {
                float z = 1f - 2f * (float)_random.NextDouble();
                float r = (float)Math.Sqrt(1f - z * z);
                float theta = 2f * (float)Math.PI * (float)_random.NextDouble();
                _gradients[i * 3] = r * (float)Math.Cos(theta);
                _gradients[i * 3 + 1] = r * (float)Math.Sin(theta);
                _gradients[i * 3 + 2] = z;
            }
        }

        private int permutate(int x)
        {
            const int mask = GradientSizeTable - 1;
            return _perm[x & mask];
        }

        private int gradientIndex(int ix, int iy, int iz)
        {
            // Turn an XYZ triplet into a single gradient table index.
            return permutate(ix + permutate(iy + permutate(iz)));
        }

        private float lattice(int ix, int iy, int iz, float fx, float fy, float fz)
        {
            // Look up a random gradient at [ix,iy,iz] and dot it with the [fx,fy,fz] vector.
            int index = gradientIndex(ix, iy, iz);
            int g = index * 3;
            return _gradients[g] * fx + _gradients[g + 1] * fy + _gradients[g + 2] * fz;
        }

        private float lerp(float t, float value0, float value1)
        {
            // Simple linear interpolation.
            return value0 + t * (value1 - value0);
        }

        private float smooth(float x)
        {
            /* Smoothing curve. This is used to calculate interpolants so that the noise
              doesn't look blocky when the frequency is low. */
            return x * x * (3 - 2 * x);
        }
    }
}
