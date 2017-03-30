// Copyright (c) 2015-2017 Bartosz Rachwal.

using System.Collections.Generic;
using System.Windows;

namespace SpectrumAnalysis.Algorithms.Smoothing
{
    public interface ISmoother
    {
        List<Point> Smooth(IList<Point> points, double alpha);
    }
}