using System.Collections.Generic;
using System.Linq;

namespace D2Traderie.Project.Models
{
    public class SearchSettings
    {
        // Platform
        public bool PC { get; set; } = true;
        public bool Switch { get; set; } = false;
        public bool Playstation { get; set; } = false;
        public bool xBox { get; set; } = false;

        // Mode
        public bool Softcore { get; set; } = true;
        public bool Hardcore { get; set; } = false;

        // Ladder
        public bool Ladder { get; set; } = true;
        public bool NonLadder { get; set; } = false;

        // Game Version
        public bool GameVersionClassic { get; set; } = false;
        public bool GameVersionLOD { get; set; } = false;
        public bool GameVersionROTW { get; set; } = true;

        // Identified - tylko zidentyfikowane
        public bool AllIdentified { get; set; } = false;
        public bool Identified { get; set; } = true;
        public bool Unidentified { get; set; } = false;

        // Ethereal - tylko nie-eteryczne
        public bool EtherealAll { get; set; } = false;
        public bool Ethereal { get; set; } = false;
        public bool NotEthereal { get; set; } = true;

        // Make Offer - tylko zdefiniowane oferty
        public bool AllOffers { get; set; } = false;
        public bool MakeOffers { get; set; } = false;
        public bool DefinedOffers { get; set; } = true;

        // Region
        public bool Americas { get; set; } = false;
        public bool Europe { get; set; } = false;
        public bool Asia { get; set; } = false;

        // Upgraded
        public bool AllUpgraded { get; set; } = true;
        public bool OnlyUpgraded { get; set; } = false;
        public bool NotUpgraded { get; set; } = false;

        // --- Budowanie parametrów URL ---

        public string GetPlatformParam()
        {
            var selected = new List<string>();
            if (PC) selected.Add("PC");
            if (Switch) selected.Add("switch");
            if (Playstation) selected.Add("playstation");
            if (xBox) selected.Add("xbox");
            return selected.Count > 0 ? string.Join(",", selected) : "PC";
        }

        public string GetModeParam()
        {
            if (Softcore && Hardcore) return null;
            if (Hardcore) return "hardcore";
            return "softcore";
        }

        public string GetLadderParam()
        {
            if (Ladder && NonLadder) return null;
            if (NonLadder) return "false";
            return "true";
        }

        public string GetGameVersionParam()
        {
            var selected = new List<string>();
            if (GameVersionClassic) selected.Add("classic");
            if (GameVersionLOD) selected.Add("lord%20of%20destruction");
            if (GameVersionROTW) selected.Add("reign%20of%20the%20warlock");
            return selected.Count > 0 && selected.Count < 3 ? string.Join("%2C", selected) : null;
        }

        public string GetUnidentifiedParam()
        {
            if (AllIdentified) return null;
            if (Unidentified) return "true";
            if (Identified) return "false";
            return null;
        }

        public string GetEtherealParam()
        {
            if (EtherealAll) return null;
            if (Ethereal) return "true";
            if (NotEthereal) return "false";
            return null;
        }

        public string GetMakeOfferParam()
        {
            if (AllOffers) return null;
            if (MakeOffers) return "true";
            if (DefinedOffers) return "false";
            return null;
        }
    }
}