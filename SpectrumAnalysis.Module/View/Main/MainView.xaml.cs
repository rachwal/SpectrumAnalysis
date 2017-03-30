// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using SpectrumAnalysis.Algorithms.Model;
using SpectrumAnalysis.Analysis;
using SpectrumAnalysis.Data.Cursor;
using SpectrumAnalysis.Module.ViewModel.Main;
using Xceed.Wpf.AvalonDock;

namespace SpectrumAnalysis.Module.View.Main
{
    public sealed class LabelTickInfo<T>
    {
        internal LabelTickInfo()
        {
        }

        public T Tick { get; internal set; }

        public object Info { get; internal set; }

        public int Index { get; internal set; }
    }

    public class MyExponentialLabelProvider : NumericLabelProviderBase
    {
        public override UIElement[] CreateLabels(ITicksInfo<double> ticksInfo)
        {
            var ticks = ticksInfo.Ticks;
            Init(ticks);
            var tick = new UIElement[ticks.Length];
            var tickInfo = new LabelTickInfo<double> {Info = ticksInfo.Info};
            for (var i = 0; i < tick.Length; i++)
            {
                tickInfo.Tick = ticks[i];
                tickInfo.Index = i;
                var labelText = "";

                labelText = $"{ticks[i]:G3}";

                var label = new TextBlock
                {
                    Text = labelText,
                    ToolTip = ticks[i].ToString(CultureInfo.InvariantCulture)
                };

                tick[i] = label;
            }
            return tick;
        }
    }

    public class MyNumericAxisControl : AxisControl<double>
    {
        public MyNumericAxisControl()
        {
            LabelProvider = new ExponentialLabelProvider();
            TicksProvider = new NumericTicksProvider();
            ConvertToDouble = d => d;
            Range = new Range<double>(0.0, 10.0);
        }
    }

    public class MyVerticalAxis : AxisBase<double>
    {
        public MyVerticalAxis() : base(new MyNumericAxisControl(), d => d, d => d)
        {
            Placement = AxisPlacement.Left;
        }

        protected override void ValidatePlacement(AxisPlacement newPlacement)
        {
            if (newPlacement == AxisPlacement.Bottom || newPlacement == AxisPlacement.Top)
                throw new ArgumentException();
        }
    }

    public partial class MainView
    {
        public MainView(IMainViewModel viewModel, IDataCursor dataCursor, IPeaksRepository peaksRepository,
            ILorentzian lorentzianModel)
        {
            InitializeComponent();

            SpectrumQuickView.DataTransform = new Log10YTransform();
            SpectrumQuickView.VerticalAxis = new VerticalAxis {LabelProvider = new MyExponentialLabelProvider()};

            SpectrumAnalysis.DataTransform = new Log10YTransform();
            SpectrumAnalysis.VerticalAxis = new VerticalAxis {LabelProvider = new MyExponentialLabelProvider()};

            FWHM.AxisGrid.DrawHorizontalMinorTicks = true;
            FWHM.AxisGrid.DrawHorizontalTicks = true;
            FWHM.AxisGrid.DrawVerticalMinorTicks = true;
            FWHM.AxisGrid.DrawVerticalTicks = true;
            FWHM.VerticalAxis = new VerticalAxis {LabelProvider = new MyExponentialLabelProvider()};

            PeakPosition.AxisGrid.DrawHorizontalMinorTicks = true;
            PeakPosition.AxisGrid.DrawHorizontalTicks = true;
            PeakPosition.AxisGrid.DrawVerticalMinorTicks = true;
            PeakPosition.AxisGrid.DrawVerticalTicks = true;
            PeakPosition.VerticalAxis = new VerticalAxis {LabelProvider = new MyExponentialLabelProvider()};

            PeakValue.AxisGrid.DrawHorizontalMinorTicks = true;
            PeakValue.AxisGrid.DrawHorizontalTicks = true;
            PeakValue.AxisGrid.DrawVerticalMinorTicks = true;
            PeakValue.AxisGrid.DrawVerticalTicks = true;
            PeakValue.DataTransform = new Log10YTransform();
            PeakValue.VerticalAxis = new VerticalAxis {LabelProvider = new MyExponentialLabelProvider()};

            ViewModel = viewModel;
            ViewModel.QuickViewChanged += OnSpectraChanged;
            ViewModel.AnalysisChanged += OnAnalysisChanged;

            cursor = dataCursor;

            dataCursor.SpectraLoaded += OnSpectraChanged;
            dataCursor.SpectraChanged += OnSpectraChanged;
            dataCursor.VoltagesLoaded += OnVoltagesChanged;
            dataCursor.VoltagesChanged += OnVoltagesChanged;

            peaks = peaksRepository;

            lorentzian = lorentzianModel;
        }

        private IMainViewModel ViewModel
        {
            get { return (IMainViewModel) DataContext; }
            set { DataContext = value; }
        }

        private void TextBoxValuePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextBoxTextAllowed(e.Text);
        }

        private void TextBoxValuePasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text1 = (string) e.DataObject.GetData(typeof(string));
                if (!TextBoxTextAllowed(text1)) e.CancelCommand();
            }
            else e.CancelCommand();
        }

        private bool TextBoxTextAllowed(string text2)
        {
            return Array.TrueForAll(text2.ToCharArray(),
                c => char.IsDigit(c) || char.IsControl(c));
        }

        private void OnAnalysisChanged(object sender, EventArgs e)
        {
            UpdatePeakPosition();
            UpdatePeakValue();
            UpdatePeakFwhm();
        }

        private void UpdatePeakPosition()
        {
            for (var childIndex = PeakPosition.Children.Count - 1; childIndex > 0; childIndex--)
            {
                if (PeakPosition.Children[childIndex] is LineGraph ||
                    PeakPosition.Children[childIndex] is MarkerPointsGraph)
                {
                    PeakPosition.Children.RemoveAt(childIndex);
                }
            }

            for (var peakIndex = 0; peakIndex < ViewModel.NumberOfDetectedPeaks; peakIndex++)
            {
                var peakId = peakIndex;
                var peak = peaks.Get(peakId);

                if (peak == null)
                {
                    continue;
                }

                var points = peak.Keys.Select(voltage => new Point(voltage, peak[voltage].Gamma)).ToList();

                var data = new ObservableDataSource<Point>(points);

                var peakData = new MarkerPointsGraph(data)
                {
                    Marker = new CirclePointMarker
                    {
                        Pen = new Pen(new SolidColorBrush(peakColors[peakId]), 4),
                        Fill = new SolidColorBrush(peakColors[peakId])
                    },
                    Description = new StandardDescription($"peak {peakId} position")
                };
                PeakPosition.Children.Add(peakData);
            }
        }

        private void UpdatePeakValue()
        {
            for (var childIndex = PeakValue.Children.Count - 1; childIndex > 0; childIndex--)
            {
                if (PeakValue.Children[childIndex] is LineGraph ||
                    PeakValue.Children[childIndex] is MarkerPointsGraph)
                {
                    PeakValue.Children.RemoveAt(childIndex);
                }
            }

            var xmin = double.MaxValue;
            var xmax = double.MinValue;
            var ymin = double.MaxValue;
            var ymax = double.MinValue;

            for (var peakIndex = 0; peakIndex < ViewModel.NumberOfDetectedPeaks; peakIndex++)
            {
                var peakId = peakIndex;
                var peak = peaks.Get(peakId);

                if (peak == null)
                {
                    continue;
                }

                var points = peak.Keys.Select(voltage => new Point(voltage, peak[voltage].Alpha)).ToList();

                xmin = Math.Min(xmin, points.Min(p => p.X));
                ymin = Math.Min(ymin, points.Min(p => p.Y));
                xmax = Math.Max(xmax, points.Max(p => p.X));
                ymax = Math.Max(ymax, points.Max(p => p.Y));

                var data = new ObservableDataSource<Point>(points);

                var peakData = new MarkerPointsGraph(data)
                {
                    Marker =
                        new CirclePointMarker
                        {
                            Pen = new Pen(new SolidColorBrush(peakColors[peakId]), 4),
                            Fill = new SolidColorBrush(peakColors[peakId])
                        },
                    Description = new StandardDescription($"peak {peakId} value")
                };

                PeakValue.Children.Add(peakData);
            }

            var logymin = Math.Log10(ymin);
            var logymax = Math.Log10(ymax);

            var rect = new Rect(new Point(xmin, logymin + 0.1 * logymax), new Point(xmax, logymax - 0.1 * logymax));

            PeakValue.Visible = rect;
            PeakValue.UpdateLayout();
        }

        private void UpdatePeakFwhm()
        {
            for (var childIndex = FWHM.Children.Count - 1; childIndex > 0; childIndex--)
            {
                if (FWHM.Children[childIndex] is LineGraph ||
                    FWHM.Children[childIndex] is MarkerPointsGraph)
                {
                    FWHM.Children.RemoveAt(childIndex);
                }
            }

            for (var peakIndex = 0; peakIndex < ViewModel.NumberOfDetectedPeaks; peakIndex++)
            {
                var peakId = peakIndex;
                var peak = peaks.Get(peakId);

                if (peak == null)
                {
                    continue;
                }

                var points = peak.Keys.Select(voltage => new Point(voltage, peak[voltage].Beta)).ToList();

                var data = new ObservableDataSource<Point>(points);

                var peakData = new MarkerPointsGraph(data)
                {
                    Marker =
                        new CirclePointMarker
                        {
                            Pen = new Pen(new SolidColorBrush(peakColors[peakId]), 4),
                            Fill = new SolidColorBrush(peakColors[peakId])
                        },
                    Description = new StandardDescription($"peak {peakId} FWHM")
                };

                FWHM.Children.Add(peakData);
            }
        }

        private void OnVoltagesChanged(object sender, EventArgs e)
        {
            voltagesList.ItemsSource = new ObservableCollection<double>(cursor.GetAllVoltages());
        }

        private void OnSpectraChanged(object sender, EventArgs e)
        {
            RebuildQuickView();
            UpdateAutoscale();

            RebuidAnalysis();
        }

        private void RebuidAnalysis()
        {
            var spectrum = new ObservableDataSource<Point>(cursor.GetSpectrum());
            var analysisSpectrum = new LineGraph(spectrum)
            {
                LinePen = new Pen(new SolidColorBrush(Colors.Green), 3),
                Description = new PenDescription("Raw Data"),
                FilteringEnabled = true
            };

            for (var childIndex = SpectrumAnalysis.Children.Count - 1; childIndex > 0; childIndex--)
            {
                if (SpectrumAnalysis.Children[childIndex] is LineGraph ||
                    SpectrumAnalysis.Children[childIndex] is MarkerPointsGraph)
                {
                    SpectrumAnalysis.Children.RemoveAt(childIndex);
                }
            }

            SpectrumAnalysis.Children.Add(analysisSpectrum);

            for (var peakIndex = 0; peakIndex < ViewModel.NumberOfDetectedPeaks; peakIndex++)
            {
                var peakId = peakIndex;
                var peak = peaks.Get(peakId, cursor.GetCurrentVoltage());

                if (peak == null)
                {
                    continue;
                }

                var delta = 3 * peak.Gamma;
                var x0 = peak.Gamma - delta;
                var x1 = peak.Gamma + delta;
                var points = lorentzian.Model(x0, x1, peak.Alpha, peak.Beta, peak.Gamma);
                var data = new ObservableDataSource<Point>(points);
                var peakGraph = new LineGraph(data)
                {
                    LinePen = new Pen(new SolidColorBrush(peakColors[peakId]), 4),
                    Description = new PenDescription($"peak {peakId}")
                };
                SpectrumAnalysis.Children.Add(peakGraph);
            }
        }

        private void RebuildQuickView()
        {
            var spectrum = new ObservableDataSource<Point>(cursor.GetSpectrum());

            var quickViewSpectrum = new LineGraph(spectrum)
            {
                LinePen = new Pen(new SolidColorBrush(Colors.Green), 3),
                Description = new PenDescription("Raw Data"),
                FilteringEnabled = true
            };

            for (var childIndex = SpectrumQuickView.Children.Count - 1; childIndex > 0; childIndex--)
            {
                if (SpectrumQuickView.Children[childIndex] is LineGraph ||
                    SpectrumQuickView.Children[childIndex] is MarkerPointsGraph)
                {
                    SpectrumQuickView.Children.RemoveAt(childIndex);
                }
            }

            SpectrumQuickView.Children.Add(quickViewSpectrum);

            if (ViewModel.EnableQuickViewPeaks)
            {
                RebuildQuickViewPeaks();
            }
        }

        private void RebuildQuickViewPeaks()
        {
            var quickViewPeaks = ViewModel.QuickViewPeaks;

            for (var index = 0; index < quickViewPeaks.Count; index++)
            {
                var peak = new LineGraph(new ObservableDataSource<Point>(quickViewPeaks[index]))
                {
                    LinePen = new Pen(new SolidColorBrush(peakColors[index]), 2),
                    Description = new PenDescription($"peak {index + 1}")
                };
                SpectrumQuickView.Children.Add(peak);
            }
        }

        private void SelectedVoltageChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox) sender;
            ViewModel.LoadDataForVoltage?.Execute(listBox?.SelectedIndex);
        }

        private void OnDocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void UpdateAutoscale()
        {
            var spectrum = cursor.GetSpectrum();
            if (spectrum.Count == 0)
            {
                return;
            }

            var x1 = spectrum.First().X;
            var x2 = spectrum.Last().X;

            var xmin = Math.Min(x1, x2);
            var xmax = Math.Max(x1, x2);

            var ymin = spectrum.Min(el => el.Y);
            var ymax = spectrum.Max(el => el.Y);

            var logymin = Math.Log10(ymin);
            var logymax = Math.Log10(ymax);

            var rect = new Rect(new Point(xmin, logymin + 0.1 * logymax), new Point(xmax, logymax - 0.1 * logymax));

            SpectrumQuickView.Visible = rect;
            SpectrumAnalysis.Visible = rect;

            SpectrumQuickView.UpdateLayout();
            SpectrumAnalysis.UpdateLayout();
        }

        #region PeakColors

        private readonly List<Color> peakColors = new List<Color>
        {
            Colors.Red,
            Colors.Blue,
            Colors.DarkOrange,
            Colors.BlueViolet,
            Colors.Black,
            Colors.SaddleBrown,
            Colors.CadetBlue,
            Colors.Magenta,
            Colors.Chartreuse,
            Colors.Aqua
        };

        private IDataCursor cursor;
        private IPeaksRepository peaks;
        private ILorentzian lorentzian;

        #endregion
    }
}