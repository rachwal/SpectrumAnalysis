// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.ObjectModel;

namespace SpectrumAnalysis.Data.Voltage
{
    public interface IVoltageRepository
    {
        void Add(double value);
        ReadOnlyCollection<double> Get();
        double Get(int index);
        int Count();
        void Clear();

        void BeginLoading();
        void EndLoading();
        event EventHandler Loaded;
    }
}