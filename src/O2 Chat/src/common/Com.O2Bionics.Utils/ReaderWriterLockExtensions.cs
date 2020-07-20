using System;
using System.Threading;

namespace Com.O2Bionics.Utils
{
    // see http://chabster.blogspot.com/2013/07/a-story-of-orphaned-readerwriterlockslim.html for details

    public static class ReaderWriterLockExtensions
    {
        public static T Read<T>(this ReaderWriterLockSlim l, Func<T> func)
        {
            var lockIsHeld = false;
            try
            {
                try
                {
                }
                finally
                {
                    l.EnterReadLock();
                    lockIsHeld = true;
                }

                return func();
            }
            finally
            {
                if (lockIsHeld) l.ExitReadLock();
            }
        }


        public static void Read(this ReaderWriterLockSlim l, Action action)
        {
            Read(
                l,
                () =>
                    {
                        action();
                        return 0;
                    });
        }

        public static T Write<T>(this ReaderWriterLockSlim l, Func<T> func)
        {
            var lockIsHeld = false;
            try
            {
                try
                {
                }
                finally
                {
                    l.EnterWriteLock();
                    lockIsHeld = true;
                }

                return func();
            }
            finally
            {
                if (lockIsHeld) l.ExitWriteLock();
            }
        }


        public static void Write(this ReaderWriterLockSlim l, Action action)
        {
            Write(
                l,
                () =>
                    {
                        action();
                        return 0;
                    });
        }

        public static T UpgradeableRead<T>(this ReaderWriterLockSlim l, Func<T> func)
        {
            var lockIsHeld = false;
            try
            {
                try
                {
                }
                finally
                {
                    l.EnterUpgradeableReadLock();
                    lockIsHeld = true;
                }

                return func();
            }
            finally
            {
                if (lockIsHeld) l.ExitUpgradeableReadLock();
            }
        }


        public static void UpgradeableRead(this ReaderWriterLockSlim l, Action action)
        {
            UpgradeableRead(
                l,
                () =>
                    {
                        action();
                        return 0;
                    });
        }
    }
}