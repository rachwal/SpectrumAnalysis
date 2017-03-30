// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;
using System.Windows;

namespace SpectrumAnalysis.Algorithms.Model
{
    public class Lorentzian : ILorentzian
    {
        public List<Point> Model(double start, double end, double alpha, double beta, double gamma)
        {
            var peaks = new List<Point>();
            var pointsCount = 500.0;
            var step = (end - start) / pointsCount;
            for (var i = 0; i < pointsCount; i++)
            {
                var x = start + step * i;
                var y = alpha / (1.0 + Math.Pow((x - gamma) / beta, 2.0));
                peaks.Add(new Point(x, y));
            }
            return peaks;
        }

        public List<Point> Model(double alpha, double beta, double gamma)
        {
            var peaks = new List<Point>();
            var startPoint = gamma - 16.0 * beta;
            var endPoint = gamma + 2.0 * beta;
            var numberOfPoints = 500.0;
            var step = (endPoint - startPoint) / numberOfPoints;
            for (var modelPointIndex = 0; modelPointIndex < 2.0 * numberOfPoints; modelPointIndex++)
            {
                var x = startPoint + step * modelPointIndex;
                var y = alpha / (1.0 + Math.Pow((x - gamma) / beta, 2.0));
                peaks.Add(new Point(x, y));
            }
            return peaks;
        }
    }
}