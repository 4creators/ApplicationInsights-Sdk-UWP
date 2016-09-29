namespace Microsoft.ApplicationInsights.Extensibility.Implementation.Platform
{
    using System;
    using System.IO;
    using System.Text;
#if !WINDOWS_UWP
	using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
	using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
	using Windows.ApplicationModel;
	using Windows.Storage;
#endif

    /// <summary>
    /// Shared, platform-neutral tests for <see cref="PlatformImplementation"/> class.
    /// </summary>
    [TestClass]
    public class PlatformImplementationTest : IDisposable
    {
        public PlatformImplementationTest()
        {
            // Make sure configuration files created by other tests don't brake these.
            DeleteConfigurationFile();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [TestMethod]
        public void ReadConfigurationXmlReturnsContentsOfApplicationInsightsConfigFileInApplicationInstallationDirectory()
        {
            const string TestFileContent = "42";
            CreateConfigurationFile(TestFileContent);
            var platform = new PlatformImplementation();

            string s = platform.ReadConfigurationXml();
            
            Assert.AreEqual(TestFileContent, s);
        }

        [TestMethod]
        public void ReadConfigurationXmlIgnoresMissingApplicationInsightsConfigurationFileByReturningEmptyString()
        {
            var platform = new PlatformImplementation();

            string configuration = platform.ReadConfigurationXml();
            
            Assert.AreEqual(0, configuration.Length);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                DeleteConfigurationFile();
            }
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
			File.Delete(Path.Combine(Environment.CurrentDirectory, "ApplicationInsights.config"));
#else
			var task = ApplicationData.Current.LocalFolder.GetFileAsync("ApplicationInsights.config").AsTask();
			task.Wait(10000);
			if (task.IsCompleted && !task.IsFaulted)
				task.Result.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask().Wait(10000);
#endif
		}

		private static Stream OpenConfigurationFile()
        {
#if !WINDOWS_UWP
			return File.OpenWrite(Path.Combine(Environment.CurrentDirectory, "ApplicationInsights.config"));
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
