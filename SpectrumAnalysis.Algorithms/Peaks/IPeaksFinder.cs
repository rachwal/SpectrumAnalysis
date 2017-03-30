// Copyright (c) 2015-2017 Bartosz Rachwal.

using System.Collections.Generic;
using System.Windows;
using SpectrumAnalysis.Algorithms.Model;

namespace SpectrumAnalysis.Algorithms.Peaks
{
    public interface IPeaksFinder
    {
        List<Peak> Find(IList<Point> spectrum, IList<Point> smoothSpectrum, int window, double trashold,
            int maxPeaksCount);

        void Fit(IList<Point> points, ref double alpha, ref double beta, ref double gamma);
    }
}