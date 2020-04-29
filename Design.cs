using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransformerOptimizer.Components.Base;
using TransformerOptimizer.Components.Derived;
using TransformerOptimizer.Parameters;
using static TransformerOptimizer.Functions.Functions;
using static TransformerOptimizer.Data.Constants;

namespace TransformerOptimizer
{
    /// <summary>
    /// Represents an individual transformer.
    /// </summary>
    [Serializable]
    public class Design
    {
        /// <summary>
        /// Constructor.                
        /// </summary>
        /// <param name="specification">Specification for the design, used to get operating frequency.</param>
        /// <param name="core">Core to be used in this design.</param>
        /// <param name="rotatedTube">Whether or not the tube to be used is rotated or not.</param>
        /// <param name="tube">Tube to be placed over core legs.</param>
        /// <param name="windings">Windings that will be used in this design.</param>
        /// <param name="uiStyle">Optional, how the windings will be connected if a UI core is used.</param>
        protected internal Design(Specification specification, Core core, bool rotatedTube, Tube tube, List<Winding> windings, UIStyle uiStyle = UIStyle.NA)
        {
            this.CompensationFactor = 1.02;
            this.Specification = specification;
            this.Core = core;
            this.RotatedTube = rotatedTube;
            this.Tube = tube;
            this.Windings = new List<Winding>();
            foreach (var w in windings)
            {
                Winding wind = new Winding(w);
                this.Windings.Add(wind);
            }
            Nulled = false;
            Passed = false;
            UIStyle = uiStyle;
        }

        /// <summary>
        /// When called, the design will calculate its' values.
        /// </summary>
        public void PerformCalculations()
        {
            //Give each winding a reference to the designs' core & tube and the prevous winding and the design.         
            for (int i = 0; i < Windings.Count; i++)
            {
                Windings[i].Design = this;
                Windings[i].Core = Core;
                Windings[i].Tube = Tube;
                Windings[i].RotatedTube = RotatedTube;
                Windings[i].WindingOrder = i + 1;
                if (i > 0)
                    Windings[i].PreviousWinding = Windings[i - 1];
            }

            if (IsUI)
            {
                //Get primary turns
                foreach (Winding w in Windings.Where(w => w.IsPrimary))
                {
                    foreach (Section s in w.Sections)
                    {
                        //Remember Flux = Volts per Turn / (4.44 * Frequency) = Volts / (4.44 * Frequency * Turns)
                        //Therefore Turns = Volts / (4.44 * Frequency * Flux)
                        //Technically 4.44 should be 4 * Waveform Factor; but as of 04/03/2019 all inputs are assumed to be perfect sine/cosine waves with a Waveform Factor = 1.11
                        s.Turns = (int)Math.Ceiling(s.PhaseVoltageRange / (4.44 * Specification.Frequency * Flux));
                        if (UIStyle == UIStyle.SERIES) //If the windings are to be connected in series divide the turns by two.
                            s.Turns = (int)Math.Floor((double)s.Turns / 2);
                    }
                }
                double primaryNominalVoltage = Windings.Where(w => w.IsPrimary).First().NominalVoltage / (UIStyle == UIStyle.SERIES ? 2 : 1);
                int primaryNominalTurns = (int)Math.Ceiling(primaryNominalVoltage / (4.44 * Specification.Frequency * Flux));
                AdjustedFlux = primaryNominalVoltage / (4.44 * Specification.Frequency * primaryNominalTurns);
                foreach (var w in Windings.Where(w => !w.IsPrimary))
                {
                    foreach (Section s in w.Sections)
                    {
                        //Determine the turns for each secondary section.
                        s.Turns = (int)Math.Ceiling((s.PhaseVoltageRange * CompensationFactor / primaryNominalVoltage) * primaryNominalTurns);
                        if (UIStyle == UIStyle.SERIES)
                            s.Turns = (int)Math.Ceiling((double)s.Turns / 2);
                    }
                }
            }
            else //See above except no possibily of dividing turns by two. Could probably be made with inline ifs to make it simplier to read/modifiy.
            {
                foreach (Winding w in Windings.Where(w => w.IsPrimary))
                {
                    foreach (Section s in w.Sections)
                    {
                        s.Turns = (int)Math.Ceiling(s.PhaseVoltageRange / (4.44 * Specification.Frequency * Flux));
                    }
                }
                double primaryNominalVoltage = Windings.Where(w => w.IsPrimary).First().NominalVoltage;
                int primaryNominalTurns = (int)Math.Ceiling(primaryNominalVoltage / (4.44 * Specification.Frequency * Flux));
                AdjustedFlux = primaryNominalVoltage / (4.44 * Specification.Frequency * primaryNominalTurns);
                foreach (var w in Windings.Where(w => !w.IsPrimary))
                {
                    foreach (Section s in w.Sections)
                        s.Turns = (int)Math.Ceiling((s.PhaseVoltageRange * CompensationFactor / primaryNominalVoltage) * primaryNominalTurns);
                }
            }

            //Get the first primary winding, as primary windings as either 1 isolated or series/parallel it honeslty doesn't matter which one we pick, but we are guaranteed the first.
            Winding primaryWinding = Windings.Where(x => x.IsPrimary).First();
            //Get the first secondary winding, there can be multiple secondry windings but only one needs to used, as we get the first as we are guaranteed to have at least one. (Except in the case of reactors/autotransformers, as of 04/03/2019 they would have to be entered as two isolation and connected after but this may change in the future)
            Winding secondaryWinding = Windings.Where(x => !x.IsPrimary).First();
            //Determine a new primary current that also feeds the losses of the design, as of 04/03/2019 this needs to be looked at in depth to determine its effectiveness.
            double primaryCurrent = ((((double)secondaryWinding.TotalTurns / primaryWinding.TotalTurns) * primaryWinding.PhaseVoltage * secondaryWinding.LowestPhaseCurrent * (RatedPhasekVA / (secondaryWinding.RatedVA / 1000)) + Windings.Select(w => w.WorstCaseLosses).Sum() + Core.Losses(AdjustedFluxDensity, (int)Specification.Frequency)) / (primaryWinding.PhaseVoltage)) * Core.ExcitationFactor * Core.Lamination.ExcitationFactor;
            //Set the primary VA to a new value based on the calculated current to feed the losses.
            foreach (Winding w in Windings.Where(x => x.IsPrimary))
                w.CalcedVA = primaryCurrent * w.TapVoltages.Last();

            //Get the build of the coils
            BuildUp = (RotatedTube ? Tube.Thickness + Tube.Wrap + (Tube.Depth - Core.Lamination.Tongue) / 2 : Tube.Thickness + Tube.Wrap + (Tube.Width - Core.Lamination.Tongue) / 2) +
                Windings.Select(w => w.Build).Sum();
            //Determine the actual build percent, as in UI and 3 phase there are two coils in each window. so the BuildFactor just divides by 2 in those cases, etc.
            Build = BuildUp / (Core.Lamination.WindowWidth * Core.Lamination.BuildFactor) * 100;

            //Calculate the temperature rise according to reuben lee's equation
            TemperatureRise = (Core.Losses(AdjustedFluxDensity, (int)Specification.Frequency, true) + Core.NumberOfCoils * Windings.Select(w => w.WorstCaseLosses).Sum() + StrayLosses) / (0.1 * Math.Pow((Core.Weight + Core.NumberOfCoils * Windings.Select(w => w.Weight).Sum()) / 1.073, 0.6666));
            //UI - take 70% of temperature rise, needs to be analyzed
            if (IsUI)
                TemperatureRise = 0.7 * TemperatureRise;
        }

        /// <summary>
        /// Determines if the design will be considered a pass or a fail.
        /// Compares the calculated values of the design with the limits set in <paramref name="failure"/>.
        /// 
        /// Note: 04/03/2019 this probably should be removed, or at least be made into a function that returns a boolean based on a given failure options so that it can be a dynamic range of pass/fail.
        /// </summary>
        /// <param name="failure">Limits for the design to pass/fail.</param>
        public void PerformValidation(FailureParameters failure)
        {
            Passed = true;
            if (Build > failure.MaximumBuild || Build <= 0)
                Passed = false;
            if (TemperatureRise > failure.MaximumTemperatureRise)
                Passed = false;
            if (Weight > failure.MaximumWeight)
                Passed = false;
            if (Depth > failure.MaximumDepth)
                Passed = false;
            if (Height > failure.MaximumHeight)
                Passed = false;
            if (Width > failure.MaximumWidth)
                Passed = false;
            if (Losses > failure.MaximumLosses)
                Passed = false;
            if (Efficiency < failure.MinimumEfficiency)
                Passed = false;
            if (DoeEfficiency < failure.MinimumDoeEfficiency)
                Passed = false;
            foreach (Winding w in Windings)
            {
                if (w.Sections.Any(s => s.NumberOfLayers <= 0))
                {
                    Nulled = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Whether or not the design is considered 'Null'.
        /// 
        /// A design is 'Null' if the <see cref="Section.NumberOfLayers"/> of any section in the design is less than or equal to 0.
        /// This is typically caused by having design parameters that are too small for a design.
        /// </summary>
        public bool Nulled { get; private set; }
        /// <summary>
        /// Whether or not the design is considered to have passed its' conditions.
        /// See <see cref="PerformValidation(FailureParameters)"/> for more information on passing and failing.
        /// </summary>
        public bool Passed { get; private set; }
        /// <summary>
        /// Returns the adjusted flux value of the design based on the calculated number of turns.
        /// </summary>
        public double AdjustedFlux { get; private set; }
        /// <summary>
        /// The percent of window space used in this design.
        /// </summary>
        public double Build { get; private set; }
        /// <summary>
        /// The amount of space in the window a single coil takes up.
        /// </summary>
        private double BuildUp { get; set; }
        /// <summary>
        /// The temperature rise of the design.
        /// 
        /// As of 04/03/2019:
        ///     For EI designs Reuben Lee's equation (see pg. 55 of Electronic Transformers and Circuits Third Edition 1988)
        ///     For UI designs the temperature is 0.7 * Reuben Lee's equation.
        /// </summary>
        public double TemperatureRise { get; private set; }

        /// <summary>
        /// The specification parameters of the design.
        /// </summary>
        public Specification Specification { get; }
        /// <summary>
        /// The core used by the design.
        /// </summary>
        public Core Core { get; }
        /// <summary>
        /// The tube placed over the core legs.
        /// </summary>
        public Tube Tube { get; }
        /// <summary>
        /// Whether or not the tube used by the design is rotated or not.
        /// 
        /// Note: a handmade tube is always considered to not be rotated.
        /// </summary>
        public bool RotatedTube { get; }
        /// <summary>
        /// The windings that are used by this design.
        /// </summary>
        public List<Winding> Windings { get; }
        /// <summary>
        /// A factor to increase the number of turns in the secondary windings.
        /// 
        /// This factor is hardcoded to 1.02, or a 2% increase in turns calculated.
        /// </summary>
        public double CompensationFactor { get; }
        /// <summary>
        /// Whether the core used is an EI shape.
        /// </summary>
        private bool IsEI { get { return Core.Lamination.Shape == CoreShape.EI; } }
        /// <summary>
        /// Whether the core used is a UI shape.
        /// </summary>
        private bool IsUI { get { return Core.Lamination.Shape == CoreShape.UI; } }
        /// <summary>
        /// The style in which the windings will be connected is the core is a UI shape.
        /// </summary>
        protected internal UIStyle UIStyle { get; }

        /// <summary>
        /// ID of this design.
        /// 
        /// As of 04/03/2019 there is really no special logic here; calls GetHashCode() and returns string of value.
        /// </summary>
        public string DesignID { get { return this.GetHashCode().ToString(); } }
        /// <summary>
        /// Whether the design is three phase or not.
        /// 
        /// Note: if any windings are three phase, then the entire design is three phase.
        /// </summary>
        public bool IsThreePhase { get { return Windings.Where(w => w.Phase == Phase.THREE).Count() > 0; } }
        /// <summary>
        /// The flux density of the design using the calculated <see cref="AdjustedFlux"/> value.
        /// </summary>
        public double AdjustedFluxDensity { get { return 10 * AdjustedFlux / Core.CrossSectionalAreaSqMeter; } }
        /// <summary>
        /// The supplied flux of the design.
        /// </summary>
        public double Flux { get { return Core.FluxDensity * Core.CrossSectionalAreaSqMeter / 10; } }
        /// <summary>
        /// Returns the height of the design, taken with windows facing 'viewer' and legs vertical.
        /// </summary>
        public double Height { get { return Core.Lamination.WindowHeight + 2 * Core.Lamination.Yoke; } }
        /// <summary>
        /// Returns the width of the design, taken with windows facing 'viewer' and legs vertical.
        /// </summary>
        public double Width { get { return Core.Lamination.Length + (Core.NumberOfCoils > 1 ? 2 * BuildUp : 0); } }
        /// <summary>
        /// Returns the depth of the design, taken with windows facing 'viewer' and legs vertical.
        /// </summary>
        public double Depth { get { return Core.Stack + 2 * BuildUp; } }

        /// <summary>
        /// Returns the total weight of the design.
        /// 
        /// As of 04/03/2019 only the core and winding weights are taken into consideration.
        /// </summary>
        public double Weight { get { return Core.Weight + (Core.NumberOfCoils * Windings.Select(w => w.Weight).Sum()); } }
        /// <summary>
        /// Returns the total cost of the design.
        /// 
        /// As of 04/03/2019 only the core and winding costs are taken into consideration.
        /// </summary>
        public double Cost { get { return Core.Cost + (Core.NumberOfCoils * Windings.Select(w => w.Cost).Sum()); } }
        /// <summary>
        /// Ratio of the cost of the core and the cost of the coils.
        /// </summary>
        public double CostRatio { get { return Core.Cost / (Core.NumberOfCoils * Windings.Sum(w => w.Cost)); } }
        /// <summary>
        /// Rated kVA of the design.
        /// </summary>
        public double RatedVA { get { return (IsThreePhase ? 3 : 1) * Windings.Where(w => w.IsPrimary).Select(w => w.RatedVA).Sum(); } }
        /// <summary>
        /// Rated kVA of the design in a single phase.
        /// </summary>
        public double RatedPhasekVA { get { return Windings.Where(w => w.IsPrimary).First().RatedVA / 1000; } }
        /// <summary>
        /// Adjusted kVA of the design including an increased primary current in order to feed the losses calculated by this design in a single phase.
        /// </summary>
        public double CalcedPhasekVA { get { return Windings.Where(w => w.IsPrimary).First().CalcedVA / 1000; } }
        /// <summary>
        /// The losses generated by this design.
        /// 
        /// As of 04/03/2019 only core losses and coil losses are taken into consideration, no eddy current or stray losses.
        /// </summary>
        public double Losses { get { return BaseLosses + StrayLosses; } }
        private double BaseLosses { get { return Core.Losses(AdjustedFluxDensity, (int)Specification.Frequency) + Core.NumberOfCoils * Windings.Sum(w => w.WorstCaseLosses); } }
        private double StrayLosses { get { return Specification.StrayLosses * BaseLosses; } }
        /// <summary>
        /// Calculated efficiency of the design.
        /// 
        /// As of 04/03/2019 it is calculated simply with kVA and Losses, no D.O.E. or other testing parameters are used.
        /// </summary>
        public double Efficiency { get { return (Core.NumberOfCoils * CalcedPhasekVA * 1000) / ((Core.NumberOfCoils * CalcedPhasekVA * 1000) + Losses) * 100; } }
        /// <summary>
        /// Calculated doe efficiency of the design.
        /// 
        /// Load = 35%
        /// Operating temperature = 75°C        
        /// </summary>
        public double DoeEfficiency
        {
            get
            {
                Windings.ForEach(w => Console.WriteLine(w.Name + "  " + w.LowestTapLosses(75) * Math.Pow(0.35, 2)));
                Console.WriteLine(Windings.Select(w => w.LowestTapLosses(75) * Math.Pow(0.35, 2)).Sum());
                Console.WriteLine(Core.Losses(AdjustedFluxDensity, (int)Specification.Frequency));
                return 100 * ((RatedVA * 0.35) / (RatedVA * 0.35 + ((1 + Specification.StrayLosses) * Core.NumberOfCoils * Windings.Select(w => w.LowestTapLosses(75) * Math.Pow(0.35, 2)).Sum() + Core.Losses(AdjustedFluxDensity, (int)Specification.Frequency))));
            }
        }

        /// <summary>
        /// String containing all the column headers to be used when printing a design.
        /// 
        /// As of 04/03/2019, this property is really bad, this should be moved to either a static property or require user to print out fields manually.
        /// </summary>
        [JsonIgnore]
        public string CsvHeaders
        {
            get
            {
                List<string> temp = new List<string>
                {
                    "DesignID",
                    "Passed",
                    "Flux",
                    "Adj Flux",
                    "Grade",
                    "Thickness",
                    "StdLam",
                    "Tongue",
                    "Stack",
                    "Width",
                    "Height",
                    "Weight",
                    "Losses",
                    "Cost",
                    "Tube Name"
                };
                foreach (var w in Windings)
                {
                    foreach (var s in w.Sections)
                    {
                        temp.Add("Section");
                        temp.Add("Start Voltage");
                        temp.Add("End Voltage");
                        temp.Add("Wire");
                        temp.Add("Bifilar");
                        temp.Add("Material");
                        temp.Add("Current Density");
                        temp.Add("Turns");
                        temp.Add("Current");
                        temp.Add("Weight");
                        temp.Add("Cost");
                    }
                }
                temp.Add("Build");
                temp.Add("Inside Exposed Area");
                temp.Add("Outside Exposed Area");
                temp.Add("Weight");
                temp.Add("Σ Losses");
                temp.Add("Efficiency");
                temp.Add("DOE Efficiency");
                temp.Add("Temp Rise");
                temp.Add("Cost");
                temp.Add("$/$");
                //foreach (var w in Windings)
                //    temp.Add(w.Name + " Worst Case Losses");

                return string.Join(",", temp.ToArray());
            }
        }
        /// <summary>
        /// String containing all the column data to be used when printing a design.
        /// 
        /// As of 04/03/2019, this property is really bad, should make users get the fields they want individually.
        /// </summary>
        [JsonIgnore]
        public string CsvData
        {
            get
            {
                List<string> temp = new List<string>
                {
                    DesignID,
                    Passed.ToString(),
                    Core.FluxDensity.ToString(),
                    AdjustedFluxDensity.ToString("F3"),
                    Core.Lamination.Grade.ToString(),
                    Core.Lamination.Thickness.ToString(),
                    Core.Lamination.StandardLamination.ToString(),
                    Core.Lamination.Tongue.ToString(),
                    Core.Stack.ToString(),
                    Core.Lamination.WindowWidth.ToString(),
                    Core.Lamination.WindowHeight.ToString(),
                    Core.Weight.ToString("F3"),
                    Core.Losses(AdjustedFluxDensity, (int)Specification.Frequency).ToString("F3"),
                    Core.Cost.ToString("F3"),
                    Tube.Name
                };
                List<Section> sections = new List<Section>();
                foreach (var w in Windings)
                {
                    foreach (var s in w.Sections)
                        sections.Add(s);
                }
                //sections = sections.OrderBy(s => s.SectionOrder).ToList();
                foreach (var s in sections)
                {
                    temp.Add(s.Name);
                    temp.Add(s.StartingVoltage.ToString());
                    temp.Add(s.EndingVoltage.ToString());
                    temp.Add(s.Wire.Name);
                    if (s.Wire is Foil)
                        temp.Add((s.Wire as Foil).BifilarNumber.ToString() + "H1W");
                    else
                        temp.Add(s.Wire.Bifilar.ToString().Substring(1));
                    temp.Add(s.Wire.WireMaterial.ToString());
                    temp.Add(s.CurrentDensity.ToString("F3"));
                    temp.Add(s.Turns.ToString());
                    temp.Add(s.SectionCurrent.ToString("F3"));
                    temp.Add(s.Weight.ToString("F3"));
                    temp.Add(s.Cost.ToString("F3"));
                }
                temp.Add(Build.ToString("F3"));
                temp.Add(Windings.Sum(w => w.InternalExposedArea).ToString("F3"));
                temp.Add(Windings.Sum(w => w.ExternalExposedArea).ToString("F3"));
                temp.Add(Weight.ToString("F3"));
                temp.Add(Losses.ToString("F2"));
                temp.Add(Efficiency.ToString("F3"));
                temp.Add(DoeEfficiency.ToString("F3"));
                temp.Add(TemperatureRise.ToString("F3"));
                temp.Add(Cost.ToString("F3"));
                temp.Add(((Core.Cost / (IsThreePhase ? 3 * Windings.Sum(w => w.Cost) : Windings.Sum(w => w.Cost))).ToString("F1")));
                //foreach (var w in Windings)
                //    temp.Add(w.WorstCaseLosses.ToString("F3"));

                return string.Join(",", temp.ToArray());
            }
        }
        /// <summary>
        /// String array containing all the column headers to be used when printing a design.
        /// 
        /// As of 04/03/2019, this property is really bad, this should be moved to either a static property or require user to print out fields manually.
        /// </summary>
        [JsonIgnore]
        public string[] CsvDataSplit
        {
            get
            {
                List<string> temp = new List<string>
                {
                    DesignID,
                    Passed.ToString(),
                    Core.FluxDensity.ToString(),
                    AdjustedFluxDensity.ToString("F3"),
                    Core.Lamination.Grade.ToString(),
                    Core.Lamination.Thickness.ToString(),
                    Core.Lamination.StandardLamination.ToString(),
                    Core.Lamination.Tongue.ToString(),
                    Core.Stack.ToString(),
                    Core.Lamination.WindowWidth.ToString(),
                    Core.Lamination.WindowHeight.ToString(),
                    Core.Weight.ToString("F3"),
                    Core.Losses(AdjustedFluxDensity, (int)Specification.Frequency).ToString("F3"),
                    Core.Cost.ToString("F3"),
                    Tube.Name
                };
                List<Section> sections = new List<Section>();
                foreach (var w in Windings)
                {
                    foreach (var s in w.Sections)
                        sections.Add(s);
                }
                //sections = sections.OrderBy(s => s.SectionOrder).ToList();
                foreach (var s in sections)
                {
                    temp.Add(s.Name);
                    temp.Add(s.StartingVoltage.ToString());
                    temp.Add(s.EndingVoltage.ToString());
                    temp.Add(s.Wire.Name);
                    if (s.Wire is Components.Derived.Foil)
                        temp.Add((s.Wire as Components.Derived.Foil).BifilarNumber.ToString() + "H1W");
                    else
                        temp.Add(s.Wire.Bifilar.ToString().Substring(1));
                    temp.Add(s.Wire.WireMaterial.ToString());
                    temp.Add(s.CurrentDensity.ToString("F3"));
                    temp.Add(s.Turns.ToString());
                    temp.Add(s.SectionCurrent.ToString("F3"));
                    temp.Add(s.Weight.ToString("F3"));
                    temp.Add(s.Cost.ToString("F3"));
                }
                temp.Add(Build.ToString("F3"));
                temp.Add(Windings.Sum(w => w.InternalExposedArea).ToString("F3"));
                temp.Add(Windings.Sum(w => w.ExternalExposedArea).ToString("F3"));
                temp.Add(Weight.ToString("F3"));
                temp.Add(Losses.ToString("F2"));
                temp.Add(Efficiency.ToString("F3"));
                temp.Add(DoeEfficiency.ToString("F3"));
                temp.Add(TemperatureRise.ToString("F3"));
                temp.Add(Cost.ToString("F3"));
                temp.Add(((Core.Cost / (IsThreePhase ? 3 * Windings.Sum(w => w.Cost) : Windings.Sum(w => w.Cost))).ToString("F1")));
                return temp.ToArray();
            }
        }

        /// <summary>
        /// Returns a string represnting this design in JSON format.
        /// 
        /// As of 04/03/2019, this property is really bad, even though I tried to mark properties are non-serializable they got serialized anyway, causing some pretty massive recursion loops in the sections/windings.        
        /// TODO: create a constructor would load a passed JSON string.
        /// </summary>
        public string JsonConvertObject()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
        }
        /// <summary>
        /// Returns the byte array of this design from binary serialization.
        /// 
        /// As of 04/03/2019, this property is probably bad, I have done much testing on this.
        /// /// TODO: create a constructor would load a passed byte array.
        /// </summary>
        public byte[] GetByteArray()
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            binaryFormatter.Serialize(ms, this);
            return ms.ToArray();
        }
    }
}
