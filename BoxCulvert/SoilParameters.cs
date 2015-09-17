using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace BoxCulvert
{
    public class SoilParameters
    {
        private Section mParent;

        private float mFillHeight;
        private float mSoilUnitWeight;
        private float mElasticityModulus;
        private float mInternalFrictionAngle;
        private float mPoissonsRatio;
        private float mSurcharge;
        private float mBeddingCoefficient;
        private bool mPerformRackingAnalysis;
        private float mPGA;

        [Category("Applied Loads")]
        public float FillHeight { get { return mFillHeight; } set { mFillHeight = value; IsDirty = true; } }
        [Category("Applied Loads")]
        public float Surcharge { get { return mSurcharge; } set { mSurcharge = value; IsDirty = true; } }

        [Category("Soil Parameters")]
        public float SoilUnitWeight { get { return mSoilUnitWeight; } set { mSoilUnitWeight = value; IsDirty = true; } }
        [Category("Soil Parameters")]
        public float InternalFrictionAngle { get { return mInternalFrictionAngle; } set { mInternalFrictionAngle = value; IsDirty = true; } }

        [Category("Soil Parameters")]
        public float ShearModulus { get { return mElasticityModulus / (2 * (1 + mPoissonsRatio)); } }
        [Category("Soil Parameters")]
        public float ElasticityModulus { get { return mElasticityModulus; } set { mElasticityModulus = value; IsDirty = true; } }
        [Category("Soil Parameters")]
        public float PoissonsRatio { get { return mPoissonsRatio; } set { mPoissonsRatio = value; IsDirty = true; } }
        [Category("Soil Parameters")]
        public float BeddingCoefficient { get { return mBeddingCoefficient; } set { mBeddingCoefficient = value; IsDirty = true; } }

        [Browsable(false)]
        public bool IsDirty
        {
            get
            {
                return mParent.IsDirty;
            }
            internal set
            {
                mParent.IsDirty = value;
            }
        }

        [Category("Loads")]
        public float FillLoad { get { return FillHeight * SoilUnitWeight; } }
        [Category("Soil Parameters")]
        public float PressureCoefficient { get { return 1.0f - (float)Math.Sin(InternalFrictionAngle * Math.PI / 180.0); } }
        [Category("Loads")]
        public float SoilPressureTop { get { return FillHeight * SoilUnitWeight * PressureCoefficient; } }
        [Category("Loads")]
        public float SoilPressureBottom { get { return (FillHeight + mParent.SectionProperties.OuterHeight) * SoilUnitWeight * PressureCoefficient; } }
        [Category("Loads")]
        public float SurchargeLoad { get { return Surcharge * PressureCoefficient; } }

        [Category("Racking Analysis")]
        public bool PerformRackingAnalysis { get { return mPerformRackingAnalysis; } set { mPerformRackingAnalysis = value; IsDirty = true; } }
        [Category("Racking Analysis")]
        public float PGA { get { return mPGA; } set { mPGA = value; IsDirty = true; } }
        [Category("Racking Analysis")]
        public float DepthToInvertLevel { get { return FillHeight + mParent.SectionProperties.OuterHeight; } }
        [Category("Racking Analysis")]
        public float StressAtInvertLevel { get { return DepthToInvertLevel + SoilUnitWeight; } }
        [Category("Racking Analysis")]
        public float StressReductionFactor
        {
            get
            {
                // In feet
                // Rd = 1.0 - 0.00233z for z < 30 ft
                // Rd = 1.174 - 0.00814z for 30 ft < z < 75 ft
                // Rd = 0.744 - 0.00244z for 75 ft < z < 100 ft
                // Rd = 0.5 for z > 100 ft
                float z = DepthToInvertLevel / 0.3048f;
                if (z < 30)
                    return 1.0f - 0.00233f * z;
                else if (z < 75)
                    return 1.174f - 0.00814f * z;
                else if (z < 100)
                    return 0.744f - 0.00244f * z;
                else
                    return 0.5f;
            }
        }
        [Category("Racking Analysis")]
        public float MaximumShearStress
        {
            get
            {
                // Tmax = (PGA/g) * Sv * Rd
                return PGA * StressAtInvertLevel * StressReductionFactor;
            }
        }
        [Category("Racking Analysis")]
        public float MaximumShearStrain
        {
            get
            {
                // gmax = Tmax / G
                return MaximumShearStress / ShearModulus;
            }
        }
        [Category("Racking Analysis")]
        public float FreeFieldDeformation
        {
            get
            {
                // d = gmax * H
                return MaximumShearStrain * mParent.SectionProperties.OuterHeight;
            }
        }

        public SoilParameters(Section parent)
        {
            mParent = parent;

            mFillHeight = 3.0f;
            mSoilUnitWeight = 20.0f;
            mInternalFrictionAngle = 30.0f;
            mSurcharge = 12.0f;
            mBeddingCoefficient = 10000.0f;
            mElasticityModulus = 30000.0f;
            mPoissonsRatio = 0.3f;
            mPerformRackingAnalysis = true;
            mPGA = 0.4f;
        }

        public static SoilParameters FromStream(Section parent, BinaryReader r)
        {
            SoilParameters s = new SoilParameters(parent);

            s.FillHeight = r.ReadSingle();
            s.SoilUnitWeight = r.ReadSingle();
            s.InternalFrictionAngle = r.ReadSingle();
            s.Surcharge = r.ReadSingle();
            s.BeddingCoefficient = r.ReadSingle();
            s.ElasticityModulus = r.ReadSingle();
            s.PoissonsRatio = r.ReadSingle();
            s.PerformRackingAnalysis = r.ReadBoolean();
            s.PGA = r.ReadSingle();

            return s;
        }

        public void SaveFile(BinaryWriter w)
        {
            w.Write(FillHeight);
            w.Write(SoilUnitWeight);
            w.Write(InternalFrictionAngle);
            w.Write(Surcharge);
            w.Write(BeddingCoefficient);
            w.Write(ElasticityModulus);
            w.Write(PoissonsRatio);
            w.Write(PerformRackingAnalysis);
            w.Write(PGA);
        }

        public SoilParameters Clone()
        {
            SoilParameters s = new SoilParameters(mParent);

            s.FillHeight = FillHeight;
            s.SoilUnitWeight = SoilUnitWeight;
            s.InternalFrictionAngle = InternalFrictionAngle;
            s.Surcharge = Surcharge;
            s.BeddingCoefficient = BeddingCoefficient;
            s.ElasticityModulus = ElasticityModulus;
            s.PoissonsRatio = PoissonsRatio;
            s.PerformRackingAnalysis = PerformRackingAnalysis;
            s.PGA = PGA;

            return s;
        }
    }
}
