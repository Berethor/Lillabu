using System;
using System.Collections.Generic;
using System.Text;

namespace LilaApp.Algorithm.SecondSolverHelper
{
    public abstract class MetricsBase<T> : IMetrics<T>
    {
        public abstract double Calculate(T[] v1, T[] v2);

        public virtual T[] GetCentroid(IList<T[]> data)
        {
            if (data == null)
            {
                throw new ArgumentException("Data is null");
            }
            if (data.Count == 0)
            {
                throw new ArgumentException("Data is empty");
            }
            double[][] dist = new double[data.Count][];
            for (int i = 0; i < data.Count - 1; i++)
            {
                dist[i] = new double[data.Count];
                for (int j = i; j < data.Count; j++)
                {
                    if (i == j)
                    {
                        dist[i][j] = 0;
                    }
                    else
                    {
                        dist[i][j] = Math.Pow(Calculate(data[i], data[j]), 2);
                        if (dist[j] == null)
                        {
                            dist[j] = new double[data.Count];
                        }
                        dist[j][i] = dist[i][j];
                    }
                }
            }
            double minSum = Double.PositiveInfinity;
            int bestIdx = -1;
            for (int i = 0; i < data.Count; i++)
            {
                double dSum = 0;
                for (int j = 0; j < data.Count; j++)
                {
                    dSum += dist[i][j];
                }
                if (dSum < minSum)
                {
                    minSum = dSum;
                    bestIdx = i;
                }
            }

            return data[bestIdx];
        }
    }
}
