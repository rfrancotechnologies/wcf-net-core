using System;
using System.ServiceModel.Channels;
using System.Xml;

namespace Com.RFranco.Wcf.WSSecurity
{
    /// <summary>
    /// WS Security constants
    /// </summary>
    public static class WSSecurityConstants
    {
        public const string NAMESPACE_WSS_SECURITY_SECEXT = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
        public const string NAMESPACE_WSS_SECURITY_UTILITY = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
        public const string ENCODING_TYPE_BASE64 = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
    }

    /// <summary>
    /// Security token interface
    /// </summary>
    public interface SecurityToken
    {
        /// <summary>
        /// Specify how it is attached to message
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="messageVersion"></param>
        void WriteSecurityTokenContent(XmlDictionaryWriter writer, MessageVersion messageVersion);
    }
    
    /// <summary>
    /// https://docs.oasis-open.org/wss/v1.1/wss-v1.1-spec-errata-os-SOAPMessageSecurity.htm
    /// </summary>
    public class SecurityHeader : MessageHeader
    {
        SecurityToken[] SecurityTokens;
        /// <summary>
        /// SecurityHeader constructor
        /// </summary>
        /// <param name="securityTokens">List of security tokens to be attached to the message</param>
        public SecurityHeader(params SecurityToken[] securityTokens)
        {
            if (securityTokens == null) throw new ArgumentNullException(nameof(securityTokens));

            SecurityTokens = securityTokens;
        }

        public override string Name { get; } = "Security";

        public override string Namespace { get; } = WSSecurityConstants.NAMESPACE_WSS_SECURITY_SECEXT;

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            Array.ForEach(SecurityTokens, (securityToken) => securityToken.WriteSecurityTokenContent(writer, messageVersion));
        }
    }
}