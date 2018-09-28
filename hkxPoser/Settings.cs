using System;
using System.Drawing;
using System.Xml;
using System.Xml.Serialization;

namespace hkxPoser
{
    public class Settings
    {
        public Size ClientSize { get; set; }
        public SharpDX.Color ScreenColor { get; set; }

        public static Settings Default
        {
            get
            {
                Settings settings = new Settings();
                settings.ClientSize = new Size(640, 640);
                settings.ScreenColor = new SharpDX.Color(192, 192, 192, 255);

                return settings;
            }
        }

        public void Dump()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(Console.Out, settings);
            serializer.Serialize(writer, this);
            writer.Close();
        }

        public static Settings Load(string path)
        {
            XmlReader reader = XmlReader.Create(path);
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            Settings settings = serializer.Deserialize(reader) as Settings;
            reader.Close();

            return settings;
        }
    }
}
