using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using Com.O2Bionics.ChatService.Web.Console.Controllers;
using JetBrains.Annotations;
using log4net;
using NUnit.Framework;

namespace Com.O2Bionics.Web.App.Tests
{
    [TestFixture]
    public class ControllersTest
    {
        private static readonly string m_httpPostName = typeof(HttpPostAttribute).FullName;
        private static readonly string m_httpGetName = typeof(HttpGetAttribute).FullName;
        private static readonly ILog m_log = LogManager.GetLogger(typeof(ControllersTest));
        private static readonly string m_controllerName = typeof(Controller).FullName;
        private static readonly string m_objectName = typeof(object).FullName;

        [Test]
        public void ActionsHaveVerbsTest()
        {
            var assembly = typeof(HomeController).Assembly;
            var types = assembly.GetExportedTypes();
            var errors = new List<string>();
            var controllerCount = 0;
            var methodCount = 0;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < types.Length; i++)
            {
                var type = types[i];
                var typeName = type.FullName;
                if (null == typeName || !typeName.EndsWith("Controller"))
                    continue;

                ++controllerCount;
                CheckController(type, typeName, out var count, errors);
                if (0 == count)
                    errors.Add($"The controller '{typeName}' must have public methods.");
                else
                    methodCount += count;
            }

            if (0 == controllerCount)
                errors.Add($"The assembly '{assembly.FullName}' must have controllers.");
            else if (0 == methodCount)
                errors.Add($"The assembly '{assembly.FullName}' must have controllers {controllerCount} with methods.");
            else if (0 == errors.Count)
            {
                m_log.Debug(
                    $"The assembly '{assembly.FullName}' {controllerCount} controllers {methodCount} methods passed the test.");
                return;
            }

            ReportErrors(errors);
        }

        private static void CheckController([NotNull] Type type, [NotNull] string typeName, out int methodCount, [NotNull] List<string> errors)
        {
            methodCount = 0;
            for (;;)
            {
                CheckControllerImpl(type, typeName, out var count, errors);
                methodCount += count;

                type = type.BaseType;
                if (null == type)
                    break;
                typeName = type.FullName;
                if (null == typeName || m_controllerName == typeName || m_objectName == typeName)
                    break;
            }
        }

        private static void CheckControllerImpl([NotNull] Type type, [NotNull] string typeName, out int methodCount, [NotNull] List<string> errors)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod);
            methodCount = methods.Length;
            if (0 == methodCount)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var j = 0; j < methodCount; j++)
            {
                var error = CheckMethod(typeName, methods[j]);
                if (!string.IsNullOrEmpty(error))
                    errors.Add(error);
            }
        }

        [CanBeNull]
        private static string CheckMethod([NotNull] string typeName, [NotNull] MethodInfo methodInfo)
        {
            var attributes = methodInfo.GetCustomAttributes(true);
            SetFlags(attributes, out var post, out var get);

            return get != post
                ? null
                : $"Type '{typeName}', method '{methodInfo.Name}' must have either attribute: {m_httpGetName} or {m_httpPostName}";
        }

        private static void SetFlags([CanBeNull] object[] attributes, out bool post, out bool get)
        {
            post = get = false;
            if (null == attributes)
                return;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < attributes.Length; i++)
            {
                var name = attributes[i].GetType().FullName;
                if (m_httpPostName == name)
                    post = true;
                else if (m_httpGetName == name)
                    get = true;
            }
        }

        private static void ReportErrors([NotNull] List<string> errors)
        {
            Debug.Assert(0 < errors.Count);

            var sb = new StringBuilder($"Stopping the application because the Controllers have {errors.Count} errors: ");
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < errors.Count; i++)
                sb.AppendLine(errors[i]);

            var message = sb.ToString();
            throw new Exception(message);
        }
    }
}