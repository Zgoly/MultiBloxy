using System;
using System.IO;
using System.Xml.Linq;

namespace MultiBloxy
{
    public class Config
    {
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");
        private static XDocument configDocument;

        static Config()
        {
            Load();
        }

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

        public static T Get<T>(string key, T defaultValue = default)
        {
            var element = configDocument.Root.Element(key);
            if (element != null)
            {
                return (T)Convert.ChangeType(element.Value, typeof(T));
            }
            return defaultValue;
        }

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