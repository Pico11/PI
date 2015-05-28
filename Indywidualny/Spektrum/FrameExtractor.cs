using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace Spektrum
{
    public class FrameExtractor
    {
        private const int DefaultWindowWidth = 1024;
        public static float[] GetHammingWindow(int width=DefaultWindowWidth)
        {
            return Window.Hamming(width).Select(d => (float)d).ToArray();
        }
        /// <summary>
        /// Extracts one frame from sample array using given window applied starting at given index
        /// </summary>
        /// <param name="window">window function to use</param>
        /// <param name="samples">audio samples</param>
        /// <param name="offset">index of first sample to extract</param>
        /// <returns>Extracted frame</returns>
        public float[] ExtractOneFrame(float[] window, float[] samples, int offset)
        {
            var extracted = new float[window.Length];
            var end = samples.Length - offset >= window.Length ? extracted.Length : samples.Length - offset;
            //jeśli w pozostałej części tablicy próbek zmieści się ramka weźmiemy jej długość
            //w przeciwnym wypadku bierzemy tyle, ile jest próbek
            for (int i = 0, j = offset; i < end; i++, j++)
            {
                extracted[i] = samples[j] * window[i];
            }
            //reszta pozostaje wypełniona 0
            return extracted;
        }
        /// <summary>
        /// Extracts multiple frames with given period
        /// </summary>
        /// <param name="window">window function to use</param>
        /// <param name="samples">audio samples</param>
        /// <param name="windowPeriod">apply window periodically each n-samples</param>
        /// <returns>sequence of extracted frames</returns>
        public IEnumerable<float[]> ExtractFrames(float[] window, float[] samples, int windowPeriod)
        {
            for (var i = 0; i < samples.Length; i+=windowPeriod)
            {
                yield return ExtractOneFrame(window, samples, i);
            }
        }
        /// <summary>
        /// Extracts multiple frames with given period using default window function
        /// </summary>
        /// <param name="samples">audio samples</param>
        /// <param name="windowPeriod">apply window periodically each n-samples</param>
        /// <returns>sequence of extracted frames</returns>
        public IEnumerable<float[]> ExtractFrames(float[] samples, int windowPeriod)
        {
            var window = GetHammingWindow();
            //for (int i = 0; i < samples.Length; i += windowPeriod)
            //{
            //    yield return ExtractOneFrame(window, samples, i);
            //}
            return ExtractFrames(window,samples,windowPeriod);
        }
    }
}
