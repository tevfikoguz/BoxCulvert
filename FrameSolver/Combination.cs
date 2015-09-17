using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameSolver
{
    public class Combination : SerializableItem
    {
        public string Name { get; set; }

        public bool IsEnvelope { get; set; }

        public bool HasEnvelopeValues { get; private set; }

        public CombinableList<NodeDeformation> MinNodeDeformations { get; private set; }
        public CombinableList<Reaction> MinReactions { get; private set; }
        public CombinableList<FrameInternalForce> MinFrameInternalForces { get; private set; }

        public CombinableList<NodeDeformation> MaxNodeDeformations { get; private set; }
        public CombinableList<Reaction> MaxReactions { get; private set; }
        public CombinableList<FrameInternalForce> MaxFrameInternalForces { get; private set; }

        private Dictionary<AnalysisCase, double> acCoefficients;
        private Dictionary<Combination, double> coCoefficients;

        public Combination(string name, bool isEnvelope)
        {
            Name = name;
            IsEnvelope = isEnvelope;

            acCoefficients = new Dictionary<AnalysisCase, double>();
            coCoefficients = new Dictionary<Combination, double>();

            MinNodeDeformations = new CombinableList<NodeDeformation>();
            MinReactions = new CombinableList<Reaction>();
            MinFrameInternalForces = new CombinableList<FrameInternalForce>();

            MaxNodeDeformations = new CombinableList<NodeDeformation>();
            MaxReactions = new CombinableList<Reaction>();
            MaxFrameInternalForces = new CombinableList<FrameInternalForce>();
        }

        public Combination(string name)
            : this(name, false)
        {
            ;
        }

        public void SetCoefficient(double coefficient, AnalysisCase analysisCase)
        {
            if (acCoefficients.ContainsKey(analysisCase))
                acCoefficients[analysisCase] = coefficient;
            else
                acCoefficients.Add(analysisCase, coefficient);
        }

        public void SetCoefficient(double coefficient, Combination combination)
        {
            if (coCoefficients.ContainsKey(combination))
                coCoefficients[combination] = coefficient;
            else
                coCoefficients.Add(combination, coefficient);
        }

        public void Update()
        {
            MinNodeDeformations.Clear();
            MinReactions.Clear();
            MinFrameInternalForces.Clear();

            MaxNodeDeformations.Clear();
            MaxReactions.Clear();
            MaxFrameInternalForces.Clear();

            if (IsEnvelope)
            {
                HasEnvelopeValues = true;
            }

            foreach (KeyValuePair<AnalysisCase, double> pair in acCoefficients)
            {
                AnalysisCase analysisCase = pair.Key;
                double coefficient = pair.Value;
                if (IsEnvelope)
                {
                    MinNodeDeformations = MinNodeDeformations.Min(analysisCase.NodeDeformations.Multiply(coefficient));
                    MinReactions = MinReactions.Min(analysisCase.Reactions.Multiply(coefficient));
                    MinFrameInternalForces = MinFrameInternalForces.Min(analysisCase.FrameInternalForces.Multiply(coefficient));

                    MaxNodeDeformations = MaxNodeDeformations.Max(analysisCase.NodeDeformations.Multiply(coefficient));
                    MaxReactions = MaxReactions.Max(analysisCase.Reactions.Multiply(coefficient));
                    MaxFrameInternalForces = MaxFrameInternalForces.Max(analysisCase.FrameInternalForces.Multiply(coefficient));
                }
                else
                {
                    MinNodeDeformations = MinNodeDeformations.MultiplyAndAdd(coefficient, analysisCase.NodeDeformations);
                    MinReactions = MinReactions.MultiplyAndAdd(coefficient, analysisCase.Reactions);
                    MinFrameInternalForces = MinFrameInternalForces.MultiplyAndAdd(coefficient, analysisCase.FrameInternalForces);

                    MaxNodeDeformations = MaxNodeDeformations.MultiplyAndAdd(coefficient, analysisCase.NodeDeformations);
                    MaxReactions = MaxReactions.MultiplyAndAdd(coefficient, analysisCase.Reactions);
                    MaxFrameInternalForces = MaxFrameInternalForces.MultiplyAndAdd(coefficient, analysisCase.FrameInternalForces);
                }
            }

            foreach (KeyValuePair<Combination, double> pair in coCoefficients)
            {
                Combination combination = pair.Key;
                double coefficient = pair.Value;
                combination.Update();
                if (IsEnvelope || combination.HasEnvelopeValues)
                {
                    HasEnvelopeValues = true;

                    MinNodeDeformations = MinNodeDeformations.Min(combination.MinNodeDeformations.Multiply(coefficient));
                    MinReactions = MinReactions.Min(combination.MinReactions.Multiply(coefficient));
                    MinFrameInternalForces = MinFrameInternalForces.Min(combination.MinFrameInternalForces.Multiply(coefficient));

                    MaxNodeDeformations = MaxNodeDeformations.Max(combination.MaxNodeDeformations.Multiply(coefficient));
                    MaxReactions = MaxReactions.Max(combination.MaxReactions.Multiply(coefficient));
                    MaxFrameInternalForces = MaxFrameInternalForces.Max(combination.MaxFrameInternalForces.Multiply(coefficient));
                }
                else
                {
                    MinNodeDeformations = MinNodeDeformations.MultiplyAndAdd(coefficient, combination.MinNodeDeformations);
                    MinReactions = MinReactions.MultiplyAndAdd(coefficient, combination.MinReactions);
                    MinFrameInternalForces = MinFrameInternalForces.MultiplyAndAdd(coefficient, combination.MinFrameInternalForces);

                    MaxNodeDeformations = MaxNodeDeformations.MultiplyAndAdd(coefficient, combination.MaxNodeDeformations);
                    MaxReactions = MaxReactions.MultiplyAndAdd(coefficient, combination.MaxReactions);
                    MaxFrameInternalForces = MaxFrameInternalForces.MultiplyAndAdd(coefficient, combination.MaxFrameInternalForces);
                }
            }
        }
    }
}
