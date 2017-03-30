// Copyright (c) 2015-2017 Bartosz Rachwal.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using SpectrumAnalysis.Algorithms.Model;

namespace SpectrumAnalysis.Algorithms.Peaks
{
    public class PeaksFinder : IPeaksFinder
    {
        public List<Peak> Find(IList<Point> spectrum, IList<Point> smoothSpectrum, int window, double trashold,
            int maxPeaksCount)
        {
            var candidateIds = FindCandidateIds(smoothSpectrum, window, trashold);
            var candidates = FindCandidates(spectrum, candidateIds);

            var initialFrequency = spectrum.Min(e => e.X);
            var finalFrequency = spectrum.Max(e => e.X);

            var peaks = new List<Peak>();

            for (var id = 0; id < candidates.Count; id++)
            {
                var peak = candidates[id];

                var a = peak.Alpha;
                var b = 10000000.0;
                var g = peak.Gamma;

                var peakData = new List<Point> { new Point(initialFrequency, 0) };
                var peakArea = spectrum.Where(p => p.X < peak.Xmax && p.X > peak.Xmin).ToList();
                peakData.AddRange(peakArea);
                peakData.Add(new Point(finalFrequency, 0));

                Fit(peakData, ref a, ref b, ref g);

                if (b > 6000000 && a > 0.0000000001)
                {
                    peaks.Add(new Peak { Alpha = a, Beta = b, Gamma = g });
                }
            }

            var orderedPeaks = peaks.OrderByDescending(p => p.Alpha).Take(maxPeaksCount).ToList();
            return orderedPeaks;
        }

        public void Fit(IList<Point> points, ref double alpha, ref double beta, ref double gamma)
        {
            var model = new Model();

            var minimumDeltaValue = 1;
            var minimumDeltaParameters = 1;
            var maximumIterations = 10;

            var pointCount = points.Count;

            Vector<double> dataX = new DenseVector(points.Select(p => p.X).ToArray());
            Vector<double> dataY = new DenseVector(points.Select(p => p.Y).ToArray());

            var iterations = new List<Vector<double>>();

            var guess = new[] { alpha, beta, gamma };

            Vector<double> parametersCurrent = new DenseVector(guess);
            Vector<double> parametersNew = new DenseVector(parametersCurrent.Count);

            var valueCurrent = GetObjectiveValue(model, pointCount, dataX, dataY, parametersCurrent[0],
                parametersCurrent[1], parametersCurrent[2]);

            while (true)
            {
                var jacobian = GetObjectiveJacobian(model, pointCount, dataX, parametersCurrent[0], parametersCurrent[1],
                    parametersCurrent[2]);
                var residual = model.GetResidualVector(pointCount, dataX, dataY, parametersCurrent[0],
                    parametersCurrent[1], parametersCurrent[2]);

                try
                {
                    var jacobianT = jacobian.Transpose();
                    var res = jacobianT.Multiply(residual);
                    var jjT = jacobian.Transpose().Multiply(jacobian);

                    var cholesky = jjT.Cholesky();
                    var step = cholesky.Solve(res);
                    parametersCurrent.Subtract(step, parametersNew);
                }
                catch (Exception)
                {
                    alpha = 0;
                    beta = 0;
                    gamma = 0;
                    return;
                }

                var valueNew = GetObjectiveValue(model, pointCount, dataX, dataY, parametersCurrent[0],
                    parametersCurrent[1], parametersCurrent[2]);

                iterations.Add(parametersNew.Clone());

                if (ShouldTerminate(valueCurrent, valueNew, iterations.Count, parametersCurrent, parametersNew,
                    minimumDeltaValue, minimumDeltaParameters, maximumIterations))
                {
                    break;
                }

                parametersNew.CopyTo(parametersCurrent);
                valueCurrent = valueNew;
            }

            alpha = parametersCurrent[0];
            beta = parametersCurrent[1];
            gamma = parametersCurrent[2];
        }

        private List<int> FindCandidateIds(IList<Point> smoothSpectrum, int window, double trashold)
        {
            var candidates = new List<int>();
            var side = window / 2;

            for (var i = 0; i < smoothSpectrum.Count; i++)
            {
                var current = smoothSpectrum[i].Y;
                if (current < trashold)
                {
                    continue;
                }

                var range = smoothSpectrum.Select(e => e.Y);
                if (i > side)
                {
                    range = range.Skip(i - side);
                }

                range = range.Take(window);

                if (!(Math.Abs(current - range.Max()) < 0.00000000000000001))
                {
                    continue;
                }

                if (i - 1 > 0)
                {
                    candidates.Add(i - 1);
                }
            }
            return candidates;
        }

        private List<Peak> FindCandidates(IList<Point> spectrum, List<int> candidateIds)
        {
            var rawPeaks = new List<Peak>();
            for (var i = 0; i < candidateIds.Count; i++)
            {
                var peak = new Peak
                {
                    Alpha = spectrum[candidateIds[i]].Y,
                    Beta = 0,
                    Gamma = spectrum[candidateIds[i]].X,
                    Xmax = spectrum[candidateIds[i]].X + 100000000.0,
                    Xmin = spectrum[candidateIds[i]].X - 100000000.0
                };
                rawPeaks.Add(peak);
            }

            var orderedRawPeaks = rawPeaks.OrderByDescending(p => p.Alpha).ToList();
            return orderedRawPeaks;
        }

        private double GetObjectiveValue(Model model, int pointCount, IList<double> dataX, IList<double> dataY,
            double alpha, double beta, double gamma)
        {
            var value = 0.0;

            for (var j = 0; j < pointCount; j++)
            {
                var y = model.GetValue(dataX[j], alpha, beta, gamma);

                value += Math.Pow(y - dataY[j], 2.0);
            }

            value *= 0.5;
            return value;
        }

        private Matrix<double> GetObjectiveJacobian(Model model, int pointCount, Vector<double> dataX, double alpha,
            double beta, double gamma)
        {
            var jacobian = new DenseMatrix(pointCount, 3);

            for (var j = 0; j < pointCount; j++)
            {
                var gradient = model.GetGradient(dataX[j], alpha, beta, gamma);

                jacobian.SetRow(j, gradient);
            }
            return jacobian;
        }

        private bool ShouldTerminate(double valueCurrent, double valueNew, int iterationCount,
            Vector<double> parametersCurrent, Vector<double> parametersNew, double minimumDeltaValue,
            double minimumDeltaParameters, double maximumIterations)
        {
            return iterationCount >= maximumIterations;
        }

        private class Model
        {
            public double GetValue(double x, double alpha, double beta, double gamma)
            {
                return alpha / (1.0 + Math.Pow((x - gamma) / beta, 2.0));
            }

            public Vector<double> GetGradient(double x, double alpha, double beta, double gamma)
            {
                var gradient = new DenseVector(3)
                {
                    [0] = 1.0 / (1.0 + Math.Pow((x - gamma) / beta, 2.0)),
                    [1] =
                    2.0 * alpha * beta * Math.Pow(x - gamma, 2.0) /
                    Math.Pow(gamma * gamma + x * x - 2.0 * gamma * x + beta * beta, 2.0),
                    [2] =
                    2.0 * alpha * beta * beta * (x - gamma) /
                    Math.Pow(beta * beta + x * x - 2.0 * x * gamma + gamma * gamma, 2.0)
                };
                return gradient;
            }

            public Vector<double> GetResidualVector(int pointCount, IList<double> dataX, IList<double> dataY,
                double alpha, double beta, double gamma)
            {
                var residual = new DenseVector(pointCount);
                for (var j = 0; j < pointCount; j++)
                {
                    var y = GetValue(dataX[j], alpha, beta, gamma);
                    residual[j] = y - dataY[j];
                }
                return residual;
            }
        }
    }
}