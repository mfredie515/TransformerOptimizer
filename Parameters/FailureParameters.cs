using System;
using System.Collections.Generic;
using System.Text;

namespace TransformerOptimizer.Parameters
{
    /// <summary>
    /// Provides a holder object for all the parameters a design will be checked against to determine if it is 'Passed'.
    /// </summary>
    public class FailureParameters
    {
        /// <summary>
        /// Maximum Build of the design in percent.
        /// </summary>
        public double MaximumBuild { get; set; }
        /// <summary>
        /// Maximum Temperature Rise of the design.
        /// </summary>
        public double MaximumTemperatureRise { get; set; }
        /// <summary>
        /// Maximum Height of the design, taken with windows facing 'viewer' and core legs vertical.
        /// </summary>
        public double MaximumHeight { get; set; }
        /// <summary>
        /// Maximum Width of the design, taken with windows facing 'viewer' and core legs vertical.
        /// </summary>
        public double MaximumWidth { get; set; }
        /// <summary>
        /// Maximum Depth of the design, taken with windows facing 'viewer' and core legs vertical.        
        /// </summary>
        public double MaximumDepth { get; set; }
        /// <summary>
        /// Maximum Weight of the design, combined core + coil weights.
        /// </summary>
        public double MaximumWeight { get; set; }
        /// <summary>
        /// Maximum Losses in the design, combined core + coil losses
        /// </summary>
        public double MaximumLosses { get; set; }
        /// <summary>
        /// Minimum Effeciency of the design.
        /// </summary>
        public double MinimumEfficiency { get; set; }
        /// <summary>
        /// Minimum DOE Effeciency of the design.
        /// </summary>
        public double MinimumDoeEfficiency { get; set; }
    }
}
