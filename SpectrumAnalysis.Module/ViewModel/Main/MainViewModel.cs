// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Application.Module.Properties;
using Prism.Events;
using SpectrumAnalysis.Analysis;
using SpectrumAnalysis.Common.Commands;
using SpectrumAnalysis.Common.Events;
using SpectrumAnalysis.Data.Cursor;
using SpectrumAnalysis.Module.Events;

namespace SpectrumAnalysis.Module.ViewModel.Main
{
    public class MainViewModel : IMainViewModel, INotifyPropertyChanged
    {
        private readonly BackgroundWorker allPeaksAnalysis = new BackgroundWorker();
        private readonly IDataCursor dataCursor;

        private readonly BackgroundWorker mainPeakAnalysis = new BackgroundWorker();
        private readonly IPeaksAnalyzer peaksAnalyzer;

        private double alpha = 0.15;
        private bool enableQuickViewPeaks = false;
        private int numberOfDetectedPeaks = 5;
        private int searchWindow = 3;
        private double threshold = 0.000000000001;


        public MainViewModel(IDataCursor cursor, IPeaksAnalyzer analyzer, IEventAggregator eventAggregator)
        {
            mainPeakAnalysis.DoWork += StartMainPeakAnalysis;
            mainPeakAnalysis.RunWorkerCompleted += MainPeakAnalysisCompleted;
            mainPeakAnalysis.WorkerReportsProgress = true;
            mainPeakAnalysis.ProgressChanged += MainPeakAnalysisProgressChanged;

            allPeaksAnalysis.DoWork += StartAllPeaksAnalysis;
            allPeaksAnalysis.RunWorkerCompleted += AllPeaksAnalysisCompleted;
            allPeaksAnalysis.WorkerReportsProgress = true;
            allPeaksAnalysis.ProgressChanged += AllPeaksAnalysisProgressChanged;

            peaksAnalyzer = analyzer;
            dataCursor = cursor;

            dataCursor.SpectraLoaded += OnSpectrumLoaded;
            dataCursor.SpectraChanged += OnSpectrumChanged;

            eventAggregator.GetEvent<NewProjectEvent>().Subscribe(NewProject);
            eventAggregator.GetEvent<CleanDataEvent>().Subscribe(CleanData);
            eventAggregator.GetEvent<RunAnalysisEvent>().Subscribe(RunAnalysis);
        }

        public double FilterDistance { get; set; }

        public double Alpha
        {
            get { return alpha; }
            set
            {
                alpha = value;
                OnPropertyChanged(nameof(AlphaDescription));
            }
        }

        public string AlphaDescription => $"Alpha: {Alpha:F3}";
        public string CurrentVoltageDescription => $"Voltage: {dataCursor.GetCurrentVoltage():F3}";

        public int SearchWindow
        {
            get { return searchWindow; }
            set
            {
                searchWindow = value;
                OnPropertyChanged(nameof(SearchWindowDescription));
            }
        }

        public string SearchWindowDescription => $"SearchWindow: {SearchWindow}";
        public int WindowSizeMax { get; set; }

        public double Threshold
        {
            get { return threshold; }
            set
            {
                threshold = value;
                OnPropertyChanged(nameof(ThresholdDescription));
            }
        }

        public string ThresholdDescription => $"Threshold: {Threshold:F3}";

        public int NumberOfDetectedPeaks
        {
            get { return numberOfDetectedPeaks; }
            set
            {
                numberOfDetectedPeaks = value;
                OnPropertyChanged(nameof(NumberOfDetectedPeaksDescription));

                RefreshQuickView();

                AnalysisChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string NumberOfDetectedPeaksDescription => $"Show No. Detected Peaks: {NumberOfDetectedPeaks}";
        public double MaxDistance { get; set; } = 200.0;

        public bool EnableQuickViewPeaks
        {
            get { return enableQuickViewPeaks; }
            set
            {
                enableQuickViewPeaks = value;
                RefreshQuickView();
            }
        }

        public bool AnalyzeMainPeakAvailable { get; set; } = true;
        public int MainPeakAnalysisPercent { get; set; }
        public bool AnalyzeAllPeaksAvailable { get; set; } = true;
        public int AllPeaksAnalysisPercent { get; set; }

        public List<IReadOnlyCollection<Point>> QuickViewPeaks { get; } = new List<IReadOnlyCollection<Point>>();

        public ICommand AnalyzeMainPeak => new DelegateCommand(o =>
        {
            AnalyzeMainPeakAvailable = false;
            AnalyzeAllPeaksAvailable = false;
            OnPropertyChanged(nameof(AnalyzeMainPeakAvailable));
            OnPropertyChanged(nameof(AnalyzeAllPeaksAvailable));

            mainPeakAnalysis.RunWorkerAsync();
        });

        public ICommand ProcessPeaks => new DelegateCommand(o =>
        {
            AnalyzeMainPeakAvailable = false;
            AnalyzeAllPeaksAvailable = false;
            OnPropertyChanged(nameof(AnalyzeMainPeakAvailable));
            OnPropertyChanged(nameof(AnalyzeAllPeaksAvailable));

            allPeaksAnalysis.RunWorkerAsync();
        });

        public ICommand LoadDataForVoltage
            => new DelegateCommand(index => { dataCursor.SetIndex(Convert.ToInt32(index)); });

        public event EventHandler AnalysisChanged;
        public event EventHandler QuickViewChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void StartBatch(object sender, RunWorkerCompletedEventArgs e)
        {
            mainPeakAnalysis.RunWorkerCompleted -= StartBatch;
            allPeaksAnalysis.RunWorkerCompleted += CleanBatch;

            allPeaksAnalysis.RunWorkerAsync();
        }

        private void CleanBatch(object sender, RunWorkerCompletedEventArgs e)
        {
            allPeaksAnalysis.RunWorkerCompleted -= CleanBatch;
        }

        private void RunAnalysis(string obj)
        {
            mainPeakAnalysis.RunWorkerCompleted += StartBatch;
            mainPeakAnalysis.RunWorkerAsync();
        }

        private void MainPeakAnalysisProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MainPeakAnalysisPercent = e.ProgressPercentage;
            OnPropertyChanged(nameof(MainPeakAnalysisPercent));
        }

        private void AllPeaksAnalysisProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            AllPeaksAnalysisPercent = e.ProgressPercentage;
            OnPropertyChanged(nameof(AllPeaksAnalysisPercent));
        }

        private void StartMainPeakAnalysis(object sender, DoWorkEventArgs e)
        {
            peaksAnalyzer.AnalyzeMainPeak(Alpha, SearchWindow, Threshold, Convert.ToInt32(MaxDistance * 1000000),
                percent =>
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action(() =>
                        {
                            if (mainPeakAnalysis.WorkerReportsProgress)
                            {
                                mainPeakAnalysis.ReportProgress(percent);
                            }
                        }));
                });
        }

        private void MainPeakAnalysisCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainPeakAnalysis.WorkerReportsProgress = false;

            AnalysisChanged?.Invoke(this, EventArgs.Empty);

            AnalyzeMainPeakAvailable = true;
            AnalyzeAllPeaksAvailable = true;
            MainPeakAnalysisPercent = 100;

            OnPropertyChanged(nameof(MainPeakAnalysisPercent));
            OnPropertyChanged(nameof(AnalyzeMainPeakAvailable));
            OnPropertyChanged(nameof(AnalyzeAllPeaksAvailable));
        }

        private void StartAllPeaksAnalysis(object sender, DoWorkEventArgs e)
        {
            peaksAnalyzer.AnalyzeHighOrderPeaks(Alpha, SearchWindow, Threshold, 10, percent =>
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(() =>
                    {
                        if (allPeaksAnalysis.WorkerReportsProgress)
                        {
                            allPeaksAnalysis.ReportProgress(percent);
                        }
                    }));
            });
        }

        private void AllPeaksAnalysisCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            allPeaksAnalysis.WorkerReportsProgress = false;

            AnalysisChanged?.Invoke(this, EventArgs.Empty);

            AnalyzeMainPeakAvailable = true;
            AnalyzeAllPeaksAvailable = true;
            AllPeaksAnalysisPercent = 100;

            OnPropertyChanged(nameof(AllPeaksAnalysisPercent));
            OnPropertyChanged(nameof(AnalyzeMainPeakAvailable));
            OnPropertyChanged(nameof(AnalyzeAllPeaksAvailable));
        }

        private void CleanData(string obj)
        {
            dataCursor.Clean();
            peaksAnalyzer.Clean();

            QuickViewChanged?.Invoke(this, EventArgs.Empty);
            AnalysisChanged?.Invoke(this, EventArgs.Empty);
        }

        private void NewProject(string obj)
        {
            CleanData(string.Empty);
        }

        private void OnSpectrumLoaded(object sender, EventArgs e)
        {
            WindowSizeMax = dataCursor.GetSpectrum()?.Count ?? 3;

            if (WindowSizeMax >= 30)
            {
                SearchWindow = WindowSizeMax / 10;
            }
        }

        private void OnSpectrumChanged(object sender, EventArgs e)
        {
            RefreshQuickView();
        }

        private void RefreshQuickView()
        {
            QuickViewPeaks.Clear();

            if (EnableQuickViewPeaks)
            {
                QuickViewPeaks.AddRange(peaksAnalyzer.QuickAnalysis(SearchWindow, Alpha, Threshold,
                    NumberOfDetectedPeaks));
            }

            QuickViewChanged?.Invoke(this, EventArgs.Empty);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}