namespace Microsoft.ApplicationInsights.Extensibility.Implementation.Platform
{
    using System;
	using System.IO;
    using System.Collections.Generic;
	using System.Security;
	using System.Threading.Tasks;
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
			
			try
			{
				Task<Stream> task = null;
				try
				{
					task = ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("ApplicationInsights.config");

					task.Wait(15000);

					if (task.IsCompleted && !task.IsFaulted)
					{
						using (var reader = new StreamReader(task.Result))
							return reader.ReadToEnd();
					}
				}
				catch(AggregateException aex)
				{
					if (aex.InnerException != null)
					{
						switch (aex.InnerException.GetType().Name)
						{
							case "FileNotFoundException":
								CoreEventSource.Log.ApplicationInsightsConfigNotFoundWarning(configFilePath);
								break;
							case "DirectoryNotFoundException":
								CoreEventSource.Log.ApplicationInsightsConfigNotFoundWarning(configFilePath);
								break;
							case "IOException":
								CoreEventSource.Log.ApplicationInsightsConfigNotFoundWarning(configFilePath);
								break;
							case "UnauthorizedAccessException":
								CoreEventSource.Log.ApplicationInsightsConfigNotFoundWarning(configFilePath);
								break;
							case "SecurityException":
								CoreEventSource.Log.ApplicationInsightsConfigNotFoundWarning(configFilePath);
								break;
							default:
								CoreEventSource.Log.LogError(aex.Message, "UWP");
								throw aex;
						}
					}
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
