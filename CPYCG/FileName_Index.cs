using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPYCG
{
    public class FileName_Index
    {
        public string fileName;
        public int index;

        public FileName_Index(string fileName)
        {
            this.fileName = fileName;
        }

        public FileName_Index(string fileName, int index)
        {
            this.fileName = fileName;
            this.index = index;
        }
    }
}
