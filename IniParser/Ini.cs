using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IniParser
{
    public static class Ini
    {
        #region Read
        public static async Task<T?> ParseAsync<T>(string location) where T : class
        {
            if (string.IsNullOrEmpty(location)) return null;

            if (!File.Exists(location)) return null;

            var obj = (T)Activator.CreateInstance(typeof(T));
            var header = string.Empty;
            var tempList = new Dictionary<string, List<string>>();

            using (var reader = new StreamReader(location))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);

                    if (string.IsNullOrEmpty(line)) continue;

                    if (IsHeader(obj, line, out var tempHeader))
                    {
                        if (tempList.Count > 0)
                        {
                            AssignList(obj, header, tempList);
                            tempList.Clear();
                        }

                        header = tempHeader;
                        continue;
                    }

                    ProcessKeyPair(obj, header, line, tempList);
                }
            }

            if (tempList.Count > 0) AssignList(obj, header, tempList);

            return obj;
        }

        public static T? Parse<T>(string location) where T : class
        {
            if (string.IsNullOrEmpty(location)) return null;

            if (!File.Exists(location)) return null;

            var obj = (T)Activator.CreateInstance(typeof(T));
            var header = string.Empty;
            var tempList = new Dictionary<string, List<string>>();

            using (var reader = new StreamReader(location))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (string.IsNullOrEmpty(line)) continue;

                    if (IsHeader(obj, line, out var tempHeader))
                    {
                        if (tempList.Count > 0)
                        {
                            AssignList(obj, header, tempList);
                            tempList.Clear();
                        }

                        header = tempHeader;
                        continue;
                    }

                    ProcessKeyPair(obj, header, line, tempList);
                }
            }

            if (tempList.Count > 0) AssignList(obj, header, tempList);

            return obj;
        }

        private static bool IsHeader<T>(T obj, string header, out string propName)
        {
            propName = string.Empty;

            if (header[0] != '[' || header[^1] != ']') return false;

            var span = header.AsSpan(1, header.Length - 2); // Slice off the brackets
            if (span.IndexOf('[') >= 0 || span.IndexOf(']') >= 0) return false;

            propName = span.ToString(); // Still an allocation, but minimal

            var propType = obj!.GetType().GetProperty(propName);
            if (propType == null) throw new InvalidCastException($"{propName} does not exist in Config File");

            var headerInst = propType.GetValue(obj, null);
            if (headerInst is null)
            {
                var newHeaderInst = Activator.CreateInstance(propType.PropertyType);
                propType.SetValue(obj, newHeaderInst, null);
            }

            return true;
        }

        private static bool ProcessKeyPair<T>(T obj, string header, string line, Dictionary<string, List<string>> tempList)
        {
            var parts = line.Split('=');

            if (parts.Length != 2) return false;

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            bool isList = key.EndsWith("[]");
            key = isList ? key[..^2] : key;

            var headerProperty = typeof(T).GetProperty(header);

            var headerInst = headerProperty.GetValue(obj, null);

            var sectionProperty = headerInst.GetType().GetProperty(key);

            if (sectionProperty is null) return false;

            if (isList && (sectionProperty.PropertyType == typeof(List<string>) || sectionProperty.PropertyType == typeof(string[])))
            {
                if (!tempList.ContainsKey(key))
                {
                    tempList.Add(key, new List<string>());
                }

                tempList[key].Add(value);
            }
            else
            {
                object convertedValue = ConvertValue(sectionProperty.PropertyType, value);
                sectionProperty.SetValue(headerInst, convertedValue);
            }

            return true;
        }

        private static object ConvertValue(Type type, string value)
        {
            if (type == typeof(bool)) return Convert.ToBoolean(value);
            if (type == typeof(char)) return char.Parse(value);
            if (type == typeof(sbyte)) return sbyte.Parse(value);
            if (type == typeof(byte)) return byte.Parse(value);
            if (type == typeof(short)) return short.Parse(value);
            if (type == typeof(ushort)) return ushort.Parse(value);
            if (type == typeof(int)) return Convert.ToInt32(value);
            if (type == typeof(uint)) return Convert.ToUInt32(value);
            if (type == typeof(long)) return Convert.ToInt64(value);
            if (type == typeof(ulong)) return Convert.ToUInt64(value);
            if (type == typeof(float)) return Convert.ToSingle(value);
            if (type == typeof(double)) return Convert.ToDouble(value);
            if (type == typeof(decimal)) return Convert.ToDecimal(value);
            if (type == typeof(string)) return value;
            // Add more type conversions as needed

            throw new InvalidOperationException($"Unsupported type: {type}");
        }

        private static void AssignList<T>(T obj, string header, Dictionary<string, List<string>> tempList)
        {
            var headerProperty = obj!.GetType().GetProperty(header);
            var headerPropInstance = headerProperty.GetValue(obj, null);

            foreach (var item in tempList)
            {
                var prop = headerPropInstance.GetType().GetProperty(item.Key);
                if (prop == null) continue;

                if (prop.PropertyType == typeof(List<string>))
                    prop.SetValue(headerPropInstance, item.Value, null);
                else if (prop.PropertyType == typeof(string[]))
                    prop.SetValue(headerPropInstance, item.Value.ToArray());
            }
        }

        #endregion


        #region Write

        public static void Write<T>(string location, T obj) where T : class
        {
            if (string.IsNullOrEmpty(location)) return;
            if (location.EndsWith(".ini") == false) throw new ArgumentException("File must end with .ini");
            if (obj is null) return;

            if (!Directory.Exists(location))
            {
                Directory.CreateDirectory(location);
            }

            var sb = new StringBuilder();

            using var writer = new StreamWriter(location);

            // Get All Properties
            var properties = obj.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var property in properties)
            {
                // Write section header
                var propName = property.Name;
                var propValue = property.GetValue(obj, null);
                if (propValue is null) continue;

                sb.Append($"[{propName}]");

                var subProperties = propValue.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var subProperty in subProperties)
                {
                    // Get sub-property name and value
                    var subPropName = subProperty.Name;
                    var subPropValue = subProperty.GetValue(propValue, null);
                    if (subPropValue is null) continue;
                    if (subPropValue is List<string> list)
                    {
                        foreach (var item in list)
                        {
                            sb.AppendLine($"{subPropName}[] = {item}");
                        }
                    }
                    else if (subPropValue is string[] array)
                    {
                        foreach (var item in array)
                        {
                            sb.AppendLine($"{subPropName}[] = {item}");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"{subPropName} = {subPropValue}");
                    }
                }
                sb.AppendLine();
            }

            // Write to file
            writer.Write(sb.ToString().TrimEnd());
        }

        #endregion
    }
}
