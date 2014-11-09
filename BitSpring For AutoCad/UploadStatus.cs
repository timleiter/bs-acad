using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitSpring_For_AutoCad
{
    class UploadStatus
    {
        public long bytesTotal { get; set; }
        public long bytesUploaded { get; set; }
        public String fileName { get; set; }
        public long secondsRemaining { get; set; }
    }
}
