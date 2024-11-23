using System;
using System.IO;
using System.Xml.Linq;

namespace MultiBloxy
{
    public class Config
    {
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");
        private static XDocument configDocument;

        // Static constructor to initialize the config document
        static Config()
        {
            Load();
        }

        // Loads the configuration from the config file
        public static void Load()
        {
            if (File.Exists(ConfigFilePath))
            {
                configDocument = XDocument.Load(ConfigFilePath);
            }
            else
            {
                configDocument = new XDocument(new XElement("Config"));
            }
        }

        // Saves the configuration to the config file
        public static void Save()
        {
            if (!configDocument.Root.HasElements)
            {
                if (File.Exists(ConfigFilePath))
                {
                    File.Delete(ConfigFilePath);
                }
            }
            else
            {
                configDocument.Save(ConfigFilePath);
            }
        }

        // Sets a configuration value for a given key
        public static void Set(string key, object value)
        {
            var element = configDocument.Root.Element(key);
            if (element != null)
            {
                element.Value = value.ToString();
            }
            else
            {
                configDocument.Root.Add(new XElement(key, value));
            }
            Save();
        }

        // Gets a configuration value for a given key, with default value
        public static T Get<T>(string key, T defaultValue = default)
        {
            var element = configDocument.Root.Element(key);
            if (element != null)
            {
                return (T)Convert.ChangeType(element.Value, typeof(T));
            }
            return defaultValue;
        }

        // Checks if a configuration key exists
        public static bool Has(string key)
        {
            return configDocument.Root.Element(key) != null;
        }

        // Removes a configuration key
        public static void Remove(string key)
        {
            var element = configDocument.Root.Element(key);
            if (element != null)
            {
                element.Remove();
                Save();
            }
        }
    }
}