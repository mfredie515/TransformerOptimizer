using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TransformerOptimizer.Parameters
{
    /// <summary>
    /// Base class of an iterable range.
    /// </summary>
    public abstract class IterableRange
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the object.</param>
        protected internal IterableRange(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Name of the object.
        /// </summary>
        protected internal string Name { get; }
        /// <summary>
        /// Reference to another IterableRange object, should be used in the implementation of IncrementValue().
        /// </summary>
        protected internal IterableRange NextRange { get; set; }

        /// <summary>
        /// Abstract function, implementation should make object iterate through values from MinValue to MaxValue with a step size of StepSize.
        /// If the maximum value is exceeded, it should reset its' current value to the minimum and call IncrementValue of the IterableRange reference with NextRange.
        /// If no IterableRange is referenced exception IterationFinishedException should be thrown.
        /// </summary>
        protected internal abstract void IncrementValue();
        /// <summary>
        /// Abstract function, implementation should return the maximum number of iterations that will occur.
        /// Numeric types can probably simply return (MaxValue - MinValue) / StepSize.
        /// Other types will probably need to be have overloaded operators to do the same or have a more complex function.
        /// </summary>
        protected internal abstract int Iterations { get; }
    }

    /// <summary>
    /// Base class of an iterable range of type T. 
    /// </summary>
    /// <typeparam name="T">Type of Iterable Range, currently no restrictions as of 03/15/2019.</typeparam>
    public abstract class IterableRange<T> : IterableRange, IEnumerable<T>, IEnumerable
    {
        /// <summary>
        /// Constructor.
        /// Sets the current value of the object to the supplied minimum value.
        /// </summary>
        /// <param name="name">Name of the object.</param>
        /// <param name="minValue">Minimum value for the iteration.</param>
        /// <param name="maxValue">Maximum value for the iteration.</param>
        /// <param name="stepSize">Step Size value for the iteration.</param>
        protected internal IterableRange(string name, T minValue, T maxValue, T stepSize) : base(name)
        {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.StepSize = stepSize;
            this.CurrentValue = minValue;
        }

        /// <summary>
        /// Minimum value for the iteration.
        /// </summary>
        public T MinValue { get; private set; }
        /// <summary>
        /// Maximum value for the iteration.
        /// </summary>
        public T MaxValue { get; private set; }
        /// <summary>
        /// Step Size value for the iteration.
        /// </summary>
        public T StepSize { get; private set; }
        /// <summary>
        /// Current value of the object, bounded by [MinValue, MaxValue]
        /// </summary>
        protected internal virtual T CurrentValue { get; set; }

        /// <summary>
        /// Sets the range of the object to the supplied parameters.
        /// Sets the current value to supplied minimum value.
        /// </summary>
        /// <param name="minValue">Minimum value for the iteration.</param>
        /// <param name="maxValue">Maximum value for the iteration.</param>
        /// <param name="stepSize">Step Size value for the iteration.</param>
        public virtual void SetRange(T minValue, T maxValue, T stepSize)
        {
            this.CurrentValue = minValue;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.StepSize = stepSize;
        }
        /// <summary>
        /// Sets the current value of CurrentValue.
        /// A publicly exposed version of the protected internal set of CurrentValue.
        /// Currently (03/15/2019) no verification on value, should be made to restrict value to be within bounds [MinValue, MaxValue]
        /// </summary>
        /// <param name="value">What the current value should be set to.</param>
        public virtual void SetCurrentValue(T value)
        {
            CurrentValue = value;
        }

        /// <summary>
        /// Abstract function, implementation should return IEnumerator for object.
        /// </summary>
        /// <returns>IEnumerator of object, values from the minimum to maximum in objects step size increments.</returns>
        public abstract IEnumerator<T> GetEnumerator();

        /// <summary>
        /// Required by IEnumerable; returns value of GetEnumerator().
        /// </summary>
        /// <returns>Reference to abstract function GetEnumerator().</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    /// <summary>
    /// Determines if certain value/values should be skipped during iteration.
    /// 
    /// Honestly pretty confusing, values must be of same type but different values.
    /// Current usage is for comparing two enum's, as they can be cast down to two integers.    
    /// </summary>
    /// <typeparam name="T">Type to store, must implement IEquatable, no other restrictions as of 03/15/2019.</typeparam>
    public class RangeCombinationSkips<T> where T : IEquatable<T>
    {
        /// <summary>
        /// List of holder objects
        /// </summary>
        private List<Holder> list;

        /// <summary>
        /// Constructor.
        /// Initializes list.
        /// </summary>
        protected internal RangeCombinationSkips()
        {
            list = new List<Holder>();
        }

        /// <summary>
        /// Adds a new holder object to the list.
        /// </summary>
        /// <param name="field1">Field1 of holder object.</param>
        /// <param name="field2">Field2 of holder object.</param>
        public virtual void AddSkipValues(T field1, T field2)
        {
            list.Add(new Holder(field1, field2));
        }
        /// <summary>
        /// Clears the list.
        /// </summary>
        public void Clear()
        {
            list.Clear();
        }

        /// <summary>
        /// Determines if the given combination of fields should be skipped.
        /// 
        /// As of 03/15/2019 only usage is determining whether or not skip a certain grade and thickness combination.    
        /// 
        /// Example: Each field is an index in two seperate arrays and each field increments from 1 to 4, representing a streets and houses on the street.
        ///          If the number of houses on street 2 is only 2, then when field2 increments to 3 an unexpected action may occur.
        ///          So in this case if field1 corresponds to street 2, any field2 values that would represent house 3 are determined to be skipped.
        ///          
        /// It's honestly really dumb and should be replaced by a better method.
        /// </summary>
        /// <param name="field1">Field to be compared to a holders Field1.</param>
        /// <param name="field2">Field to be compared to a holders Field2.</param>
        /// <returns>Boolean determining if the combination should be skipped. True = Skip, False = Dont Skip.</returns>
        protected internal bool SkipValue(T field1, T field2)
        {
            if (list.Where(h => EqualityComparer<T>.Default.Equals(h.Field1, field1) && EqualityComparer<T>.Default.Equals(h.Field2, field2)).Count() > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Holder object for RangeCombinationSkips.
        /// Contains two fields
        /// </summary>
        private class Holder
        {
            /// <summary>
            /// Constructor.            
            /// </summary>
            /// <param name="field1">Field 1</param>
            /// <param name="field2">Field 2</param>
            public Holder(T field1, T field2)
            {
                this.Field1 = field1;
                this.Field2 = field2;
            }

            /// <summary>
            /// Value of first field.
            /// </summary>
            protected internal T Field1 { get; }
            /// <summary>
            /// Value of second field
            /// </summary>
            protected internal T Field2 { get; }
        }
    }
}
