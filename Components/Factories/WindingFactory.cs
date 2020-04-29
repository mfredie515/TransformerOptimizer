using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransformerOptimizer.Components.Base;
using static TransformerOptimizer.Data.LoadedData;
using static TransformerOptimizer.Exceptions.Exceptions;
using static TransformerOptimizer.Functions.Functions;

namespace TransformerOptimizer.Components.Factories
{
    /// <summary>
    /// Creates and stores a list of list of windings based on given parameters.
    /// </summary>
    public class WindingFactory
    {
        /// <summary>
        /// Constructor.
        ///
        /// Generates a list of list of windings for the design.
        /// 
        /// Taking in a list of base windings that contain no wires; a lists of wires that satisfy the conditions of each section in the windings as generated.        
        /// 
        /// Note: As the sections do not 'know' if they are to be used on an EI or UI style lamination and do not 'know' if they would be connected in series or parallel in the case of a UI lamination.
        /// The wires are found always using the full section current. Meaning that current density ranges for parallel windings would have to be half of the desired range.
        /// If using both series/EI and parallel the current density range may become too large to operate over in a resonable amount of time.
        /// 
        /// If not wires are found meeting a sections' conditions a <see cref="Exceptions.Exceptions.NoWiresFound"/> is thrown.
        /// 
        /// For each wire combination with a winding, a new winding object is created and put into a list with the other windings corresponding to a design.
        /// Once all windings and the respective lists are generated, the collection can be permutated to create more combinations.
        /// Permutations will keep the same section order within the winding, but allow the winding order to change.
        /// 
        /// Note: Permutations are on the order of N^2, be careful of large combinations of windings.
        /// 
        /// Example: Base Windings - A, B
        ///          Wires - A1, A2, B1, C1, C2
        ///          
        ///          Winding Combinations - {A.A1, B.B1}, {A.A2, B.B1}
        ///          Permutations - {A.A1, B.B1}, {B.B1, A.A1}, {A.A2, B.B1}, {B.B1, A.A2}
        ///         
        /// </summary>
        /// <param name="baseWindings">A list of windings to be the base for each generated winding.</param>
        /// <param name="usePermutations">Whether or not windings should be permutated in order to find possibly better designs.</param>
        /// <param name="func">Optional parameter, a function delegate that returns a boolean, and takes two integer inputs. As of 03/15/2019 only used with <see cref="Optimizer.IncrementCurrentProcessProgress(int, int)"/>.</param>
        protected internal WindingFactory(List<Winding> baseWindings, bool usePermutations, Func<int, int, bool> func)
        {
            Windings = new List<List<Winding>>();
            List<List<Winding>> indexedWindings = new List<List<Winding>>();
            List<Section> sections = new List<Section>();

            foreach (var w in baseWindings)
            {
                foreach (var s in w.Sections)
                {
                    s.Wires = Wires.GetWires(s.IterateWireMaterial, s.IterateWireShapes, s.BifilarRange, s.SectionCurrent / s.CurrentDensityMaximum, s.SectionCurrent / s.CurrentDensityMinimum, s);
                    sections.Add(s);
                    if (s.Wires.Count == 0)
                        throw new NoWiresFound("Not all sections have a wire combination. Failure Section: " + s.Name);
                }
            }

            sections = sections.OrderBy(s => s.SectionOrder).ToList();
            List<List<Section>> indexSections = new List<List<Section>>();
            foreach (var s in sections)
            {
                List<Section> newSections = new List<Section>();
                foreach (var wire in s.Wires)
                {
                    newSections.Add(new Section(s, wire));
                }
                indexSections.Add(newSections);
            }
            IEnumerable<IEnumerable<Section>> completedSections = indexSections.CartesianProduct();
            foreach (IEnumerable<Section> section in completedSections)
            {
                List<Base.Winding> toAddWindings = new List<Base.Winding>();
                foreach (var w in baseWindings)
                {
                    Base.Winding winding = new Base.Winding(w);
                    winding.Sections = section.GetSectionsForWinding(winding).ToList();
                    winding.SetSectionsToSelf();
                    toAddWindings.Add(winding);
                }
                indexedWindings.Add(toAddWindings);
            }

            if (usePermutations)
                Windings = GetPermutations(indexedWindings, baseWindings.Count, func);
            else
                Windings = indexedWindings;
        }

        /// <summary>
        /// List of list of windings.
        /// </summary>
        protected internal List<List<Winding>> Windings { get; }
    }
}
