// Copyright (c) 2015-2017 Bartosz Rachwal.

namespace SpectrumAnalysis.Analysis
{
    public class Kalman
    {
        private const double Q = 0.000001;
        private const double R = 0.00001;
        private double p = 1, x = 0, k;

        private void MeasurementUpdate()
        {
            k = (p + Q) / (p + Q + R);
            p = R * (p + Q) / (R + p + Q);
        }

        public double Update(double measurement)
        {
            MeasurementUpdate();
            var result = x + (measurement - x) * k;
            x = result;
            return result;
        }
    }
}