namespace Microsoft.ApplicationInsights.Extensibility.Implementation
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility.Implementation.Platform;
    using Microsoft.ApplicationInsights.TestFramework;
#if !WINDOWS_UWP
	using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
	using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
	using Windows.ApplicationModel;
	using Windows.Storage;
#endif

	[TestClass]
    public class PlatformTest
    {
        [TestCleanup]
        public void TestCleanup()
        {
            PlatformSingleton.Current = null;
        }

        [TestMethod]
        public void ClassIsPublicToAllowTestingOnWindowsRuntime()
        {
            Assert.IsFalse(typeof(PlatformSingleton).GetTypeInfo().IsPublic);
        }

        [TestMethod]
        public void ClassIsStaticToServeOnlyAsSingletonFactory()
        {
            Assert.IsTrue(typeof(PlatformSingleton).GetTypeInfo().IsAbstract && typeof(PlatformSingleton).GetTypeInfo().IsSealed);
        }

        [TestMethod]
        public void CurrentIsAutomaticallyInitializedForEasyAccess()
        {
            IPlatform current = PlatformSingleton.Current;
            Assert.IsNotNull(current);
        }

        [TestMethod]
        public void CurrentCanBeSetToEnableMocking()
        {
            var platform = new StubPlatform();
            PlatformSingleton.Current = platform;
            Assert.AreSame(platform, PlatformSingleton.Current);
        }
        
        [TestMethod]
        public void ReadConfigurationXmlReturnsContentsOfApplicationInsightsConfigFileInApplicationInstallationDirectory()
        {
            CreateConfigurationFile("42");
            try
            {
                Assert.AreEqual("42", PlatformSingleton.Current.ReadConfigurationXml());
            }
            finally
            {
                DeleteConfigurationFile();
            }
        }

        [TestMethod]
        public void ReadConfigurationXmlIgnoresMissingApplicationInsightsConfigurationFileByReturningEmptyString()
        {
			var platform = new PlatformImplementation();
            string configuration = platform.ReadConfigurationXml();
            Assert.IsNotNull(configuration);
            Assert.AreEqual(0, configuration.Length);
        }

        private static void CreateConfigurationFile(string content)
        {
            using (Stream fileStream = OpenConfigurationFile())
            {
                byte[] configurationBytes = Encoding.UTF8.GetBytes(content);
                fileStream.Write(configurationBytes, 0, configurationBytes.Length);
            }           
        }
        
        private static void DeleteConfigurationFile()
        {
#if !WINDOWS_UWP
			File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApplicationInsights.config"));
#else
			string filePath = null;
			if (File.Exists(filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "ApplicationInsights.config")))
				File.Delete(filePath);
#endif
		}

		private static Stream OpenConfigurationFile()
        {
#if !WINDOWS_UWP
			return File.OpenWrite(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApplicationInsights.config"));
#else
			var task = ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync("ApplicationInsights.config", CreationCollisionOption.OpenIfExists);
			task.Wait(10000);
			if (task.IsCompleted && !task.IsFaulted)
				return task.Result;
			else
				throw new System.IO.IOException(String.Empty, task.Exception);
#endif
		}
	}
}
