using System;
using System.Collections.Generic;
using System.Text;
using TransformerOptimizer.Components.Base;
using static TransformerOptimizer.Data.Constants;
using static TransformerOptimizer.Functions.Functions;
using static TransformerOptimizer.Data.LoadedData.Prices;

namespace TransformerOptimizer.Components.Derived
{
    /// <summary>
    /// Contains information related to a foil used in windings in place of wire.
    /// 
    /// Derived from <see cref="Wire"/>.
    /// </summary>
    [Serializable]
    public class Foil : Wire
    {
        private readonly double requiredCSA;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="requiredCSA">Required cross sectional area of the foil to meet for current density.</param>
        /// <param name="wireMaterial">What material the foil will be made of.</param>
        protected internal Foil(double requiredCSA, WireMaterial wireMaterial)
        {
            this.Name = "Foil";
            this.requiredCSA = requiredCSA;
            this.WireShape = WireShape.FOIL;
            this.WireMaterial = wireMaterial;
        }
        /// <summary>
        /// Copy Constructor.
        /// 
        /// Copies the required cross sectional area for the foil and the foil material.
        /// </summary>
        /// <param name="foil">Foil to copy from.</param>
        protected internal Foil(Foil foil)
        {
            this.Name = foil.Name;
            this.requiredCSA = foil.requiredCSA;
            this.WireShape = WireShape.FOIL;
            this.WireMaterial = foil.WireMaterial;
        }

        /// <summary>
        /// In foil windings the bifilar is always set to <see cref="Bifilar._XH1W"/>.
        /// See <see cref="BifilarNumber"/> for the number of foils to be used.
        /// </summary>
        public override Bifilar Bifilar { get { return Bifilar._XH1W; } }
        /// <summary>
        /// Returns the number of foils needed to get the required cross sectional area.
        /// </summary>
        public int BifilarNumber { get { return (int)Math.Ceiling(requiredCSA / (ConductorWidth * ConductorThickness)); } }
        /// <summary>
        /// In foil windings the width of the foil is equal to the winding length available.
        /// </summary>
        public override double ConductorWidth { get { return Section.WindingLength; } }
        /// <summary>
        /// In foil windings the skew factor is 0.
        /// </summary>
        public override int SkewFactor { get { return 0; } }
        /// <summary>
        /// In foil windings the base thickness of a foil is always 0.005.
        /// </summary>
        public override double ConductorThickness { get { return 0.005; } }
        /// <summary>
        /// In foil windings the insulation is 0.
        /// </summary>
        public override double InsulationThickness { get { return 0; } }
        /// <summary>
        /// Total thickness of the foil layer.
        /// </summary>
        public override double TotalThickness { get { return BifilarNumber * ConductorThickness; } }
        /// <summary>
        /// Returns the resistance of the foil in Ohms.
        /// </summary>
        public override double ResistancePer1000Inches { get { return (WireMaterial == WireMaterial.COPPER ? 0.017 : 0.028) * InchToMeter(1000) / SqInchToSqMilimeter(CrossSectionalArea); } }
        /// <summary>
        /// Returns the weight if the foil in Lbs.
        /// </summary>
        public override double WeightPer1000Inches { get { return (WireMaterial == WireMaterial.COPPER ? 0.321 : 0.097) * 1000; } } // * CrossSectionalArea; } }
        /// <summary>
        /// Returns the cost of the foil in USD.
        /// </summary>
        public override double Cost { get { return (double)GetWireMaterialPrice(this.WireMaterial) + 2; } }

        /// <summary>
        /// Cross sectional area of the foil.
        /// </summary>
        public override double CrossSectionalArea { get { return TotalWidth * TotalThickness; } } //BifilarMultiplier(Bifilar, this) *  TotalWidth * TotalThickness; } }

        /// <summary>
        /// Section that the foil is attached to.
        /// </summary>
        protected internal Section Section { get; set; }
    }
}
