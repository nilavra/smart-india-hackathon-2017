using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using WebMatrix.Data;
using System.Linq;

public class Utilities
{
    /************************************** 
        Utilities.cs private functions:
            - xor
            - extractBit
            - extractBits
            - concatBits
            - setBit
    *************************************/

    public static FileInfo GetNewestFile(DirectoryInfo directory) {
       return directory.GetFiles()
           .Union(directory.GetDirectories().Select(d => GetNewestFile(d)))
           .OrderByDescending(f => (f == null ? DateTime.MinValue : f.LastWriteTime))
           .FirstOrDefault();                      
    }

    public static byte[] xor(byte[] a, byte[] b) 
    {
        byte[] outa = new byte[a.Length];
        for (int i = 0; i < a.Length; i++) 
        {
            outa[i] = (byte) (a[i] ^ b[i]);
        }
        
        return outa;
    }
    
    
    
    public static void setBit(byte[] data, int pos, int val) 
    {
        int posByte = pos / 8;
        int posBit = pos % 8;
        byte tmpB = data[posByte];
        tmpB = (byte) (((0xFF7F >> posBit) & tmpB) & 0x00FF);
        byte newByte = (byte) ((val << (8 - (posBit + 1))) | tmpB);
        data[posByte] = newByte;
    }
    
    
    
    public static int extractBit(byte[] data, int pos) 
    {
        int posByte = pos / 8;
        int posBit = pos % 8;
        byte tmpB = data[posByte];
        int bit = tmpB >> (8 - (posBit + 1)) & 0x0001;
        return bit;
    }
    
    
    
    
    public static byte[] extractBits(byte[] input, int pos, int n)
    {
        int numOfBytes = (n - 1) / 8 + 1;
        byte[] outa = new byte[numOfBytes];
        
        for (int i = 0; i < n; i++) 
        {
            int val = Utilities.extractBit(input, pos + i);
            Utilities.setBit(outa, i, val);
        }
        
        return outa;
    }
    
    
    
    public static byte[] concatBits(byte[] a, int aLen, byte[] b, int bLen) 
    {
        int numOfBytes = (aLen + bLen - 1) / 8 + 1;
        byte[] outa = new byte[numOfBytes];
        int j = 0;
        
        for (int i = 0; i < aLen; i++) 
        {
            int val = Utilities.extractBit(a, i);
            Utilities.setBit(outa, j, val);
            j++;
        }
        
        for (int i = 0; i < bLen; i++) 
        {
            int val = Utilities.extractBit(b, i);
            Utilities.setBit(outa, j, val);
            j++;
        }
        
        return outa;
    }


    public static void Upsert(string filename, string filetype)
    {
        var db = Database.Open("kryptokraft_db");
        var sql = " select count(*) NUM_FILE from FILES where filename = @0 ";
        int num_file = db.QuerySingle(sql, filename).NUM_FILE;
        if(num_file < 1)
        {
            sql = "insert into FILES(FILENAME, FILETYPE, CRT_DT) values(@0, @1, getdate())";
            db.Execute(sql, filename, filetype);
        }
        else
        {
            sql = "update FILES set CRT_DT = GETDATE() where FILENAME = @0";
            db.Execute(sql, filename);
        }
    }
    
    
    
    
    
    
/*****************    
    public static string GetUniqueKey(int maxSize)
    {
        char[] chars = 
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        
        byte[] data = new byte[1];
        
        using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
        {
            crypto.GetNonZeroBytes(data);
            data = new byte[maxSize];
            crypto.GetNonZeroBytes(data);
        }

        StringBuilder result = new StringBuilder(maxSize);
        
        foreach (byte b in data)
        {
            result.Append(chars[b % (chars.Length)]);
        }
        
        return result.ToString();
    }
********************/
}
