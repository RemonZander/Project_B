using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project_B_V2._0
{
    public class Mederwerker : User
    {
        public string? BeveiligingsCode { get; set; }

        public Roles Role { get; set; }

    }
}