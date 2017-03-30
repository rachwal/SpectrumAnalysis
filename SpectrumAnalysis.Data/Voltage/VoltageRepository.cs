// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpectrumAnalysis.Data.Voltage
{
    public class VoltageRepository : IVoltageRepository
    {
        private readonly List<double> values = new List<double>();

        private bool loading;
        public event EventHandler Loaded;

        public void Add(double value)
        {
            values.Add(value);
            OnLoaded();
        }

        public ReadOnlyCollection<double> Get()
        {
            return values.AsReadOnly();
        }

        public double Get(int index)
        {
            if (index > -1 && index < values.Count)
                return values[index];
            return 0.0;
        }

        public int Count()
        {
            return values.Count;
        }

        public void Clear()
        {
            values.Clear();
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

        private void OnLoaded()
        {
            if (!loading)
            {
                Loaded?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}