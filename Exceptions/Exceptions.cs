using System;
using System.Collections.Generic;
using System.Text;

namespace TransformerOptimizer.Exceptions
{
    /// <summary>
    /// Static class containing different types of exceptions thrown by this DLL.
    /// </summary>
    public static class Exceptions
    {
        /// <summary>
        /// Custom exception class used to determine is thrown exception is from an optimization operation or generic exception.
        /// </summary>
        public abstract class TransformerOptimizerException : Exception
        {
            /// <summary>
            /// Base constructor.
            /// </summary>
            public TransformerOptimizerException() { }
            /// <summary>
            /// Overloaded constructor for message.
            /// </summary>
            /// <param name="message">Message of the exception.</param>
            public TransformerOptimizerException(string message) : base(message) { }
            /// <summary>
            /// Overloaded constructor for message and inner exception.
            /// </summary>
            /// <param name="message">Message of the exception.</param>
            /// <param name="inner">Inner exception.</param>
            public TransformerOptimizerException(string message, Exception inner) : base(message, inner) { }
        }
        /// <summary>
        /// Thrown when an IterableRange object increments its' value to the maximum and the IterableRange object does not have any reference to another in its' Next field.
        /// </summary>
        public class IterationFinishedException : TransformerOptimizerException
        {
            /// <summary>
            /// Base constructor.
            /// </summary>
            public IterationFinishedException() { }
            /// <summary>
            /// Overloaded constructor for message.
            /// </summary>
            /// <param name="message">Message of the exception.</param>
            public IterationFinishedException(string message) : base(message) { }
            /// <summary>
            /// Overloaded constructor for message and inner exception.
            /// </summary>
            /// <param name="message">Message of the exception.</param>
            /// <param name="inner">Inner exception.</param>
            public IterationFinishedException(string message, Exception inner) : base(message, inner) { }
        }
        /// <summary>
        /// Thrown if no wires could be found to meet the critera provided by a section. 
        /// </summary>
        public class NoWiresFound : TransformerOptimizerException
        {
            /// <summary>
            /// Base constructor.
            /// </summary>
            public NoWiresFound() { }
            /// <summary>
            /// Overloaded constructor for message.
            /// </summary>
            /// <param name="message">Message of the exception.</param>
            public NoWiresFound(string message) : base(message) { }
            /// <summary>
            /// Overloaded constructor for message and inner exception.
            /// </summary>
            /// <param name="message">Message of the exception.</param>
            /// <param name="inner">Inner exception.</param>
            public NoWiresFound(string message, Exception inner) : base(message, inner) { }
        }
        /// <summary>
        /// Thrown if no cores could be found to meet the critera provided by user.
        /// </summary>
        public class NoCoresFound : TransformerOptimizerException
        {
            /// <summary>
            /// Base constructor.
            /// </summary>
            public NoCoresFound() { }
            /// <summary>
            /// Overloaded constructor for message.
            /// </summary>
            /// <param name="message">Message of the exception.</param>
            public NoCoresFound(string message) : base(message) { }
            /// <summary>
            /// Overloaded constructor for message and inner exception.
            /// </summary>
            /// <param name="message">Message of the exception.</param>
            /// <param name="inner">Inner exception.</param>
            public NoCoresFound(string message, Exception inner) : base(message, inner) { }
        }
        /// <summary>
        /// Thrown if the given core/lamination thickness is not a valid number.
        /// Should not be thrown 03/15/2019 as core/lamination factories will just skip them and increment the value.
        /// </summary>
        public class InvalidCoreThicknessIteration : TransformerOptimizerException
        {
            /// <summary>
            /// Base constructor.
            /// </summary>
            public InvalidCoreThicknessIteration() { }
            /// <summary>
            /// Overloaded constructor for message.
            /// </summary>
            /// <param name="message">Message of the exception.</param>
            public InvalidCoreThicknessIteration(string message) : base(message) { }
            /// <summary>
            /// Overloaded constructor for message and inner exception.
            /// </summary>
            /// <param name="message">Message of the exception.</param>
            /// <param name="inner">Inner exception.</param>
            public InvalidCoreThicknessIteration(string message, Exception inner) : base(message, inner) { }
        }
        /// <summary>
        /// Thrown if the given core/lamination grade is not a valid option.
        /// Should not be thrown 03/15/2019 as core/lamination factories will just skip them and increment the value.
        /// </summary>
        public class InvalidCoreGradeIteration : TransformerOptimizerException
        {
            /// <summary>
            /// Base constructor.
            /// </summary>
            public InvalidCoreGradeIteration() { }
            /// <summary>
            /// Overloaded constructor for message.
            /// </summary>
            /// <param name="message">Message of the exception.</param>
            public InvalidCoreGradeIteration(string message) : base(message) { }
            /// <summary>
            /// Overloaded constructor for message and inner exception.
            /// </summary>
            /// <param name="message">Message of the exception.</param>
            /// <param name="inner">Inner exception.</param>
            public InvalidCoreGradeIteration(string message, Exception inner) : base(message, inner) { }
        }
    }
}
