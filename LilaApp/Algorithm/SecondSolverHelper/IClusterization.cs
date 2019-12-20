using System;
using System.Collections.Generic;
using System.Text;

namespace LilaApp.Algorithm.SecondSolverHelper
{
    public interface IClusterization<T>
    {
        ClusterizationResult<T> MakeClusterization(IList<DataItem<T>> data);
    }
    public class ClusterizationResult<T>
    {
        public IList<T[]> Centroids { get; set; }
        public IDictionary<T[], IList<DataItem<T>>> Clusterization { get; set; }
        public double Cost { get; set; }
    }
}
