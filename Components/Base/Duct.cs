using System;
using System.Collections.Generic;
using System.Text;
using static TransformerOptimizer.Data.Constants;

namespace TransformerOptimizer.Components.Base
{
    /// <summary>
    /// Represents a layer of ducts to be placed within a winding or between two windings.
    /// </summary>
    [Serializable]
    public class Duct
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ductLocation">How the ducts will be located around the coil.</param>
        /// <param name="width">Width of the duct.</param>
        /// <param name="height">Height of the duct.</param>
        public Duct(DuctLocation ductLocation, double width, double height)
        {
            this.DuctLocation = ductLocation;
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Returns the width of the duct.
        /// </summary>
        public double Width { get; }
        /// <summary>
        /// Returns the height of the duct.
        /// </summary>
        public double Height { get; }
        /// <summary>
        /// Returns how the ducts will be located around the coil.
        /// </summary>
        public DuctLocation DuctLocation { get; }

        //TODO make each base component inherit/impement from some type so that a reference to a previous/next object can be checked; Tube and Section.
    }
}
