using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static TransformerOptimizer.Data.Constants;

namespace TransformerOptimizer.Components.Base
{
    /// <summary>
    /// Winding made up of multiple sections.
    /// </summary>
    [Serializable]
    public class Winding
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the winding.</param>
        /// <param name="windingOrder">Order of the winding in the coil.</param>
        /// <param name="index">Used for system determination.</param>
        /// <param name="fullCapacity">Are the taps are rated for full capcity or not.</param>
        /// <param name="isPrimary">If the winding is a primary winding or not.</param>
        /// <param name="ratedVA">Rated VA of the winding.</param>
        /// <param name="nominalVoltage">Nominal line voltage of the winding</param>
        /// <param name="phase">Phase of the winding.</param>
        /// <param name="connection">Connection of the winding.</param>
        /// <param name="voltages">List of voltages of the winding.</param>
        public Winding(string name, int windingOrder, int index, bool fullCapacity, bool isPrimary, double ratedVA, double nominalVoltage, Phase phase, Connection connection, double[] voltages)
        {
            this.Name = name;
            this.WindingOrder = windingOrder;
            this.Index = index;
            this.FullCapacity = fullCapacity;
            this.IsPrimary = isPrimary;
            this.RatedVA = ratedVA;
            this.CalcedVA = ratedVA;
            this.NominalVoltage = nominalVoltage;
            this.TapVoltages = voltages.ToList();
            this.Phase = phase;
            this.Connection = connection;
            this.Sections = new List<Section>();
        }
        /// <summary>
        /// Copy constructor.
        /// 
        /// Creates a new winding copy of given winding parameter.
        /// </summary>
        /// <param name="winding">Winding to copy.</param>
        protected internal Winding(Winding winding)
        {
            this.Name = winding.Name;
            this.WindingOrder = winding.WindingOrder;
            this.Index = winding.Index;
            this.Core = winding.Core;
            this.Tube = winding.Tube;
            this.FullCapacity = winding.FullCapacity;
            this.IsPrimary = winding.IsPrimary;
            this.RatedVA = winding.RatedVA;
            this.CalcedVA = winding.CalcedVA;
            this.NominalVoltage = winding.NominalVoltage;
            this.Phase = winding.Phase;
            this.Connection = winding.Connection;
            this.TapVoltages = winding.TapVoltages;
            this.Sections = new List<Section>();

            foreach (Section s in winding.Sections)
                this.Sections.Add(new Section(s));

            this.SetSectionsToSelf();
        }

        /// <summary>
        /// Sets the winding reference in each section to this winding object.
        /// Also sets the previous section reference for each section that would have a previous section.
        /// </summary>
        protected internal void SetSectionsToSelf()
        {
            List<Section> sections = Sections.OrderBy(s => s.StartingVoltage).ToList();
            for (int i = 0; i < Sections.Count; i++)
            {
                sections[i].Winding = this;
                if (i != 0)
                    sections[i].PreviousSection = sections[i - 1];
            }
        }

        /// <summary>
        /// Name of the winding.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Order of the winding in the coil.
        /// </summary>
        public int WindingOrder { get; protected internal set; }
        /// <summary>
        /// Used for system determination.
        /// </summary>
        public int Index { get; protected internal set; }
        /// <summary>
        /// Used for system determination.
        /// </summary>
        public int WindingNumber { get; protected internal set; }
        /// <summary>
        /// Design that the windings are attached to. Used to determine current density in the case of series/parallel UI windings.
        /// 
        /// With the addition of this field on 04/03/2019 ideally the core, tube, and previous winding referance can be removed and instead go though the design reference.
        /// </summary>
        [JsonIgnore]
        public Design Design { get; protected internal set; }
        /// <summary>
        /// Core the windings go onto.
        /// </summary>
        [JsonIgnore]
        public Core Core { get; protected internal set; }
        /// <summary>
        /// Tube to be used over the core.
        /// </summary>
        [JsonIgnore]
        public Tube Tube { get; protected internal set; }
        /// <summary>
        /// Returns if the tube used over the core is rotated.
        /// </summary>
        public bool RotatedTube { get; protected internal set; }
        /// <summary>
        /// Returns whether the taps are rated for full capacity or not.
        /// </summary>
        public bool FullCapacity { get; protected internal set; }
        /// <summary>
        /// Returns if the winding is a primary or secondary winding.
        /// </summary>
        public bool IsPrimary { get; protected internal set; }
        /// <summary>
        /// Rated VA of the winding.
        /// </summary>
        public double RatedVA { get; protected internal set; }
        /// <summary>
        /// Calculated VA of the winding.
        /// </summary>
        public double CalcedVA { get; protected internal set; }
        /// <summary>
        /// Nominal voltage of the winding.
        /// </summary>
        public double NominalVoltage { get; protected internal set; }
        /// <summary>
        /// Phase of the winding.
        /// </summary>
        public Phase Phase { get; }
        /// <summary>
        /// Connection of the winding.
        /// </summary>
        public Connection Connection { get; }
        /// <summary>
        /// Reference to the previous winding in the coil.
        /// </summary>     
        [JsonIgnore]
        public Winding PreviousWinding { get; protected internal set; }
        /// <summary>
        /// List of sections that make up the winding.
        /// </summary>
        public List<Section> Sections { get; protected internal set; }
        /// <summary>
        /// List of voltages of the winding.
        /// </summary>
        public List<double> TapVoltages { get; protected internal set; }

        /// <summary>
        /// Returns the nominal line voltage of the winding.
        /// </summary>
        public double LineVoltage { get { return NominalVoltage; } }
        /// <summary>
        /// Returns the nominal phase voltage of the winding.
        /// </summary>
        public double PhaseVoltage { get { return NominalVoltage * (Connection == Connection.WYE ? 1 / Math.Sqrt(3) : 1); } }
        /// <summary>
        /// Returns the nominal current of the winding.
        /// </summary>
        public double NominalCurrent
        {
            get
            {
                return RatedVA / PhaseVoltage;
            }
        }
        /// <summary>
        /// Returns the lowest line current that will be seen in the winding.
        /// </summary>
        public double LowestLineCurrent
        {
            get
            {
                if (IsThreePhase)
                {
                    return RatedVA * (Connection == Connection.DELTA ? Math.Sqrt(3) : 1) / (TapVoltages.Max() * (Connection == Connection.WYE ? 1 / Math.Sqrt(3) : 1));
                }
                else
                    return RatedVA / TapVoltages.Max();
            }
        }
        /// <summary>
        /// Returns the highest line current that will be seen in the winding.
        /// </summary>
        public double HighestLineCurrent
        {
            get
            {
                if (IsThreePhase)
                {
                    return RatedVA * (Connection == Connection.DELTA ? Math.Sqrt(3) : 1) / (TapVoltages.Min() * (Connection == Connection.WYE ? 1 / Math.Sqrt(3) : 1));
                }
                else
                    return RatedVA / TapVoltages.Min();
            }
        }
        /// <summary>
        /// Returns the lowest phase current that will be seen in the winding.
        /// </summary>
        public double LowestPhaseCurrent
        {
            get
            {
                if (IsThreePhase)
                {
                    return RatedVA / (TapVoltages.Max() / (Connection == Connection.WYE ? Math.Sqrt(3) : 1));
                }
                else
                    return RatedVA / TapVoltages.Max();
            }
        }
        /// <summary>
        /// Returns the highest phase current that will be seen in the winding.
        /// </summary>
        public double HighestPhaseCurrent
        {
            get
            {
                if (IsThreePhase)
                {
                    return RatedVA / (TapVoltages.Min() / (Connection == Connection.WYE ? Math.Sqrt(3) : 1));
                }
                else
                    return RatedVA / TapVoltages.Min();
            }
        }
        /// <summary>
        /// Returns whether the winding is three phase or not.
        /// </summary>
        public bool IsThreePhase { get { return Phase == Phase.THREE; } }
        /// <summary>
        /// Returns the total weight of the winding.
        /// </summary>
        public double Weight { get { return Sections.Select(s => s.Weight).Sum(); } }
        /// <summary>
        /// Returns the total cost of the winding.
        /// </summary>
        public double Cost { get { return Sections.Select(s => s.Cost).Sum(); } }
        /// <summary>
        /// Returns the total number of turns in the winding.
        /// </summary>
        public int TotalTurns { get { return Sections.Select(s => s.Turns).Sum(); } }
        /// <summary>
        /// Returns the build of the winding.
        /// </summary>
        public double Build { get { return Sections.Select(s => s.Build).Sum(); } }
        /// <summary>
        /// Returns of the losses of the winding that occur in a specific worst case scenario.
        /// 
        /// If the number of sections is 1, then the worst case losses would just be the normal losses.
        /// If the number of sections is greater than 1, then based on the recursive resistance and current of each of the sections the worst case of the combinations is determined.
        /// </summary>
        public double WorstCaseLosses
        {
            get
            {
                List<double> losses = new List<double>();

                foreach (Section s in Sections)
                {
                    losses.Add(s.Losses(s.SectionCurrent, true));
                }

                return losses.Max();
            }
        }
        /// <summary>
        /// Returns the sum of the sections exposed duct area.
        /// </summary>
        public double InternalExposedArea { get { return Sections.Sum(s => s.MeanDuctArea); } }
        /// <summary>
        /// Returns the area exposed on the surface of the winding.        
        /// </summary>
        public double ExternalExposedArea { get { return (WindingOrder == Design.Windings.Count ? (Sections.Last().LengthMeanTurn + 4 * Sections.Last().Build) * Sections.Last().WindingLength : 0); } }
    }
}
