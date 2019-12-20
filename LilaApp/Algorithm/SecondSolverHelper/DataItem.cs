using System;
using System.Collections.Generic;
using System.Text;

namespace LilaApp.Algorithm.SecondSolverHelper
{
    public class DataItem<T>
    {

        private T[] _input = null;
        private T[] _output = null;

        public DataItem(T[] input, T[] output)
        {
            _input = input;
            _output = output;
        }

        public T[] Input
        {
            get { return _input; }
        }

        public T[] Output
        {
            get { return _output; }
            set { _output = value; }
        }

    }
}
