using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerLibrary
{
    public class PlayerAssignment
    {
        public int Team {  get; set; }
        public int PlayerIndex { get; set; }
        public Player? Player { get; set; } 
        public int Number {  get; set; }
    }
}
