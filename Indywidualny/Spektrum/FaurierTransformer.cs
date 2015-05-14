using System.Linq;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace Spektrum
{
    public class FourierTransformer
    {
        public Complex[] ApplyDFT(Complex[] samples,FourierOptions options)
        {
            return Fourier.NaiveForward(samples, options);
        }
        public Complex[] ApplyDFT(Complex[] samples)
        {
            return ApplyDFT(samples,FourierOptions.Default);
        }

        public float[] ApplyDFT(float[] samples)
        {
            var complexSamples = samples.Select(s => new Complex(s, 0)).ToArray();

            complexSamples = ApplyDFT(complexSamples,FourierOptions.AsymmetricScaling);

            return complexSamples.Select(c=>(float)c.Real).ToArray();
        }
        public double[] ApplyDFT(double[] samples)
        {
            var complexSamples = samples.Select(s => new Complex(s, 0)).ToArray();

            complexSamples = ApplyDFT(complexSamples);
            
            return complexSamples.Select(c => c.Real).ToArray();
        }


        public Complex[] ApplyFFT(Complex[] samples)
        {
            Fourier.Forward(samples, FourierOptions.Default);

            return samples;
        }
        public float[] ApplyFFT(float[] samples)
        {
            var complexSamples = samples.Select(s => new Complex(s, 0)).ToArray();

            Fourier.Forward(complexSamples, FourierOptions.Default);

            return complexSamples.Select(c => (float)c.Magnitude).ToArray();
        }
        public float[] ApplyFFT(double[] samples)
        {
            var complexSamples = samples.Select(s => new Complex(s, 0)).ToArray();

            Fourier.Forward(complexSamples, FourierOptions.Default);

            return complexSamples.Select(c => (float)c.Magnitude).ToArray();
        }




        public Complex[] ApplyiDFT(Complex[] samples, FourierOptions options)
        {
            return  Fourier.NaiveInverse(samples, options);
        }
        public Complex[] ApplyiDFT(Complex[] samples)
        {
            return ApplyiDFT(samples, FourierOptions.Default);
        }

        public Complex[] ApplyiFFT(Complex[] samples, FourierOptions options)
        {
            var copy = samples.ToArray();
            Fourier.Inverse(copy, options);

            return copy;
        }
        public Complex[] ApplyiFFT(Complex[] samples)
        {
            return ApplyiFFT(samples,FourierOptions.Default);
        }

    }
}
