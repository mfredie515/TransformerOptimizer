using System;
using System.Collections.Generic;
using System.Text;
using static TransformerOptimizer.Data.Constants;

namespace TransformerOptimizer.Components.Base
{
    /// <summary>
    /// Contains information to be used between all designs and holds the list of base windings to be used throughout the designs.
    /// </summary>
    [Serializable]
    public class Specification
    {
        /// <summary>
        /// Holder for base windings.
        /// </summary>
        [NonSerialized]
        public List<Winding> BaseWindings;

        /// <summary>
        /// Constructor.
        /// </summary>
        protected internal Specification()
        {
            BaseWindings = new List<Winding>();
        }

        /// <summary>
        /// Clears the list of windings.
        /// </summary>
        public void ClearWindings()
        {
            BaseWindings.Clear();
        }

        /// <summary>
        /// Creates a new winding with no sections and adds it to the list of windings.
        /// </summary>
        /// <param name="name">Name of the winding.</param>
        /// <param name="windingOrder">Order of the winding in the coil.</param>
        /// <param name="windingNumber">Used for system determination.</param>
        /// <param name="fullCapacity">Are the taps are rated for full capcity or not.</param>
        /// <param name="isPrimary">Is the winding primary or secondary.</param>
        /// <param name="kva">Rated kVA of the winding.</param>
        /// <param name="nominalVoltage">Nominal line voltage of the winding.</param>
        /// <param name="voltages">List of voltages of the winding.</param>
        /// <param name="phase">Phase of the winding.</param>
        /// <param name="connection">Connection of the winding.</param>
        public void AddNewWinding(string name, int windingOrder, int windingNumber, bool fullCapacity, bool isPrimary, double kva, double nominalVoltage, double[] voltages,
            Phase phase, Connection connection)
        {
            Winding winding = new Winding(name, windingOrder, windingNumber, fullCapacity, isPrimary, kva * 1000, nominalVoltage, phase, connection, voltages);
            BaseWindings.Add(winding);
        }
        /// <summary>
        /// Creates a new section and adds it to the given winding.
        /// </summary>
        /// <param name="winding">Winding to add section to.</param>
        /// <param name="order">Order of the section.</param>
        /// <param name="startVoltage">Starting voltage of the section.</param>
        /// <param name="endVoltage">Ending voltage of the section.</param>
        /// <param name="bulgeFactor">Bulge factor of the section.</param>
        /// <param name="margin">Margin of the section.</param>
        /// <param name="layerPaper">Total layer paper thickness of the section.</param>
        /// <param name="wrap">Total wrap thickness of the section.</param>
        /// <param name="cdMin">Minimum current density to find wires with.</param>
        /// <param name="cdMax">Maximum current density to find wires with.</param>
        /// <param name="wireMaterial">Material to find wires with.</param>
        /// <param name="wireShape">Shape to find wires with.</param>
        /// <param name="bifilars">Bifilars to find wires with.</param>
        public void AddSection(Winding winding, int order, double startVoltage, double endVoltage, double bulgeFactor, double margin, double layerPaper, double wrap,
            double cdMin, double cdMax, WireMaterial wireMaterial, WireShape wireShape, Bifilar[] bifilars)
        {
            winding.Sections.Add(new Section(winding, order, startVoltage, endVoltage, bulgeFactor, margin, layerPaper, wrap, cdMin, cdMax, wireMaterial, wireShape, bifilars));
        }
        /// <summary>
        /// Add winding to list.
        /// </summary>
        /// <param name="winding">Winding to add.</param>
        public void AddWinding(Winding winding)
        {
            BaseWindings.Add(winding);
        }

        /// <summary>
        /// Phase of the transformer.
        /// </summary>
        public Phase Phase { get; set; }
        /// <summary>
        /// Frequency at which the transformer will be operated at.
        /// </summary>
        public double Frequency { get; set; }
        /// <summary>
        /// Tube winding margin for the transformer.
        /// </summary>
        public double TubeWindowMargin { get; set; }
        /// <summary>
        /// Destruction factor of the core.
        /// </summary>
        public double DestructionFactor { get; set; }
        /// <summary>
        /// Excitation factor of the core.
        /// </summary>
        public double ExcitationFactor { get; set; }
        /// <summary>
        /// Stacking factor of the laminations.
        /// </summary>
        public double StackingFactor { get; set; }
        /// <summary>
        /// Stray Losses of the design in percent.
        /// </summary>
        public double StrayLosses { get; set; }
    }
}
