﻿using OxLibrary;
using OxDAOEngine.Data.Types;
using System.Xml;

namespace OxDAOEngine.XML
{
    public class XmlHelper
    {
        public static string NormalizeNameString(string name) => 
            name.Trim().Replace(" ", "_");
        public static XmlElement? AppendElement(XmlElement parentElement, string name, string? value, bool nullIfEmpty = false)
        {
            name = NormalizeNameString(name);
            string normalizedValue = value ?? string.Empty;
            if (normalizedValue == string.Empty && nullIfEmpty)
                return null;

            XmlElement fieldElement = parentElement.OwnerDocument.CreateElement(name);

            if (normalizedValue != string.Empty)
                fieldElement.InnerText = normalizedValue;

            parentElement.AppendChild(fieldElement);
            return fieldElement;
        }

        public static XmlElement? AppendElement(XmlElement parentElement, string name, object? value)
        {
            name = NormalizeNameString(name);
            return value switch
            {
                null => null,
                Bitmap bitmap =>
                    AppendElement(parentElement, name, OxBase64.BitmapToBase64(bitmap)),
                _ => TypeHelper.IsTypeHelpered(value)
                    ? AppendElement(parentElement, name, TypeHelper.XmlValue(value))
                    : AppendElement(parentElement, name, value?.ToString()),
            };
        }

        public static XmlElement? AppendElement(XmlElement parentElement, string name, bool value) => 
            AppendElement(parentElement, NormalizeNameString(name), value.ToString());

        public static string Value(XmlElement parentElement, string name)
        {
            name = NormalizeNameString(name);

            foreach (XmlNode node in parentElement.ChildNodes)
                if (name == node.Name)
                    return node.InnerText;

            return string.Empty;
        }

        public static int ValueInt(XmlElement parentElement, string name) => 
            int.TryParse(Value(parentElement, name), out int value) 
                ? value 
                : 0;

        public static bool ValueBool(XmlElement parentElement, string name) => 
            bool.TryParse(Value(parentElement, name), out bool value) && value;

        public static Guid ValueGuid(XmlElement parentElement, string name) =>
            Guid.TryParse(Value(parentElement, name), out Guid id)
                ? id
                : Guid.Empty;

        public static T Value<T>(XmlElement parentElement, string name)
            where T : notnull, Enum => 
            TypeHelper.Parse<T>(Value(parentElement, name));

        public static Bitmap? ValueBitmap(XmlElement parentElement, string name) =>
            OxBase64.Base64ToBitmap(Value(parentElement, name));
    }
}