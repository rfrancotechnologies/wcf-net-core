using System;
using System.Security.Cryptography;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Com.RFranco.Wcf.WSSecurity
{
    /// <summary>
    /// Username security token implementation
    /// https://www.oasis-open.org/committees/download.php/13392/wss-v1.1-spec-pr-UsernameTokenProfile-01.htm
    /// </summary>
    public class UsernameSecurityToken : SecurityToken
    {

        public const string NAMESPACE_PASSWORDDIGEST = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest";
        public const string NAMESPACE_PASSWORDTEXT = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";
        public enum PasswordType
        {
            PasswordText, PasswordDigest
        }

        private readonly string _username;
        private readonly string _nonce;
        private readonly string _created;
        private readonly Password _password;

        /// <summary>
        /// Username security token constructor
        /// </summary>
        /// <param name="username">User name</param>
        /// <param name="password">Password</param>
        /// <param name="passwordType">Password type: PasswordText, PasswordDigest</param>
        public UsernameSecurityToken(string username, string password, PasswordType passwordType = PasswordType.PasswordText)
        {
            _username = username;
            _created = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");    //  https://www.w3.org/TR/xmlschema-2/#dateTime
            _nonce = CalculateNonce();
            _password = CreatePassword(password, _nonce, _created, passwordType);
        }

        private Password CreatePassword(string password, string nonce, string created, PasswordType passwordType)
        {
            return (passwordType == PasswordType.PasswordDigest) ?
            new Password
            {
                Text = CreateHashedPassword(nonce, created, password),
                Type = NAMESPACE_PASSWORDDIGEST
            } :
            new Password
            {
                Text = password,
                Type = NAMESPACE_PASSWORDTEXT
            };
        }

        [XmlRoot(ElementName = "Password", Namespace = WSSecurityConstants.NAMESPACE_WSS_SECURITY_SECEXT)]
        public class Password
        {
            [XmlAttribute(AttributeName = "Type")] public string Type { get; set; }
            [XmlText] public string Text { get; set; }
        }

        [XmlRoot(ElementName = "Nonce", Namespace = WSSecurityConstants.NAMESPACE_WSS_SECURITY_SECEXT)]
        public class Nonce
        {
            [XmlAttribute(AttributeName = "EncodingType")]
            public string EncodingType { get; set; } = WSSecurityConstants.ENCODING_TYPE_BASE64;

            [XmlText] public string Text { get; set; }
        }

        [XmlRoot(ElementName = "UsernameToken", Namespace = WSSecurityConstants.NAMESPACE_WSS_SECURITY_SECEXT)]
        public class UsernameToken
        {
            [XmlElement(ElementName = "Username", Namespace = WSSecurityConstants.NAMESPACE_WSS_SECURITY_SECEXT)]
            public string Username { get; set; }

            [XmlElement(ElementName = "Password", Namespace = WSSecurityConstants.NAMESPACE_WSS_SECURITY_SECEXT)]
            public Password Password { get; set; }

            [XmlElement(ElementName = "Nonce", Namespace = WSSecurityConstants.NAMESPACE_WSS_SECURITY_SECEXT)]
            public Nonce Nonce { get; set; }

            [XmlElement(ElementName = "Created", Namespace = WSSecurityConstants.NAMESPACE_WSS_SECURITY_UTILITY)]
            public string Created { get; set; }

            [XmlAttribute(AttributeName = "Id", Namespace = WSSecurityConstants.NAMESPACE_WSS_SECURITY_UTILITY)]
            public string Id { get; set; }
        }

        public void WriteSecurityTokenContent(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            new XmlSerializer(typeof(UsernameToken)).Serialize(writer,
                new UsernameToken
                {
                    Username = _username,
                    Password = _password,
                    Nonce = new Nonce { Text = _nonce },
                    Created = _created
                });
        }

        private static string CalculateNonce()
        {
            var byteArray = new byte[32];
            using (var rnd = RandomNumberGenerator.Create())
            {
                rnd.GetBytes(byteArray);
            }
            return Convert.ToBase64String(byteArray);
        }

        private static string CreateHashedPassword(string nonceStr, string created, string password)
        {
            var nonce = Convert.FromBase64String(nonceStr);
            var createdBytes = Encoding.UTF8.GetBytes(created);
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var combined = new byte[createdBytes.Length + nonce.Length + passwordBytes.Length];
            Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
            Buffer.BlockCopy(createdBytes, 0, combined, nonce.Length, createdBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, combined, nonce.Length + createdBytes.Length, passwordBytes.Length);

            return Convert.ToBase64String(SHA1.Create().ComputeHash(combined));
        }
    }
}