using System;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils;
using log4net;

namespace Com.O2Bionics.ChatService
{
    public class ChatServiceBase
    {
        protected readonly ILog Log;

        protected ChatServiceBase()
        {
            Log = LogManager.GetLogger(GetType());
        }

        protected T HandleExceptions<T>(
            Func<T> func,
            [CallerMemberName] string callerMember = "")
        {
            var logContext = WcfHelper.CreateContext();
            try
            {
                var result = func();
                return result;
            }
            catch (Exception e)
            {
                Log.Error($"Exception while call to {callerMember}.", e);
                throw;
            }
            finally
            {
                logContext?.Dispose();
            }
        }

        protected void HandleExceptions(Action action, [CallerMemberName] string callerMember = "")
        {
            HandleExceptions(
                () =>
                    {
                        action();
                        return 0;
                    },
                // ReSharper disable once ExplicitCallerInfoArgument
                callerMember);
        }

        protected T HandleExceptions<T>(Func<CallResultStatus, T> toResult, Func<T> func, [CallerMemberName] string callerMember = "")
        {
            var logContext = WcfHelper.CreateContext();
            try
            {
                try
                {
                    return func();
                }
                catch (CallResultException e)
                {
                    return toResult(e.Status);
                }
                catch (ValidationException e)
                {
                    return toResult(new CallResultStatus(CallResultStatusCode.ValidationFailed, e.Messages));
                }
            }
            catch (Exception e)
            {
                Log.Error($"Exception while call to {callerMember}.", e);
                throw;
            }
            finally
            {
                logContext?.Dispose();
            }
        }

        protected void LogMethodCall(object args = null, [CallerMemberName] string name = "")
        {
            if (!Log.IsDebugEnabled) return;

            var argsString = args != null ? args.JsonStringify() : "";
            Log.DebugFormat("call to {0}({1})", name, argsString);
        }

#if ERRORTRACKERTEST
        public void TestThrowError(string message)
        {
            HandleExceptions(
                () =>
                    throw new FaultException<ErrorTrackerFault>(
                        new ErrorTrackerFault { Message = message },
                        $"{nameof(ChatServiceBase)}.{nameof(TestThrowError)} passed - check the Elastic server."));
        }
#endif
    }
}