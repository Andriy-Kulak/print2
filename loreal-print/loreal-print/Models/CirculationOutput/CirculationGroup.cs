using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.CirculationOutput
{
    public class CirculationGroup
    {
        public CirculationGroup()
        {
            Init();
        }

        private void Init()
        {
            CirculationGroupType = string.Empty;
            CirculationType = string.Empty;
            CirculationTable = string.Empty;

            CirculationContents = new List<CirculationContent>();
        }

        public string CirculationGroupType { get; set; }
        public string CirculationType { get; set; }
        public string CirculationTable { get; set; }
        public List<CirculationContent> CirculationContents { get; set; }
    }
}