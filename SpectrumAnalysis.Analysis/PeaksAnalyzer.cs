// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SpectrumAnalysis.Algorithms.Model;
using SpectrumAnalysis.Algorithms.Peaks;
using SpectrumAnalysis.Algorithms.Smoothing;
using SpectrumAnalysis.Data.Cursor;

namespace SpectrumAnalysis.Analysis
{
    public class PeaksAnalyzer : IPeaksAnalyzer
    {
        private readonly IDataCursor dataCursor;
        private readonly ISmoother dataSmoother;
        private readonly ILorentzian lorentzian;
        private readonly IPeaksRepository peaks;
        private readonly IPeaksFinder peaksFinder;

        public PeaksAnalyzer(IDataCursor cursor, IPeaksFinder finder, ISmoother smoother,
            IPeaksRepository peaksRepository, ILorentzian lorentzianModel)
        {
            lorentzian = lorentzianModel;
            dataCursor = cursor;
            peaksFinder = finder;
            dataSmoother = smoother;
            peaks = peaksRepository;
        }

        public IReadOnlyCollection<IReadOnlyCollection<Point>> QuickAnalysis(int searchWindow, double alpha,
            double threshold, int maxPeaksCount)
        {
            var quickViewPeaks = new List<IReadOnlyList<Point>>();
            var spectrum = dataCursor.GetSpectrum();
            var smoothSpectrum = dataSmoother.Smooth(spectrum, alpha);
            var quickpeaks = peaksFinder.Find(spectrum, smoothSpectrum, searchWindow, threshold, maxPeaksCount);

            if (quickpeaks.Count == 0)
            {
                return quickViewPeaks;
            }

            var initialFrequency = spectrum.First().X;
            var finalFrequency = spectrum.Last().X;

            for (var i = 0; i < quickpeaks.Count; i++)
            {
                var peak = quickpeaks[i];
                var a = peak.Alpha;
                var b = peak.Beta;
                var g = peak.Gamma;
                var currentPeak = lorentzian.Model(initialFrequency, finalFrequency, a, b, g);
                quickViewPeaks.Add(currentPeak);
            }

            return quickViewPeaks;
        }

        public void AnalyzeMainPeak(double alpha, int searchWindow, double threshold, double maxDistance,
            Action<int> reportProgress)
        {
            peaks.Clean();

            var kalman = new Kalman();

            var voltages = dataCursor.GetAllVoltages();
            var filter = 0.0;
            for (var voltage = 0; voltage < voltages.Count; voltage++)
            {
                reportProgress.Invoke(100 * voltage / voltages.Count);

                var currentRawData = dataCursor.GetSpectrum(voltage);
                var processedData = dataSmoother.Smooth(currentRawData, alpha);

                var fittedPeaks = peaksFinder.Find(currentRawData, processedData, searchWindow, threshold, 1);

                if (fittedPeaks.Count == 0)
                {
                    continue;
                }

                var modelPeak = fittedPeaks.First();

                var reference = peaks.Get(0);
                if (reference != null && reference.Count > 0)
                {
                    if (Math.Abs(filter - modelPeak.Gamma) > maxDistance)
                    {
                        continue;
                    }
                }

                if (modelPeak.Alpha > 0 && modelPeak.Beta > 0 && modelPeak.Gamma > 0)
                {
                    peaks.Update(0, voltages[voltage], modelPeak, true);
                    filter = kalman.Update(modelPeak.Gamma);
                }
            }
        }

        public void AnalyzeHighOrderPeaks(double alpha, int searchWindow, double threshold, int maxPeaksCount,
            Action<int> reportProgress)
        {
            var voltages = dataCursor.GetAllVoltages();
            for (var voltage = 0; voltage < voltages.Count; voltage++)
            {
                reportProgress.Invoke(100 * voltage / voltages.Count);

                var currentRawData = dataCursor.GetSpectrum(voltage);
                var processedData = dataSmoother.Smooth(currentRawData, alpha);
                var fittedPeaks = peaksFinder.Find(currentRawData, processedData, searchWindow, threshold, maxPeaksCount);

                if (fittedPeaks.Count == 0)
                {
                    continue;
                }

                var modeledPeaks = fittedPeaks.OrderByDescending(p => p.Alpha).ToList();

                for (var peakId = 0; peakId < modeledPeaks.Count; peakId++)
                {
                    ProcessPeak(voltages[voltage], modeledPeaks[peakId]);
                }
            }
        }

        public void Clean()
        {
            peaks.Clean();
        }

        private void ProcessPeak(double voltage, Peak value)
        {
            var mainPeak = peaks.Get(0);
            for (var i = 1; i < peaks.Count(); i++)
            {
                var candidate = peaks.Get(i);
                var firstEntry = candidate.First().Key;

                if (!mainPeak.ContainsKey(firstEntry) || !mainPeak.ContainsKey(voltage))
                {
                    continue;
                }

                var mainPeakFirstValue = mainPeak[firstEntry].Gamma;
                var mainPeakCurrentValue = mainPeak[voltage].Gamma;

                var candidateFistValue = candidate[firstEntry].Gamma;
                var candidateCurrentValue = value.Gamma;

                var candidateDelta = candidateFistValue - candidateCurrentValue;
                var mainPeakDelta = mainPeakFirstValue - mainPeakCurrentValue;

                var crit = candidateDelta - mainPeakDelta;
                if (Math.Abs(crit) < 100000000)
                {
                    value.Id = candidate[firstEntry].Id;
                    candidate[voltage] = value;
                    return;
                }
            }
            var id = peaks.Count();
            value.Id = id;
            peaks.Update(id, voltage, value, true);
        }
    }
}