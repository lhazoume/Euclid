using Euclid;

namespace EuclidBenchmark
{
    public static class VectorCases
    {
        public static void MultiplyScalar(int iterations)
        {
            Vector vector = Vector.Create(10, 1.0);
            for (int i = 0; i < iterations; i++)
            {
                Vector v = vector * 1.0;
            }
        }

        public static void MultiplyVector(int iterations)
        {
            Vector v1 = Vector.Create(10, 1.0),
                v2 = Vector.Create(20, 2.0);
            for (int i = 0; i < iterations; i++)
            {
                Matrix m = v1 * v2;
            }
        }

        public static void AddVector(int iterations)
        {
            Vector v1 = Vector.Create(10, 1.0),
                v = Vector.Create(10, 0.0);
            for (int i = 0; i < iterations; i++)
                v += v1;
        }

        public static void AddVectorScalar(int iterations)
        {
            Vector vector = Vector.Create(10);
            for (int i = 0; i < iterations; i++)
            {
                Vector v = vector + 1.0;
            }
        }
        public static void SubstractVectorScalar(int iterations)
        {
            Vector v = Vector.Create(10, 0.0);
            for (int i = 0; i < iterations; i++)
                v -= 1.0;
        }
    }
}
