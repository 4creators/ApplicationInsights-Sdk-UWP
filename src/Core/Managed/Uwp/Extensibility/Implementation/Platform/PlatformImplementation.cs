namespace Microsoft.ApplicationInsights.Extensibility.Implementation.Platform
{
    using System;
	using System.IO;
    using System.Collections.Generic;
	using System.Security;
	using Microsoft.ApplicationInsights.Extensibility.Implementation.External;
	using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
	using Windows.ApplicationModel;
	using Windows.Storage;

	internal class PlatformImplementation : IPlatform
    {
        private IDebugOutput debugOutput = null;
        
        public IDictionary<string, object> GetApplicationSettings()
        {
            return null;
        }

        public string ReadConfigurationXml()
        {
			// Config file should be in the base directory of the app domain
			string configFilePath = Path.Combine(Package.Current.InstalledLocation.Path, "ApplicationInsights.config");

			var task = ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("ApplicationInsights.config");
			task.Wait(5000);
			
			try
			{
				if (task.IsCompleted && !task.IsFaulted)
				{
					using (var reader = new StreamReader(task.Result))
						return reader.ReadToEnd();
				}
			}
			catch (FileNotFoundException)
			{
				// For cases when file was deleted/modified while reading
				CoreEventSource.Log.ApplicationInsightsConfigNotFoundWarning(configFilePath);
			}
			catch (DirectoryNotFoundException)
			{
				// For cases when file was deleted/modified while reading
				CoreEventSource.Log.ApplicationInsightsConfigNotFoundWarning(configFilePath);
			}
			catch (IOException)
			{
				// For cases when file was deleted/modified while reading
				CoreEventSource.Log.ApplicationInsightsConfigNotFoundWarning(configFilePath);
			}
			catch (UnauthorizedAccessException)
			{
				CoreEventSource.Log.ApplicationInsightsConfigNotFoundWarning(configFilePath);
			}
			catch (SecurityException)
			{
				CoreEventSource.Log.ApplicationInsightsConfigNotFoundWarning(configFilePath);
			}

			return string.Empty;
		}

        public ExceptionDetails GetExceptionDetails(Exception exception, ExceptionDetails parentExceptionDetails)
        {
            return ExceptionConverter.ConvertToExceptionDetails(exception, parentExceptionDetails);
        }

        /// <summary>
        /// Returns the platform specific Debugger writer to the VS output console.
        /// </summary>
        public IDebugOutput GetDebugOutput()
        {
            if (this.debugOutput == null)
            {
                this.debugOutput = new TelemetryDebugWriter(); 
            }
            
            return this.debugOutput;
        }
    }
}
