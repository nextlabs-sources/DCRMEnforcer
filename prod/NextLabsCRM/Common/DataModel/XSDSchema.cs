using System;
using System.Xml;
using System.Xml.Schema;
using System.IO;

namespace NextLabs.CRMEnforcer.Common.DataModel
{
    static class XSDSchema
    {
        private const string SecureEntitySchema = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
        xmlns='urn:nextlabs-crm-schema'
        elementFormDefault='qualified'
        targetNamespace='urn:nextlabs-crm-schema'>

        <xs:simpleType name='logicnametype'>
         <xs:restriction base='xs:string'>
         <xs:pattern value='[a-z0-9A-Z_]+'/>
         <xs:maxLength value='100'/>
         </xs:restriction>
        </xs:simpleType>

        <xs:simpleType name='displaynametype'>
         <xs:restriction base='xs:string'>
         <xs:pattern value='[^\~\*\$\\&amp;]+'/>
         <xs:whiteSpace value='preserve'/>
         <xs:maxLength value='255'/>
         </xs:restriction>
        </xs:simpleType>

        <xs:simpleType name='pluralnametype'>
         <xs:restriction base='xs:string'>
          <xs:pattern value='[^\~\*\$\\&amp;]+'/>
          <xs:whiteSpace value='preserve'/>
          <xs:maxLength value='255'/>
         </xs:restriction>
        </xs:simpleType>

        <xs:simpleType name='datatypetype'>
          <xs:restriction base='xs:string'>
            <xs:enumeration value='BigInt'/>
	        <xs:enumeration value='Boolean'/>
	        <xs:enumeration value='Customer'/>
	        <xs:enumeration value='DateTime'/>
	        <xs:enumeration value='Decimal'/>
	        <xs:enumeration value='Double'/>
	        <xs:enumeration value='EntityName'/>
	        <xs:enumeration value='Integer'/>
	        <xs:enumeration value='Lookup'/>
	        <xs:enumeration value='Memo'/>
	        <xs:enumeration value='Money'/>
	        <xs:enumeration value='Owner'/>
	        <xs:enumeration value='Picklist'/>
	        <xs:enumeration value='State'/>
	        <xs:enumeration value='Status'/>
	        <xs:enumeration value='String'/>
	        <xs:enumeration value='Uniqueidentifier'/>
          </xs:restriction>
        </xs:simpleType>

          <xs:element name='secureentity'>
            <xs:complexType>
              <xs:sequence>
                <xs:element name='secured' type='xs:boolean' minOccurs='0' />
                <xs:element name='schema' minOccurs='0' maxOccurs='unbounded'>
                  <xs:complexType>
                    <xs:sequence>
                      <xs:element name='owneridexist' type='xs:boolean' minOccurs='0' />
                      <xs:element name='primaryidname' type='logicnametype' minOccurs='1' />
                      <xs:element name='logicname' type='logicnametype' minOccurs='1' />
                      <xs:element name='displayname' type='displaynametype' minOccurs='0' />
                      <xs:element name='pluralname' type='pluralnametype' minOccurs='0' />
                      <xs:element name='objecttypecode' type = 'xs:int' minOccurs = '0' />
                      <xs:element name='attributes' minOccurs='0' maxOccurs='unbounded'>
                        <xs:complexType>
                          <xs:sequence>
                            <xs:element name='attribute' minOccurs='0' maxOccurs='unbounded'>
                              <xs:complexType>
                                <xs:sequence>
                                  <xs:element name='logicname' type='logicnametype' minOccurs='0' />
                                  <xs:element name='displayname' type='displaynametype' minOccurs='0' />
                                  <xs:element name='datatype' type='datatypetype' minOccurs='0' />
                                  <xs:element name='required' type='xs:boolean' minOccurs='0' />
                                  <xs:element name='description' type='xs:string' minOccurs='0' />
                                  <xs:element name='options' minOccurs='0' maxOccurs='unbounded'>
                                    <xs:complexType>
                                      <xs:sequence>
                                        <xs:element name='option' minOccurs='0' maxOccurs='unbounded'>
                                          <xs:complexType>
                                            <xs:attribute name='label' type='xs:string' />
                                            <xs:attribute name='value' type='xs:integer' />
                                          </xs:complexType>
                                        </xs:element>
                                      </xs:sequence>
                                    </xs:complexType>
                                  </xs:element>
                                </xs:sequence>
                              </xs:complexType>
                            </xs:element>
                          </xs:sequence>
                        </xs:complexType>
                      </xs:element>
                    </xs:sequence>
                  </xs:complexType>
                </xs:element>
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>";

        private const string GeneralSettingSchema = @"<xs:schema  xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns='urn:nextlabs-crm-schema'
        elementFormDefault='qualified'
        targetNamespace='urn:nextlabs-crm-schema'>

        <xs:simpleType name='policydecisiontype'>
          <xs:restriction base='xs:string'>
            <xs:enumeration value='Deny'/>
	        <xs:enumeration value='Allow'/>
          </xs:restriction>
        </xs:simpleType>

        <xs:simpleType name='hostnametype'>
         <xs:restriction base='xs:string'>
          <xs:pattern value='^((http|https):(\/\/|\\\\))?[a-z0-9A-Z\\\/\-_]+(\.{1}[a-z0-9A-Z\\\/\-_]+)+[a-z0-9A-Z\\\/\-_]?$'/>
          <xs:whiteSpace value='preserve'/>
          <xs:maxLength value='255'/>
         </xs:restriction>
        </xs:simpleType>

          <xs:element name='generalsetting'>
            <xs:complexType>
              <xs:sequence>
                <xs:element name='policycontrolhost' type='hostnametype' minOccurs='0' />
                <xs:element name='policycontrolport' type='xs:integer' minOccurs='0' />
                <xs:element name='ishttps' type='xs:boolean' minOccurs='0' />
                <xs:element name='oauthserverhost' type='hostnametype' minOccurs='0' />
                <xs:element name='oauthport' type='xs:integer' minOccurs='0' />
                <xs:element name='clientid' type='xs:string' minOccurs='0' />
                <xs:element name='clientpassword' type='xs:string' minOccurs='0' />
                <xs:element name='policydecision' type='policydecisiontype' minOccurs='0' />
                <xs:element name='message' type='xs:string' minOccurs='0' />
                <xs:element name='defaultmessage' type='xs:string' minOccurs='0' />
                <xs:element name='cacherefreshinterval' type='xs:int' minOccurs='0' />
                <xs:element name='timecostenable' type='xs:boolean' minOccurs='0' />
                <xs:element name='spactioneable' type='xs:boolean' minOccurs='0' />
                <xs:element name='wcusrname' type='xs:string' minOccurs='0' />
                <xs:element name='wcpwd' type='xs:string' minOccurs='0' />
                <xs:element name='userhostip' type='xs:string' minOccurs='0' />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>";

        private const string TestPCSchema = @"<xs:schema  xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns='urn:nextlabs-crm-schema'
        elementFormDefault='qualified'
        targetNamespace='urn:nextlabs-crm-schema'>

        <xs:simpleType name='hostnametype'>
         <xs:restriction base='xs:string'>
          <xs:pattern value='^((http|https):(\/\/|\\\\))?[a-z0-9A-Z\\\/\-_]+(\.{1}[a-z0-9A-Z\\\/\-_]+)+[a-z0-9A-Z\\\/\-_]?$'/>
          <xs:whiteSpace value='preserve'/>
          <xs:maxLength value='255'/>
         </xs:restriction>
        </xs:simpleType>

          <xs:element name='testpc'>
            <xs:complexType>
              <xs:sequence>
                <xs:element name='policycontrolhost' type='hostnametype' minOccurs='0' />
                <xs:element name='policycontrolport' type='xs:integer' minOccurs='0' />
                <xs:element name='ishttps' type='xs:boolean' minOccurs='0' />
                <xs:element name='oauthserverhost' type='hostnametype' minOccurs='0' />
                <xs:element name='oauthport' type='xs:integer' minOccurs='0' />
                <xs:element name='clientid' type='xs:string' minOccurs='0' />
                <xs:element name='clientpassword' type='xs:string' minOccurs='0' />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>";

        private const string TestWCSchema = @"<xs:schema  xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns='urn:nextlabs-crm-schema'
        elementFormDefault='qualified'
        targetNamespace='urn:nextlabs-crm-schema'>

        <xs:simpleType name='hostnametype'>
         <xs:restriction base='xs:string'>
          <xs:pattern value='^((http|https):(\/\/|\\\\))?[a-z0-9A-Z\\\/\-_]+(\.{1}[a-z0-9A-Z\\\/\-_]+)+[a-z0-9A-Z\\\/\-_]?$'/>
          <xs:whiteSpace value='preserve'/>
          <xs:maxLength value='255'/>
         </xs:restriction>
        </xs:simpleType>

          <xs:element name='testwc'>
            <xs:complexType>
              <xs:sequence>
                <xs:element name='ishttps' type='xs:boolean' minOccurs='0' />
                <xs:element name='oauthserverhost' type='hostnametype' minOccurs='0' />
                <xs:element name='oauthport' type='xs:integer' minOccurs='0' />
                <xs:element name='wcusrname' type='xs:string' minOccurs='0' />
                <xs:element name='wcpwd' type='xs:string' minOccurs='0' />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>";

        private const string LogSettingSchema = @"<xs:schema xmlns:xs = 'http://www.w3.org/2001/XMLSchema' xmlns = 'urn:nextlabs-crm-schema'
            elementFormDefault = 'qualified'
            targetNamespace = 'urn:nextlabs-crm-schema' >
            <xs:simpleType name='logleveltype'>
              <xs:restriction base='xs:string'>
                <xs:enumeration value='Debug'/>
	            <xs:enumeration value='Information'/>
                <xs:enumeration value='Warning'/>
                <xs:enumeration value='Error'/>
              </xs:restriction>
            </xs:simpleType>
            <xs:element name = 'logsettings' >
                <xs:complexType >
                    <xs:sequence >
                    <xs:element name = 'currentlevel' type = 'logleveltype' minOccurs = '0' />
                    </xs:sequence >       
                </xs:complexType >    
            </xs:element >
           </xs:schema >";

        private const string IOCommandSchema = @"<xs:schema xmlns:xs = 'http://www.w3.org/2001/XMLSchema' xmlns = 'urn:nextlabs-crm-schema'
            elementFormDefault = 'qualified'
            targetNamespace = 'urn:nextlabs-crm-schema' >
            <xs:simpleType name='cmmandactiontype'>
              <xs:restriction base='xs:string'>
                <xs:enumeration value='Remove'/>
                <xs:enumeration value='PublishAll'/>
                <xs:enumeration value='EnableSPAction'/>
                <xs:enumeration value='DisableSPAction'/>
              </xs:restriction>
            </xs:simpleType>
            <xs:element name = 'iocommand' >
                <xs:complexType >
                    <xs:sequence >
                    <xs:element name = 'version' type = 'xs:string' minOccurs = '0' />
                    <xs:element name = 'action' type = 'cmmandactiontype' minOccurs = '1' />
                    </xs:sequence >       
                </xs:complexType >    
            </xs:element >
           </xs:schema >";

        private const string N1RelationshipSchema = @"<xs:schema xmlns:xs = 'http://www.w3.org/2001/XMLSchema' xmlns = 'urn:nextlabs-crm-schema'
            elementFormDefault = 'qualified'
            targetNamespace = 'urn:nextlabs-crm-schema' >
            <xs:simpleType name = 'logicnametype' >
             <xs:restriction base='xs:string'>
             <xs:pattern value = '[a-z0-9A-Z_]+' />
             <xs:maxLength value = '100' />
             </xs:restriction>
            </xs:simpleType>
              <xs:element name = 'n1relationship' >
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name = 'relatedentity' type='logicnametype' minOccurs='0' />
                    <xs:element name = 'relationships' minOccurs='0' maxOccurs='unbounded'>
                      <xs:complexType>
                        <xs:sequence>
                          <xs:element name = 'relationship' minOccurs='0' maxOccurs='unbounded'>
                            <xs:complexType>
                              <xs:sequence>
                                <xs:element name = 'primaryentity' type='logicnametype' minOccurs='0' />
                                <xs:element name = 'primaryfield' type='logicnametype' minOccurs='0' />
                                <xs:element name = 'lookupfield' type='logicnametype' minOccurs='0' />
                              </xs:sequence>
                              <xs:attribute name = 'name' type='logicnametype' />
                            </xs:complexType>
                          </xs:element>
                        </xs:sequence>
                      </xs:complexType>
                    </xs:element>
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:schema>";

        private const string NNRelationshipSchema = @"<xs:schema xmlns:xs = 'http://www.w3.org/2001/XMLSchema' xmlns = 'urn:nextlabs-crm-schema'
            elementFormDefault = 'qualified'
            targetNamespace = 'urn:nextlabs-crm-schema' >
            <xs:simpleType name='logicnametype'>
             <xs:restriction base='xs:string'>
             <xs:pattern value='[a-z0-9A-Z_]+'/>
             <xs:maxLength value='100'/>
             </xs:restriction>
            </xs:simpleType>
              <xs:element name='nnrelationship'>
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name='relatedentity' type='logicnametype' minOccurs='0' />
                    <xs:element name='relationships' minOccurs='0' maxOccurs='unbounded'>
                      <xs:complexType>
                        <xs:sequence>
                          <xs:element name='relationship' minOccurs='0' maxOccurs='unbounded'>
                            <xs:complexType>
                              <xs:sequence>
                                <xs:element name='relationshipentityname' type='logicnametype' minOccurs='0' />
                                <xs:element name='primaryentity' type='logicnametype' minOccurs='0' />
                                <xs:element name='primaryfield' type='logicnametype' minOccurs='0' />
                                <xs:element name='lookupfield' type='logicnametype' minOccurs='0' />
                              </xs:sequence>
                              <xs:attribute name='name' type='logicnametype' />
                            </xs:complexType>
                          </xs:element>
                        </xs:sequence>
                      </xs:complexType>
                    </xs:element>
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:schema>";

        private static XmlReader CreateXMLReader(string xml, XmlReaderSettings settings)
        {
            byte[] xmlBytes = System.Text.Encoding.Unicode.GetBytes(xml);
            MemoryStream stream = new MemoryStream(xmlBytes);
            XmlReader reader = null;

            if (settings != null)
            {
                reader = XmlReader.Create(stream, settings);
            }
            else
            {
                reader = XmlReader.Create(stream);
            }
            return reader;
        }

        private static void CheckXml(string xmlSchema,string xml)
        {
            // Create the XmlSchemaSet class.
            XmlSchemaSet sc = new XmlSchemaSet();

            // Add the schema to the collection.
            sc.Add("urn:nextlabs-crm-schema", CreateXMLReader(xmlSchema, null));

            // Set the validation settings.
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = sc;
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

            // Create the XmlReader object.
            XmlReader reader = CreateXMLReader(xml, settings);

            // Parse the file. 
            while (reader.Read()) ;
        }

        private static void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            throw e.Exception;
        }

        public static void CheckSecureEntityXml(string xml)
        {
            CheckXml(SecureEntitySchema, xml);
        }

        public static void CheckLogSettingsXml(string xml)
        {
            CheckXml(LogSettingSchema, xml);
        }

        public static void CheckN1RelationshipXml(string xml)
        {
            CheckXml(N1RelationshipSchema, xml);
        }

        public static void CheckNNRelationshipXml(string xml)
        {
            CheckXml(NNRelationshipSchema, xml);
        }

        public static void CheckGeneralSettingXml(string xml)
        {
            CheckXml(GeneralSettingSchema, xml);
        }

        public static void CheckTestPCXml(string xml)
        {
            CheckXml(TestPCSchema, xml);
        }

        public static void CheckTestWCXml(string xml)
        {
            CheckXml(TestWCSchema, xml);
        }

        public static void CheckCommandXml(string xml)
        {
            CheckXml(IOCommandSchema, xml);
        }
    }
}
