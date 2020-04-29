using System;
using System.Collections.Generic;
using System.Text;

namespace TransformerOptimizer.Components.Base
{
    /// <summary>
    /// Contains information related to a core that will be used in a design.
    /// </summary>
    [Serializable]
    public class Core
    {
        /// <summary>
        /// Constructor.
        /// 
        /// Creates a core using the given lamination and stack to be used in a design.
        /// </summary>
        /// <param name="lamination">What lamination will be used to make the core.</param>
        /// <param name="stack">Core stack.</param>
        /// <param name="fluxDensity">What flux density the core should operate at.</param>
        /// <param name="tubeWindowMargin">Length between top/bottom of the window and the top/bottom of the tube.</param>
        /// <param name="stackingFactor">Ratio of <see cref="Lamination.Thickness"/> stack to <see cref="Stack"/>.</param>
        /// <param name="desctructionFactor">Destruction Factor of the lamination.</param>
        /// <param name="excitationFactor">Excitation Factor of the core.</param>
        protected internal Core(Lamination lamination, double stack, double fluxDensity, double tubeWindowMargin, double stackingFactor, double desctructionFactor, double excitationFactor)
        {
            this.Lamination = lamination;
            this.Stack = stack;
            this.FluxDensity = fluxDensity;
            this.TubeWindowMargin = tubeWindowMargin;
            this.StackingFactor = stackingFactor;
            this.DestructionFactor = desctructionFactor;
            this.ExcitationFactor = excitationFactor;
        }

        /// <summary>
        /// What lamination the core is made of.
        /// </summary>
        public Lamination Lamination { get; }
        /// <summary>
        /// Stack of the core.
        /// </summary>
        public double Stack { get; }
        /// <summary>
        /// What flux density the core operates at.
        /// </summary>
        public double FluxDensity { get; }
        /// <summary>
        /// Length between top/bottom of the window and the top/bottom of the tube.
        /// </summary>
        public double TubeWindowMargin { get; }
        /// <summary>
        /// Core stacking factor, ratio of <see cref="Lamination.Thickness"/> stack to <see cref="Stack"/>. Between 0 and 1.
        /// 
        /// Example: For a 0.96 stacking factor, 96 pieces of 1 inch laminations stacked would be equivelent to a 100 inch core stack.
        /// </summary>
        public double StackingFactor { get; }
        /// <summary>
        /// Destruction Factor of the lamination.
        /// </summary>
        public double DestructionFactor { get; }
        /// <summary>
        /// Excitation Factor of the core.
        /// </summary>
        public double ExcitationFactor { get; }

        /// <summary>
        /// Name of the core, made up of the lamination name and the core stack.
        /// </summary>
        public string Name { get { return Lamination.Name + 'x' + Stack.ToString(); } }
        /// <summary>
        /// Number of laminations that are stacked together to form the core.
        /// </summary>
        public int NumberOfLaminations { get { return (int)Math.Ceiling(Math.Ceiling(Stack / Lamination.Thickness)); } }
        /// <summary>
        /// The depth of the core, same as <see cref="Stack"/>.
        /// </summary>
        public double Depth { get { return Stack; } }
        /// <summary>
        /// Cross sectional area of the core used to determine flux in square inches.
        /// </summary>
        public double CrossSectionalAreaSqInch { get { return Lamination.Tongue * Stack * StackingFactor; } }
        /// <summary>
        /// Cross sectional area of the core used to determine flux in square meters.
        /// </summary>
        public double CrossSectionalAreaSqMeter { get { return ((CrossSectionalAreaSqInch * Math.Pow(25.4, 2.0)) / Math.Pow(1000, 2)); } }
        /// <summary>
        /// Weight of the core.
        /// </summary>
        public double Weight { get { return NumberOfLaminations * Lamination.Weight; } }
        /// <summary>
        /// Weight of the legs of the core.
        /// </summary>
        public double WeightLegs { get { return Weight * (Lamination.Phase == Data.Constants.Phase.SINGLE ? 1 : 0.4285714286); } }
        /// <summary>
        /// Number of coils that will be fitted onto the core in a complete design.
        /// </summary>
        public double NumberOfCoils
        {
            get
            {
                if (Lamination.Shape == Data.Constants.CoreShape.EI)
                    return Lamination.Phase == Data.Constants.Phase.SINGLE ? 1 : 3;
                else if (Lamination.Shape == Data.Constants.CoreShape.UI)
                    return 2;
                return 1;
            }
        }

        /// <summary>
        /// Cost of the core.
        /// </summary>
        public double Cost { get { return Lamination.Surcharge > 0 ? Weight * Lamination.Cost + Weight * Lamination.ScrapFactor * Lamination.Surcharge / 2000 : Weight * Lamination.Cost; } }
    }
}
