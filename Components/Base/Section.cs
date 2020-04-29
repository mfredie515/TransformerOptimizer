using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static TransformerOptimizer.Functions.Functions;
using static TransformerOptimizer.Data.Constants;

namespace TransformerOptimizer.Components.Base
{
    /// <summary>
    /// An individual section of a <see cref="Base.Winding"/>.
    /// </summary>
    [Serializable]
    public class Section
    {
        /// <summary>
        /// Constructor.       
        /// 
        /// Note: For a <paramref name="wireMaterial"/> of <see cref="Data.Constants.WireMaterial.COPPER"/> or <see cref="Data.Constants.WireMaterial.ALUMINUM"/> the current density minimum and maximum work as expected.
        /// If <paramref name="wireMaterial"/> is <see cref="Data.Constants.WireMaterial.ANY"/> then for the Aluminum wire the maximum current density is capped at 1300, and for the Copper wire the minimum durrent density is capped at 1000.
        /// 
        /// </summary>
        /// <param name="winding">Winding to add the section to.</param>
        /// <param name="order">Order of the section in the coil.</param>
        /// <param name="startVoltage">Starting voltage of the section.</param>
        /// <param name="endVoltage">Ending voltage of the section.</param>
        /// <param name="bulgeFactor">Bulge factor of the section.</param>
        /// <param name="margin">Margin of the section.</param>
        /// <param name="layerPaper">Total thickness of insulation between layers.</param>
        /// <param name="wrap">Total thickness of wrap after the section.</param>
        /// <param name="cdMin">Minimum current density for wires to pass.</param>
        /// <param name="cdMax">Maximum current density for wires to pass.</param>
        /// <param name="wireMaterial">Wire materials to iterate.</param>
        /// <param name="wireShape">Wire shapes to iterate.</param>
        /// <param name="bifilars">Bifilar ranges to iterate.</param>
        /// <param name="wireShapes">Optional parameter used to specify which cominatiopn of wire shapes for iterate over.</param>
        public Section(Winding winding, int order, double startVoltage, double endVoltage, double bulgeFactor, double margin, double layerPaper, double wrap, double cdMin, double cdMax, WireMaterial wireMaterial, WireShape wireShape, Bifilar[] bifilars, WireShape[] wireShapes = null)
        {
            this.Winding = winding;
            this.SectionOrder = order;
            this.WindingName = winding.Name;
            this.Name = "Section " + order;
            this.StartingVoltage = startVoltage;
            this.EndingVoltage = endVoltage;
            this.BulgeFactor = bulgeFactor;
            this.Margin = margin;
            this.LayerPaper = layerPaper;
            this.Wrap = wrap;
            this.CurrentDensityMinimum = cdMin;
            this.CurrentDensityMaximum = cdMax;
            this.IterateWireMaterial = wireMaterial;
            this.IterateWireShape = wireShape;
            this.BifilarRange = bifilars;
            this.IterateWireShapes = wireShapes;
            this.Ducts = new List<Duct>();
        }
        /// <summary>
        /// Copy contructor.
        /// 
        /// Copies the fields from <paramref name="section"/> and adds reference to <paramref name="wire"/>.
        /// </summary>
        /// <param name="section">Section to copy.</param>
        /// <param name="wire">Wire to add reference to.</param>
        protected internal Section(Section section, Wire wire)
        {
            this.WindingName = section.WindingName;
            this.Name = section.Name;
            this.SectionOrder = section.SectionOrder;
            this.BulgeFactor = section.BulgeFactor;
            this.Margin = section.Margin;
            this.StartingVoltage = section.StartingVoltage;
            this.EndingVoltage = section.EndingVoltage;
            this.LayerPaper = section.LayerPaper;
            this.Wrap = section.Wrap;
            this.Wire = wire;
            if (wire is Derived.Foil)
                (wire as Derived.Foil).Section = this;
            this.Ducts = section.Ducts;
        }
        /// <summary>
        /// Copy constructor.
        /// 
        /// Copies the fields from <paramref name="section"/>.
        /// </summary>
        /// <param name="section">Section to copy.</param>
        protected internal Section(Section section)
        {
            this.WindingName = section.WindingName;
            this.Name = section.Name;
            this.SectionOrder = section.SectionOrder;
            this.BulgeFactor = section.BulgeFactor;
            this.Margin = section.Margin;
            this.StartingVoltage = section.StartingVoltage;
            this.EndingVoltage = section.EndingVoltage;
            this.LayerPaper = section.LayerPaper;
            this.Wrap = section.Wrap;
            if (section.Wire is Derived.Foil)
            {
                this.Wire = new Derived.Foil(section.Wire as Derived.Foil);
                (this.Wire as Derived.Foil).Section = this;
            }
            else
                this.Wire = section.Wire;
            this.Ducts = section.Ducts;
        }

        /// <summary>
        /// Name of the assoiated winding name.
        /// </summary>
        public string WindingName { get; }
        /// <summary>
        /// Name of the section.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The order that the section is placed in the coil.
        /// </summary>
        public int SectionOrder { get; protected internal set; }
        /// <summary>
        /// The number of turns in the section.
        /// </summary>
        public int Turns { get; protected internal set; }
        /// <summary>
        /// Returns the bulge factor to be used in the calculation of <see cref="LengthMeanTurn"/> and <see cref="PreviousBuild"/>.
        /// </summary>
        public double BulgeFactor { get; protected internal set; }
        /// <summary>
        /// Maximum current density for passing wires. Used only in <see cref="Factories.WindingFactory"/>.
        /// </summary>
        public double CurrentDensityMaximum { get; protected internal set; }
        /// <summary>
        /// Minimum current density for passing wires. Used only in <see cref="Factories.WindingFactory"/>.
        /// </summary>
        public double CurrentDensityMinimum { get; protected internal set; }
        /// <summary>
        /// The desired gap between the start of the section and the top of the <see cref="Winding.Tube"/> and the end of the section and the bottom of the <see cref="Winding.Tube"/>. 
        /// </summary>
        public double Margin { get; protected internal set; }
        /// <summary>
        /// The starting voltage for this section.
        /// </summary>
        public double StartingVoltage { get; protected internal set; }
        /// <summary>
        /// The ending voltage for this section.
        /// </summary>
        public double EndingVoltage { get; protected internal set; }
        /// <summary>
        /// The total layer paper thickness to place between each layer of wire.
        /// </summary>
        public double LayerPaper { get; protected internal set; }
        /// <summary>
        /// The total wrap thickness to place on the outside of the section.
        /// </summary>
        public double Wrap { get; protected internal set; }
        /// <summary>
        /// Which winding the section is a member of.
        /// </summary>
        public Winding Winding { get; protected internal set; }
        /// <summary>
        /// The previous section of the winding.
        /// </summary>
        public Section PreviousSection { get; protected internal set; }
        /// <summary>
        /// The wire that the section is using.
        /// </summary>
        public Wire Wire { get; protected internal set; }
        /// <summary>
        /// Which wire material to iterate through. Used only in <see cref="Factories.WindingFactory"/>.
        /// </summary>
        public WireMaterial IterateWireMaterial { get; protected internal set; }
        /// <summary>
        /// Which wire shapes to iterate through. Used only in <see cref="Factories.WindingFactory"/>.
        /// </summary>
        public WireShape IterateWireShape { get; protected internal set; }
        /// <summary>
        /// Which wire shapes to iterate through. Used only in <see cref="Factories.WindingFactory"/>.
        /// </summary>
        public WireShape[] IterateWireShapes { get; protected internal set; }
        /// <summary>
        /// The range of bifilars to iterate through. Used only in <see cref="Factories.WindingFactory"/>.
        /// </summary>
        public Bifilar[] BifilarRange { get; protected internal set; }
        /// <summary>
        /// A list of wires that meet the sections <see cref="CurrentDensityMinimum"/> and <see cref="CurrentDensityMaximum"/> requirements. Used only in <see cref="Factories.WindingFactory"/>.
        /// </summary>
        public List<Wire> Wires { get; protected internal set; }

        /// <summary>
        /// Returns the line voltage range of the section.
        /// </summary>
        public double VoltageRange { get { return EndingVoltage - StartingVoltage; } }
        /// <summary>
        /// Returns the phase voltage range of the section.
        /// </summary>
        public double PhaseVoltageRange { get { return (EndingVoltage - StartingVoltage) / (Winding.Connection == Connection.DELTA ? 1 : (Winding.Connection == Connection.WYE ? Math.Sqrt(3) : 1)); } }
        /// <summary>
        /// Returns the average length of a complete turn in the section.
        /// </summary>
        public double LengthMeanTurn
        {
            get
            {
                if (Winding.Tube == null)
                    return 0;
                double w, d, t, a;
                if (PreviousSection == null && Winding.PreviousWinding == null)
                {
                    w = Winding.RotatedTube ? Winding.Tube.Depth : Winding.Tube.Width;
                    d = Winding.RotatedTube ? Winding.Tube.Width : Winding.Tube.Depth;
                    t = Winding.Tube.Thickness;
                    a = BulgeFactor * BuildNoWrap;// + Ducts.Select(i => i.DuctMeanLengthTurnIncrease()).Sum();
                }
                else if (PreviousSection == null && Winding.PreviousWinding != null)
                {
                    w = (Winding.RotatedTube ? Winding.Tube.Depth : Winding.Tube.Width) + 2 * Winding.Tube.Thickness;
                    d = (Winding.RotatedTube ? Winding.Tube.Width : Winding.Tube.Depth) + 2 * Winding.Tube.Thickness;
                    t = Winding.PreviousWinding.Sections.Last().PreviousBuildWithDucts;// Winding.PreviousWinding.Sections.Last().PreviousBuild + Winding.PreviousWinding.Sections.Last().Ducts.Select(i => i.DuctBuildUpIncrease()).Sum();
                    a = BulgeFactor * BuildNoWrap;// + Ducts.Select(i => i.DuctMeanLengthTurnIncrease()).Sum();
                }
                else
                {
                    w = (Winding.RotatedTube ? Winding.Tube.Depth : Winding.Tube.Width) + 2 * Winding.Tube.Thickness;
                    d = (Winding.RotatedTube ? Winding.Tube.Width : Winding.Tube.Depth) + 2 * Winding.Tube.Thickness;
                    t = PreviousSection.PreviousBuildWithDucts; //PreviousSection.PreviousBuild + PreviousSection.Ducts.Select(i => i.DuctBuildUpIncrease()).Sum();
                    a = BulgeFactor * BuildNoWrap;// + Ducts.Select(i => i.DuctMeanLengthTurnIncrease()).Sum();
                }
                return 2 * w + 2 * d + 2 * Math.PI * t + Math.PI * a + Ducts.Select(i => i.DuctMeanLengthTurnIncrease()).Sum();
            }
        }
        /// <summary>
        /// Returns the maximum number of turns that can fit in a single layer of the section.
        /// </summary>
        public int TurnsPerLayer { get { return (int)Math.Floor(WindingLength / (Wire.WireShape == WireShape.ROUND ? Wire.TotalThickness : Wire.TotalWidth)) - Wire.SkewFactor; } }
        /// <summary>
        /// Returns the number of layers that the section will contain.
        /// </summary>
        public int NumberOfLayers { get { return (int)Math.Ceiling((double)Turns / TurnsPerLayer); } }
        /// <summary>
        /// Returns the length available on the core for the winding of the section.
        /// 
        /// See <see cref="Core.TubeWindowMargin"/> and <see cref="Margin"/> for information on each component.
        /// </summary>
        public double WindingLength { get { return Winding.Core == null ? 0 : Winding.Core.Lamination.WindowHeight - 2 * Margin - Winding.Core.TubeWindowMargin; } }
        /// <summary>
        /// Returns the sum of the buildup of the previous sections.
        /// </summary>
        public double PreviousBuild { get { return PreviousSection == null ? BulgeFactor * BuildNoWrap : BulgeFactor * BuildNoWrap + PreviousSection.PreviousBuild; } }
        private double PreviousBuildWithDucts { get { return PreviousSection == null ? (Winding.PreviousWinding == null ? BulgeFactor * BuildNoWrap + Ducts.Select(d => d.DuctBuildUpIncrease()).Sum() : Winding.PreviousWinding.Sections.Last().PreviousBuildWithDucts) : BulgeFactor * BuildNoWrap + PreviousSection.PreviousBuildWithDucts + Ducts.Select(d => d.DuctBuildUpIncrease()).Sum() + PreviousSection.Ducts.Select(d => d.DuctBuildUpIncrease()).Sum(); } }
        /// <summary>
        /// Returns the buildup of the section with added outside wrap.
        /// </summary>
        public double Build { get { return BuildNoWrap + Wrap; } }
        /// <summary>
        /// Returns the buildup of the section without any outside wrap added.
        /// </summary>
        public double BuildNoWrap { get { return NumberOfLayers * (Wire.TotalThickness) + (NumberOfLayers - 1) * LayerPaper + Ducts.Where(d => d.DuctLocation == DuctLocation.ALL_AROUND).Select(d => d.Height).Sum(); } }
        /// <summary>
        /// Returns the approximate length of wire used in the section.
        /// </summary>
        public double WireLength { get { return LengthMeanTurn * Turns; } }
        /// <summary>
        /// Returns the current density of the section based on <see cref="SectionNominalCurrent"/> and the UI style if applicable.
        /// </summary>
        public double CurrentDensity { get { return SectionCurrent / Wire.CrossSectionalArea / (Winding.Design.UIStyle == UIStyle.PARALLEL ? 2 : 1); } }
        /// <summary>
        /// Returns the resistance of the section.
        /// </summary>
        public double Resistance { get { return WireLength / 1000 * Wire.ResistancePer1000Inches; } }
        /// <summary>
        /// Returns the total resistance that would be seen by this section.
        /// </summary>
        public double RecursiveResistance
        {
            get
            {
                if (PreviousSection == null)
                    return Resistance;
                else
                    return Resistance + PreviousSection.RecursiveResistance;
            }
        }
        /// <summary>
        /// Returns the weight of the section.
        /// 
        /// As of 04/01/2019 only the weight of the wire is used to determine the total weight.
        /// </summary>
        public double Weight { get { return WireLength / 1000 * Wire.WeightPer1000Inches; } }
        /// <summary>
        /// Returns the I²R losses in the section with its calculated <see cref="SectionCurrent"/>.
        /// </summary>
        /// <returns>Losses seen by this section.</returns>
        public double Losses() { return Resistance * Math.Pow(SectionCurrent, 2.0); }
        /// <summary>
        /// Returns the I²R losses in the section with a given current.
        /// 
        /// If <paramref name="recursiveResistance"/> is true, the resistance to be used in calculations is the sum of all sections that the <paramref name="current"/> would flow through.
        /// </summary>
        /// <param name="current">The current to use in loss calculations.</param>
        /// <param name="recursiveResistance">Whether to use <see cref="RecursiveResistance"/> as the resistance in loss calculations.</param>
        /// <returns>Losses seen by this section.</returns>
        public double Losses(double current, bool recursiveResistance = false)
        {
            if (recursiveResistance == false)
                return Resistance * Math.Pow(current, 2.0);
            else
                return RecursiveResistance * Math.Pow(current, 2.0);
        }
        /// <summary>
        /// Returns the cost of the section.
        /// 
        /// As of 04/01/2019 only <see cref="Wire.Cost"/> is used to determine cost.
        /// </summary>
        public double Cost { get { return Weight * Wire.Cost; } }
        /// <summary>
        /// Returns the determined VA of the section based on the start and end voltages and calculated current through the section.
        /// </summary>
        public double SectionVA { get { return (EndingVoltage - StartingVoltage) * SectionCurrent / (Winding.Connection == Connection.WYE ? Math.Sqrt(3) : 1); } }
        /// <summary>
        /// Returns the phase current through the section based on the tap phase voltage.
        /// 
        /// If there are no taps, this method returns <see cref="SectionNominalCurrent"/>.
        /// </summary>
        public virtual double SectionCurrent
        {
            get
            {
                if (Winding.FullCapacity)
                {
                    return Winding.CalcedVA / (EndingVoltage / (Winding.Connection == Connection.WYE ? Math.Sqrt(3) : 1));
                }
                else
                {
                    return SectionNominalCurrent;
                }
            }
        }
        /// <summary>
        /// Returns the phase current through the section using the winding VA and the nominal phase voltage.
        /// </summary>
        public virtual double SectionNominalCurrent
        {
            get
            {
                return Winding.CalcedVA / Winding.PhaseVoltage;
            }
        }

        /// <summary>
        /// Returns the width of the length of mean turn.
        /// </summary>
        public double MeanLengthTurnWidth { get { return (LengthMeanTurn / 2) - MeanLengthTurnHeight; } }
        /// <summary>
        /// Returns the height of the length of mean turn.
        /// 
        /// As of 04/11/2019 this is the tube stack dimension.
        /// </summary>
        public double MeanLengthTurnHeight { get { return (Winding.RotatedTube ? Winding.Tube.Width : Winding.Tube.Depth); } }
        /// <summary>
        /// Returns the mean duct area of the ducts.
        /// 
        /// As of 04/11/2019 this math is weird and probably wrong.
        /// </summary>
        public double MeanDuctArea { get { return 2 * (Ducts.Count == 0 ? 0 : (2 * Ducts.Sum(d => d.Height) * (MeanLengthTurnWidth + MeanLengthTurnHeight))) * WindingLength; } }

        /// <summary>
        /// Container for the ducts that this section uses.
        /// 
        /// As of 04/08/2019 this is only a placeholder, the ducts should be seperate with a ref to the objects before and after it.
        /// </summary>
        public List<Duct> Ducts { get; protected internal set; }
    }
}
