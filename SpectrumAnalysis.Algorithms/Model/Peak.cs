// Copyright (c) 2015-2017 Bartosz Rachwal.

namespace SpectrumAnalysis.Algorithms.Model
{
    public class Peak
    {
        public Peak()
        {
        }

        public Peak(Peak value)
        {
            Alpha = value.Alpha;
            Beta = value.Beta;
            Gamma = value.Gamma;
            Xmin = value.Xmin;
            Xmax = value.Xmax;
        }

        public double Alpha { get; set; }
        public double Beta { get; set; }
        public double Gamma { get; set; }
        public double Xmin { get; set; }
        public double Xmax { get; set; }
        public int Id { get; set; }
    }
}