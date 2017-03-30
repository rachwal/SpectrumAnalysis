// Copyright (c) 2015-2017 Bartosz Rachwal.

using System.Collections.Generic;
using System.Windows;

namespace SpectrumAnalysis.Algorithms.Smoothing
{
    public class GaussianSmoother : ISmoother
    {
        public List<Point> Smooth(IList<Point> points, double alpha)
        {
            //F_(t+1)=F_(t)+alpha*(t-F_(t))
            if (points.Count == 0)
            {
                return new List<Point>();
            }

            var smoothed = new List<Point> {points[0]};

            for (var i = 1; i < points.Count; i++)
            {
                var currentPoint = points[i];

                currentPoint.X = points[i].X;
                currentPoint.Y = smoothed[i - 1].Y + alpha * (points[i - 1].Y - smoothed[i - 1].Y);

                smoothed.Add(currentPoint);
            }

            return smoothed;
        }
    }
}