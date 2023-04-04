using System.Collections.Generic;

namespace ChemTools
{
    public class AppSettings
    {
        public string ExportDir { get; set; }
        public string ErrorDir { get; set; }
    }

    public class NucleosideSettings
    {
        public List<ChemElement> Nucleosides { get; set; }
        public List<ChemElementWithHydrogen> PhosphateGroups { get; set; }
        public List<ChemElementWithHydrogen> Carbamates { get; set; }
    }

    public class ChemElement
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public double Mass { get; set; }
    }

    public class ChemElementWithHydrogen : ChemElement
    {
        public short HydrogenCount { get; set; }
    }
}