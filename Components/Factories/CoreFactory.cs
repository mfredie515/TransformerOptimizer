using System;
using System.Collections.Generic;
using System.Text;
using TransformerOptimizer.Components.Base;
using TransformerOptimizer.Parameters;
using static TransformerOptimizer.Exceptions.Exceptions;

namespace TransformerOptimizer.Components.Factories
{
    /// <summary>
    /// Creates and stores a list of cores based on given parameters.
    /// </summary>
    public class CoreFactory
    {
        /// <summary>
        /// Constructor.
        /// 
        /// Generates a list of cores based on the given parameters.
        /// </summary>
        /// <param name="laminations">A list of laminations to create cores out of.</param>
        /// <param name="stackRange">IterableRange representing the range of stacks to create with.</param>
        /// <param name="fluxRange">IterableRange representing the range of stacks to create with.</param>
        /// <param name="stackingFactor">Stacking factor of the core.</param>
        /// <param name="destructionFactor">Destruction factor of the core.</param>
        /// <param name="excitationFactor">Excitation factor of the core.</param>
        /// <param name="tubeWindowMargin">Tube Window Margin to be used in the core.</param>
        /// <param name="func">Optional parameter, a function delegate that returns a boolean, and takes two integer inputs. As of 03/15/2019 only used with <see cref="Optimizer.IncrementCurrentProcessProgress(int, int)"/>.</param>
        protected internal CoreFactory(List<Lamination> laminations, RangeDouble stackRange, RangeDouble fluxRange, double stackingFactor, double destructionFactor, double excitationFactor, double tubeWindowMargin, Func<int, int, bool> func = null)
        {
            if ((stackRange.MinValue != stackRange.MaxValue) && stackRange.StepSize == 0)
                throw new NoCoresFound("Stack has different minimum and maximum values with a step size of zero.");
            int i = 0;
            int maxIterations = fluxRange.Iterations * stackRange.Iterations * laminations.Count;
            Cores = new List<Core>();
            fluxRange.NextRange = stackRange;
            while (true)
            {
                try
                {
                    foreach (var lam in laminations)
                    {
                        Cores.Add(new Core(lam, stackRange.CurrentValue, fluxRange.CurrentValue, tubeWindowMargin, stackingFactor, destructionFactor, excitationFactor));
                        func?.Invoke(++i, maxIterations);
                    }
                    fluxRange.IncrementValue();
                }
                catch (IterationFinishedException) { break; }
            }
        }

        /// <summary>
        /// List of cores.
        /// </summary>
        protected internal List<Core> Cores { get; }
    }
}
