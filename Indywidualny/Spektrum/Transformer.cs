using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Spektrum
{
    public class Transformer
    {
        public float[] ApplyDFT(float[] samples)
        {
            var complesxSamples = samples.Select(s => new Complex(s, 0));


            return complesxSamples.Select(c=>(float)c.Real).ToArray();
        }
        public float[] ApplyDFT(double[] samples)
        {
            var complesxSamples = samples.Select(s => new Complex(s, 0)).ToArray();

            MathNet.Numerics.IntegralTransforms.Fourier.BluesteinForward(complesxSamples, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);

            return complesxSamples.Select(c => (float)c.Magnitude).ToArray();
        }
    }
}
