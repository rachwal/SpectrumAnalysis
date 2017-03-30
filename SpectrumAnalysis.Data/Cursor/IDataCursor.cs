// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace SpectrumAnalysis.Data.Cursor
{
    public interface IDataCursor
    {
        void Reset();
        void Prev();
        void Next();
        int Index();
        void SetIndex(int index);
        void Clean();

        List<Point> GetSpectrum();
        List<Point> GetSpectrum(int index);
        List<Point> GetSpectrum(int index, double min, double max);
        List<Point> GetSpectrum(double min, double max);
        double GetMaxSpectraValue();

        double GetCurrentVoltage();
        ReadOnlyCollection<double> GetAllVoltages();
        int GetVoltagesCount();

        event EventHandler SpectraLoaded;
        event EventHandler SpectraChanged;

        event EventHandler VoltagesLoaded;
        event EventHandler VoltagesChanged;
    }
}