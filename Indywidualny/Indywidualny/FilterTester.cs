using Nagrywanie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indywidualny
{
    class FilterTester
    {
        public void Convert(string directory)
        {
            var filter = new Filter();
            var files=System.IO.Directory.GetFiles(directory, "*.wav", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var newFileBase = file.Substring(0, file.Length - 4);
                for (int i = 1; i <= 10; i++)
                {
                    filter.LowPass(file, newFileBase + i.ToString() + ".wav",i*1000);
                }

            }
        }

    }
}
