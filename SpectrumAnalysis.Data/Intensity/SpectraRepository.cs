// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpectrumAnalysis.Data.Intensity
{
    public class SpectraRepository : ISpectraRepository
    {
        private readonly Dictionary<double, Dictionary<double, double>> intensities =
            new Dictionary<double, Dictionary<double, double>>();

        private bool loading;

        private double max;
        public event EventHandler Loaded;

        public void Add(double voltage, double frequency, double value)
        {
            if (!intensities.ContainsKey(voltage))
            {
                intensities.Add(voltage, new Dictionary<double, double>());
            }
            var currentVoltage = intensities[voltage];

            if (currentVoltage.ContainsKey(frequency))
            {
                return;
            }

            intensities[voltage][frequency] = value;

            if (value > max)
            {
                max = value;
            }

            OnLoaded();
        }

        public double GetMaxValue()
        {
            return max;
        }

        public double GetMaxValue(double voltage)
        {
            if (!intensities.ContainsKey(voltage))
            {
                return 0;
            }
            var serie = intensities[voltage];
            return serie.Max(e => e.Value);
        }

        public void Clear()
        {
            intensities.Clear();
        }

        public void EndLoading()
        {
            loading = false;
            OnLoaded();
        }

        public void BeginLoading()
        {
            loading = true;
        }

        public Dictionary<double, double> Get(double voltage)
        {
            return !intensities.ContainsKey(voltage) ? new Dictionary<double, double>() : intensities[voltage];
        }

        private void OnLoaded()
        {
            if (!loading)
            {
                Loaded?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}