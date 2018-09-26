using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using SharpDX;

public static class BinaryReaderMethods
{
    public static void ReadVector4(this BinaryReader reader, out Vector4 v)
    {
        v.X = reader.ReadSingle();
        v.Y = reader.ReadSingle();
        v.Z = reader.ReadSingle();
        v.W = reader.ReadSingle();
    }

    public static void ReadQuaternion(this BinaryReader reader, out Quaternion q)
    {
        q.X = reader.ReadSingle();
        q.Y = reader.ReadSingle();
        q.Z = reader.ReadSingle();
        q.W = reader.ReadSingle();
    }

    public static string ReadHeaderString(this BinaryReader reader)
    {
        StringBuilder string_builder = new StringBuilder();
        while (true)
        {
            char c = reader.ReadChar();
            if (c == 10)
                    break;
            string_builder.Append(c);
        }
        return string_builder.ToString();
    }

    public static string ReadCString(this BinaryReader reader)
    {
        StringBuilder string_builder = new StringBuilder();
        while (true)
        {
            char c = reader.ReadChar();
            if (c == 0)
                    break;
            string_builder.Append(c);
        }
        return string_builder.ToString();
    }
}
