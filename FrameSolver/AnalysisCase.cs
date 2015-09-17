using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameSolver
{
    public class AnalysisCase : SerializableItem
    {
        public string Name { get; set; }

        public CombinableList<NodeDeformation> NodeDeformations { get; private set; }
        public CombinableList<Reaction> Reactions { get; private set; }
        public CombinableList<FrameInternalForce> FrameInternalForces { get; private set; }

        public AnalysisCase(string name)
        {
            Name = name;

            NodeDeformations = new CombinableList<NodeDeformation>();
            Reactions = new CombinableList<Reaction>();
            FrameInternalForces = new CombinableList<FrameInternalForce>();
        }
    }
}
