using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameSolver
{
    public class Constraint
    {
        private List<Node> nodes = new List<Node>();

        public List<Node> Nodes { get { return nodes; } }
        public DOF ConstrainedDOFs { get; set; }

        private Constraint() { ; }

        public Constraint(DOF constraintDOFs, params Node[] constraintNodes)
        {
            ConstrainedDOFs = constraintDOFs;
            nodes.AddRange(constraintNodes);
        }
    }
}
