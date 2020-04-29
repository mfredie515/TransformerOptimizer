using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TransformerOptimizer.Parameters;
using TransformerOptimizer.Components.Base;
using TransformerOptimizer.Components.Factories;
using static TransformerOptimizer.Exceptions.Exceptions;

namespace TransformerOptimizer
{
    /// <summary>
    /// Takes user design input and calculates every possible transformer and returns the results to the user.
    /// </summary>
    public class Optimizer
    {
        private Stopwatch stopwatch;
        private Thread creationThread;
        private List<Thread> validationThreads;
        private ConcurrentQueue<Design> creationQueue, completedQueue;
        private readonly int numberValThreads;
        private Dictionary<string, IterableRange> parameters;
        private RangeCombinationSkips<int> laminationSkips;
        private RangeLaminationDetails laminationDetails;
        private bool usePermutations;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="numberValThreads">Number of validation threads to create when running the optimizer.</param>
        public Optimizer(int numberValThreads)
        {
            this.numberValThreads = numberValThreads;
            this.parameters = new Dictionary<string, IterableRange>();
            Initialize();
        }
        /// <summary>
        /// Initialize the parameter ranges.
        /// </summary>
        private void Initialize()
        {
            this.Specification = new Specification();
            this.Failure = new FailureParameters();
            this.stopwatch = new Stopwatch();

            //Core Parameters
            parameters.Add("FluxDensity", new RangeDouble("FluxDensity", 0, 0, 0));
            parameters.Add("Tongue", new RangeDouble("Tongue", 0, 0, 0));
            parameters.Add("Yoke", new RangeDouble("Yoke", 0, 0, 0));
            parameters.Add("Stack", new RangeDouble("Stack", 0, 0, 0));
            parameters.Add("Width", new RangeDouble("Width", 0, 0, 0));
            parameters.Add("Height", new RangeDouble("Height", 0, 0, 0));
            parameters.Add("StdLamination", new RangeInteger("StdLamination", 0, 0, 0));
            parameters.Add("LamShape", new RangeInteger("LamShape", 0, 0, 0));
            parameters.Add("LamGrade", new RangeInteger("LamGrade", 0, 0, 0));
            parameters.Add("LamThickness", new RangeInteger("LamThickness", 0, 0, 0));
            parameters.Add("UI_Styles", new RangeInteger("UI_Styles", 0, 0, 0));
            laminationSkips = new RangeCombinationSkips<int>();
            laminationDetails = new RangeLaminationDetails("LaminationDetails", null);
            //parameters.Add("LamDetails", new CoreRange("LamDetails", null));
        }

        /// <summary>
        /// Returns an IterableRange with the given name.
        /// 
        /// IterableRange is a base class for most ranges so the result should be cast to whatever range is needed.
        /// </summary>
        /// <param name="name">Name of the IterableRange to find.</param>
        /// <returns>The IterableRange that has the given name.</returns>
        public IterableRange GetRange(string name)
        {
            return parameters[name];
        }
        /// <summary>
        /// Range of the laminations to skip; very confusing I know.
        /// 
        /// As of 04/04/2019 this should be removed and a new way of generating laminations that just takes an array/list input so that the user can skip what they want.
        /// </summary>
        public RangeCombinationSkips<int> LaminationSkips { get { return laminationSkips; } }
        /// <summary>
        /// Laminations to iterate over.
        /// </summary>
        public RangeLaminationDetails ToIterateLaminations { get { return laminationDetails; } }
        /// <summary>
        /// Starts threads that will generate all of the possible designs from the input ranges.
        /// 
        /// As of 04/04/2019 this should be called by another thread as it will lock up the calling thread untill it is finished.
        /// Indeally, this may be able to be converted to an async function to allow the main thread to call it and get the result.
        /// </summary>
        /// <param name="usePermutations">Whether or not the winding factory should use all permutations of windings.</param>
        /// <returns>A list of all the designs generated given the user input ranges.</returns>
        public List<Design> Run(bool usePermutations)
        {
            this.usePermutations = usePermutations;
            if (!usePermutations && !Specification.BaseWindings.Any(w => w.WindingOrder == -1))
                Specification.BaseWindings = Specification.BaseWindings.OrderBy(w => w.WindingOrder).ToList();
            AbortThreads();
            HasException = false;
            ExceptionMessage = null;
            Finished = false;
            FinishedGeneration = false;
            creationQueue = new ConcurrentQueue<Design>();
            completedQueue = new ConcurrentQueue<Design>();
            validationThreads = new List<Thread>();
            creationThread = new Thread(new ThreadStart(GenerateDesigns));
            for (int i = 0; i < numberValThreads; i++)
                validationThreads.Add(new Thread(new ThreadStart(ValidateDesigns)));

            stopwatch.Restart();
            creationThread.Start();
            foreach (Thread thread in validationThreads)
                thread.Start();

            while (ThreadIsAlive()) ;

            AbortThreads();
            CurrentProcess = "Finished";
            return completedQueue.Where(d => d.Nulled == false).ToList();

        }
        /// <summary>
        /// Whether a not any thread is alive/still running.
        /// </summary>
        /// <returns>Whether any thread is still running.</returns>
        private bool ThreadIsAlive()
        {
            bool b = false;
            b |= creationThread.IsAlive;
            foreach (Thread thread in validationThreads)
                b |= thread.IsAlive;
            return b;
        }
        /// <summary>
        /// Aborts all currently running threads used by the optimizer.
        /// </summary>
        public void AbortThreads()
        {
            CurrentProcess = "Process Aborted";
            if (creationThread != null && creationThread.IsAlive)
            {
                try
                {
                    creationThread.Abort();
                }
                catch (Exception) { }
            }
            if (validationThreads != null && validationThreads.Count > 0)
            {
                foreach (Thread t in validationThreads)
                {
                    if (t != null && t.IsAlive)
                    {
                        try
                        {
                            t.Abort();
                        }
                        catch (Exception) { }
                    }
                }
            }
            stopwatch.Stop();
            Finished = true;
        }

        #region Threading Methods
        /// <summary>
        /// Threaded function to generate the designs.
        /// 
        /// Creates the laminations, then cores, then windings, then finally the designs.
        /// </summary>
        private void GenerateDesigns()
        {
            try
            {
                CurrentProcess = "Generating Laminations";
                //Laminations = new LaminationFactory(laminationSkips, (RangeInteger)parameters["StdLamination"], (RangeInteger)parameters["LamShape"], Specification.Phase, (RangeInteger)parameters["LamGrade"], (RangeInteger)parameters["LamThickness"],
                //    (RangeDouble)parameters["Tongue"], (RangeDouble)parameters["Yoke"], (RangeDouble)parameters["Width"], (RangeDouble)parameters["Height"], IncrementCurrentProcessProgress);
                Laminations = new LaminationFactory(ToIterateLaminations, Specification.Phase, (RangeDouble)parameters["Tongue"], (RangeDouble)parameters["Yoke"], (RangeDouble)parameters["Width"], (RangeDouble)parameters["Height"], IncrementCurrentProcessProgress);
                if (Laminations.Laminations.Count == 0)
                    throw new NoCoresFound("No laminations could be generated with the given parameters.");
                CurrentProcess = "Generating Cores";
                Cores = new CoreFactory(Laminations.Laminations, (RangeDouble)parameters["Stack"], (RangeDouble)parameters["FluxDensity"], Specification.StackingFactor,
                    Specification.DestructionFactor, Specification.ExcitationFactor, Specification.TubeWindowMargin, IncrementCurrentProcessProgress);
                CurrentProcess = "Gathering Tubes";
                //Now uses static data
                CurrentProcess = "Gathering Wires";
                //Now uses static data
                CurrentProcess = "Generating Windings";
                Windings = new WindingFactory(Specification.BaseWindings, usePermutations, IncrementCurrentProcessProgress);
                CurrentProcess = "Gathering Core Losses";
                //Now uses static data
                CurrentProcess = "Creating Designs";
                Designs = new DesignFactory(Specification, Cores.Cores, Windings.Windings, (RangeInteger)parameters["UI_Styles"], creationQueue, IncrementCurrentProcessProgress);
                FinishedGeneration = true;
            }
            catch (TransformerOptimizerException ex)
            {
                HasException = true;
                ExceptionMessage = ex.Message;
                AbortThreads();
            }
        }
        /// <summary>
        /// Threaded function to iterate through the creation queue and call <see cref="Design.PerformCalculations"/> and <see cref="Design.PerformValidation(FailureParameters)"/> then add it to the completed queue.
        /// 
        /// As of 04/04/2019 how this works is multiple threads all run this method, and are locked out by the concurrent queue; ideally if there is one manager thread that distributes the designs across other thread queues it may be faster.
        /// </summary>
        private void ValidateDesigns()
        {
            while (!FinishedGeneration || creationQueue.Count > 0)
            {
                if (creationQueue.TryDequeue(out Design design))
                {
                    if (FinishedGeneration)
                        CurrentProcess = "Validating Designs";
                    design.PerformCalculations();
                    design.PerformValidation(Failure);
                    completedQueue.Enqueue(design);
                }
            }
        }
        /// <summary>
        /// Determines what percent should be calculated for <see cref="CurrentProcessProgress"/>.
        /// </summary>
        /// <param name="value">The current 'index' of iterations.</param>
        /// <param name="maxValue">The maximum number of iterations.</param>
        /// <returns>True</returns>
        private bool IncrementCurrentProcessProgress(int value, int maxValue)
        {
            if (Convert.ToInt32(((double)value / maxValue) * 100) > 100)
                CurrentProcessProgress = 100;
            else
                CurrentProcessProgress = Convert.ToInt32(((double)value / maxValue) * 100);
            return true;
        }
        #endregion

        /// <summary>
        /// Reference to design specification.
        /// </summary>
        public Specification Specification { get; private set; }
        /// <summary>
        /// Reference to failure parameters.
        /// </summary>
        public FailureParameters Failure { get; private set; }
        /// <summary>
        /// Reference to lamination factory.
        /// </summary>
        private LaminationFactory Laminations { get; set; }
        /// <summary>
        /// Reference to core factory.
        /// </summary>
        private CoreFactory Cores { get; set; }
        /// <summary>
        /// Reference to winding factory.
        /// </summary>
        private WindingFactory Windings { get; set; }
        /// <summary>
        /// Reference to design factory.
        /// </summary>
        private DesignFactory Designs { get; set; }
        /// <summary>
        /// Set to True once all designs have been completed.
        /// </summary>
        public bool Finished { get; private set; }
        /// <summary>
        /// Whether or not <see cref="GenerateDesigns"/> has finished making all combinations.
        /// </summary>
        private bool FinishedGeneration { get; set; }
        /// <summary>
        /// Number of designs that have yet to have <see cref="Design.PerformCalculations"/> and <see cref="Design.PerformValidation(FailureParameters)"/> ran on them.
        /// </summary>
        public int QueuedDesigns { get { return creationQueue.Count; } }
        /// <summary>
        /// Number of designs that have had <see cref="Design.PerformCalculations"/> and <see cref="Design.PerformValidation(FailureParameters)"/> ran on them.
        /// </summary>
        public int CompletedDesigns { get { return completedQueue.Count; } }
        /// <summary>
        /// Number of completed designs that are considered to be passing. See <see cref="Design.Passed"/>.
        /// </summary>
        public int PassedDesigns { get { return completedQueue.Where(d => d.Passed == true).Count(); } }
        /// <summary>
        /// Number of completed designs that are considered failures. See <see cref="Design.Passed"/>.
        /// </summary>
        public int FailedDesigns { get { return CompletedDesigns - PassedDesigns - NulledDesigns; } }
        /// <summary>
        /// Number of completed designs that are considered nulled. See <see cref="Design.Nulled"/>.
        /// </summary>
        public int NulledDesigns { get { return completedQueue.Where(d => d.Nulled == true).Count(); } }
        /// <summary>
        /// How much time in milliseconds the optimizier is running for.
        /// </summary>
        public long TimeElapsed { get { return stopwatch.ElapsedMilliseconds; } }
        /// <summary>
        /// Whether or not a <see cref="TransformerOptimizerException"/> was thrown when generating designs.
        /// </summary>
        public bool HasException { get; private set; }
        /// <summary>
        /// The message contained by a <see cref="TransformerOptimizerException"/> that occurs when generating designs.
        /// </summary>
        public string ExceptionMessage { get; private set; }
        /// <summary>
        /// A string describing the current process.
        /// </summary>
        public string CurrentProcess { get; private set; }
        /// <summary>
        /// The percent finished of the current process.
        /// </summary>
        public int CurrentProcessProgress { get; private set; }
    }
}
