// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Windows;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Common;

namespace SpectrumAnalysis.Module.View.Main
{
    public sealed class Log10YTransform : DataTransform
    {
        public override DataRect DataDomain { get; } = DataRect.FromPoints(-8.98846567431158E+307, double.Epsilon,
            8.98846567431158E+307, double.MaxValue);

        public override Point DataToViewport(Point pt)
        {
            var y = pt.Y;
            return new Point(pt.X, y >= 0.0 ? Math.Log10(y) : double.MinValue);
        }

        public override Point ViewportToData(Point pt)
        {
            return new Point(pt.X, Math.Pow(10.0, pt.Y));
        }
    }
}