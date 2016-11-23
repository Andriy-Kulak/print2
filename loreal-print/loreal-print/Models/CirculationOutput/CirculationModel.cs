using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.CirculationOutput
{
    public class CirculationModel
    {
        public CirculationModel()
        {
            Init();
        }

        private void Init()
        {
            Year = string.Empty;

            CirculationGroups = new List<CirculationGroup>();
        }

        public int BookID { get; set; }
        public int VersionID { get; set; }
        public string Year { get; set; }

        public List<CirculationGroup> CirculationGroups { get; set; }
    }
}