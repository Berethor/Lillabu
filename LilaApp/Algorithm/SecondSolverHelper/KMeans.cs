using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LilaApp.Algorithm.SecondSolverHelper
{
    internal class KMeans : IClusterization<double>
    {
        private int _clusterCount;
        private IMetrics<double> _metrics;
        private int _maxIterations;

        internal KMeans(int clusterCount, IMetrics<double> metrics, int maxIterations)
        {
            _clusterCount = clusterCount;
            _metrics = metrics;
            _maxIterations = maxIterations;
        }

        public ClusterizationResult<double> MakeClusterization(IList<DataItem<double>> data)
        {
            Dictionary<double[], IList<DataItem<double>>> clusterization = new Dictionary<double[], IList<DataItem<double>>>();

            Random r = new Random(DateTime.Now.Second);
            double[] min = new double[data.First().Input.Length];
            double[] max = new double[data.First().Input.Length];

            for (int i = 0; i < data.First().Input.Length; i++)
            {
                min[i] = (from d in data
                          select d.Input[i]).Min();
                max[i] = (from d in data
                          select d.Input[i]).Max();
            }
            for (int i = 0; i < _clusterCount; i++)
            {
                double[] v = new double[data.First().Input.Length];
                for (int j = 0; j < data.First().Input.Length; j++)
                {
                    v[j] = min[j] + r.NextDouble() * Math.Abs(max[j] - min[j]);
                }
                clusterization.Add(v, new List<DataItem<double>>());
            }

            bool convergence = true;
            double lastCost = Double.MaxValue;
            int iterations = 0;
            while (true)
            {
                convergence = true;

                foreach (DataItem<double> item in data)
                {
                    var candidates = from v in clusterization.Keys
                                     select new
                                     {
                                         Dist = _metrics.Calculate(v, item.Input),
                                         Cluster = v
                                     };
                    double minDist = (from c in candidates
                                      select c.Dist).Min();
                    var minCandidates = from c in candidates
                                        where c.Dist == minDist
                                        select c.Cluster;
                    double[] key = minCandidates.First();
                    clusterization[key].Add(item);
                }

                double cost = 0;
                List<double[]> newMeans = new List<double[]>();
                foreach (double[] key in clusterization.Keys)
                {
                    double[] v = new double[key.Length];
                    if (clusterization[key].Count > 0)
                    {
                        v = _metrics.GetCentroid((from x in clusterization[key]
                                                  select x.Input).ToArray());
                        cost += (from d in clusterization[key]
                                 select Math.Pow(_metrics.Calculate(key, d.Input), 2)).Sum();
                    }
                    else
                    {
                        for (int j = 0; j < data.First().Input.Length; j++)
                        {
                            v[j] = min[j] + r.NextDouble() * Math.Abs(max[j] - min[j]);
                        }
                    }
                    newMeans.Add(v);
                }
                if (lastCost <= cost)
                {
                    break;
                }


                iterations++;
                if (iterations == _maxIterations)
                {
                    break;
                }


                lastCost = cost;

                clusterization.Clear();
                foreach (double[] mean in newMeans)
                {
                    clusterization.Add(mean, new List<DataItem<double>>());
                }
            }
            return new ClusterizationResult<double>()
            {
                Centroids = new List<double[]>(clusterization.Keys),
                Clusterization = clusterization,
                Cost = lastCost
            };

        }
    }
}
