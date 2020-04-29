using System;
using System.Collections.Generic;
using System.Text;

namespace TransformerOptimizer.Components.Base
{
    /// <summary>
    /// Represents a tube that is fitted over a leg of the core.
    /// </summary>
    [Serializable]
    public class Tube
    {
        /// <summary>
        /// Constructor.
        /// 
        /// Generates a tube with the given parameters.
        /// </summary>
        /// <param name="name">Name of the tube.</param>
        /// <param name="width">Width of the tube, assuming this side is facing 'viewer'.</param>
        /// <param name="depth">Depth of the tube, assuming <see cref="Width"/> is facing 'viewer'. </param>
        /// <param name="length">Length of the tube.</param>
        /// <param name="thickness">Wall thickness of the tube.</param>
        /// <param name="wrap">Thickness of wrap over the tube.</param>
        /// <param name="cost">Cost of the tube.</param>
        internal Tube(string name, double width, double depth, double length, double thickness, double wrap, double cost)
        {
            this.Name = name;
            this.Width = width;
            this.Depth = depth;
            this.Length = length;
            this.Thickness = thickness;
            this.Wrap = wrap;
            this.Cost = cost;
        }

        /// <summary>
        /// Name of the tube.
        /// 
        /// If handmade, name is 'Handmade Tube'.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Width of the tube, assuming this side is facing 'viewer'.
        /// </summary>
        public double Width { get; }
        /// <summary>
        /// Depth of the tube, assuming <see cref="Width"/> is facing 'viewer'.
        /// </summary>
        public double Depth { get; }
        /// <summary>
        /// Length of the tube.
        /// </summary>
        public double Length { get; }
        /// <summary>
        /// Thickness of the tube.
        /// 
        /// Handmade tubes have a thickness of 0.0625 inch.
        /// </summary>
        public double Thickness { get; }
        /// <summary>
        /// Wrap around the tube.
        /// 
        /// Handmade tubes have 2 pieces of 0.010 insulation, paper is not specified.
        /// Standard tubes have 0 wrap.
        /// </summary>
        public double Wrap { get; }
        /// <summary>
        /// Cost of the tube, as on 03/18/2019 all tubes cost 0.
        /// </summary>
        public double Cost { get; }

        /// <summary>
        /// Boolean representing if the tube is a handmade tube or a standard tube.
        /// </summary>
        public bool IsHandmade { get { return Name == "Handmade Tube"; } }
    }
}
