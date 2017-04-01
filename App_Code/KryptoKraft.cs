using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Text;


public class KryptoKraft
{
    //encrypted filename: orig_filename.encrypted
    //returns RSA encrypted key for sharing
    public static string encrypt(string inp_filepath, string rsa_public_key_xml) 
    {
        //inp_filepath = System.Web.Hosting.HostingEnvironment.MapPath(inp_filepath);
        //converting to absolute path
        string filename = Path.GetFileName(inp_filepath); 

        string enc_filepath = "~/f2_encrypted/encrypted_"+ filename;
        //converting to absolute path
        enc_filepath = System.Web.Hosting.HostingEnvironment.MapPath(enc_filepath);
        
        string af_key = System.Guid.NewGuid().ToString();
        byte[] af_key_bytes = Encoding.ASCII.GetBytes(af_key);
        
        byte[] inp_file = File.ReadAllBytes(inp_filepath);
        
        byte[] enc_file = AESFeistel.encrypt(inp_file, af_key_bytes);
        
        HttpContext.Current.Trace.Warn("Output file = " + enc_filepath);

        File.WriteAllBytes(enc_filepath, enc_file);
        
        string enc_af_key = RSA.encrypt(af_key, rsa_public_key_xml);
        
        return enc_af_key;
    }
    
    
    
    
    //outputs decrypted filepath
    public static void decrypt(string enc_filepath, string enc_af_key, string rsa_private_key_xml) 
    {
        //converting to absolute path
        //enc_filepath = System.Web.Hosting.HostingEnvironment.MapPath(enc_filepath); 
        
        string filenameNoExt = Path.GetFileName(enc_filepath); //removes .enc
        string dec_filepath = "~/f4_decrypted/" + filenameNoExt;
        //converting to absolute path
        dec_filepath = System.Web.Hosting.HostingEnvironment.MapPath(dec_filepath);
        
        string af_key = RSA.decrypt(enc_af_key, rsa_private_key_xml);
        byte[] af_key_bytes = Encoding.ASCII.GetBytes(af_key);
        
        byte[] enc_file = File.ReadAllBytes(enc_filepath);
        
        byte[] dec_file = AESFeistel.decrypt(enc_file, af_key_bytes);
        
        File.WriteAllBytes(dec_filepath, dec_file);
        
    }
    



/*************************************    
    public static void Main(string[] args)
    {
        //string textClar = "This is AES-Feistel-AES Sandwich :)";
		//AES
		string k = GetUniqueKey(16);
		Console.WriteLine("Type the path for encryption:");
        string path = Console.ReadLine();
		//Console.WriteLine("length of byte array k:"+Encoding.ASCII.GetBytes(k).Length);
		byte[] array = File.ReadAllBytes(@path);
		key_array = Encoding.ASCII.GetBytes(k);
			
		string extension = Path.GetExtension(path);
        string filenameNoExtension =
        Path.GetFileNameWithoutExtension(path);
        string dir = Path.GetDirectoryName(path);
        string pathenc=dir+"\\"+filenameNoExtension+ "_encrypted"+extension;
        string pathdec=dir+"\\"+filenameNoExtension+"_decrypted"+extension;
			
		
            
        byte[] enc = encrypt(array,Encoding.ASCII.GetBytes(k));
		//Console.WriteLine("Encrypted text:"+Encoding.ASCII.GetString(enc));
		File.WriteAllBytes(@pathenc, enc);

		byte[] dec = decrypt(enc,Encoding.ASCII.GetBytes(k));
		//Console.WriteLine("Decrypted text:"+Encoding.ASCII.GetString(dec));
		File.WriteAllBytes(@pathdec, dec);
			
		Console.Write("Press any key to quit . . . ");
		Console.ReadKey(true);
    }
*******************/
}
