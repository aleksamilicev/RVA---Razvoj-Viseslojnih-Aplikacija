using RVA.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace RVA.Server.Storage
{
    /// <summary>
    /// XML implementacija IDataStorage interfejsa
    /// </summary>
    public class XmlStorage : IDataStorage
    {
        private readonly ILogger _logger;

        public string FileExtension => "xml";
        public string FormatName => "XML";

        public XmlStorage(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void SaveData<T>(IEnumerable<T> data, string filePath) where T : class
        {
            try
            {
                _logger.Debug($"Saving data to XML file: {filePath}");

                EnsureDirectoryExists(filePath);
                CreateBackup(filePath);

                var serializer = new XmlSerializer(typeof(List<T>));
                using var stream = new FileStream(filePath, FileMode.Create);
                serializer.Serialize(stream, data.ToList());

                _logger.Info($"Successfully saved {data.Count()} items to {filePath}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving data to XML file {filePath}", ex);
                throw new Exception($"Failed to save data to XML file: {filePath}", ex);
            }
        }

        public void SaveSingleEntity<T>(T entity, string filePath) where T : class
        {
            try
            {
                _logger.Debug($"Saving single entity to XML file: {filePath}");

                EnsureDirectoryExists(filePath);
                CreateBackup(filePath);

                var serializer = new XmlSerializer(typeof(T));
                using var stream = new FileStream(filePath, FileMode.Create);
                serializer.Serialize(stream, entity);

                _logger.Info($"Successfully saved single entity to {filePath}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving single entity to XML file {filePath}", ex);
                throw new Exception($"Failed to save entity to XML file: {filePath}", ex);
            }
        }

        public IEnumerable<T> LoadData<T>(string filePath) where T : class
        {
            try
            {
                _logger.Debug($"Loading data from XML file: {filePath}");

                if (!FileExists(filePath))
                {
                    _logger.Warn($"XML file not found: {filePath}");
                    return new List<T>();
                }

                var serializer = new XmlSerializer(typeof(List<T>));
                using var stream = new FileStream(filePath, FileMode.Open);
                var result = (List<T>)serializer.Deserialize(stream);

                _logger.Info($"Successfully loaded {result.Count} items from {filePath}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading data from XML file {filePath}", ex);
                throw new Exception($"Failed to load data from XML file: {filePath}", ex);
            }
        }

        public T LoadSingleEntity<T>(string filePath) where T : class
        {
            try
            {
                _logger.Debug($"Loading single entity from XML file: {filePath}");

                if (!FileExists(filePath))
                {
                    _logger.Warn($"XML file not found: {filePath}");
                    return null;
                }

                var serializer = new XmlSerializer(typeof(T));
                using var stream = new FileStream(filePath, FileMode.Open);
                var result = (T)serializer.Deserialize(stream);

                _logger.Info($"Successfully loaded single entity from {filePath}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading single entity from XML file {filePath}", ex);
                throw new Exception($"Failed to load entity from XML file: {filePath}", ex);
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
                // Ne bacamo izuzetak jer backup nije kritičan
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