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
        private const string CSV_SEPARATOR = ",";
        private const string LIST_SEPARATOR = ";"; // Za liste ID-jeva

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
                        string formatted = FormatValueForCsv(value);
                        return EscapeCsvValue(formatted);
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
                _logger.Debug($"Found {properties.Count} serializable properties for type {typeof(T).Name}");

                // Debug: log property names
                _logger.Debug($"Properties: {string.Join(", ", properties.Select(p => $"{p.Name}({p.PropertyType.Name})"))}");

                var result = new List<T>();

                // Parse header to match properties by name
                var headerValues = ParseCsvLine(lines[0]);
                _logger.Debug($"CSV Headers: {string.Join(", ", headerValues)}");

                // Skip header (first line)
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = ParseCsvLine(lines[i]);
                    _logger.Debug($"Line {i}: Found {values.Length} values");

                    var item = CreateInstanceFromCsvValues<T>(properties, headerValues, values);
                    if (item != null)
                    {
                        result.Add(item);
                    }
                    else
                    {
                        _logger.Warn($"Failed to create instance from line {i}: {lines[i]}");
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
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead && p.CanWrite)
                .ToList();

            // Prvo pokušaj sa DataMember atributom
            var dataMemberProperties = properties
                .Where(p => p.GetCustomAttribute<DataMemberAttribute>() != null)
                .ToList();

            if (dataMemberProperties.Any())
            {
                return dataMemberProperties;
            }

            // Ako nema DataMember atributa, uzmi sva javna svojstva
            return properties
                .Where(p => IsSerializableType(p.PropertyType))
                .ToList();
        }

        private bool IsSerializableType(Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                type = nullableType;

            return type.IsPrimitive ||
                   type.IsEnum ||
                   type == typeof(string) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(TimeSpan) ||
                   type == typeof(decimal) ||
                   type == typeof(Guid) ||
                   IsListOfSimpleType(type);
        }

        private bool IsListOfSimpleType(Type type)
        {
            if (!type.IsGenericType) return false;

            var genericTypeDef = type.GetGenericTypeDefinition();
            if (genericTypeDef != typeof(List<>) && genericTypeDef != typeof(IList<>) &&
                genericTypeDef != typeof(IEnumerable<>) && genericTypeDef != typeof(ICollection<>))
                return false;

            var elementType = type.GetGenericArguments()[0];
            return elementType.IsPrimitive || elementType == typeof(string) || elementType == typeof(Guid);
        }

        private string FormatValueForCsv(object value)
        {
            if (value == null)
                return string.Empty;

            // Handle lists (ClothingIds, EquipmentIds)
            if (value is System.Collections.IEnumerable enumerable && !(value is string))
            {
                var items = enumerable.Cast<object>().Select(x => x?.ToString() ?? "");
                return string.Join(LIST_SEPARATOR, items);
            }

            // Handle DateTime with timezone
            if (value is DateTimeOffset dateTimeOffset)
                return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);

            if (value is DateTime dateTime)
                return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);

            // Handle TimeSpan
            if (value is TimeSpan timeSpan)
                return timeSpan.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);

            // DODAJ OVO: Handle double values explicitly
            if (value is double doubleValue)
                return doubleValue.ToString("F6", CultureInfo.InvariantCulture);

            // DODAJ OVO: Handle decimal values explicitly
            if (value is decimal decimalValue)
                return decimalValue.ToString("F6", CultureInfo.InvariantCulture);

            // DODAJ OVO: Handle float values explicitly
            if (value is float floatValue)
                return floatValue.ToString("F6", CultureInfo.InvariantCulture);

            return value.ToString();
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Escape quotes and wrap in quotes if contains separator or quotes
            if (value.Contains(CSV_SEPARATOR) || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
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

        private T CreateInstanceFromCsvValues<T>(List<PropertyInfo> properties, string[] headers, string[] values) where T : class
        {
            try
            {
                var instance = Activator.CreateInstance<T>();

                for (int i = 0; i < Math.Min(headers.Length, values.Length); i++)
                {
                    var headerName = headers[i].Trim();
                    var value = values[i];

                    // Pronađi svojstvo po imenu (case-insensitive)
                    var prop = properties.FirstOrDefault(p =>
                        string.Equals(p.Name, headerName, StringComparison.OrdinalIgnoreCase));

                    if (prop != null && !string.IsNullOrEmpty(value))
                    {
                        try
                        {
                            var convertedValue = ConvertFromString(value, prop.PropertyType);
                            if (convertedValue != null)
                            {
                                prop.SetValue(instance, convertedValue);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"Failed to convert value '{value}' for property '{prop.Name}' (type: {prop.PropertyType.Name}): {ex.Message}");
                        }
                    }
                    else if (prop == null)
                    {
                        _logger.Debug($"Property '{headerName}' not found in type {typeof(T).Name}");
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
                if (string.IsNullOrWhiteSpace(value))
                    return GetDefaultValue(targetType);

                value = value.Trim();

                // Handle nullable types
                var nullableType = Nullable.GetUnderlyingType(targetType);
                if (nullableType != null)
                {
                    return ConvertFromString(value, nullableType);
                }

                if (targetType == typeof(string))
                    return value;

                // Handle lists (ClothingIds, EquipmentIds)
                if (IsListOfSimpleType(targetType))
                {
                    return ConvertToList(value, targetType);
                }

                if (targetType.IsEnum)
                    return Enum.Parse(targetType, value, true); // ignoreCase = true

                // Handle DateTime with timezone
                if (targetType == typeof(DateTimeOffset))
                {
                    if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dateTimeOffsetResult))
                        return dateTimeOffsetResult;
                    return GetDefaultValue(targetType);
                }

                if (targetType == typeof(DateTime))
                {
                    // First try to parse as DateTimeOffset, then extract DateTime
                    if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dateTimeOffsetResult))
                        return dateTimeOffsetResult.DateTime;

                    if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateResult))
                        return dateResult;

                    return GetDefaultValue(targetType);
                }

                // Handle TimeSpan
                if (targetType == typeof(TimeSpan))
                {
                    if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out TimeSpan timeSpanResult))
                        return timeSpanResult;
                    return GetDefaultValue(targetType);
                }

                if (targetType == typeof(bool))
                    return bool.Parse(value);

                if (targetType == typeof(int))
                    return int.Parse(value);

                if (targetType == typeof(double))
                    return double.Parse(value, CultureInfo.InvariantCulture);

                if (targetType == typeof(decimal))
                    return decimal.Parse(value, CultureInfo.InvariantCulture);

                if (targetType == typeof(Guid))
                    return Guid.Parse(value);

                // Generic conversion for other types
                return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                _logger.Debug($"Conversion failed for value '{value}' to type '{targetType.Name}': {ex.Message}");
                return GetDefaultValue(targetType);
            }
        }

        private object ConvertToList(string value, Type listType)
        {
            try
            {
                var elementType = listType.GetGenericArguments()[0];
                var listInstance = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                var addMethod = listInstance.GetType().GetMethod("Add");

                if (string.IsNullOrWhiteSpace(value))
                    return listInstance;

                var items = value.Split(new[] { LIST_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in items)
                {
                    var convertedItem = ConvertFromString(item.Trim(), elementType);
                    if (convertedItem != null)
                    {
                        addMethod.Invoke(listInstance, new[] { convertedItem });
                    }
                }

                return listInstance;
            }
            catch (Exception ex)
            {
                _logger.Debug($"Failed to convert list value '{value}' to type '{listType.Name}': {ex.Message}");
                return Activator.CreateInstance(listType);
            }
        }

        private object GetDefaultValue(Type type)
        {
            if (type == typeof(string))
                return string.Empty;

            if (IsListOfSimpleType(type))
                return Activator.CreateInstance(type);

            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}