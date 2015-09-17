using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameSolver
{
    public class FrameSection : SerializableItem
    {
        public string Name { get; set; }
        public Material Material { get; set; }
        public double Area { get; set; }
        public double MomentOfInertia { get; set; }

        private FrameSection() { ; }

        public FrameSection(string name, Material material, double area, double momentofinertia)
        {
            Name = name;
            Material = material;
            Area = area;
            MomentOfInertia = momentofinertia;
        }
    }
}
