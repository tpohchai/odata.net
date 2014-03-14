//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


#if ASTORIA_CLIENT
namespace System.Data.Services.Client
#else
namespace System.Data.Services
#endif
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal static partial class UriUtil
    {
#if !ASTORIA_CLIENT
        private static Uri CreateBaseComparableUri(Uri uri)
        {
            Debug.Assert(uri != null, "uri != null");

            uri = new Uri(uri.OriginalString.ToUpper(CultureInfo.InvariantCulture), UriKind.RelativeOrAbsolute);

            UriBuilder builder = new UriBuilder(uri);
            builder.Host = "h";
            builder.Port = 80;
            builder.Scheme = "http";
            return builder.Uri;
        }
#endif

        internal static string GetNameFromAtomLinkRelationAttribute(string value)
        {
            string name = null;
            if (!String.IsNullOrEmpty(value))
            {
                Uri uri = null;
                try
                {
                    uri = new Uri(value, UriKind.RelativeOrAbsolute);
                }
                catch (UriFormatException)
                {   
                }

                if ((null != uri) && uri.IsAbsoluteUri)
                {
                    string unescaped = uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
                    if (unescaped.StartsWith(XmlConstants.DataWebRelatedNamespace, StringComparison.Ordinal))
                    {
                        name = unescaped.Substring(XmlConstants.DataWebRelatedNamespace.Length);
                    }
                }
            }

            return name;
        }
#if !ASTORIA_CLIENT
        internal static bool IsBaseOf(Uri baseUriWithSlash, Uri requestUri)
        {
            return baseUriWithSlash.IsBaseOf(requestUri);
        }
        internal static bool UriInvariantInsensitiveIsBaseOf(Uri current, Uri uri)
        {
            Debug.Assert(current != null, "current != null");
            Debug.Assert(uri != null, "uri != null");

            Uri upperCurrent = CreateBaseComparableUri(current);
            Uri upperUri = CreateBaseComparableUri(uri);

            return UriUtil.IsBaseOf(upperCurrent, upperUri);
        }
#endif
    }

    internal static partial class XmlUtil
    {
        private static NameTable CreateAtomNameTable()
        {
            NameTable table = new NameTable();
            table.Add(XmlConstants.AtomNamespace);
            table.Add(XmlConstants.DataWebNamespace);
            table.Add(XmlConstants.DataWebMetadataNamespace);

            table.Add(XmlConstants.AtomContentElementName);
#if ASTORIA_CLIENT
            table.Add(XmlConstants.AtomContentSrcAttributeName);
#endif
            table.Add(XmlConstants.AtomEntryElementName);
            table.Add(XmlConstants.AtomETagAttributeName);
            table.Add(XmlConstants.AtomFeedElementName);
#if ASTORIA_CLIENT
            table.Add(XmlConstants.AtomIdElementName);
#endif
            table.Add(XmlConstants.AtomInlineElementName);
#if ASTORIA_CLIENT
            table.Add(XmlConstants.AtomLinkElementName);
            table.Add(XmlConstants.AtomLinkRelationAttributeName);
#endif
            table.Add(XmlConstants.AtomNullAttributeName);
            table.Add(XmlConstants.AtomPropertiesElementName);
            table.Add(XmlConstants.AtomTitleElementName);
            table.Add(XmlConstants.AtomTypeAttributeName);

            table.Add(XmlConstants.XmlErrorCodeElementName);
            table.Add(XmlConstants.XmlErrorElementName);
            table.Add(XmlConstants.XmlErrorInnerElementName);
            table.Add(XmlConstants.XmlErrorMessageElementName);
            table.Add(XmlConstants.XmlErrorTypeElementName);
            return table;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing the XmlReader")]
        internal static XmlReader CreateXmlReader(Stream stream, Encoding encoding)
        {
            Debug.Assert(null != stream, "null stream");

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.CheckCharacters = false;
#if ASTORIA_CLIENT
            settings.CloseInput = true;
#endif
            settings.IgnoreWhitespace = true;
            settings.NameTable = XmlUtil.CreateAtomNameTable();

            if (null == encoding)
            {   
                return XmlReader.Create(stream, settings);
            }

            return XmlReader.Create(new StreamReader(stream, encoding), settings);
        }

        internal static XmlWriterSettings CreateXmlWriterSettings(Encoding encoding)
        {
            Debug.Assert(null != encoding, "null != encoding");

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.CheckCharacters = false;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = encoding;
            settings.Indent = true;
            settings.NewLineHandling = NewLineHandling.Entitize;

            Debug.Assert(!settings.CloseOutput, "!settings.CloseOutput -- otherwise default changed?");

            return settings;
        }

        internal static XmlWriter CreateXmlWriterAndWriteProcessingInstruction(Stream stream, Encoding encoding)
        {
            Debug.Assert(null != stream, "null != stream");
            Debug.Assert(null != encoding, "null != encoding");

            XmlWriterSettings settings = CreateXmlWriterSettings(encoding);
            XmlWriter writer = XmlWriter.Create(stream, settings);
            writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"" + encoding.WebName + "\" standalone=\"yes\"");
            return writer;
        }

#if ASTORIA_CLIENT
        internal static string GetAttributeEx(this XmlReader reader, string attributeName, string namespaceUri)
        {
            return reader.GetAttribute(attributeName, namespaceUri) ?? reader.GetAttribute(attributeName);
        }

        internal static void RemoveDuplicateNamespaceAttributes(System.Xml.Linq.XElement element)
        {
            Debug.Assert(element != null, "element != null");

            HashSet<string> names = new HashSet<string>(EqualityComparer<string>.Default);
            foreach (System.Xml.Linq.XElement e in element.DescendantsAndSelf())
            {
                bool attributesFound = false;
                foreach (var attribute in e.Attributes())
                {
                    if (!attributesFound)
                    {
                        attributesFound = true;
                        names.Clear();
                    }

                    if (attribute.IsNamespaceDeclaration)
                    {
                        string localName = attribute.Name.LocalName;
                        bool alreadyPresent = names.Add(localName) == false;
                        if (alreadyPresent)
                        {
                            attribute.Remove();
                        }
                    }
                }
            }
        }
#endif
    }
}