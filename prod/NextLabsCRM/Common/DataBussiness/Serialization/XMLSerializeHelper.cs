using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Serialization
{
    public class XMLSerializeHelper: IStringSerialize
    {
        /// <summary>
        /// Deserilize from xml
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="xml">xml string</param>
        /// <returns>null: either type is null or xml is null/empty; valid object: otherwise</returns>
        /// <exception cref="System.Exception">throw exception when deserialization failed</exception>
        object IStringSerialize.Deserialize(Type type, string xml)
        {
            if (type == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }

            StringReader reader = null;
            try
            {
                reader = new StringReader(xml);

                XmlSerializer xmlDeserializer = new XmlSerializer(type);
                return xmlDeserializer.Deserialize(reader);
            }
            catch (Exception exp)
            {
                string msg = string.Format("Failed to deserialize {0} from XML {1} for {2}",
                    type.ToString(), xml, exp.Message);
                throw new Exception(msg, exp);
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Serializer an object to XML
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns>null:either type is null or obj is null; xml: otherwise</returns>
        /// <exception cref="System.Exception">thow exception when serilaization failed</exception>
        string IStringSerialize.Serialize(Type type, object obj)
        {
            if( type == null)
            {
                return null;
            }

            if (obj == null)
            {
                return null;
            }

            MemoryStream stream = new MemoryStream();
            XmlSerializer xmlSerializer = new XmlSerializer(type);
            string xml = null;
            StreamReader streamReader = null;
            try
            {
                xmlSerializer.Serialize(stream, obj);

                stream.Position = 0;
                streamReader = new StreamReader(stream);
                xml = streamReader.ReadToEnd();
            }
            catch (Exception exp)
            {
                string msg = string.Format("Failed to Serialize {0} from XML {1} for {2}",
                    type.ToString(), xml, exp.Message);
                throw new Exception(msg, exp);
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                }

                if (stream != null)
                {
                    stream.Close();
                }
            }
            return xml;
        }

        string IStringSerialize.SerializeWithoutNamespace(Type type, object obj)
        {
            if (type == null)
            {
                return null;
            }

            if (obj == null)
            {
                return null;
            }

            MemoryStream stream = new MemoryStream();
            XmlSerializer xmlSerializer = new XmlSerializer(type);
            string xml = null;
            StreamReader streamReader = null;
            try
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                xmlSerializer.Serialize(stream, obj, ns);

                stream.Position = 0;
                streamReader = new StreamReader(stream);
                xml = streamReader.ReadToEnd();
                xml = xml.Replace("<?xml version=\"1.0\"?>\r\n", string.Empty);
                return xml;
            }
            catch (Exception exp)
            {
                string msg = string.Format("Failed to Serialize {0} from XML {1} for {2}",
                    type.ToString(), xml, exp.Message);
                throw new Exception(msg, exp);
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                }

                if (stream != null)
                {
                    stream.Close();
                }
            }
        }
    }
}
