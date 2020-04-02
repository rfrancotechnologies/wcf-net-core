# wcf-net-core

WS Security / Diagnostic libs in .NetCore (WCF support in dotnet core)

An example of use:

```csharp
var client = new HelloWorldWsClient();

//  To audit messages
client.Endpoint.EndpointBehaviors.Add(new Log4NetAuditBehavior());

using (new OperationContextScope(client.InnerChannel))
{
    OperationContext.Current.OutgoingMessageHeaders.Add(
        new SecurityHeader(
            new UsernameSecurityToken("user", "pass"),  // To attach <UsernameToken>. By default in PasswordText type
            new TimestampSecurityToken()));             // To attach  <Timestamp Id="...">. By default expires in 60 sec.
   ...

    var response = client.HelloAsync().Result;

}

```

An example of message / request:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
   <s:Header>
      <Action xmlns="http://schemas.microsoft.com/ws/2005/05/addressing/none" s:mustUnderstand="1" />
      <Security xmlns="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd">
         <UsernameToken xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
            <Username>user</Username>
            <Password Type="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText">pass</Password>
            <Nonce EncodingType="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary">hA2Rep129GdTQ9+12rjcuHmjF10qtZ4WCo1DJfghh6cM=</Nonce>
            <Created xmlns="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd">2020-04-02T10:47:09Z</Created>
         </UsernameToken>
         <Timestamp xmlns="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
            <Created>2020-04-02T10:47:09Z</Created>
            <Expires>2020-04-02T10:48:09Z</Expires>
         </Timestamp>
      </Security>
   </s:Header>
   <s:Body xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">...</s:Body>
</s:Envelope>
```
