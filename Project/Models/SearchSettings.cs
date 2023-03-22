using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.Models
{
    public class SearchSettings
    {
        public bool Etheral { get; set; } = true;
        public bool Ladder { get; set; } = true;
        public bool Softcore { get; set; } = true;
        public bool Hardcore { get; set; } = false;
        public bool Asia { get; set; } = false;
        public bool America { get; set; } = false;
        public bool Europe { get; set; } = false;
        public bool AllIdentified { get; set; } = true;
        public bool Identified { get; set; } = false;
        public bool Unidentified { get; set; } = false;
        public bool AllOffers { get; set; } = true;
        public bool MakeOffers { get; set; } = false;
        public bool DefinedOffers { get; set; } = true;
        public bool AllUpgraded { get; set; } = true;
        public bool OnlyUpgraded { get; set; } = false;
        public bool NotUpgraded { get; set; } = false;
        public bool PC { get; set; } = true;
        public bool Switch { get; set; } = false;
        public bool Playstation { get; set; } = false;
        public bool xBox { get; set; } = false;
    }
}
