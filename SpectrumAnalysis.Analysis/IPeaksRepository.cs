// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;
using SpectrumAnalysis.Algorithms.Model;

namespace SpectrumAnalysis.Analysis
{
    public interface IPeaksRepository
    {
        Dictionary<double, Peak> Get(int id);
        Peak Get(int id, double voltage);
        void Update(int id, double voltage, Peak value, bool verify);
        void Clean();
        int Count();
    }
}