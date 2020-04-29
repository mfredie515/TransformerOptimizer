using System;
using System.Collections.Generic;
using System.Text;
using TransformerOptimizer.Data;
using static TransformerOptimizer.Exceptions.Exceptions;

namespace TransformerOptimizer.Parameters
{
    /// <summary>
    /// Implementation of IterableRange of type int.
    /// </summary>
    public class RangeInteger : IterableRange<int>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of object.</param>
        /// <param name="minValue">Minimum value for the iteration.</param>
        /// <param name="maxValue">Maximum value for the iteration.</param>
        /// <param name="stepSize">Step size value for the iteration.</param>
        protected internal RangeInteger(string name, int minValue, int maxValue, int stepSize) : base(name, minValue, maxValue, stepSize) { }

        /// <summary>
        /// Returns the number of iterations that this range will go over.
        /// </summary>
        protected internal override int Iterations { get { return StepSize == 0 ? 1 : Convert.ToInt32(((MaxValue - MinValue) / StepSize) + 1); } }

        /// <summary>
        /// Returns an IEnumerator for the object with each value that would be iterated.
        /// </summary>
        /// <returns>An IEnumerator with each value that would be iterated.</returns>
        public override IEnumerator<int> GetEnumerator()
        {
            if (StepSize == 0)
                yield return MinValue;
            else
            {
                for (int i = MinValue; i <= MaxValue; i += StepSize)
                    yield return i;
            }
        }

        /// <summary>
        /// Increments the current value by the step size.
        /// If the maximum value is exceeded the IncrementValue function of the referenced IterableRange in NextRange is called.
        /// If no IterableRange is referenced then an IterationFinishedException is thrown. 
        /// </summary>
        protected internal override void IncrementValue()
        {
            CurrentValue += StepSize;
            if (CurrentValue > MaxValue)
            {
                CurrentValue = MinValue;
                if (NextRange != null)
                    NextRange.IncrementValue();
                else
                    throw new IterationFinishedException("Ranges have iterated throughout.");
            }
            else if (StepSize == 0 && CurrentValue == MaxValue)
            {
                CurrentValue = MinValue;
                if (NextRange != null)
                    NextRange.IncrementValue();
                else
                    throw new IterationFinishedException("Ranges have iterated throughout.");
            }
        }
    }

    /// <summary>
    /// Implementation of IterableRange of type double.
    /// </summary>
    public class RangeDouble : IterableRange<double>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of object.</param>
        /// <param name="minValue">Minimum value for the iteration.</param>
        /// <param name="maxValue">Maximum value for the iteration.</param>
        /// <param name="stepSize">Step size value for the iteration.</param>
        protected internal RangeDouble(string name, double minValue, double maxValue, double stepSize) : base(name, minValue, maxValue, stepSize) { }

        /// <summary>
        /// Returns the number of iterations that this range will go over.
        /// </summary>
        protected internal override int Iterations { get { return StepSize == 0.0 ? 1 : Convert.ToInt32(((MaxValue - MinValue) / StepSize) + 1); } }

        /// <summary>
        /// Returns an IEnumerator for the object with each value that would be iterated.
        /// </summary>
        /// <returns>An IEnumerator with each value that would be iterated.</returns>
        public override IEnumerator<double> GetEnumerator()
        {
            if (StepSize == 0)
                yield return MinValue;
            else
            {
                for (double d = MinValue; d <= MaxValue; d += StepSize)
                    yield return d;
            }
        }

        /// <summary>
        /// Increments the current value by the step size.
        /// If the maximum value is exceeded the IncrementValue function of the referenced IterableRange in NextRange is called.
        /// If no IterableRange is referenced then an IterationFinishedException is thrown. 
        /// </summary>
        protected internal override void IncrementValue()
        {
            CurrentValue += StepSize;
            if (CurrentValue > MaxValue)
            {
                CurrentValue = MinValue;
                if (NextRange != null)
                    NextRange.IncrementValue();
                else
                    throw new IterationFinishedException("Ranges have iterated throughout.");
            }
            else if (StepSize == 0 && CurrentValue == MaxValue)
            {
                CurrentValue = MinValue;
                if (NextRange != null)
                    NextRange.IncrementValue();
                else
                    throw new IterationFinishedException("Ranges have iterated throughout.");
            }
        }
    }

    /// <summary>
    /// Implementation of IterableRand of object LaminationDetails.
    /// </summary>
    public class RangeLaminationDetails : IterableRange<Constants.LaminationDetails>
    {
        private int _index;

        /// <summary>
        /// Constructor.
        /// 
        /// Passes null to the base constructor for MinValue, MaxValue, and StepSize.
        /// </summary>
        /// <param name="name">Name of object.</param>
        /// <param name="laminationDetails">Array of laminations to iterate with.</param>
        protected internal RangeLaminationDetails(string name, Constants.LaminationDetails[] laminationDetails = null) : base(name, null, null, null)
        {
            this.LaminationDetails = laminationDetails;
            _index = 0;
        }

        /// <summary>
        /// Array of laminations to iterate with.
        /// </summary>
        public Constants.LaminationDetails[] LaminationDetails { get; set; }

        /// <summary>
        /// Returns the number of iterations that this range will go over.
        /// </summary>
        protected internal override int Iterations { get { return LaminationDetails.Length; } }

        /// <summary>
        /// Returns an IEnumerator for the object with each value that would be iterated.
        /// </summary>
        /// <returns>An IEnumerator with each value that would be iterated.</returns>
        public override IEnumerator<Constants.LaminationDetails> GetEnumerator()
        {
            foreach (Constants.LaminationDetails ld in LaminationDetails)
            {
                yield return ld;
            }
        }

        /// <summary>
        /// Overrides base method; returns the current lamination detail.
        /// </summary>
        protected internal override Constants.LaminationDetails CurrentValue { get { return LaminationDetails[_index]; } set => base.CurrentValue = value; }

        /// <summary>
        /// Increments the current value to the next lamination detail.
        /// If the maximum value is exceeded the IncrementValue function of the referenced IterableRange in NextRange is called.
        /// If no IterableRange is referenced then an IterationFinishedException is thrown. 
        /// </summary>
        protected internal override void IncrementValue()
        {
            _index += 1;
            if (_index >= Iterations)
            {
                _index = 0;
                if (NextRange != null)
                    NextRange.IncrementValue();
                else
                    throw new IterationFinishedException("Ranges have iterated throughout.");
            }
        }
    }
}
