using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastFM.Common.Classes
{
    public class ScrobblerSourceStatus
    {
        public Guid Identifier { get; set; }

        public bool IsEnabled { get; set; }
    }
}
