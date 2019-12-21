using System;
using System.Linq;
using System.Collections.Generic;
using LilaApp.Models;

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
            for (int i = 0; i < v1.Length - 1; i++)
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

            var dims = data.First().Length - 1;

            double sum = 0;
            double[] mean = new double[dims+1];
            for (int i = 0; i < dims; i++)
            {
                mean[i] = 0;
            }
            foreach (double[] item in data)
            {
                var mass = item.Last();
                sum += mass;

                for (int i = 0; i < dims; i++)
                {
                    mean[i] += item[i] * mass;
                }

            }
            for (int i = 0; i < dims; i++)
            {
                mean[i] = mean[i] / sum; //data.Count
            }

            var incomeSum = 0.0;
            for (var i = 0 ; i < data.Count; i++)
            {
                var item = new Point(data[i][0], data[i][1], price:data[i][2]);
                var center = new Point(mean[0], mean[1]);
                var dist = MathFunctions.GetDistanceToPoint(item, center);
                var income = MathFunctions.GetWaypointIncome(dist, item.Price);
                incomeSum += income;
            }

            mean[dims] = incomeSum;

            return mean;
        }

        #endregion
    }
}
