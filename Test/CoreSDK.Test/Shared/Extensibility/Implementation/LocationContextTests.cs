﻿namespace Microsoft.ApplicationInsights.Extensibility.Implementation
{
    using System.Collections.Generic;
    using System.Reflection;
#if !WINDOWS_UWP
	using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
	using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

	using Assert = Xunit.Assert;
    
    [TestClass]
    public class LocationContextTests
    {
        [TestMethod]
        public void ClassIsPublicToAllowSpecifyingCustomLocationContextPropertiesInUserCode()
        {
            Assert.True(typeof(LocationContext).GetTypeInfo().IsPublic);
        }

        [TestMethod]
        public void IpIsNullByDefaultToAvoidSendingItToEndpointUnnecessarily()
        {
            var context = new LocationContext(new Dictionary<string, string>());
            Assert.Null(context.Ip);
        }

        [TestMethod]
        public void IpCanBeChangedByUserToSpecifyACustomValue()
        {
            var context = new LocationContext(new Dictionary<string, string>());
            context.Ip = "192.168.1.1";
            Assert.Equal("192.168.1.1", context.Ip);
        }

        [TestMethod]
        public void IpRejectsNonIpv4Address()
        {
            var context = new LocationContext(new Dictionary<string, string>());
            context.Ip = "2401:4893:f0:5c:2452:4474:03d2:9375";
            Assert.Null(context.Ip);
        }
    }
}
