// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SpectrumAnalysis.Module.ViewModel.Main
{
    public interface IMainViewModel
    {
        List<IReadOnlyCollection<Point>> QuickViewPeaks { get; }

        bool EnableQuickViewPeaks { get; set; }
        double Alpha { get; set; }
        string AlphaDescription { get; }
        string CurrentVoltageDescription { get; }
        int SearchWindow { get; set; }
        string SearchWindowDescription { get; }
        int WindowSizeMax { get; }
        double Threshold { get; set; }
        string ThresholdDescription { get; }
        int NumberOfDetectedPeaks { get; set; }
        string NumberOfDetectedPeaksDescription { get; }

        double MaxDistance { get; set; }

        bool AnalyzeMainPeakAvailable { get; set; }
        int MainPeakAnalysisPercent { get; set; }

        bool AnalyzeAllPeaksAvailable { get; set; }
        int AllPeaksAnalysisPercent { get; set; }

        ICommand AnalyzeMainPeak { get; }
        ICommand ProcessPeaks { get; }
        ICommand LoadDataForVoltage { get; }

        event EventHandler AnalysisChanged;
        event EventHandler QuickViewChanged;
    }
}