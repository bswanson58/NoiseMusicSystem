using System;
using CuttingEdge.Conditions;
using ReusableBits.Support;

namespace ReusableBits.Threading {
	public class RecurringTaskSchedule {
		private DateTime?	mStartTime;
		private DateTime?	mEndTime;
		private TimeSpan?	mInterval;
		private TimeSpan?	mDelay;
		private long?		mRepeatCount;
		private DateTime?	mLastExecutionStart;
		private DateTime?	mLastExecutionFinish;
		private long		mExecutionCount;

		public	bool		IsPaused { get; private set; }

		public RecurringTaskSchedule StartAt( DateTime time ) {
			mStartTime = time;

			return( this );
		}

		public RecurringTaskSchedule StartAt( RecurringInterval interval ) {
			return( StartAt( interval.Interval ));
		}

		public RecurringTaskSchedule StartAt( TimeSpan timeSpan ) {
			mStartTime = TimeProvider.Now() + timeSpan;

			return( this );
		}

		public RecurringTaskSchedule EndAt( DateTime time ) {
			mEndTime = time;

			return( this );
		}

		public RecurringTaskSchedule EndAt( RecurringInterval interval ) {
			return( EndAt( interval.Interval ));
		}

		public RecurringTaskSchedule EndAt( TimeSpan timeSpan ) {
			mEndTime = TimeProvider.Now() + timeSpan;

			return( this );
		}

		public RecurringTaskSchedule RepeatCount( long count ) {
			mRepeatCount = count;

			return( this );
		}

		public RecurringTaskSchedule Interval( TimeSpan time ) {
			mInterval = time;

			return( this );
		}

		public RecurringTaskSchedule Interval( RecurringInterval interval ) {
			mInterval = interval.Interval;

			return( this );
		}

		public RecurringTaskSchedule Delay( TimeSpan time ) {
			mDelay = time;

			return( this );
		}

		public RecurringTaskSchedule Delay( RecurringInterval interval ) {
			mDelay = interval.Interval;

			return( this );
		}

		public void Pause() {
			IsPaused = true;
		}

		public void Resume() {
			IsPaused = false;
		}

		public void UpdateLastExecutionTime( DateTime executionStart, DateTime executionFinish ) {
			mLastExecutionStart = executionStart;
			mLastExecutionFinish = executionFinish;
			mExecutionCount++;
		}

		public DateTime? CalculateNextExecutionTime() {
			DateTime?	retValue = null;

			if(!IsPaused ) {
				var now = TimeProvider.Now();

				if(( mStartTime.HasValue ) &&
				   ( mStartTime.Value > now )) {
					retValue = mStartTime.Value;
				}
				else {
					if( mStartTime.HasValue ) {
						if( mStartTime.Value <= now ) {
							retValue = mLastExecutionStart.HasValue ? OffsetNextExecutionTime() : now;
						}
					}
					else {
						retValue = mLastExecutionStart.HasValue ? OffsetNextExecutionTime() : now;
					}
				}

				if(( mRepeatCount.HasValue ) &&
				   ( mRepeatCount.Value <= mExecutionCount )) {
					retValue = null;
				}

				if(( mEndTime.HasValue ) &&
				   ( retValue.HasValue ) &&
				   ( retValue.Value > mEndTime.Value )) {
					retValue = null;
				}
			}

			return( retValue );
		}

		private DateTime OffsetNextExecutionTime() {
			Condition.Requires( mLastExecutionStart.HasValue );
			Condition.Requires( mLastExecutionFinish.HasValue );

			var retValue = TimeProvider.Now();

			if(( mLastExecutionStart.HasValue ) &&
			   ( mLastExecutionFinish.HasValue )) {
				var intervalTime = mLastExecutionStart.Value;
				var delayTime = mLastExecutionFinish.Value;

				if( mInterval.HasValue ) {
					intervalTime += mInterval.Value;
				}
				if( mDelay.HasValue ) {
					delayTime += mDelay.Value;
				}

				retValue = intervalTime > delayTime ? intervalTime : delayTime;
			}

			return( retValue );
		}
	}
}
