// Copyright (c) 2015-2017 Bartosz Rachwal.

using System.Collections.Generic;

namespace SpectrumAnalysis.Data.Frequency
{
    public class FrequencyRepository : IFrequencyRepository
    {
        private readonly List<double> values = new List<double>();

        public void Add(double value)
        {
            values.Add(value);
        }

        public double Get(int index)
        {
            return values[index];
        }
    }
}