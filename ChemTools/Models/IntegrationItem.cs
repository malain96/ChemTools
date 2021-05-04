using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemTools.Models
{
    public class IntegrationItem
    {
        public int Number { get; set; }
        public decimal? RetentionTime { get; set; }
        public decimal? Area { get; set; }
        public decimal? Height { get; set; }
        public decimal? RelativeArea { get; set; }
        public decimal? RelativeHeight { get; set; }
    }
}
