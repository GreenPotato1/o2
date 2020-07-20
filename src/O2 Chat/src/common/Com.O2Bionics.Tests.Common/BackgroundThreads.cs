using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using log4net;
using NUnit.Framework;

namespace Com.O2Bionics.Tests.Common
{
    public class BackgroundThreads
    {
        private const string PerfTestsLogFileName = "C:\\O2Bionics\\O2Chat\\Logs\\PerfTests.log";

        private static readonly ILog m_log = LogManager.GetLogger(typeof(BackgroundThreads));

        private readonly List<Thread> m_threads;

        private long m_errors;

        public BackgroundThreads(int count, [NotNull] Action<int> action)
        {
            Assert.Greater(count, 0, nameof(count));
            action.NotNull(nameof(action));

            Count = count;

            m_threads = Enumerable.Range(0, count)
                .Select(
                    threadIndex => CreateBackgroundThread(
                        "th " + threadIndex,
                        () =>
                            {
                                try
                                {
                                    action(threadIndex);
                                }
                                catch (Exception e)
                                {
                                    Interlocked.Increment(ref m_errors);
                                    m_log.Error(e);
                                }
                            }))
                .ToList();
        }

        public BackgroundThreads(int count, [NotNull] Func<int, Task> action)
        {
            Assert.Greater(count, 0, nameof(count));
            action.NotNull(nameof(action));

            Count = count;

            m_threads = Enumerable.Range(0, count)
                .Select(
                    threadIndex => CreateBackgroundThread(
                        "th " + threadIndex,
                        () =>
                            {
                                try
                                {
                                    action(threadIndex).WaitAndUnwrapException();
                                }
                                catch (Exception e)
                                {
                                    Interlocked.Increment(ref m_errors);
                                    m_log.Error(e);
                                }
                            }))
                .ToList();
        }

        public int Count { get; }

        public long Errors => Interlocked.Read(ref m_errors);

        public void StartAndJoin()
        {
            foreach (var thread in m_threads)
                thread.Start();
            foreach (var thread in m_threads)
                thread.Join();
        }

        public void Measure(string testName, string sampleName, int callsPerThread)
        {
            var count = callsPerThread * Count;

            var sw = Stopwatch.StartNew();
            StartAndJoin();
            sw.Stop();

            const double epsilon = 1.0e-5;
            var cps = Math.Abs(sw.Elapsed.TotalSeconds) < epsilon ? 0.0 : count / sw.Elapsed.TotalSeconds;

            // log level filter can be set to some high value during performance tests, so use the highest level here
            m_log.FatalFormat(
                "{0} {1} calls in {2:0.000} ms.; {3:0.000} cps",
                count,
                sampleName,
                sw.Elapsed.TotalMilliseconds,
                cps);

            File.AppendAllText(
                PerfTestsLogFileName,
                $"{DateTime.Now:s} {testName}:{sampleName} threads:{Count} iterations:{count}, time:{sw.Elapsed.TotalSeconds:0.000}s., cps:{cps:0.000}{Environment.NewLine}");

            Assert.That(m_errors, Is.EqualTo(0));
        }

        public static Thread CreateBackgroundThread(string name, Action action)
        {
            return new Thread(() => action()) { IsBackground = true, Name = name };
        }
    }
}