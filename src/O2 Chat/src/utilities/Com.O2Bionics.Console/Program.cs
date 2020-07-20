using System;
using System.Diagnostics;
using System.Text;
using Com.O2Bionics.Console.Properties;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;
using log4net;
using log4net.Config;

namespace Com.O2Bionics.Console
{
    public static class Program
    {
        private static ILog Log => LogManager.GetLogger(typeof(Program));

        private static int Main(string[] args)
        {
            var watch = Stopwatch.StartNew();
            int result;
            try
            {
                XmlConfigurator.Configure();
                var reader = new JsonSettingsReader();
                Log.InfoFormat(Resources.UsingConfigFile1, reader.ConfigFilePath);

                var name = GetName(args);
                var commands = CommandList.GetCommands();
                var command = GetCommand(name, commands);
                if (null == name || null == command)
                {
                    HowToUse(commands, reader);
                    return 338342197;
                }

                command.Run(name, reader);
                result = 0;
            }
            catch (Exception e)
            {
                result = 725890130;
                Log.Error("Main", e);
            }

            Log.InfoFormat(Resources.ProgramElapsedMs1, watch.ElapsedMilliseconds);
            return result;
        }

        private static string GetName(string[] args)
        {
            return null == args || 0 == args.Length ? null : args[0];
        }

        [CanBeNull]
        private static ICommand GetCommand([CanBeNull] string name, [NotNull] ICommand[] commands)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < commands.Length; i++)
            {
                var names = commands[i].Names;
                // ReSharper disable once ForCanBeConvertedToForeach
                // ReSharper disable once LoopCanBeConvertedToQuery
                for (var j = 0; j < names.Length; j++)
                    if (name == names[j])
                        return commands[i];
            }

            return null;
        }

        private static void HowToUse([NotNull] ICommand[] commands, [NotNull] JsonSettingsReader reader)
        {
            var builder = new StringBuilder();
            builder.AppendLine(Resources.Usage);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < commands.Length; i++)
            {
                builder.AppendLine();
                try
                {
                    var usage = commands[i].GetUsage(reader);
                    builder.AppendLine(usage);
                }
                catch (Exception e)
                {
                    Log.Error("Usage error.", e);
                }
            }

            Log.Info(builder.ToString());
        }
    }
}