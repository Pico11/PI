using System;
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
            var timeStep = 1 / 44100d;
            var current = 0.0;
            for (int i = 0; i < samplesCount; i++)
            {
                var s1 = Math.Sin(current * 123);
                var s2 = Math.Sin(current * 17);
                var c1 = Math.Cos(current * 43);
                var c2 = Math.Cos(current * 75);
                samples[i] = (s1 + s2 + c1 + c2) ;
                current += timeStep;
            }

            return samples;
        }

        public void Check()
        {
            var samples = Generate(44100 * 3);
            var transformed = new Transformer().ApplyDFT(samples);


        }
        [TestMethod]
        public void TestMethod1()
        {

        }
        [TestMethod]
        public void TransformTest()
        {
            var transformer = new Transformer();
            var samples = Generate(44100);
            var transformed = transformer.ApplyDFT(samples);



        }
    }
}
