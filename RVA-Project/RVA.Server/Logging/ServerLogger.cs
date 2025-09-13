using log4net;
using log4net.Config;
using RVA.Shared.Interfaces;
using System;
using System.IO;

namespace RVA.Server.Logging
{
    /// <summary>
    /// Minimalna implementacija server loggera korišćenjem log4net.
    /// </summary>
    public class ServerLogger : ILogger
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ServerLogger));
        private static bool _isConfigured = false;

        public ServerLogger()
        {
            ConfigureLogging();
        }

        /// <summary>
        /// Konfiguriše log4net – prvo pokušava iz config fajla, a ako ne uspe koristi programsku konfiguraciju.
        /// </summary>
        private static void ConfigureLogging()
        {
            if (_isConfigured) return;

            try
            {
                XmlConfigurator.Configure();

                if (!_log.Logger.Repository.Configured)
                {
                    ConfigureProgrammatically();
                }

                _isConfigured = true;
                _log.Info("ServerLogger successfully configured");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configuring ServerLogger: {ex.Message}");
                ConfigureBasicConsoleLogging();
                _isConfigured = true;
            }
        }

        /// <summary>
        /// Fallback konfiguracija (file + console appender).
        /// </summary>
        private static void ConfigureProgrammatically()
        {
            var hierarchy = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();

            var patternLayout = new log4net.Layout.PatternLayout
            {
                ConversionPattern = "%date %-5level %logger - %message%newline"
            };
            patternLayout.ActivateOptions();

            var fileAppender = new log4net.Appender.RollingFileAppender
            {
                AppendToFile = true,
                File = "Logs/server.log",
                Layout = patternLayout,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "5MB",
                RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true
            };
            fileAppender.ActivateOptions();

            var consoleAppender = new log4net.Appender.ConsoleAppender
            {
                Layout = patternLayout
            };
            consoleAppender.ActivateOptions();

            hierarchy.Root.AddAppender(fileAppender);
            hierarchy.Root.AddAppender(consoleAppender);
            hierarchy.Root.Level = log4net.Core.Level.Info;
            hierarchy.Configured = true;

            var logDirectory = Path.GetDirectoryName(fileAppender.File);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        private static void ConfigureBasicConsoleLogging()
        {
            var hierarchy = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
            var patternLayout = new log4net.Layout.PatternLayout
            {
                ConversionPattern = "%date %-5level - %message%newline"
            };
            patternLayout.ActivateOptions();

            var consoleAppender = new log4net.Appender.ConsoleAppender
            {
                Layout = patternLayout
            };
            consoleAppender.ActivateOptions();

            hierarchy.Root.AddAppender(consoleAppender);
            hierarchy.Root.Level = log4net.Core.Level.Info;
            hierarchy.Configured = true;
        }

        #region ILogger Implementation

        public void Debug(string message) => _log.Debug(message);
        public void Info(string message) => _log.Info(message);
        public void Warn(string message) => _log.Warn(message);
        public void Error(string message) => _log.Error(message);
        public void Fatal(string message) => _log.Fatal(message);

        public void Debug(string message, Exception exception) => _log.Debug(message, exception);
        public void Info(string message, Exception exception) => _log.Info(message, exception);
        public void Warn(string message, Exception exception) => _log.Warn(message, exception);
        public void Error(string message, Exception exception) => _log.Error(message, exception);
        public void Fatal(string message, Exception exception) => _log.Fatal(message, exception);

        #endregion
    }
}
