using System;

namespace UnitTest
{
    static class SampleGenerator
    {
        //sampling is usually 44100
        //frequency is therefore multiplied in relative method
        //for 44100 samples we have 1-1
        //for other we have     F/samples -to- 1

        public static double[] GenerateConstantFrequencies(int samplesCount)
        {
            var samples = new double[samplesCount];
            const double timeStep =  2*Math.PI/44100;//one period per second(44100 samples)
            var current = 0.0;
            for (var i = 0; i < samplesCount; i++)
            {
                var s1 = Math.Sin(current * 823) * 4;
                var s2 = Math.Sin(current * 217);
                var s3 = Math.Sin(current * 65) * 3.5;
                var c1 = Math.Cos(current * 643);
                var c2 = Math.Cos(current * 375) * 2;
                var c3 = Math.Cos(current * 90) * 1.5;
                samples[i] = (s1 + s2 + s3 + c1 + c2 + c3);

                current += timeStep;
            }
            return samples;
        }

        public static double[] GenerateRelativeFrequencies(int samplesCount)
        {
            var samples = new double[samplesCount];
            var timeStep = 1 / (double)(2*samplesCount);
            var current = 0.0;
            for (var i = 0; i < samplesCount; i++)
            {
                var s1 = Math.Sin(current * 823) * 4;
                var s2 = Math.Sin(current * 217);
                var s3 = Math.Sin(current * 65) * 3.5;
                var c1 = Math.Cos(current * 643);
                var c2 = Math.Cos(current * 375) * 2;
                var c3 = Math.Cos(current * 90) * 1.5;
                samples[i] = (s1 + s2 + s3 + c1 + c2 + c3);
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



    }
}
