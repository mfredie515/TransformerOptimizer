using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransformerOptimizer.Components.Base;
using TransformerOptimizer.Components.Derived;
using static TransformerOptimizer.Data.Constants;
using static TransformerOptimizer.Data.LoadedData.CoreLosses;

namespace TransformerOptimizer.Functions
{
    /// <summary>
    /// Contains various functions used throughout the program.
    /// </summary>
    public static class Functions
    {
        /// <summary>
        /// Returns the current density adjusted for the maximum and minimum bounds used by different wire materials.
        /// </summary>
        /// <param name="cd">Unadjusted current density of the wire.</param>
        /// <param name="wireMaterial">Material of the wire.</param>
        /// <returns>Adjusted current density.</returns>
        public static double CurrentDensityMaterialFactor(this double cd, WireMaterial wireMaterial)
        {
            if (wireMaterial == WireMaterial.ALUMINUM && cd > 1300)
                return 1300;
            if (wireMaterial == WireMaterial.COPPER && cd < 1000)
                return 1000;
            return cd;
        }

        /// <summary>
        /// Returns the increase in the <see cref="Section.LengthMeanTurn"/> in a section because of ducts.
        /// 
        /// The base equation is k * BF * H
        ///  - k is a constant based on the duct style.
        ///  - BF is the bulge factor of the duct, this is hardcoded to 1.
        ///  - H is the duct height.
        /// </summary>
        /// <param name="duct">Duct to operate on.</param>
        /// <returns>The increase in the mean length turn of a section.</returns>
        public static double DuctMeanLengthTurnIncrease(this Duct duct)
        {
            double k;
            switch (duct.DuctLocation)
            {
                case DuctLocation.FRONT_AND_BACK:
                    k = 2;
                    break;
                case DuctLocation.ALL_AROUND:
                    k = 3.14;
                    break;
                case DuctLocation.THREE_SIDED:
                    k = 2.57;
                    break;
                default:
                    k = 0;
                    break;
            }
            return k * duct.Height;
        }

        /// <summary>
        /// Return the increase in buildup in a section because of ducts.
        /// </summary>
        /// <param name="duct">Duct to operate on.</param>
        /// <returns>The increase in the buildup of a section.</returns>
        public static double DuctBuildUpIncrease(this Duct duct)
        {
            double k;
            switch (duct.DuctLocation)
            {
                case DuctLocation.FRONT_AND_BACK:
                    k = 0.5;
                    break;
                case DuctLocation.ALL_AROUND:
                    k = 0; // All Around ducts are 0 because they are already included in the buildup calculations of a section.
                    break;
                case DuctLocation.THREE_SIDED:
                    k = 0.75;
                    break;
                default:
                    k = 0;
                    break;
            }
            return k * duct.Height;
        }

        /// <summary>
        /// Extension method for a Winding to get the losses at the lowest tap. Optional parameter to set the operating temperature to get the losses at.
        /// </summary>
        /// <param name="winding">Winding to operate on.</param>
        /// <param name="operatingTemperature">Optional; temperature at which the losses should be calculated at.</param>
        /// <returns>Value of the losses at the windings lowest tap.</returns>
        public static double LowestTapLosses(this Winding winding, double operatingTemperature = 20)
        {
            // This is assuming that TapVoltages is in ascending order; currently I dont think there is any enforcement on this though.
            double lowestVoltage = winding.TapVoltages.First();
            // The lowest voltage/tap must be in the first section, either inside or at the end.
            // Resistance would be lowestVoltage / Section Ending Voltage * Section Resistance.
            double resistance = lowestVoltage / winding.Sections.First().EndingVoltage * winding.Sections.First().Resistance;
            double current = winding.CalcedVA / winding.PhaseVoltage;

            double wireConst = winding.Sections.First().Wire.TemperatureChangeFactor;
            return Math.Pow(current, 2) * resistance * ((wireConst + operatingTemperature) / (wireConst + 20));
        }

        /// <summary>
        /// Extension method for a <see cref="Grade"/> to return it's name as a string.
        /// 
        /// Uses extension method vs reflection because it should be better performance wise.
        /// </summary>
        /// <param name="grade">Grade to get name of.</param>
        /// <returns>String name of grade.</returns>
        public static string GetGradeToString(this Grade grade)
        {
            if (grade == Grade.M085_ButtLap || grade == Grade.M085_StepLap)
                return "M085";
            return grade.ToString();
        }

        /// <summary>
        /// Extension method for core to get its' losses.
        /// Uses core weight multiplied by lookup table/linear interpolation value of losses in W/Lb.
        /// </summary>
        /// <param name="core">Core to have its' losses calculated.</param>
        /// <param name="adjustedFluxDensity">Adjusted Flux Density of the core.</param>
        /// <param name="frequency">Frequency transformer is operating at.</param>
        /// <param name="useLegWeights">If the losses should be calculated using only the leg weight.</param>
        /// <returns>Total Core Losses</returns>
        public static double Losses(this Core core, double adjustedFluxDensity, int frequency, bool useLegWeights = false)
        {
            return GetCoreLosses(adjustedFluxDensity, core.Lamination.Thickness, frequency, core.Lamination.Grade.GetGradeToString()) * core.DestructionFactor * core.Lamination.DestructionFactor * (useLegWeights ? core.WeightLegs : core.Weight);
        }

        /// <summary>
        /// Gets the permutations of a sequence of sequences.
        /// If using a single sequence it is about equivalent to the other GetPermutations function.
        /// Able to call a function with two integer parameters and a boolean output.
        /// </summary>
        /// <typeparam name="T">Type of object in sequence.</typeparam>
        /// <param name="list">Sequence to get permutations.</param>
        /// <param name="length">Length of list.</param>
        /// <param name="toCall">Optional function to call while calculating permutations.</param>
        /// <returns>Sequence containing permutations of input sequences.</returns>
        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<IEnumerable<T>> list, int length, Func<int, int, bool> toCall = null)
        {
            List<List<T>> toReturn = new List<List<T>>();
            IEnumerable<IEnumerable<T>> holder;

            int maxIterations, i;
            foreach (List<T> l in list)
            {
                holder = GetPermutations<T>(l, length);
                maxIterations = holder.Count() * list.Count();
                i = 0;
                foreach (IEnumerable<T> t in holder)
                {
                    i += 1;
                    toReturn.Add(t.ToList());
                    toCall?.Invoke(i, maxIterations);
                }
            }

            return toReturn;
        }

        /// <summary>        
        /// Gets the permutations of a sequence of sequences.
        /// If using a single sequence it is about equivalent to the other GetPermutations function.
        /// Able to call a function with two integer parameters and a boolean output.
        /// 
        /// Input and Output of this function use List{List{T}} instead on IEnumerable{IEnumerable{T}}."
        /// </summary>
        /// <typeparam name="T">Type of object in sequence.</typeparam>
        /// <param name="list">Sequence to get permutations.</param>
        /// <param name="length">Length of list.</param>
        /// <param name="toCall">Optional function to call while calculating permutations.</param>
        /// <returns>Sequence containing permutations of input sequences.</returns>
        public static List<List<T>> GetPermutations<T>(List<List<T>> list, int length, Func<int, int, bool> toCall = null)
        {
            List<List<T>> toReturn = new List<List<T>>();
            IEnumerable<IEnumerable<T>> holder;

            int maxIterations, i;
            foreach (List<T> l in list)
            {
                holder = GetPermutations<T>(l, length);
                maxIterations = holder.Count() * list.Count();
                i = 0;
                foreach (IEnumerable<T> t in holder)
                {
                    i += 1;
                    toReturn.Add(t.ToList());
                    toCall?.Invoke(i, maxIterations);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the permutations of a sequence.
        /// </summary>
        /// <typeparam name="T">Type of object in sequence.</typeparam>
        /// <param name="list">Sequence to get permutations.</param>
        /// <param name="length">Length of list.</param>
        /// <returns>Sequence containing each permutation of sequences.</returns>
        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1).SelectMany((t) => list.Where(e => !t.Contains(e)), (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        /// <summary>
        /// Extension Method
        /// Creates an enumerable of the Cartesian Product of a sequence.
        /// </summary>
        /// <typeparam name="T">Type of Sequence</typeparam>
        /// <param name="sequences">IEnumerable{IEnumerable{T}} sequence to be worked on</param>
        /// <returns>Cartesian Product of the sequence</returns>
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item }));
        }

        /// <summary>
        /// Determines the number of different windings references contained in a sequence of sections.
        /// </summary>
        /// <param name="sections">Sequence of sections.</param>
        /// <returns>The number of different windings referenced within the different sections.</returns>
        public static int NumberOfWindings(this IEnumerable<Components.Base.Section> sections)
        {
            HashSet<string> windings = new HashSet<string>();
            foreach (Components.Base.Section section in sections)
                windings.Add(section.WindingName);
            return windings.Count;
        }

        /// <summary>
        /// Processes a sequence of sections and returns an IEnumerable of all the sections that are for a given winding.
        /// </summary>
        /// <param name="sections">IEnumerable{Section} Sequence to be processed.</param>
        /// <param name="winding">Winding to which sections should be matched to.</param>
        /// <returns>An IEnumerable{Section} of all the sections in the sequence that are for the given winding.</returns>
        public static IEnumerable<Components.Base.Section> GetSectionsForWinding(this IEnumerable<Section> sections, Winding winding)
        {
            return sections.Where(s => s.WindingName == winding.Name).OrderBy(s => s.SectionOrder);
        }

        /// <summary>
        /// Performs linear interpolation for a value x.
        /// </summary>
        /// <param name="x">Undetermined Input</param>
        /// <param name="yAbove">Closest value of result higher than x.</param>
        /// <param name="yBelow">Closest value of result lower than x.</param>
        /// <param name="xAbove">Closest value of input higher than x.</param>
        /// <param name="xBelow">Closest value of input lower than x.</param>
        /// <returns>Interpolated value for x.</returns>
        public static double LinearInterpolation(double x, double yAbove, double yBelow, double xAbove, double xBelow)
        {
            return yBelow + (x - xBelow) * ((yAbove - yBelow) / (xAbove - xBelow));
        }

        /// <summary>
        /// Returns the integer multiplier value for a given bifilar
        /// </summary>
        /// <param name="bifilar">Wire bifilar</param>
        /// <param name="wire">Optional wire reference, mostly to be used with Foil windings.</param>
        /// <returns>Integer multiplier of given bifilar</returns>
        public static int BifilarMultiplier(Data.Constants.Bifilar bifilar, Wire wire = null)
        {
            switch (bifilar)
            {
                case Bifilar._1H1W:
                    return 1;
                case Bifilar._1H2W:
                    return 2;
                case Bifilar._1H3W:
                    return 3;
                case Bifilar._1H4W:
                    return 4;
                case Bifilar._XH1W:
                    if (wire != null)
                    {
                        if (wire is Foil)
                            return (wire as Foil).BifilarNumber;
                        else
                            return 1;
                    }
                    else
                        return 1;
                default:
                    return 1;
            }
        }

        /// <summary>
        /// Uses MW-1000 (2006) for the corner radius of conductor
        /// </summary>
        /// <param name="wire">IWire to be calculated upon.</param>
        /// <returns>Area of wire conductor minus the area removed by the corner radius of the conductor.</returns>
        public static double RemoveCornerRadius(this Components.Base.Wire wire)
        {
            double correctedArea;
            if (wire.WireShape == Data.Constants.WireShape.RECTANGULAR)
            {
                double radius;
                if (wire.ConductorThickness <= 0.063)
                    radius = wire.ConductorThickness / 2;
                else if (wire.ConductorThickness <= 0.088)
                {
                    if (wire.ConductorWidth <= 0.187)
                        radius = 0.02;
                    else
                        radius = 0.031;
                }
                else if (wire.ConductorThickness <= 0.124)
                {
                    if (wire.ConductorWidth <= 0.187)
                        radius = 0.026;
                    else
                        radius = 0.031;
                }
                else if (wire.ConductorThickness <= 0.157)
                    radius = 0.031;
                else
                    radius = 0.039;
                correctedArea = (wire.ConductorThickness * wire.ConductorWidth) - 0.8584 * Math.Pow(radius, 2.0);
            }
            else
                correctedArea = wire.ConductorThickness * wire.ConductorWidth;
            return correctedArea * BifilarMultiplier(wire.Bifilar);
        }

        /// <summary>
        /// Converts from inches to meters.
        /// </summary>
        /// <param name="inches">Value of inches to convert.</param>
        /// <returns>Converted value in meters.</returns>
        public static double InchToMeter(double inches)
        {
            return inches * 0.0254;
        }

        /// <summary>
        /// Converts from square inches to square millimeters.
        /// </summary>
        /// <param name="inches">Value of square inches to convert.</param>
        /// <returns>Converted value in square milimeters.</returns>
        public static double SqInchToSqMilimeter(double inches)
        {
            return inches * 645.16;
        }
    }
}
