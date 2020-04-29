using System;
using System.Collections.Generic;
using System.Text;
using static TransformerOptimizer.Functions.Functions;
using static TransformerOptimizer.Data.Constants;
using static TransformerOptimizer.Data.LoadedData.Prices;

namespace TransformerOptimizer.Components.Base
{
    /// <summary>
    /// Contains information related to a wire that will be used in a winding.    
    /// </summary>
    [Serializable]
    public class Wire
    {
        /// <summary>
        /// Constructor. Required for purposed of derived classes. DO NOT USE!
        /// </summary>
        protected internal Wire() { }

        /// <summary>
        /// Constructor.
        /// 
        /// If <see cref="Bifilar"/> is <see cref="Data.Constants.Bifilar._1H1W"/> the cost of the wire is the sum of the fabrication <paramref name="cost"/> and the material price.
        /// If <see cref="Bifilar"/> is not <see cref="Data.Constants.Bifilar._1H1W"/> the cost of the wire will be the product of the cost and the <see cref="Functions.Functions.BifilarMultiplier(Bifilar, Wire)"/>.
        /// </summary>
        /// <param name="name">Name of the wire.</param>
        /// <param name="wireMaterial">Conductor material of the wire.</param>
        /// <param name="wireShape">Shape of the wire. </param>
        /// <param name="bifilar">Bifilar of the wire.</param>
        /// <param name="width">Conductor width of the wire.</param>
        /// <param name="thickness">Conductor material of the wire.</param>
        /// <param name="insThickness">Insulation thickness around the wire.</param>
        /// <param name="resistance">Resistance in Ohms of the wire per 1000 inches.</param>
        /// <param name="weight">Weight of the wire in Lbs. per 1000 inches.</param>
        /// <param name="cost">Fabrication cost of a single wire.</param>
        /// <param name="skew_factor">Skew factor of the wire, 1 for rectangular and round wire and 0 for foil.</param>
        protected internal Wire(string name, WireMaterial wireMaterial, WireShape wireShape, Bifilar bifilar, double width, double thickness, double insThickness, double resistance, double weight, double cost, int skew_factor)
        {
            this.Name = name;
            this.WireMaterial = wireMaterial;
            this.WireShape = wireShape;
            this.Bifilar = bifilar;
            this.ConductorWidth = width;
            this.ConductorThickness = thickness;
            this.InsulationThickness = insThickness;
            this.ResistancePer1000Inches = resistance / BifilarMultiplier(bifilar);
            this.WeightPer1000Inches = weight * BifilarMultiplier(bifilar);
            if (Bifilar != Bifilar._1H1W)
                this.Cost = cost;
            else
                this.Cost = (cost + (double)(wireMaterial == WireMaterial.ALUMINUM ? AluminumPrice : CopperPrice));// * BifilarMultiplier(bifilar);
            this.SkewFactor = skew_factor;
        }

        /// <summary>
        /// Name of the wire.
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// Conductor material of the wire.
        /// </summary>
        public WireMaterial WireMaterial { get; protected set; }
        /// <summary>
        /// Shape of the wire.
        /// </summary>
        public WireShape WireShape { get; protected set; }
        /// <summary>
        /// Bifilar of the wire.
        /// </summary>
        public virtual Bifilar Bifilar { get; }
        /// <summary>
        /// Width of the conductor material.
        /// 
        /// For round wires the width is changed to a value such that the cross sectional area becomes the empirical number received from manufacturers.
        /// </summary>
        public virtual double ConductorWidth { get; }
        /// <summary>
        /// Thickness of the conductor material.        
        /// </summary>
        public virtual double ConductorThickness { get; }
        /// <summary>
        /// Thickness of the insulation around the wire.
        /// </summary>
        public virtual double InsulationThickness { get; }
        /// <summary>
        /// Total width of the wire including insulation.        
        /// </summary>
        public double TotalWidth { get { return BifilarMultiplier(Bifilar) * (ConductorWidth + 2 * InsulationThickness); } }
        /// <summary>
        /// Total thickness of the wire including insulation.        
        /// </summary>
        public virtual double TotalThickness { get { return (ConductorThickness + 2 * InsulationThickness); } }
        /// <summary>
        /// Resistance of the wire in Ohms per 1000 inches.
        /// </summary>
        public virtual double ResistancePer1000Inches { get; }
        /// <summary>
        /// Weight of the wire in Lbs. per 1000 inches.
        /// </summary>
        public virtual double WeightPer1000Inches { get; }
        /// <summary>
        /// Cost of the wire.
        /// 
        /// Sum of the material base price and wire fabrication cost.
        /// </summary>
        public virtual double Cost { get; }
        /// <summary>
        /// Skew factor to subtract for the turns per layer calculation.
        /// </summary>
        public virtual int SkewFactor { get; }

        /// <summary>
        /// Returns the cross sectional area of the wire.
        /// 
        /// Rectangular wires have a corner radius that is removed in determining the area see <see cref="Functions.Functions.RemoveCornerRadius(Wire)"/>.
        /// </summary>
        public virtual double CrossSectionalArea { get { return WireShape == WireShape.ROUND ? Math.PI * Math.Pow(ConductorWidth / 2, 2.0) : this.RemoveCornerRadius(); } }
        /// <summary>
        /// Returns the area of the wire in circular mills.
        /// </summary>
        public double CircularMills { get { return 1273240 * CrossSectionalArea; } }

        /// <summary>
        /// Returns the factor used for change of resistance.
        /// </summary>
        public double TemperatureChangeFactor { get { return WireMaterial == WireMaterial.ALUMINUM ? 225 : 234.5; } }
    }
}
