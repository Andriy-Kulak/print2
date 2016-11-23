using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace loreal_print.Models.CirculationOutput
{
    public class CirculationContent
    {
        public string CirculationSubType { get; set; }
        public int CirculationSubTypeID { get; set; }

        [Range(00, 99999999, ErrorMessage = "Print Circulation must be an integer between 0 and 99,999,999.")]
        public int? PrintCirculation { get; set; }

        [Range(00, 99999999, ErrorMessage = "Digital Replica Circulation must be an integer between 0 and 99,999,999.")]
        public int? DigitalReplicaCirculation { get; set; }

        [Range(00, 99999999, ErrorMessage = "Digital Non-Replica Circulation must be an integer between 0 and 99,999,999.")]
        public int? DigitalNonReplicaCirculation { get; set; }
    }
}