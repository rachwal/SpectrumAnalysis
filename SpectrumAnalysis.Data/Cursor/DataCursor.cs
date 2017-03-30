// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SpectrumAnalysis.Data.Intensity;
using SpectrumAnalysis.Data.Voltage;

namespace SpectrumAnalysis.Data.Cursor
{
    public class DataCursor : IDataCursor
    {
        private readonly ISpectraRepository spectras;
        private readonly IVoltageRepository voltages;
        private int currentIndex;

        public DataCursor(IVoltageRepository voltageRepository, ISpectraRepository spectraRepository)
        {
            voltages = voltageRepository;
            spectras = spectraRepository;

            voltages.Loaded += OnVoltagesLoaded;
            spectras.Loaded += OnSpectrasLoaded;
        }

        private int CurrentIndex
        {
            get { return currentIndex; }
            set
            {
                if (value > voltages.Count())
                {
                    currentIndex = 0;
                    return;
                }
                if (value < 0)
                {
                    currentIndex = voltages.Count() - 1;
                    return;
                }
                currentIndex = value;
                SpectraChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler SpectraLoaded;
        public event EventHandler SpectraChanged;
        public event EventHandler VoltagesLoaded;
        public event EventHandler VoltagesChanged;

        public void SetIndex(int index)
        {
            CurrentIndex = index;
        }

        public void Clean()
        {
            voltages.Clear();
            spectras.Clear();
            Reset();
            VoltagesChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Reset()
        {
            CurrentIndex = 0;
        }

        public void Prev()
        {
            CurrentIndex--;
        }

        public void Next()
        {
            CurrentIndex++;
        }

        public int Index()
        {
            return CurrentIndex;
        }

        public List<Point> GetSpectrum()
        {
            return GetSpectrum(CurrentIndex);
        }

        public List<Point> GetSpectrum(int index)
        {
            var voltege = voltages.Get(index);
            var frequencyScan = spectras.Get(voltege);
            return frequencyScan.Select(frequency => new Point(frequency.Key, frequency.Value)).ToList();
        }

        public List<Point> GetSpectrum(int selectedIndex, double min, double max)
        {
            var spectra = GetSpectrum(selectedIndex);
            return spectra.Where(t => t.X >= min && t.X <= max).ToList();
        }

        public List<Point> GetSpectrum(double min, double max)
        {
            var spectra = GetSpectrum();
            return spectra.Where(t => t.X >= min && t.X <= max).ToList();
        }

        public double GetCurrentVoltage()
        {
            return voltages.Get(CurrentIndex);
        }

        public ReadOnlyCollection<double> GetAllVoltages()
        {
            var voltageList = voltages.Get();
            return voltageList;
        }

        public int GetVoltagesCount()
        {
            return voltages.Count();
        }

        public double GetMaxSpectraValue()
        {
            var voltege = voltages.Get(CurrentIndex);
            return spectras.GetMaxValue(voltege);
        }

        private void OnSpectrasLoaded(object sender, EventArgs e)
        {
            SpectraLoaded?.Invoke(this, EventArgs.Empty);
        }

        private void OnVoltagesLoaded(object sender, EventArgs e)
        {
            VoltagesLoaded?.Invoke(this, EventArgs.Empty);
        }
    }
}