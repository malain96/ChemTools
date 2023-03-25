using System.Collections.Generic;

namespace ChemTools
{
    public class AppSettings
    {
        public string Version { get; set; }
        public string ExportDir { get; set; }
        public string ErrorDir { get; set; }
    }

    public class NucleosideSettings
    {
        public List<Nucleoside> Nucleosides { get; set; }
        public List<PhosphateGroup> PhosphateGroups { get; set; }
    }

    public class Nucleoside
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public double Mass { get; set; }
    }

    public class PhosphateGroup
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public double Mass { get; set; }
        public short HydrogenCount { get; set; }
    }
}