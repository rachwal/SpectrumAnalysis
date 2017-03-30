// Copyright (c) 2015-2017 Bartosz Rachwal.

namespace SpectrumAnalysis.Data.Frequency
{
    public interface IFrequencyRepository
    {
        void Add(double value);
        double Get(int index);
    }
}