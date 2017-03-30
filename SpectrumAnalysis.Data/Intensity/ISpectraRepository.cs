// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;

namespace SpectrumAnalysis.Data.Intensity
{
    public interface ISpectraRepository
    {
        void Add(double voltage, double frequency, double value);
        Dictionary<double, double> Get(double voltage);
        double GetMaxValue();
        double GetMaxValue(double voltage);
        void Clear();

        void BeginLoading();
        void EndLoading();
        event EventHandler Loaded;
    }
}