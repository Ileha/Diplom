using System;
using System.Threading;

namespace MyRandom
{
	public static class MyRandom
	{
		private static readonly Random main_random;
		private static Random Rand
		{
			get
			{
				lock (global_lock)
				{
					return new Random(main_random.Next());
				}
			}
		}
		private static object global_lock;
		private static readonly ThreadLocal<Random> threadRandom = new ThreadLocal<Random>(() =>
		{
			return Rand;
		});
		public static Random randomInternal { get { return threadRandom.Value; } }

		static MyRandom()
		{
			main_random = new Random();
			global_lock = new object();
		}

		public static double GetRandomDouble()
		{
			return randomInternal.NextDouble();
		}
		public static double GetRandomDouble(double max)
		{
			return GetRandomDouble() % max;
		}
		public static double GetRandomDouble(double min, double max)
		{
			return GetRandomDouble() * (max - min) + min;
		}
		public static uint GetRandomUInt32()
		{
			return (uint)Math.Floor(GetRandomDouble(1) * uint.MaxValue);
		}
		public static double NormalDistribution(double m = 0.0d, double d = 1.0d) {
			/*
			double x, y, s;
			do {
				x = rnd.NextDouble();
				y = rnd.NextDouble();
				s = x * x + y * y;
			} while (!(0 < s && s <= 1));
			return m+d*(x * Math.Sqrt((-2*Math.Log(s))/s));
			*/
			double result = 0;
			for (int i = 0; i < 12; i++) {
				result += randomInternal.NextDouble();
			}
			return m + d * (result - 6);
		}
		public static double GammaDistribution(double alfa = 2.0d, double lambda = 1.0d) {//need to remake
            /*
             * код из википедии
            uint k = (uint)Math.Floor(alfa);
            double calk_alfa = alfa - k; 

            
            double b = Math.E / (Math.E + calk_alfa);
            double v = 0;
            double n = 0;

            do {
                double r1 = rnd.NextDouble();
                double r2 = rnd.NextDouble();

                if (r1 <= b) {
                    v = Math.Pow(r1 / b, 1.0/b);
                    n = r2 * Math.Pow(v, calk_alfa - 1);
                }
                else {
                    v = 1.0d - Math.Log((r1-b)/(1.0-b), Math.E);
                    n = r2 * Math.Exp(-v);
                }
            } while(n > Math.Pow(v, calk_alfa-1)*Math.Exp(-v));

            double substract = 0;
            for (int i = 0; i < k; i++) {
                substract += Math.Log(rnd.NextDouble(), Math.E);
            }
            return lambda * (v - substract);
            */

            uint m = (uint)Math.Floor(alfa);
            double calk_alfa = alfa - m;

            double b = Math.E / (Math.E + calk_alfa);
            double r1, r2, v;
            bool condition = true;
            do {
                r1 = randomInternal.NextDouble();
                r2 = randomInternal.NextDouble();
                v = 0;
                if (r1 < b) {
                    v = Math.Pow((r1 / b), 1.0 / calk_alfa);
                    condition = r2 <= Math.Exp(-v);
                }
                else {
                    v = 1 - Math.Log((1.0 - r1) / (1.0 - b), Math.E);
                    condition = r2 <= Math.Pow(v, calk_alfa - 1.0);
                }
            } while (!condition);
            
            v = v / lambda;

            double substract = 0;
            for (int i = 0; i < m; i++) {
                substract += Math.Log(randomInternal.NextDouble(), Math.E);
            }
            return lambda * (v - substract);

            //uint m = (uint)Math.Truncate(alfa);
            //double calk_alfa = alfa - m;

            //if (0 < calk_alfa && calk_alfa <= 1) {
            //    double s1, s2, r1, r2, r3;
            //    do
            //    {
            //        r1 = rnd.NextDouble();
            //        r2 = rnd.NextDouble();
            //        r3 = rnd.NextDouble();
            //        s1 = Math.Pow(r1, 1 / calk_alfa);
            //        s2 = Math.Pow(r2, 1 / (1 - calk_alfa));
            //    } while ((s1 + s2) > 1);
            //    double y = (-s1 * Math.Log(r3)) / (lambda * (s1 + s2));
            //    if (m == 0) { return y; }

            //    double r = 1;
            //    for (int i = 0; i < m; i++) {
            //        r *= rnd.NextDouble();
            //    }
            //    return y - 1 / lambda * Math.Log(r);
            //}
            //else {
            //    return ErlangDistribution(m, lambda);
            //}
		}

		public static double ExponentialDistribution(double lambda = 1.0d) {
			return -Math.Log(randomInternal.NextDouble(), Math.E) / lambda;
		}
		public static double ErlangDistribution(uint m = 1, double lambda = 1.0d) {//need to remake
			double res = 1;
			for (int i = 0; i < m; i++) {
                res *= randomInternal.NextDouble();
			}
			return -lambda*Math.Log(res, Math.E);

		}
		public static double ParetoDistribution(double x0 = 1.0d, double alfa = 2.0d) {
			return x0 * Math.Pow(randomInternal.NextDouble(), -1/alfa);
		}
	}
}
