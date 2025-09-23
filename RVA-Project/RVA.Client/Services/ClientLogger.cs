using log4net;
using System;

namespace RVA.Client.Services
{
    public static class ClientLogger
    {
        private static readonly ILog _logger = LogManager.GetLogger("RVA.Client");

        public static void Debug(string message) => _logger.Debug(message);
        public static void Info(string message) => _logger.Info(message);
        public static void Warn(string message) => _logger.Warn(message);
        public static void Error(string message) => _logger.Error(message);
        public static void Error(string message, Exception ex) => _logger.Error(message, ex);
        public static void Fatal(string message) => _logger.Fatal(message);
        public static void Fatal(string message, Exception ex) => _logger.Fatal(message, ex);
    }
}