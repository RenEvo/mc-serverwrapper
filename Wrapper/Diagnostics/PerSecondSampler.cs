using System;
using System.Collections.Generic;

namespace Wrapper.Diagnostics
{
    /// <summary>
    /// Completely broken, needs rework
    /// </summary>
    public class PerSecondSampler
    {
        /// <summary>
        /// The _values
        /// </summary>
        private readonly Stack<SampleValue> _values = new Stack<SampleValue>();

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            _values.Clear();
        }

        /// <summary>
        /// Samples this instance.
        /// </summary>
        public void Sample()
        {
            _values.Push(SampleValue.Create());

            // clean old
            while (_values.Count > 0 && _values.Peek().Value < DateTime.UtcNow.AddSeconds(-60))
                _values.Pop();
        }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public int Rate
        {
            get
            {
                while (_values.Count > 0 && _values.Peek().Value < DateTime.UtcNow.AddSeconds(-60))
                    _values.Pop();

                return _values.Count;
            }
        }

        /// <summary>
        /// Struct SampleValue
        /// </summary>
        private struct SampleValue
        {
            /// <summary>
            /// The value
            /// </summary>
            public DateTime Value;

            /// <summary>
            /// Creates this instance.
            /// </summary>
            /// <returns>SampleValue.</returns>
            public static SampleValue Create()
            {
                SampleValue returnValue;
                returnValue.Value = DateTime.UtcNow;
                return returnValue;
            }
        }
    }




}
