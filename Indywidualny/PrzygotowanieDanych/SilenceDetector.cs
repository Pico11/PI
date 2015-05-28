using System;
using System.Collections.Generic;
using System.Linq;

namespace PrzygotowanieDanych
{
    public class SilenceDetector
    {
        private const float SilenceTresholdShort = 2f / short.MaxValue;
        private const float SilenceTreshold24 = 2f / (1 << 23);
        private const float SilenceTresholdInt = 2f / int.MaxValue;

        public bool[] DetectSilence(float[] samples, int bitsPerSample, int minDuration)
        {
            switch (bitsPerSample)
            {
                case 16:
                    return DetectSilence(samples, SilenceTresholdShort, minDuration);
                case 24:
                    return DetectSilence(samples, SilenceTreshold24, minDuration);
                case 32:
                    return DetectSilence(samples, SilenceTresholdInt, minDuration);
                default:
                    throw new InvalidOperationException("Unsupported bit depth");
            }
        }
        /// <summary>
        /// Detects silence in samples array
        /// </summary>
        /// <param name="samples">samples array</param>
        /// <param name="silenceTreshold">border value (absolute) of silence</param>
        /// <param name="minDuration">minimum number of quiet samples before we qualify them as silence</param>
        /// <returns>array indicating whether sample in samples aray contains silence</returns>
        public bool[] DetectSilence(float[] samples, float silenceTreshold = SilenceTresholdShort, int minDuration = 128)
        {
            //we should have one channel
            //format should be 16bit PCM
            //silence is ABS<treshold
            //frames are converted to float... 

            var silence = samples.Select(f => Math.Abs(f) < silenceTreshold).ToArray();
            //silent samples, but... it can happen in the middle at single frames
            if (minDuration == 1) return silence;//if we want check single sample

            if (samples.Length < minDuration) return silence;//too short - say there is no silence
            var silenceWindows = new bool[silence.Length];
            var silent = 0;
            //starting
            for (var i = 0; i < minDuration; i++)//count silence at beginning
            {
                if (silence[i]) silent++;
                else silent = 0;
            }
            if (silent == minDuration)//no sounds at the beginning
                for (var i = 0; i < minDuration; i++)
                {
                    silenceWindows[i] = true;
                }

            for (var i = minDuration; i < silence.Length; i++)
            {
                if (silence[i]) silent++;
                else
                {
                    silenceWindows[i - 1] = false;//unmark previous sample if it was marked
                    silent = 0;
                }

                if (silent > minDuration) silenceWindows[i] = true;
            }

            return silence;
        }
        /// <summary>
        /// Cut (change to zero) 
        /// </summary>
        /// <param name="samples">samples array</param>
        /// <param name="treshold">treshold value</param>
        /// <returns>new array containing clipped values</returns>
        public float[] ZeroSilence(float[] samples, float treshold = SilenceTresholdShort)
        {
            var cut = new float[samples.Length];
            var silence = DetectSilence(samples, treshold);
            for (int i = 0; i < cut.Length; i++)
            {
                cut[i] = silence[i] ? 0 : samples[i];
            }
            return cut;
        }

        /// <summary>
        /// Detect silence and speech periods using sliding window
        /// </summary>
        /// <param name="samples">samples to examine</param>
        /// <param name="windowLength">window length</param>
        /// <param name="minDuration">minimum silence duration in samples</param>
        /// <returns>array describing silence and speech periods in number of samples; speech uses negative values</returns>
        public int[] SilenceTimes(float[] samples, int windowLength, int minDuration = 128)
        {
            //positive for silence duration, negative for not silence duration
            var silence = new List<int>();
            if (samples.Length < windowLength) return new[] { 0, -samples.Length };//too short
            var sum = 0f;

            //starting cic filter
            for (int i = 0; i < windowLength; i++)
            {
                var sample = samples[i];
                sum += sample * sample;
            }

            var silenceDuration = sum <= SilenceTresholdShort ? windowLength : 0;
            silence.Add(silenceDuration);//initial silence
            if (silenceDuration == 0)
                silence.Add(-windowLength);//noise if found in the beginning

            for (int i = windowLength; i < samples.Length; i++)
            {
                var newSample = samples[i];
                var oldSample = samples[i - windowLength];
                //at O(n) memory cost possible computation reduction by using array of squares
                sum -= oldSample * oldSample + newSample * newSample;//recalculate sum - move window

                if (sum <= SilenceTresholdShort)
                {
                    if (silenceDuration < 0)
                    {
                        silence.Add(silenceDuration);
                        silenceDuration = 1;
                    }
                    else
                        silenceDuration++;
                    //if (silenceDuration == minDuration)
                    //we will add silence time when it ends and noise time when detecting silence
                    //    silence.Add();
                }
                else
                {
                    if (silenceDuration > 0)
                    {
                        if (silenceDuration >= minDuration)
                        {
                            silence.Add(silenceDuration);
                            silenceDuration = -1;
                        }
                        else//too short silence
                            if (silence[silence.Count - 1] < 0)//prelong noise
                            {
                                silence[silence.Count - 1] -= silenceDuration;
                                silenceDuration = -1;
                            }
                    }
                    else
                    {
                        silenceDuration--;
                    }
                }
            }

            if (silenceDuration > minDuration)
                silence.Add(silenceDuration);
            else
                if (silence[silence.Count - 1] < 0)
                    silence[silence.Count - 1] -= silenceDuration;
                else
                {
                    silence.Add(-silenceDuration);
                }

            return silence.ToArray();
        }

        /// <summary>
        /// Remove leading and trailing samples containing silence
        /// </summary>
        /// <param name="samples">audio samples</param>
        /// <param name="windowLength">window length</param>
        /// <param name="speechRatio">calculated speech time ratio</param>
        /// <returns>array of samples without silence</returns>
        public float[] CutSilence(float[] samples, int windowLength, out float speechRatio)
        {
            var times = DetectSilence(samples,SilenceTresholdShort, windowLength);
            var speech = times.Count(t => !t);//not silence
            var silence = times.Count(t => t);
            var silenceStart = times.TakeWhile(t => t).Count();
            var silenceEnd = times.Reverse().TakeWhile(t => t).Count();
            var total = samples.Length - silenceStart - silenceEnd;
            var cut = samples.Skip(silenceStart).Take(total).ToArray();

            speechRatio = speech / (float)total;

            return cut;
        }
        /// <summary>
        /// Remove leading and trailing samples containing silence
        /// </summary>
        /// <param name="samples">audio samples</param>
        /// <param name="windowLength">window length</param>
        /// <returns>array of samples without silence</returns>
        public float[] CutSilence(float[] samples, int windowLength)
        {
            var times = DetectSilence(samples,SilenceTresholdShort, windowLength);
            //var speech = times.Count(t=>!t);//not silence
            //var silence = times.Count(t => t);
            var silenceStart = times.TakeWhile(t => t).Count();
            var silenceEnd = times.Reverse().TakeWhile(t=>t).Count();
            var total = samples.Length-silenceStart - silenceEnd;
            var cut = samples.Skip(silenceStart).Take(total).ToArray();

            return cut;
        }

    }
}
