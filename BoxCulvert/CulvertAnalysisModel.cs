using System;
using System.Collections.Generic;
using FrameSolver;

namespace BoxCulvert
{
    public class CulvertAnalysisModel : AnalysisModel
    {
        private Section mParent;

        public CulvertAnalysisModel(Section parent)
        {
            mParent = parent;
        }

        public void UpdateModel()
        {
            ResetModel();

            // Dimensions
            float wi = mParent.SectionProperties.InnerWidth;
            float hi = mParent.SectionProperties.InnerHeight;
            float tw = mParent.SectionProperties.OuterWallThickness;
            float tf = mParent.SectionProperties.FoundationThickness;
            float ts = mParent.SectionProperties.SlabThickness;
            int iw = mParent.SectionProperties.InnerWalls;
            float twi = mParent.SectionProperties.InnerWallThickness;
            bool gusset = mParent.SectionProperties.HasSlabGussets;
            float gw = mParent.SectionProperties.SlabGussetWidth;
            float gh = mParent.SectionProperties.SlabGussetHeight;
            float wo = mParent.SectionProperties.OuterWidth - tw;
            float ho = hi + tf / 2 + ts / 2;

            // Concrete material
            Material mat = AddMaterial("Concrete", 3000000, 0.2, 25);

            // Sections
            FrameSection outerWallSection = AddFrameSection("Outer Wall", mat, 1.0 * tw, 1.0 * tw * tw * tw / 12.0);
            FrameSection innerWallSection = AddFrameSection("Inner Wall", mat, 1.0 * twi, 1.0 * twi * twi * twi / 12.0);
            FrameSection slabSection = AddFrameSection("Slab", mat, 1.0 * ts, 1.0 * ts * ts * ts / 12.0);
            FrameSection foundationSection = AddFrameSection("Foundation", mat, 1.0 * tf, 1.0 * tf * tf * tf / 12.0);

            // Build model
            Node n1;
            Node n2;
            Frame f;

            double dx;

            // Foundation slab
            n1 = AddNode(-wo / 2, 0);
            n2 = AddNode(-wo / 2 + tw / 2, 0);
            Node lowerLeft = n1;
            f = AddFrame(foundationSection, n1, n2);
            dx = -wo / 2 + tw / 2 + wi;
            for (int i = 0; i < iw + 1; i++)
            {
                n1 = n2;
                n2 = AddNode(dx, 0);
                f = AddFrame(foundationSection, n1, n2);
                if (i < iw)
                {
                    n1 = n2;
                    n2 = AddNode(dx + twi / 2, 0);
                    f = AddFrame(foundationSection, n1, n2);
                    n1 = n2;
                    n2 = AddNode(dx + twi, 0);
                    f = AddFrame(foundationSection, n1, n2);
                }
                dx += wi + twi;
            }
            n1 = n2;
            n2 = AddNode(wo / 2, 0);
            f = AddFrame(foundationSection, n1, n2);
            Node lowerRight = n2;
            // Create a mid point (for support) if it does not exist
            if (Nodes.Count % 2 == 0)
            {
                DivideFrame(Frames[(Frames.Count - 1) / 2], 2);
            }
            SortNodes();
            Node lowerMid = Nodes[(Nodes.Count - 1) / 2];

            // Top slab
            n1 = AddNode(-wo / 2, ho);
            n2 = AddNode(-wo / 2 + tw / 2, ho);
            f = AddFrame(slabSection, n1, n2);
            dx = -wo / 2 + tw / 2 + wi;
            for (int i = 0; i < iw + 1; i++)
            {
                n1 = n2;
                n2 = AddNode(dx, ho);
                f = AddFrame(slabSection, n1, n2);
                if (i < iw)
                {
                    n1 = n2;
                    n2 = AddNode(dx + twi / 2, ho);
                    f = AddFrame(slabSection, n1, n2);
                    n1 = n2;
                    n2 = AddNode(dx + twi, ho);
                    f = AddFrame(slabSection, n1, n2);
                }
                dx += wi + twi;
            }
            n1 = n2;
            n2 = AddNode(wo / 2, ho);
            f = AddFrame(slabSection, n1, n2);
            Node rackingPoint = n2;

            // Left wall
            n1 = AddNode(-wo / 2, 0);
            n2 = AddNode(-wo / 2, tf / 2);
            f = AddFrame(outerWallSection, n1, n2);
            n1 = n2;
            n2 = AddNode(-wo / 2, tf / 2 + hi);
            f = AddFrame(outerWallSection, n1, n2);
            n1 = n2;
            n2 = AddNode(-wo / 2, ho);
            f = AddFrame(outerWallSection, n1, n2);

            // Right wall
            n1 = AddNode(wo / 2, 0);
            n2 = AddNode(wo / 2, tf / 2);
            f = AddFrame(outerWallSection, n1, n2);
            n1 = n2;
            n2 = AddNode(wo / 2, tf / 2 + hi);
            f = AddFrame(outerWallSection, n1, n2);
            n1 = n2;
            n2 = AddNode(wo / 2, ho);
            f = AddFrame(outerWallSection, n1, n2);

            // Inner walls
            dx = -wo / 2 + tw / 2 + wi + twi / 2;
            for (int i = 0; i < iw; i++)
            {
                n1 = AddNode(dx, 0);
                n2 = AddNode(dx, tf / 2);
                f = AddFrame(innerWallSection, n1, n2);
                n1 = n2;
                n2 = AddNode(dx, tf / 2 + hi);
                f = AddFrame(innerWallSection, n1, n2);
                n1 = n2;
                n2 = AddNode(dx, ho);
                f = AddFrame(innerWallSection, n1, n2);
                dx += wi + twi;
            }

            // Divide frames
            DivideFrames(0.5);

            // Springs
            double springSize = 0.1;
            Dictionary<Node, double> slabNodeDict = new Dictionary<Node, double>();
            foreach (Frame frame in Frames.FindAll((e) => e.Section == foundationSection))
            {
                double springCoefficient = frame.Length / 2.0 * mParent.SoilParameters.BeddingCoefficient;

                if (slabNodeDict.ContainsKey(frame.NodeI))
                    slabNodeDict[frame.NodeI] += springCoefficient;
                else
                    slabNodeDict.Add(frame.NodeI, springCoefficient);

                if (slabNodeDict.ContainsKey(frame.NodeJ))
                    slabNodeDict[frame.NodeJ] += springCoefficient;
                else
                    slabNodeDict.Add(frame.NodeJ, springCoefficient);
            }
            foreach (KeyValuePair<Node, double> pair in slabNodeDict)
            {
                double springCoefficient = pair.Value;

                Node upperNode = pair.Key;
                Node lowerNode = AddNode(upperNode.X, upperNode.Z - springSize);
                lowerNode.Restraints = DOF.Fixed;

                AddSpring(springCoefficient, springCoefficient, 0, lowerNode, upperNode);
            }

            // Clean up close nodes
            MergeNodes(0.01);

            // Sort according to coordinates
            SortNodes();
            SortFrames();
            SortSprings();

            // Analysis cases
            AnalysisCase deadLoad = AddAnalysisCase("Self Weight");
            AnalysisCase fillLoad = AddAnalysisCase("Earth Fill");
            AnalysisCase pressureLoad = AddAnalysisCase("Soil Pressure");
            AnalysisCase surchargeLoad = AddAnalysisCase("Surcharge");
            AnalysisCase racking1 = AddAnalysisCase("Racking (+)");
            AnalysisCase racking2 = AddAnalysisCase("Racking (-)");

            // Loads
            // Self weight
            foreach (Frame frame in Frames)
            {
                AddFrameSelfWeight(deadLoad, frame);
            }
            // Fill load
            foreach (Frame frame in Frames.FindAll((e) => e.Section == slabSection))
            {
                AddFrameUniformLoad(fillLoad, frame, 0, -mParent.SoilParameters.FillLoad);
            }
            // Soil pressure
            foreach (Frame frame in Frames.FindAll((e) => e.Section == outerWallSection))
            {
                double zi = frame.NodeI.Z;
                double zj = frame.NodeJ.Z;

                double pmin = mParent.SoilParameters.SoilPressureTop;
                double pmax = mParent.SoilParameters.SoilPressureBottom;
                double pi = (ho - zi) / ho * (pmax - pmin) + pmin;
                double pj = (ho - zj) / ho * (pmax - pmin) + pmin;

                if (frame.NodeI.X < 0)
                    AddFrameTrapezoidalLoad(pressureLoad, frame, pi, 0, pj, 0);
                else
                    AddFrameTrapezoidalLoad(pressureLoad, frame, -pi, 0, -pj, 0);
            }
            // Surcharge
            foreach (Frame frame in Frames.FindAll((e) => e.Section == outerWallSection))
            {
                if (frame.NodeI.X < 0)
                    AddFrameUniformLoad(surchargeLoad, frame, mParent.SoilParameters.SurchargeLoad, 0);
                else
                    AddFrameUniformLoad(surchargeLoad, frame, -mParent.SoilParameters.SurchargeLoad, 0);
            }

            // Solve racking
            // Temporary supports
            lowerLeft.Restraints = DOF.Pinned;
            lowerRight.Restraints = DOF.Pinned;
            // Solve under unit load
            AddNodePointLoad(racking1, rackingPoint, 1, 0, 0);
            BuildStiffnessMatrix();
            Run(racking1);
            // Calculate actual load to create racking deformation
            double deformationForUnitLoad = racking1.NodeDeformations[rackingPoint.Index].UX;
            double rackingPointLoad = mParent.SoilParameters.FreeFieldDeformation / deformationForUnitLoad;
            rackingPoint.Loads.Clear();
            AddNodePointLoad(racking1, rackingPoint, rackingPointLoad, 0, 0);
            AddNodePointLoad(racking2, rackingPoint, -rackingPointLoad, 0, 0);
            // Run racking
            Run(racking1);
            Run(racking2);
            // Remove temporary supports
            lowerLeft.Restraints = DOF.Free;
            lowerRight.Restraints = DOF.Free;

            // Solve service loads
            BuildStiffnessMatrix();
            Run(deadLoad);
            Run(fillLoad);
            Run(pressureLoad);
            Run(surchargeLoad);

            // Combinations
            // Service
            Combination service = AddLinearCombination("Service");
            service.SetCoefficient(1, deadLoad);
            service.SetCoefficient(1, fillLoad);
            service.SetCoefficient(1, pressureLoad);
            service.SetCoefficient(1, surchargeLoad);
            service.Update();
            // Factored 1
            Combination factored1 = AddLinearCombination("Factored 1");
            factored1.SetCoefficient(1.3, deadLoad);
            factored1.SetCoefficient(1.3, fillLoad);
            factored1.SetCoefficient(1.3, pressureLoad);
            factored1.SetCoefficient(1.3, surchargeLoad);
            factored1.Update();
            // Factored 2
            Combination factored2 = AddLinearCombination("Factored 2");
            factored2.SetCoefficient(1.3, deadLoad);
            factored2.SetCoefficient(1.3, fillLoad);
            factored2.SetCoefficient(0.65, pressureLoad);
            factored2.SetCoefficient(0.65, surchargeLoad);
            factored2.Update();
            // Racking
            Combination racking = AddEnvelopeCombination("Racking");
            racking.SetCoefficient(1, racking1);
            racking.SetCoefficient(1, racking2);
            racking.Update();
            // Seismic
            Combination seismic = AddLinearCombination("Seismic");
            seismic.SetCoefficient(1, deadLoad);
            seismic.SetCoefficient(1, fillLoad);
            seismic.SetCoefficient(1, pressureLoad);
            seismic.SetCoefficient(1, racking);
            seismic.Update();
            // Envelope
            Combination envelope = AddEnvelopeCombination("Envelope");
            envelope.SetCoefficient(1, service);
            envelope.SetCoefficient(1, factored1);
            envelope.SetCoefficient(1, factored2);
            envelope.SetCoefficient(1, seismic);
            envelope.Update();
        }
    }
}
