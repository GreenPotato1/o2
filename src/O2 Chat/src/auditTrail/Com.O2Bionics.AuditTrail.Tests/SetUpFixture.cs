﻿using Com.O2Bionics.Tests.Common;
using NUnit.Framework;

namespace Com.O2Bionics.AuditTrail.Tests
{
    [SetUpFixture]
    public sealed class SetUpFixture : BaseSetUpFixture
    {
        [OneTimeSetUp]
        public override void SetUp()
        {
            base.SetUp();
        }
    }
}