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
    public class SpectrumUnitTest
    {


        private const double MaxError = 0.01;
        [TestMethod]
        public void TransformTest()
        {
            var transformer = new FourierTransformer();
            var samples = SampleGenerator.GenerateRelativeFrequencies(1024);
            var complexSamples = samples.Select(s => new Complex(s, 0)).ToArray();
            var transformed = transformer.ApplyDFT(complexSamples, FourierOptions.Default);
            var inverse =
                transformer.ApplyiDFT(transformed, FourierOptions.Default).Select(c => c.Real).ToArray();
            
            //var floats = transformer.ApplyDFT(samples.Select(d => (float)d).Skip(3).ToArray());
            //var freq = new FrequenciesWindow();
            //freq.ShowFrequencies(floats.Take(floats.Length/2).ToArray());
            //freq.ShowDialog();

            var errors = samples.ToArray();
            for (var i = 0; i < errors.Length; i++)
                errors[i] -= inverse[i];
            var max = errors.Max();
            Assert.IsTrue(max<MaxError);
        }
    }
}
