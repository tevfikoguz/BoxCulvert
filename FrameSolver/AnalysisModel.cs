using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace FrameSolver
{
    public class AnalysisModel
    {
        private Matrix<double> stiffnessMatrix;
        private Matrix<double> restrainedStiffnessMatrix;

        private List<Node> nodes = new List<Node>();
        private List<Material> materials = new List<Material>();
        private List<FrameSection> framesections = new List<FrameSection>();
        private List<Frame> frames = new List<Frame>();
        private List<Spring> springs = new List<Spring>();
        private List<AnalysisCase> analysisCases = new List<AnalysisCase>();
        private List<Combination> combinations = new List<Combination>();

        public List<Node> Nodes { get { return nodes; } }
        public List<Material> Materials { get { return materials; } }
        public List<FrameSection> FrameSections { get { return framesections; } }
        public List<Frame> Frames { get { return frames; } }
        public List<Spring> Springs { get { return springs; } }
        public List<AnalysisCase> AnalysisCases { get { return analysisCases; } }
        public List<Combination> Combinations { get { return combinations; } }

        public AnalysisModel() { ; }

        /// <summary>
        /// Solves the system for the given load case.
        /// </summary>
        public void Run(AnalysisCase analysisCase)
        {
            AssignNodeIndices();

            if (stiffnessMatrix == null)
            {
                BuildStiffnessMatrix();
            }

            Vector<double> f = GetLoadVector(analysisCase);

            Vector<double> d = restrainedStiffnessMatrix.Solve(f);
            CalculateDeformations(analysisCase, d);

            Vector<double> r = stiffnessMatrix * d;
            CalculateReactions(analysisCase, r);

            CalculateFrameInternalForces(analysisCase, d);
        }

        /// <summary>
        /// Solves the system for all load cases.
        /// </summary>
        public void RunAll()
        {
            foreach (AnalysisCase analysisCase in AnalysisCases)
            {
                Run(analysisCase);
            }
        }

        /// <summary>
        /// Builds the stiffness matrix.
        /// </summary>
        public void BuildStiffnessMatrix()
        {
            AssignNodeIndices();

            ResetLastStiffnessMatrix();

            stiffnessMatrix = GetSystemStiffness();
            restrainedStiffnessMatrix = EraseRestrainedDOFs(stiffnessMatrix);
        }

        /// <summary>
        /// Assigns indices to nodes.
        /// </summary>
        public void AssignNodeIndices()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Index = i;
            }
        }

        /// <summary>
        /// Assigns indices to frames.
        /// </summary>
        public void AssignFrameIndices()
        {
            for (int i = 0; i < frames.Count; i++)
            {
                frames[i].Index = i;
            }
        }

        /// <summary>
        /// Assigns indices to springs.
        /// </summary>
        public void AssignSpringIndices()
        {
            for (int i = 0; i < springs.Count; i++)
            {
                springs[i].Index = i;
            }
        }

        /// <summary>
        /// Returns the load vector for the given load case.
        /// </summary>
        private Vector<double> GetLoadVector(AnalysisCase analysisCase)
        {
            Vector<double> p = Vector<double>.Build.Dense(nodes.Count * 3, 0);

            // Node loads
            foreach (Node node in nodes)
            {
                foreach (NodeLoad load in node.Loads.FindAll((e) => e.AnalysisCase == analysisCase))
                {
                    if (load is NodePointLoad)
                    {
                        // Applied point loads
                        NodePointLoad pl = (NodePointLoad)load;
                        int i = node.Index;
                        p[i * 3 + 0] += pl.FX;
                        p[i * 3 + 1] += pl.FZ;
                        p[i * 3 + 2] += pl.MY;
                    }
                }
            }

            // Frame loads
            foreach (Frame frame in Frames)
            {
                double l = frame.Length;
                int i = frame.NodeI.Index;
                int j = frame.NodeJ.Index;

                foreach (FrameLoad load in frame.Loads.FindAll((e) => e.AnalysisCase == analysisCase))
                {
                    if (load is FrameUniformLoad)
                    {
                        // Frame fixed-end reactions from frame uniform loads
                        FrameUniformLoad pl = (FrameUniformLoad)load;
                        double fx = pl.FX * l / 2.0;
                        double fz = pl.FZ * l / 2.0;

                        p[i * 3 + 0] += fx;
                        p[i * 3 + 1] += fz;
                        p[i * 3 + 2] += 0; // End moment

                        p[j * 3 + 0] += fx;
                        p[j * 3 + 1] += fz;
                        p[j * 3 + 2] += 0; // End moment
                    }
                    else if (load is FrameTrapezoidalLoad)
                    {
                        // Frame fixed-end reactions from frame trapezoidal loads
                        FrameTrapezoidalLoad pl = (FrameTrapezoidalLoad)load;

                        double fx = (pl.FXI + pl.FXJ) / 2.0 * l / 2.0;
                        double fz = (pl.FZI + pl.FZJ) / 2.0 * l / 2.0;

                        p[i * 3 + 0] += fx;
                        p[i * 3 + 1] += fz;
                        p[i * 3 + 2] += 0; // End moment

                        p[j * 3 + 0] += fx;
                        p[j * 3 + 1] += fz;
                        p[j * 3 + 2] += 0; // End moment
                    }
                }
            }

            // Set restrained node coefficients to 0
            foreach (Node n in nodes)
            {
                if ((n.Restraints & DOF.UX) != DOF.Free)
                    p[n.Index * 3 + 0] = 0;
                if ((n.Restraints & DOF.UZ) != DOF.Free)
                    p[n.Index * 3 + 1] = 0;
                if ((n.Restraints & DOF.RY) != DOF.Free)
                    p[n.Index * 3 + 2] = 0;
            }

            return p;
        }

        /// <summary>
        /// Returns the assembled system stiffness matrix.
        /// </summary>
        private Matrix<double> GetSystemStiffness()
        {
            Matrix<double> k = Matrix<double>.Build.Dense(nodes.Count * 3, nodes.Count * 3, 0);

            // Assemble element stiffness matrices
            foreach (Frame f in frames)
            {
                Matrix<double> me = f.ElementStiffness;
                int gi = f.NodeI.Index * 3;
                int gj = f.NodeJ.Index * 3;

                for (int i = 0; i < 6; i++)
                {
                    int ii = (i < 3 ? gi + i : gj + i - 3);
                    for (int j = 0; j < 6; j++)
                    {
                        int jj = (j < 3 ? gi + j : gj + j - 3);
                        k[ii, jj] += me[i, j];
                    }
                }
            }

            // Assemble spring stiffness matrices
            foreach (Spring s in springs)
            {
                Matrix<double> me = s.ElementStiffness;
                int gi = s.NodeI.Index * 3;
                int gj = s.NodeJ.Index * 3;

                for (int i = 0; i < 6; i++)
                {
                    int ii = (i < 3 ? gi + i : gj + i - 3);
                    for (int j = 0; j < 6; j++)
                    {
                        int jj = (j < 3 ? gi + j : gj + j - 3);
                        k[ii, jj] += me[i, j];
                    }
                }
            }

            return k;
        }

        /// <summary>
        /// Sets all restrained DOFs of the given stiffness matrix to 0.
        /// </summary>
        private Matrix<double> EraseRestrainedDOFs(Matrix<double> k)
        {
            Matrix<double> kr = k.Clone();

            // Restrained DOFs
            foreach (Node n in nodes)
            {
                if ((n.Restraints & DOF.UX) != DOF.Free)
                    EraseDOF(ref kr, n.Index * 3 + 0);
                if ((n.Restraints & DOF.UZ) != DOF.Free)
                    EraseDOF(ref kr, n.Index * 3 + 1);
                if ((n.Restraints & DOF.RY) != DOF.Free)
                    EraseDOF(ref kr, n.Index * 3 + 2);
            }

            return kr;
        }

        /// <summary>
        /// Sets the rows and columns of the stiffness corresponding 
        /// to the given DOF to 0, and the diagonal element to 1.
        /// </summary>
        private void EraseDOF(ref Matrix<double> k, int index)
        {
            Vector<double> zeroes = Vector<double>.Build.Dense(k.RowCount, 0);
            k.SetRow(index, zeroes);
            k.SetColumn(index, zeroes);
            k[index, index] = 1;
        }

        /// <summary>
        /// Calculates node deformations for the given load case and deformation vector.
        /// </summary>
        private void CalculateDeformations(AnalysisCase analysisCase, Vector<double> deformations)
        {
            analysisCase.NodeDeformations.Clear();

            for (int i = 0; i < nodes.Count; i++)
            {
                int j = nodes[i].Index;
                NodeDeformation d = new NodeDeformation(nodes[i], deformations[j * 3], deformations[j * 3 + 1], deformations[j * 3 + 2]);
                analysisCase.NodeDeformations.Add(d);
            }
        }

        /// <summary>
        /// Calculates support reactions for the given load case and deformation vector.
        /// </summary>
        private void CalculateReactions(AnalysisCase analysisCase, Vector<double> reactions)
        {
            analysisCase.Reactions.Clear();

            for (int i = 0; i < nodes.Count; i++)
            {
                int j = nodes[i].Index;
                if (nodes[i].Restraints != DOF.Free)
                {
                    Reaction r = new Reaction(nodes[i], reactions[j * 3], reactions[j * 3 + 1], reactions[j * 3 + 2]);
                    analysisCase.Reactions.Add(r);
                }
            }
        }

        /// <summary>
        /// Calculates frame internal forces the given load case and global deformation vector.
        /// </summary>
        private void CalculateFrameInternalForces(AnalysisCase analysisCase, Vector<double> deformations)
        {
            analysisCase.FrameInternalForces.Clear();

            foreach (Frame frame in Frames)
            {
                int i = frame.NodeI.Index;
                int j = frame.NodeJ.Index;
                Vector<double> globalDeformations = Vector<double>.Build.Dense(6, 0);
                globalDeformations[0] = deformations[i * 3 + 0];
                globalDeformations[1] = deformations[i * 3 + 1];
                globalDeformations[2] = deformations[i * 3 + 2];
                globalDeformations[3] = deformations[j * 3 + 0];
                globalDeformations[4] = deformations[j * 3 + 1];
                globalDeformations[5] = deformations[j * 3 + 2];
                Vector<double> localDeformations = frame.Transformation.Transpose().Inverse() * globalDeformations;
                Vector<double> internalForces = frame.LocalElementStiffness * localDeformations;
                double ni = internalForces[0];
                double vi = internalForces[1];
                double mi = -internalForces[2];
                double nj = -internalForces[3];
                double vj = -internalForces[4];
                double mj = internalForces[5];
                FrameInternalForce force = new FrameInternalForce(frame, ni, vi, mi, nj, vj, mj);
                analysisCase.FrameInternalForces.Add(force);
            }
        }

        /// <summary>
        /// Erases last stiffness matrix.
        /// </summary>
        public void ResetLastStiffnessMatrix()
        {
            stiffnessMatrix = null;
            restrainedStiffnessMatrix = null;
        }

        /// <summary>
        /// Erases all model items.
        /// </summary>
        public void ResetModel()
        {
            ResetLastStiffnessMatrix();

            Nodes.Clear();
            Materials.Clear();
            FrameSections.Clear();
            Frames.Clear();
            Springs.Clear();

            AnalysisCases.Clear();
            Combinations.Clear();
        }

        /// <summary>
        /// Adds a new node to the model.
        /// </summary>
        public Node AddNode(double x, double z)
        {
            Node n = new Node(x, z);
            Nodes.Add(n);
            return n;
        }

        /// <summary>
        /// Adds a new frame to the model.
        /// </summary>
        public Frame AddFrame(FrameSection section, Node n1, Node n2)
        {
            Frame n = new Frame(section, n1, n2);
            Frames.Add(n);
            return n;
        }

        /// <summary>
        /// Adds a new spring to the model.
        /// </summary>
        public Spring AddSpring(double stiffness, double shearStiffness, double rotationalStiffness, Node n1, Node n2)
        {
            Spring n = new Spring(stiffness, shearStiffness, rotationalStiffness, n1, n2);
            Springs.Add(n);
            return n;
        }

        /// <summary>
        /// Adds a new material to the model.
        /// </summary>
        public Material AddMaterial(string name, double elasticitymodulus, double poissonsratio, double unitweight)
        {
            Material m = new Material(name, elasticitymodulus, poissonsratio, unitweight);
            Materials.Add(m);
            return m;
        }

        /// <summary>
        /// Adds a new frame section to the model.
        /// </summary>
        public FrameSection AddFrameSection(string name, Material material, double area, double momentofinertia)
        {
            FrameSection s = new FrameSection(name, material, area, momentofinertia);
            FrameSections.Add(s);
            return s;
        }

        /// <summary>
        /// Adds a new analysis case to the model.
        /// </summary>
        public AnalysisCase AddAnalysisCase(string name)
        {
            AnalysisCase a = new AnalysisCase(name);
            AnalysisCases.Add(a);

            return a;
        }

        /// <summary>
        /// Adds a new linear combination to the model.
        /// </summary>
        public Combination AddLinearCombination(string name)
        {
            Combination c = new Combination(name);
            Combinations.Add(c);

            return c;
        }

        /// <summary>
        /// Adds a new envelope combination to the model.
        /// </summary>
        public Combination AddEnvelopeCombination(string name)
        {
            Combination c = new Combination(name, true);
            Combinations.Add(c);

            return c;
        }

        /// <summary>
        /// Adds a new node point load.
        /// </summary>
        public void AddNodePointLoad(AnalysisCase analysisCase, Node node, double fx, double fz, double my)
        {
            node.AddPointLoad(analysisCase, fx, fz, my);
        }

        /// <summary>
        /// Adds frame self weight load.
        /// </summary>
        public void AddFrameSelfWeight(AnalysisCase analysisCase, Frame frame)
        {
            frame.AddUniformLoad(analysisCase, 0, -frame.WeightPerLength);
        }

        /// <summary>
        /// Adds a new frame uniform load.
        /// </summary>
        public void AddFrameUniformLoad(AnalysisCase analysisCase, Frame frame, double fx, double fz)
        {
            frame.AddUniformLoad(analysisCase, fx, fz);
        }

        /// <summary>
        /// Adds a new frame trapezoidal load.
        /// </summary>
        public void AddFrameTrapezoidalLoad(AnalysisCase analysisCase, Frame frame, double fxi, double fzi, double fxj, double fzj)
        {
            frame.AddTrapezoidalLoad(analysisCase, fxi, fzi, fxj, fzj);
        }

        /// <summary>
        /// Divides all frames so that all are shorter than the given length.
        /// </summary>
        public void DivideFrames(double maxFrameSize)
        {
            Frame[] oldFrames = Frames.ToArray();
            foreach (Frame f in oldFrames)
            {
                DivideFrame(f, maxFrameSize);
            }
        }

        /// <summary>
        /// Divides the given frame so that it is shorter than the given length.
        /// </summary>
        public void DivideFrame(Frame f, double maxFrameSize)
        {
            int n = (int)Math.Ceiling(f.Length / maxFrameSize);
            DivideFrame(f, n);
        }

        /// <summary>
        /// Divides the given frame into given number of pieces.
        /// </summary>
        public void DivideFrame(Frame f, int n)
        {
            Node n1 = f.NodeI;
            Node n2 = f.NodeJ;

            double xStep = (n2.X - n1.X) / (double)n;
            double zStep = (n2.Z - n1.Z) / (double)n;

            Node[] nodes = new Node[n + 1];
            nodes[0] = n1;
            nodes[nodes.Length - 1] = n2;

            double x = n1.X + xStep;
            double z = n1.Z + zStep;
            for (int i = 0; i < n - 1; i++)
            {
                nodes[i + 1] = AddNode(x, z);
                x += xStep;
                z += zStep;
            }

            Frames.Remove(f);
            for (int i = 0; i < n; i++)
            {
                Frame newFrame = AddFrame(f.Section, nodes[i], nodes[i + 1]);
                // Transfer loads
                // Uniform loads
                foreach (FrameLoad masterLoad in f.Loads.FindAll((e) => e is FrameUniformLoad))
                {
                    FrameUniformLoad masterUniformLoad = masterLoad as FrameUniformLoad;
                    newFrame.AddUniformLoad(masterUniformLoad.AnalysisCase, masterUniformLoad.FX, masterUniformLoad.FZ);
                }
                // Trapezoidal loads
                foreach (FrameLoad masterLoad in f.Loads.FindAll((e) => e is FrameTrapezoidalLoad))
                {
                    FrameTrapezoidalLoad masterTrapezoidalLoad = masterLoad as FrameTrapezoidalLoad;
                    double mxi = masterTrapezoidalLoad.FXI;
                    double mzi = masterTrapezoidalLoad.FZI;
                    double mxj = masterTrapezoidalLoad.FXJ;
                    double mzj = masterTrapezoidalLoad.FZJ;
                    double xTotal = f.Length;
                    double xStart = newFrame.NodeI.DistanceTo(f.NodeI);
                    double xEnd = newFrame.NodeJ.DistanceTo(f.NodeI);
                    double fxi = xStart / xTotal * (mxj - mxi) + mxi;
                    double fzi = xStart / xTotal * (mzj - mzi) + mzi;
                    double fxj = xEnd / xTotal * (mxj - mxi) + mxi;
                    double fzj = xEnd / xTotal * (mzj - mzi) + mzi;
                    newFrame.AddTrapezoidalLoad(masterTrapezoidalLoad.AnalysisCase, fxi, fzi, fxj, fzj);
                }
            }
        }

        /// <summary>
        /// Merges all nodes so that no two nodes are within the given tolerance.
        /// </summary>
        public void MergeNodes(double mergeTolerance)
        {
            Dictionary<Node, List<Node>> toMerge = new Dictionary<Node, List<Node>>();
            Dictionary<Node, Node> toRemove = new Dictionary<Node, Node>();

            foreach (Node n1 in Nodes)
            {
                if (toMerge.ContainsKey(n1)) continue;
                if (toRemove.ContainsKey(n1)) continue;

                foreach (Node n2 in Nodes)
                {
                    if (ReferenceEquals(n1, n2)) continue;
                    if (toMerge.ContainsKey(n1)) continue;
                    if (toRemove.ContainsKey(n1)) continue;

                    if (Node.Distance(n1, n2) < mergeTolerance)
                    {
                        List<Node> removeList;
                        if (toMerge.TryGetValue(n1, out removeList))
                        {
                            removeList.Add(n2);
                        }
                        else
                        {
                            toMerge[n1] = new List<Node>() { n2 };
                        }
                        toRemove[n2] = n1;
                    }
                }
            }

            // Arrange frame nodes
            foreach (Frame f in Frames)
            {
                Node n1;
                Node n2;

                if (toRemove.TryGetValue(f.NodeI, out n1))
                {
                    f.NodeI = n1;
                }
                if (toRemove.TryGetValue(f.NodeJ, out n2))
                {
                    f.NodeJ = n2;
                }
            }

            // Assemble node loads
            foreach (KeyValuePair<Node, List<Node>> pair in toMerge)
            {
                Node master = pair.Key;
                List<Node> removed = pair.Value;

                foreach (AnalysisCase analysisCase in AnalysisCases)
                {
                    // Assemble point loads
                    NodePointLoad assembledPointLoad = new NodePointLoad(analysisCase, 0, 0, 0);
                    foreach (NodeLoad masterLoad in master.Loads.FindAll((e) => e.AnalysisCase == analysisCase && e is NodePointLoad))
                    {
                        NodePointLoad masterPointLoad = masterLoad as NodePointLoad;
                        assembledPointLoad.FX += masterPointLoad.FX;
                        assembledPointLoad.FZ += masterPointLoad.FZ;
                        assembledPointLoad.MY += masterPointLoad.MY;
                    }

                    foreach (Node slave in removed)
                    {
                        foreach (NodeLoad slaveLoad in slave.Loads.FindAll((e) => e.AnalysisCase == analysisCase && e is NodePointLoad))
                        {
                            NodePointLoad slavePointLoad = slaveLoad as NodePointLoad;
                            assembledPointLoad.FX += slavePointLoad.FX;
                            assembledPointLoad.FZ += slavePointLoad.FZ;
                            assembledPointLoad.MY += slavePointLoad.MY;
                        }
                    }

                    master.Loads.RemoveAll((e) => e.AnalysisCase == analysisCase && e is NodePointLoad);
                    if (!MathNet.Numerics.Precision.AlmostEqual(assembledPointLoad.FX + assembledPointLoad.FZ + assembledPointLoad.MY, 0))
                    {
                        master.AddPointLoad(analysisCase, assembledPointLoad.FX, assembledPointLoad.FZ, assembledPointLoad.MY);
                    }
                }
            }

            // Remove nodes
            foreach (Node n in toRemove.Keys)
            {
                Nodes.Remove(n);
            }
        }

        /// <summary>
        /// Sorts all nodes according to their coordinates.
        /// </summary>
        public void SortNodes()
        {
            Nodes.Sort((a, b) =>
            {
                return Math.Abs(a.Z - b.Z) < float.Epsilon ? (a.X < b.X ? -1 : 1) : (a.Z < b.Z ? -1 : 1);
            });

            AssignNodeIndices();
        }

        /// <summary>
        /// Sorts all frames according to their coordinates.
        /// </summary>
        public void SortFrames()
        {
            Frames.Sort((f1, f2) =>
            {
                Node a = f1.Centroid;
                Node b = f2.Centroid;
                return Math.Abs(a.Z - b.Z) < float.Epsilon ? (a.X < b.X ? -1 : 1) : (a.Z < b.Z ? -1 : 1);
            });

            AssignFrameIndices();
        }

        /// <summary>
        /// Sorts all springs according to their coordinates.
        /// </summary>
        public void SortSprings()
        {
            Springs.Sort((f1, f2) =>
            {
                Node a = f1.Centroid;
                Node b = f2.Centroid;
                return Math.Abs(a.Z - b.Z) < float.Epsilon ? (a.X < b.X ? -1 : 1) : (a.Z < b.Z ? -1 : 1);
            });

            AssignSpringIndices();
        }
    }
}
