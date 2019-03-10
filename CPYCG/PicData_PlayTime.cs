using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPYCG
{
    public class PicData_PlayTime
    {
        public byte[] picData;
        public int playTime;

        public PicData_PlayTime(byte[] picData)
        {
            this.picData = picData;
        }

        public PicData_PlayTime(byte[] picData, int playTime)
        {
            this.picData = picData;
            this.playTime = playTime;
        }
    }
}
