using RVA.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Web;

namespace RVA.Server.Storage
{
    /// <summary>
    /// JSON implementacija IDataStorage interfejsa
    /// </summary>
    public class JsonStorage : IDataStorage
    {
        private readonly ILogger _logger;
        private readonly JsonSerializerOptions _options;

        public string FileExtension => "json";
        public string FormatName => "JSON";

        public JsonStorage(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public void SaveData<T>(IEnumerable<T> data, string filePath) where T : class
        {
            try
            {
                _logger.Debug($"Saving data to JSON file: {filePath}");

                EnsureDirectoryExists(filePath);
                CreateBackup(filePath);

                var jsonString = JsonSerializer.Serialize(data.ToList(), _options);
                File.WriteAllText(filePath, jsonString);

                _logger.Info($"Successfully saved {data.Count()} items to {filePath}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving data to JSON file {filePath}", ex);
                throw new Exception($"Failed to save data to JSON file: {filePath}", ex);
            }
        }

        public void SaveSingleEntity<T>(T entity, string filePath) where T : class
        {
            try
            {
                _logger.Debug($"Saving single entity to JSON file: {filePath}");

                EnsureDirectoryExists(filePath);
                CreateBackup(filePath);

                var jsonString = JsonSerializer.Serialize(entity, _options);
                File.WriteAllText(filePath, jsonString);

                _logger.Info($"Successfully saved single entity to {filePath}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving single entity to JSON file {filePath}", ex);
                throw new Exception($"Failed to save entity to JSON file: {filePath}", ex);
            }
        }

        public IEnumerable<T> LoadData<T>(string filePath) where T : class
        {
            try
            {
                _logger.Debug($"Loading data from JSON file: {filePath}");

                if (!FileExists(filePath))
                {
                    _logger.Warn($"JSON file not found: {filePath}");
                    return new List<T>();
                }

                var jsonString = File.ReadAllText(filePath);
                var result = JsonSerializer.Deserialize<List<T>>(jsonString, _options);

                _logger.Info($"Successfully loaded {result.Count} items from {filePath}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading data from JSON file {filePath}", ex);
                throw new Exception($"Failed to load data from JSON file: {filePath}", ex);
            }
        }

        public T LoadSingleEntity<T>(string filePath) where T : class
        {
            try
            {
                _logger.Debug($"Loading single entity from JSON file: {filePath}");

                if (!FileExists(filePath))
                {
                    _logger.Warn($"JSON file not found: {filePath}");
                    return null;
                }

                var jsonString = File.ReadAllText(filePath);
                var result = JsonSerializer.Deserialize<T>(jsonString, _options);

                _logger.Info($"Successfully loaded single entity from {filePath}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading single entity from JSON file {filePath}", ex);
                throw new Exception($"Failed to load entity from JSON file: {filePath}", ex);
            }
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public void DeleteFile(string filePath)
        {
            try
            {
                if (FileExists(filePath))
                {
                    File.Delete(filePath);
                    _logger.Info($"Deleted file: {filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error deleting file {filePath}", ex);
                throw new Exception($"Failed to delete file: {filePath}", ex);
            }
        }

        public void CreateBackup(string filePath)
        {
            try
            {
                if (FileExists(filePath))
                {
                    var backupPath = $"{filePath}.backup";
                    File.Copy(filePath, backupPath, true);
                    _logger.Debug($"Created backup: {backupPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to create backup for {filePath}", ex);
            }
        }

        private void EnsureDirectoryExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.Debug($"Created directory: {directory}");
            }
        }
    }
}