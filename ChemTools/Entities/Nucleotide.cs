﻿using System;
using System.Linq;

namespace ChemTools.Entities
{
    public class Nucleotide
    {
        public string Code { get; set; }
        public double Mass { get; set; }
        public double MassResult { get; set; }

        public double ErrorMargin
        {
            get
            {
                return Math.Round((Mass - MassResult) / Mass * 1000000, 4);
            }
        }

        public int Count { get; set; }

        public string OderedCode
        {
            get
            {
                char[] characters = Code.ToArray();
                Array.Sort(characters);
                return new string(characters);
            }
        }
    }
}