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
		public static Random rnd { get { return threadRandom.Value; } }

		static MyRandom()
		{
			main_random = new Random();
			global_lock = new object();
		}

		public static double GetRandomDouble()
		{
			return rnd.NextDouble();
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
				result += rnd.NextDouble();
			}
			return m + d * (result - 6);
		}
		public static double GammaDistribution(double alfa = 2.0d, double lambda = 1.0d) {//need to remake
			/*
			  second alg
			  double b = Math.E / (Math.E + alfa);
			  double r1, r2, v;
			  bool codition = true;
			  do
			  {
				  r1 = rnd.NextDouble();
				  r2 = rnd.NextDouble();
				  v = 0;
				  if (r1 > b)
				  {
					  v = Math.Pow(r1 / b, 1 / alfa);
					  codition = r2 <= Math.Exp(-v);
				  }
				  else
				  {
					  v = 1 - Math.Log((1 - r1) / (1 - b));
					  codition = r2 <= Math.Pow(v, alfa - 1);
				  }
			  } while (!codition);
			*/

			uint m = (uint)Math.Truncate(alfa);
			double calk_alfa = alfa - m;

			if (0 < calk_alfa && calk_alfa <= 1) {
				double s1, s2, r1, r2, r3;
				do
				{
					r1 = rnd.NextDouble();
					r2 = rnd.NextDouble();
					r3 = rnd.NextDouble();
					s1 = Math.Pow(r1, 1 / calk_alfa);
					s2 = Math.Pow(r2, 1 / (1 - calk_alfa));
				} while ((s1 + s2) > 1);
				double y = (-s1 * Math.Log(r3)) / (lambda * (s1 + s2));
				if (m == 0) { return y; }

				double r = 1;
				for (int i = 0; i < m; i++) {
					r *= rnd.NextDouble();
				}
				Console.WriteLine(r);
				return y - 1 / lambda * Math.Log(r);
			}
			else {
				return ErlangDistribution(m, lambda);
			}
		}

		public static double ExponentialDistribution(double lambda = 1.0d) {
			return -Math.Log(rnd.NextDouble()) / lambda;
		}
		public static double ErlangDistribution(uint m = 1, double lambda = 1.0d) {//need to remake
			if (m == 0) { throw new Exception("m is more than 0"); }
			double res = 0;
			for (int i = 0; i < m; i++) {
				res += ExponentialDistribution(lambda);
			}
			return res;

		}
		public static double ParetoDistribution(double x0 = 1.0d, double alfa = 2.0d) {
			return x0 * Math.Pow(rnd.NextDouble(), -1/alfa);
		}
	}
}
