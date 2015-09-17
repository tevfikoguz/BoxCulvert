using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameSolver
{
    public class Material : SerializableItem
    {
        public string Name { get; set; }
        public double ElasticityModulus { get; set; }
        public double UnitWeight { get; set; }
        public double PoissonsRatio { get; set; }
        public double ShearModulus { get { return ElasticityModulus / (2.0 * (1.0 + PoissonsRatio)); } }

        private Material() { ; }

        public Material(string name, double elasticitymodulus, double poissonsratio, double unitweight)
        {
            Name = name;
            ElasticityModulus = elasticitymodulus;
            PoissonsRatio = poissonsratio;
            UnitWeight = unitweight;
        }
    }
}
