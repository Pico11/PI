using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Indywidualny;
using MathNet.Numerics.IntegralTransforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spektrum;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {

        public double[] Generate(int samplesCount)
        {
            var samples = new double[samplesCount];
            var timeStep = 1 / 2048d;
            var current = 0.0;
            for (int i = 0; i < samplesCount; i++)
            {
                var s1 = Math.Sin(current * 823) * 4;
                var s2 = Math.Sin(current * 217);
                var s3 = Math.Sin(current * 65)*3.5;
                var c1 = Math.Cos(current * 643);
                var c2 = Math.Cos(current * 375) * 2;
                var c3 = Math.Cos(current * 90) * 1.5;
                samples[i] = (s1 + s2 +s3+ c1 + c2+c3);
                switch (i % 4)
                {
                    case 0:
                        samples[i] += 0;
                        break;
                    case 1:
                        samples[i] += 3;
                        break;
                    case 2:
                        samples[i] += 0;
                        break;
                    case 3:
                        samples[i] += -3;
                        break;
                }
                current += timeStep;
            }
            return samples;
        }

        [TestMethod]
        public void TransformTest()
        {
            var transformer = new FourierTransformer();
            var samples = Generate(1024);
            var complexSamples = samples.Select(s => new Complex(s, 0)).ToArray();
            var transformed = transformer.ApplyDFT(complexSamples, FourierOptions.Default);
            var inverse =
                transformer.ApplyiDFT(transformed, FourierOptions.Default).Select(c => c.Real).ToArray();
            
            var floats = transformer.ApplyDFT(samples.Select(d => (float)d).Skip(3).ToArray());
            
            FrequenciesWindow freq = new FrequenciesWindow();

            freq.ShowFrequencies(floats.Take(floats.Length/2).ToArray());
            freq.ShowDialog();

            var errors = samples.ToArray();
            for (var i = 0; i < errors.Length; i++)
                errors[i] -= inverse[i];
            var max = errors.Max();
        }
    }
}
