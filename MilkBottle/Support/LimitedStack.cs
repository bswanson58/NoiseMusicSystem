using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MilkBottle.Support {
    /// <summary>
    /// Generic stack implementation with a maximum limit
    /// When something is pushed on the last item is removed from the list
    /// from: https://ntsblog.homedev.com.au/index.php/2010/05/06/c-stack-with-maximum-limit/
    /// </summary>
    [Serializable]
    public class LimitedStack<T> {
        private int             mStackLimit;
        private LinkedList<T>   mList;

        public  int             Count => mList.Count;

        public LimitedStack( int maxSize ) {
            mStackLimit = maxSize;
            mList = new LinkedList<T>();
        }

        public void Push( T value ) {
            if( mList.Count == mStackLimit ) {
                mList.RemoveLast();
            }

            mList.AddFirst( value );
        }

        public T Pop() {
            if( mList.Count > 0 ) {
                T value = mList.First.Value;
                mList.RemoveFirst();
                return value;
            }

            throw new InvalidOperationException( "The Stack is empty" );
        }

        public T Peek() {
            if( mList.Count > 0 ) {
                T value = mList.First.Value;
                return value;
            }

            throw new InvalidOperationException( "The Stack is empty" );
        }

        public void Clear() {
            mList.Clear();
        }

        /// <summary>
        /// Checks if the top object on the stack matches the value passed in
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsTop( T value ) {
            bool result = false;
            if( Count > 0 ) {
                result = Peek().Equals( value );
            }
            return result;
        }

        public bool Contains( T value ) {
            bool result = false;
            if( Count > 0 ) {
                result = mList.Contains( value );
            }
            return result;
        }

        public IEnumerator<T> GetEnumerator() {
            return mList.GetEnumerator();
        }

        public IReadOnlyList<T> ToList() {
            return mList.ToList();
        }
    }
}
