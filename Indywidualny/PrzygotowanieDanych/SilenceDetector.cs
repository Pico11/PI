using System.Collections.Generic;
using System.Linq;

namespace PrzygotowanieDanych
{
    public class SilenceDetector
    {
        private const float SilenceTreshold = 0.01f;
        public bool[] DetectSilence(float[] samples, int windowLength, int minDuration = 128)
        {
            var silence = new bool[samples.Length];
            if (samples.Length < windowLength) return silence;//too short
            var sum = 0f;
            //starting cic filter
            for (int i = 0; i < windowLength; i++)
            {
                var sample = samples[i];
                sum += sample * sample;
            }
            if (sum <= SilenceTreshold)
            {
                for (int i = 0; i < windowLength; i++)
                {
                    silence[i] = true;
                }
            }

            ////1 length minDuration
            //for (int i = windowLength; i < samples.Length; i++)
            //{
            //    var newSample = samples[i];
            //    var oldSample = samples[i - windowLength];
            //    //at O(n) memory cost possible computation reduction by using array of squares
            //    sum -= oldSample * oldSample + newSample * newSample;

            //    if (sum <= SilenceTreshold) silence[i] = true;
            //}


            var silenceDuration = sum <= SilenceTreshold ? windowLength : 0;
            for (int i = windowLength; i < samples.Length; i++)
            {
                var newSample = samples[i];
                var oldSample = samples[i - windowLength];
                //at O(n) memory cost possible computation reduction by using array of squares
                sum -= oldSample * oldSample + newSample * newSample;

                if (sum <= SilenceTreshold)
                {
                    silenceDuration++;
                    if (silenceDuration > minDuration)
                        silence[i] = true;
                    if (silenceDuration == minDuration)
                        for (int j = i - minDuration; j < i; j++)
                        {
                            silence[j] = true;
                        }
                }
                else
                {
                    if (silenceDuration >= minDuration) silence[i - 1] = false;//small withdraval?
                    silenceDuration = 0;
                }
            }

            return silence;
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

            var silenceDuration = sum <= SilenceTreshold ? windowLength : 0;
            silence.Add(silenceDuration);//initial silence
            if (silenceDuration == 0)
                silence.Add(-windowLength);//noise if found in the beginning

            for (int i = windowLength; i < samples.Length; i++)
            {
                var newSample = samples[i];
                var oldSample = samples[i - windowLength];
                //at O(n) memory cost possible computation reduction by using array of squares
                sum -= oldSample * oldSample + newSample * newSample;//recalculate sum - move window

                if (sum <= SilenceTreshold)
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
            var times = SilenceTimes(samples, windowLength);
            var speech = times.Sum(time => time < 0 ? -time : 0);
            var silence = times.Sum(time => time > 0 ? time : 0);
            var trailing = times[0] + (times.Last() > 0 ? times.Last() : 0);
            var total = silence + speech - trailing;
            speechRatio = speech / (float)total;
            var cut = samples.Skip(times[0]).Take(total).ToArray();

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
            var times = SilenceTimes(samples, windowLength);
            var speech = times.Sum(time => time < 0 ? -time : 0);
            var silence = times.Sum(time => time > 0 ? time : 0);
            var trailing = times[0] + (times.Last() > 0 ? times.Last() : 0);
            var total = silence + speech - trailing;
            var cut = samples.Skip(times[0]).Take(total).ToArray();

            return cut;
        }

    }
}
