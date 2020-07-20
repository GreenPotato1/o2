using System;
using System.IO;
using System.Reflection;
using Com.O2Bionics.Tests.Common;
using NUnit.Framework;

namespace Com.O2Bionics.FeatureService.Tests
{
    [SetUpFixture]
    public sealed class SetUpFixture : BaseSetUpFixture
    {
        [OneTimeSetUp]
        public override void SetUp()
        {
            var codeBase = Assembly.GetAssembly(typeof(SetUpFixture)).CodeBase;
            var localPath = new Uri(codeBase).LocalPath;
            var currentDirectory = Path.GetDirectoryName(localPath);
            if (string.IsNullOrEmpty(currentDirectory))
                throw new Exception($"Cannot get directory from codeBase='{codeBase}'.");

            Environment.CurrentDirectory = currentDirectory;

            base.SetUp();
        }
    }
}