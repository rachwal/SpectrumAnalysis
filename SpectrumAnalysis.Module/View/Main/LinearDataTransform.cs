// Copyright (c) 2015-2017 Bartosz Rachwal.

using System.Windows;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Common;

namespace SpectrumAnalysis.Module.View.Main
{
    public class LinearDataTransform : DataTransform
    {
        public override DataRect DataDomain { get; } = DataRect.FromPoints(4.94065645841247E-324, -8.98846567431158E+307,
            double.MaxValue, 8.98846567431158E+307);

        public override Point DataToViewport(Point pt)
        {
            return pt;
        }

        public override Point ViewportToData(Point pt)
        {
            return pt;
        }
    }
}