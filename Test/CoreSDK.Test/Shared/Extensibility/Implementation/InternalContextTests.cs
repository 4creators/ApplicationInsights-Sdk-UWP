﻿namespace Microsoft.ApplicationInsights.Extensibility.Implementation
{
    using System.Collections.Generic;
#if !WINDOWS_UWP
	using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
	using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

	using Assert = Xunit.Assert;

    [TestClass]
    public class InternalContextTests
    {
        [TestMethod]
        public void SdkVersionIsNullByDefaultToAvoidSendingItToEndpointUnnecessarily()
        {
            var context = new InternalContext(new Dictionary<string, string>());
            Assert.Null(context.SdkVersion);
        }

        [TestMethod]
        public void IpCanBeChangedByUserToSpecifyACustomValue()
        {
            var context = new InternalContext(new Dictionary<string, string>());
            context.SdkVersion = "0.0.11.00.1";
            Assert.Equal("0.0.11.00.1", context.SdkVersion);
        }
    }
}
