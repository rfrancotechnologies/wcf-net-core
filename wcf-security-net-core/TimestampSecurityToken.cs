using System;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Serialization;

namespace Com.RFranco.Wcf.WSSecurity
{
    /// <summary>
    /// Security Timestamps token implementation
    /// https://docs.oasis-open.org/wss/v1.1/wss-v1.1-spec-errata-os-SOAPMessageSecurity.htm
    /// </summary>
    public class TimestampSecurityToken : SecurityToken
    {
        private readonly string _created;
        private readonly string _expires;


        /// <summary>
        /// Timstamp security token constructor
        /// </summary>
        /// <param name="expiresInMilliseconds"> Specify when expires.By default 60 seconds.</param>
        public TimestampSecurityToken(int expiresInMilliseconds = 60000)
        {
            var now = DateTime.UtcNow;
            
            //  https://www.w3.org/TR/xmlschema-2/#dateTime
            _created = now.ToString("yyyy-MM-ddTHH:mm:ssZ");
            _expires = now.AddMilliseconds(expiresInMilliseconds).ToString("yyyy-MM-ddTHH:mm:ssZ"); 
        }

        [XmlRoot(ElementName = "Timestamp", Namespace = WSSecurityConstants.NAMESPACE_WSS_SECURITY_UTILITY)]
        public class Timestamp
        {
            [XmlElement(ElementName = "Created", Namespace = WSSecurityConstants.NAMESPACE_WSS_SECURITY_UTILITY)]
            public string Created { get; set; }

            [XmlElement(ElementName = "Expires", Namespace = WSSecurityConstants.NAMESPACE_WSS_SECURITY_UTILITY)]
            public string Expires { get; set; }

            [XmlAttribute(AttributeName = "Id", Namespace = WSSecurityConstants.NAMESPACE_WSS_SECURITY_UTILITY)]
            public string Id { get; set; }
        }

        public void WriteSecurityTokenContent(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            new XmlSerializer(typeof(Timestamp)).Serialize(writer,
                new Timestamp
                {
                    Created = _created,
                    Expires = _expires
                });
        }
    }
}