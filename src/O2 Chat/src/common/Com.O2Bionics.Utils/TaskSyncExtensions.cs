using System;
using System.Threading.Tasks;

namespace Com.O2Bionics.Utils
{
    /// <summary>
    /// See https://stackoverflow.com/questions/17284517/is-task-result-the-same-as-getawaiter-getresult for details.
    /// </summary>
    public static class TaskSyncExtensions
    {
        public static void WaitAndUnwrapException(this Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            task.GetAwaiter().GetResult();
        }

        public static T WaitAndUnwrapException<T>(this Task<T> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            return task.GetAwaiter().GetResult();
        }
    }
}