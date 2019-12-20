using System;
using System.Linq;
using System.Collections.Generic;

namespace LilaApp.Algorithm.SecondSolverHelper
{
    internal class EuclideanDistance : MetricsBase<double>
    {

        internal EuclideanDistance()
        {
        }

        #region IMetrics Members

        public override double Calculate(double[] v1, double[] v2)
        {
            if (v1.Length != v2.Length)
            {
                throw new ArgumentException("Vectors dimensions are not same");
            }
            if (v1.Length == 0 || v2.Length == 0)
            {
                throw new ArgumentException("Vector dimension can't be 0");
            }
            double d = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                d += (v1[i] - v2[i]) * (v1[i] - v2[i]);
            }
            return Math.Sqrt(d);
        }

        public override double[] GetCentroid(IList<double[]> data)
        {
            if (data.Count == 0)
            {
                throw new ArgumentException("Data is empty");
            }
            double[] mean = new double[data.First().Length];
            for (int i = 0; i < mean.Length; i++)
            {
                mean[i] = 0;
            }
            foreach (double[] item in data)
            {
                for (int i = 0; i < item.Length; i++)
                {
                    mean[i] += item[i];
                }
            }
            for (int i = 0; i < mean.Length; i++)
            {
                mean[i] = mean[i] / data.Count;
            }
            return mean;
        }

        #endregion
    }
}
