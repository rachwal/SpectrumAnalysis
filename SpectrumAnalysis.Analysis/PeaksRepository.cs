// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;
using SpectrumAnalysis.Algorithms.Model;

namespace SpectrumAnalysis.Analysis
{
    public class PeaksRepository : IPeaksRepository
    {
        private readonly Dictionary<int, Dictionary<double, Peak>> peaks =
            new Dictionary<int, Dictionary<double, Peak>>();

        public void Update(int id, double voltage, Peak value, bool verify)
        {
            if (!peaks.ContainsKey(id))
            {
                if (verify)
                {
                    foreach (var peak in peaks)
                    {
                        if (peak.Value.ContainsKey(voltage) &&
                            Math.Abs(peak.Value[voltage].Gamma - value.Gamma) < 1000000000)
                        {
                            return;
                        }
                    }
                }

                peaks.Add(id, new Dictionary<double, Peak>());
            }
            peaks[id][voltage] = new Peak(value);
        }

        public Dictionary<double, Peak> Get(int id)
        {
            if (!peaks.ContainsKey(id))
            {
                return null;
            }
            return peaks[id];
        }

        public Peak Get(int id, double voltage)
        {
            var peak = Get(id);
            if (peak == null)
            {
                return null;
            }
            if (!peak.ContainsKey(voltage))
            {
                return null;
            }
            return peak[voltage];
        }

        public void Clean()
        {
            peaks.Clear();
        }

        public int Count()
        {
            return peaks.Count;
        }
    }
}