using System;
using System.Security.Cryptography;
using System.Web;

public class RSAKeys
{
    public string publicKeyXml;
    public string privateKeyXml;
}

public class RSA
{
    public static RSAKeys generateKeys()
    {
        int N_BITS = 2048;
        
        //lets take a new CryptoServiceProvider (RSA) with a new N_BIT bit rsa key pair
        var RSA = new RSACryptoServiceProvider(N_BITS);
        
        RSAKeys return_obj = new RSAKeys();
        
        return_obj.publicKeyXml = RSA.ToXmlString(false); //getting public key parameters XML 
        return_obj.privateKeyXml = RSA.ToXmlString(true); //getting private key parameters XMS
        
        return return_obj;
    }
    
    
    
    
    public static string encrypt(string plainTextData, string publicKeyXml)
    {
        //we have a public key ... let's get a new RSA and load that key
        var RSA = new RSACryptoServiceProvider();
        RSA.FromXmlString(publicKeyXml);
        
        //for encryption, always handle bytes...
        var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(plainTextData);
        
        //apply OAE Padding and encrypt our data.
        //OAE Padding is only available on Microsoft Windows XP or later. 
        var bytesCypherText = RSA.Encrypt(bytesPlainTextData, false);
        
        //we might want a string representation of our cypher text... base64 will do
        var cypherText = Convert.ToBase64String(bytesCypherText);
        
        return cypherText;
    }
    
    
    
    
    public static string decrypt(string cypherText, string privateKeyXml)
    {
        //first, get our bytes back from the base64 string ...
        var bytesCypherText = Convert.FromBase64String(cypherText);
        
        //we want to decrypt, therefore we need a csp and load our private key
        var RSA = new RSACryptoServiceProvider();
        RSA.FromXmlString(privateKeyXml);
        
        //decrypt and strip OAE padding
        var bytesPlainTextData = RSA.Decrypt(bytesCypherText, false);
        
        //get our original plainText back...
        var plainTextData = System.Text.Encoding.Unicode.GetString(bytesPlainTextData);
        
        return plainTextData;
    }








}


