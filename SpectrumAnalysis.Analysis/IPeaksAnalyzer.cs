// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;
using System.Windows;

namespace SpectrumAnalysis.Analysis
{
    public interface IPeaksAnalyzer
    {
        IReadOnlyCollection<IReadOnlyCollection<Point>> QuickAnalysis(int searchWindow, double alpha, double threshold,
            int numberOfDetectedPeaks);

        void AnalyzeMainPeak(double alpha, int searchWindow, double threshold, double maxDistance,
            Action<int> reportProgress);

        void AnalyzeHighOrderPeaks(double alpha, int searchWindow, double threshold, int numberOfDetectedPeaks,
            Action<int> reportProgress);

        void Clean();
    }
}