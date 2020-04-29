using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransformerOptimizer.Components.Base;
using TransformerOptimizer.Parameters;
using static TransformerOptimizer.Data.Constants;
using static TransformerOptimizer.Data.LoadedData.Tubes;

namespace TransformerOptimizer.Components.Factories
{
    /// <summary>
    /// Creates and stores a list of designs based on given parameters.
    /// </summary>
    public class DesignFactory
    {
        /// <summary>
        /// Constructor.
        /// 
        /// Generates a list of designs and pushes them into a concurrect queue.
        /// Each core finds the closest tube that fits on it and then each core and tube combination gets combined with each list of windings to create a design.
        /// 
        /// If the core is a UI core, then depending on the selected UI styles either Series, Parallel, or Both styles of designs will be generated.
        /// </summary>
        /// <param name="specification">Specification containing base design information to be passed to designs.</param>
        /// <param name="cores">List of cores to be used.</param>
        /// <param name="windings">List of list of windings to be used.</param>
        /// <param name="uiStyles">IterableRange represnting the UI styles that should be generated as of 03/15/2019 only Series and Parallel are supported.</param>
        /// <param name="queue">Concurrent queue to push the completed designs into.</param>
        /// <param name="func">Optional parameter, a function delegate that returns a boolean, and takes two integer inputs. As of 03/15/2019 only used with <see cref="Optimizer.IncrementCurrentProcessProgress(int, int)"/>.</param>
        protected internal DesignFactory(Specification specification, List<Core> cores, List<List<Winding>> windings, RangeInteger uiStyles, ConcurrentQueue<Design> queue, Func<int, int, bool> func = null)
        {
            Designs = new List<Design>();

            bool rotatedTube;
            Tube tube;
            int maxIterations = cores.Count * windings.Count();
            int i = 0;
            foreach (Core core in cores)
            {
                rotatedTube = GetTube(core.Lamination.Tongue, core.Stack, out tube);
                foreach (List<Winding> winding in windings)
                {
                    if (core.Lamination.Shape == CoreShape.UI)
                    {
                        foreach (int uiStyle in uiStyles)
                        {
                            Designs.Add(new Design(specification, core, rotatedTube, tube, winding, GetUIStyle(uiStyle)));
                            func?.Invoke(++i, maxIterations);
                            queue.Enqueue(Designs.Last());
                        }
                    }
                    else
                    {
                        Designs.Add(new Design(specification, core, rotatedTube, tube, winding));
                        func?.Invoke(++i, maxIterations);
                        queue.Enqueue(Designs.Last());
                    }
                }
            }
        }

        /// <summary>
        /// List of designs.
        /// </summary>
        protected internal List<Design> Designs { get; }
    }
}
