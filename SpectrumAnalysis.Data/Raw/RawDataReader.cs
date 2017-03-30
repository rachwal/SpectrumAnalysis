// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;
using System.Linq;
using SpectrumAnalysis.Data.Intensity;
using SpectrumAnalysis.Data.Voltage;

namespace SpectrumAnalysis.Data.Raw
{
    public class RawDataReader : IRawDataReader
    {
        private readonly ISpectraRepository spectra;
        private readonly IVoltageRepository voltage;

        public RawDataReader(IVoltageRepository voltageRepository,
            ISpectraRepository spectraRepository)
        {
            voltage = voltageRepository;
            spectra = spectraRepository;
        }

        public void Read(string filePath)
        {
            spectra.BeginLoading();
            voltage.BeginLoading();

            var lines = System.IO.File.ReadAllLines(filePath);

            var voltages = lines[0].Split('\t');

            foreach (var value in voltages.Select(double.Parse))
            {
                voltage.Add(value);
            }

            var frequencies = new List<double>();
            for (var i = 1; i < lines.Length; i++)
            {
                var entries = lines[i].Split('\t');
                var frequency = double.Parse(entries[0]);

                frequencies.Add(frequency);

                for (var j = 1; j < entries.Length; j++)
                {
                    var currentVoltage = voltage.Get(j - 1);
                    var currentFrequency = frequencies[i - 1];
                    var value = double.Parse(entries[j]);
                    if (value > 0)
                    {
                        spectra.Add(currentVoltage, currentFrequency, value);
                    }
                }
            }
            spectra.EndLoading();
            voltage.EndLoading();
        }
    }
}