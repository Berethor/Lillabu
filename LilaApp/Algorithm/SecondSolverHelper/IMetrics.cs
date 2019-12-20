using System;
using System.Collections.Generic;
using System.Text;

namespace LilaApp.Algorithm.SecondSolverHelper
{
    public interface IMetrics<T>
    {
        double Calculate(T[] v1, T[] v2);

        T[] GetCentroid(IList<T[]> data);
    }
}
