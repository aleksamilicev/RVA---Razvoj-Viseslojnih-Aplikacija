using RVA.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;

namespace RVA.Server.Storage
{
    /// CSV implementacija IDataStorage interfejsa
    public class CsvStorage : IDataStorage
    {
        private readonly ILogger _logger;
        private const string CSV_SEPARATOR = ";";

        public string FileExtension => "csv";
        public string FormatName => "CSV";

        public CsvStorage(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void SaveData<T>(IEnumerable<T> data, string filePath) where T : class
        {
            try
            {
                _logger.Debug($"Saving data to CSV file: {filePath}");

                EnsureDirectoryExists(filePath);
                CreateBackup(filePath);

                var dataList = data.ToList();
                if (!dataList.Any())
                {
                    File.WriteAllText(filePath, string.Empty);
                    _logger.Info($"Saved empty CSV file to {filePath}");
                    return;
                }

                var properties = GetSerializableProperties<T>();
                var lines = new List<string>();

                // Header
                var header = string.Join(CSV_SEPARATOR, properties.Select(p => p.Name));
                lines.Add(header);

                // Data rows
                foreach (var item in dataList)
                {
                    var values = properties.Select(prop =>
                    {
                        var value = prop.GetValue(item);
                        return EscapeCsvValue(value?.ToString() ?? string.Empty);
                    });
                    lines.Add(string.Join(CSV_SEPARATOR, values));
                }

                File.WriteAllLines(filePath, lines);
                _logger.Info($"Successfully saved {dataList.Count} items to {filePath}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving data to CSV file {filePath}", ex);
                throw new Exception($"Failed to save data to CSV file: {filePath}", ex);
            }
        }

        public void SaveSingleEntity<T>(T entity, string filePath) where T : class
        {
            SaveData(new[] { entity }, filePath);
        }

        public IEnumerable<T> LoadData<T>(string filePath) where T : class
        {
            try
            {
                _logger.Debug($"Loading data from CSV file: {filePath}");

                if (!FileExists(filePath))
                {
                    _logger.Warn($"CSV file not found: {filePath}");
                    return new List<T>();
                }

                var lines = File.ReadAllLines(filePath);
                if (lines.Length == 0)
                {
                    return new List<T>();
                }

                var properties = GetSerializableProperties<T>();
                var result = new List<T>();

                // Skip header (first line)
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = ParseCsvLine(lines[i]);
                    if (values.Length == properties.Count)
                    {
                        var item = CreateInstanceFromCsvValues<T>(properties, values);
                        if (item != null)
                        {
                            result.Add(item);
                        }
                    }
                }

                _logger.Info($"Successfully loaded {result.Count} items from {filePath}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading data from CSV file {filePath}", ex);
                throw new Exception($"Failed to load data from CSV file: {filePath}", ex);
            }
        }

        public T LoadSingleEntity<T>(string filePath) where T : class
        {
            var data = LoadData<T>(filePath);
            return data.FirstOrDefault();
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

        private List<PropertyInfo> GetSerializableProperties<T>()
        {
            return typeof(T).GetProperties()
                .Where(p => p.CanRead && p.CanWrite &&
                           p.GetCustomAttribute<DataMemberAttribute>() != null)
                .ToList();
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Escape quotes and wrap in quotes if contains separator or quotes
            if (value.Contains(CSV_SEPARATOR) || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var currentValue = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote
                        currentValue.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == CSV_SEPARATOR[0] && !inQuotes)
                {
                    result.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            result.Add(currentValue.ToString());
            return result.ToArray();
        }

        private T CreateInstanceFromCsvValues<T>(List<PropertyInfo> properties, string[] values) where T : class
        {
            try
            {
                var instance = Activator.CreateInstance<T>();

                for (int i = 0; i < Math.Min(properties.Count, values.Length); i++)
                {
                    var prop = properties[i];
                    var value = values[i];

                    if (!string.IsNullOrEmpty(value))
                    {
                        var convertedValue = ConvertFromString(value, prop.PropertyType);
                        prop.SetValue(instance, convertedValue);
                    }
                }

                return instance;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Error creating instance from CSV values: {ex.Message}");
                return null;
            }
        }

        private object ConvertFromString(string value, Type targetType)
        {
            try
            {
                if (targetType == typeof(string))
                    return value;

                if (targetType.IsEnum)
                    return Enum.Parse(targetType, value);

                if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                    return DateTime.Parse(value, CultureInfo.InvariantCulture);

                if (targetType == typeof(bool) || targetType == typeof(bool?))
                    return bool.Parse(value);

                if (targetType == typeof(int) || targetType == typeof(int?))
                    return int.Parse(value);

                if (targetType == typeof(double) || targetType == typeof(double?))
                    return double.Parse(value, CultureInfo.InvariantCulture);

                if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                    return decimal.Parse(value, CultureInfo.InvariantCulture);

                // Generic conversion for other types
                return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch
            {
                return GetDefaultValue(targetType);
            }
        }

        private object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}