using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerLibrary
{
    public class Nationality
    {
        public int NationalityIndex { get; set; }
        public string? NationalityCode { get; set; }
        public string? NationalityName { get; set; }
        public string? NationalityNameAlt { get; set; }
        public long NameOffset { get; set; }
        public int NameFieldLength { get; set; }
        public long NameAltOffset { get; set; }
        public int NameAltFieldLength { get; set; }
        public long CodeOffset { get; set; }
        public int CodeFieldLength { get; set; }
    }
}