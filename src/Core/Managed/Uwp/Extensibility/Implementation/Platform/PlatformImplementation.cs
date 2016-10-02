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
			string configFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "ApplicationInsights.config");

			try
			{
				try // TODO remove try catch when done with async file loads
				{
					if (File.Exists(configFilePath))
						return File.ReadAllText(configFilePath);
				}
				catch(AggregateException aex)
				{
					Action<AggregateException> exit = (a) => {
						CoreEventSource.Log.LogError(a.Message, "UWP");
						throw new AggregateException(a);
					};

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
								exit(aex);
								break;
						}
					}
					else exit(aex);

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
			return this.debugOutput ?? (this.debugOutput = new TelemetryDebugWriter());
		}
    }
}
