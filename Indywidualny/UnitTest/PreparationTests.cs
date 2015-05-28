using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrzygotowanieDanych;

namespace UnitTest
{
    [TestClass]
    public class PreparationUnitTest
    {
        [TestMethod]
        public void OverflowRemoverTest()
        {
            var samples = SampleGenerator.GenerateConstantFrequencies(100);
            var max = samples.Max(s => Math.Abs(s));
            var scaled = samples.Select(s => s*1.1 / max).ToArray();
            var clipped = scaled.Select(s => s > 1 ? s - 2 : (s < -1 ? s + 2 : s));
            var clippedArray = clipped.ToArray();
            var converted = OverflowRemover.RemoveOverflows(clippedArray);

            for (var i = 0; i < scaled.Length; i++)
            {
                Assert.IsTrue(scaled[i].Equals(converted[i]));
            }
        }

        [TestMethod]
        public void NormalisationTest()
        {

        }
    }
}
