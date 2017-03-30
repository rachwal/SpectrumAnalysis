// Copyright (c) 2015-2017 Bartosz Rachwal.

using System.Collections.Generic;
using System.Windows;

namespace SpectrumAnalysis.Algorithms.Model
{
    public interface ILorentzian
    {
        List<Point> Model(double start, double end, double alpha, double beta, double gamma);
        List<Point> Model(double alpha, double beta, double gamma);
    }
}