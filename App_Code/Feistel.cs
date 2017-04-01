using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.IO;
using System.Security.Cryptography;

public class Feistel
{
    
    public static byte[] encrypt(byte[] input, byte[] K1)
    {
        //HttpContext.Current.Trace.Warn("Feistel.encrypt K1 = " + System.Text.Encoding.UTF8.GetString(K1));
        byte[] key_des = Feistel.form_des_key(K1);
        byte[][] K = Feistel.generateSubKeys(key_des);
        byte[] enc = Feistel.encrypt64Bloc(input, K, false);
        return enc;
    }
    
    
    
    public static byte[] decrypt(byte[] input, byte[] K1)
    {
        byte[] key_des = Feistel.form_des_key(K1);
        byte[][] K = Feistel.generateSubKeys(key_des);
        byte[] dec = Feistel.encrypt64Bloc(input, K, true);
        return dec;
    }
    
    
    
    
    /************************************** 
        Feistel.cs private functions:
            - encrypt64Bloc
            - extractBit
            - extractBits
            - f_func
            - form_des_key
            - generateSubKeys
            - permutFunc
            - rotLeft
            - s_func
            - separateBytes
    *************************************/
    
    
    
    
    private static byte[] rotLeft(byte[] input, int len, int pas) 
    {
        int nrBytes = (len - 1) / 8 + 1;
        byte[] outa = new byte[nrBytes];
        
        for (int i = 0; i < len; i++) 
        {
            int val = Utilities.extractBit(input, (i + pas) % len);
            Utilities.setBit(outa, i, val);
        }
        
        return outa;
    }
    
    
    
    
    
    private static byte[] permutFunc(byte[] input, int[] table) 
    {
        int nrBytes = (table.Length - 1) / 8 + 1;
        byte[] outa = new byte[nrBytes];
        
        for (int i = 0; i < table.Length; i++) 
        {
            int val = Utilities.extractBit(input, table[i] - 1);
            Utilities.setBit(outa, i, val);
        }
        
        return outa;
    }
    
    
    
    
    private static byte[] f_func(byte[] R, byte[] K) 
    {
        byte[] tmp;
        tmp = Feistel.permutFunc(R, expandTbl);
        tmp = Utilities.xor(tmp, K);
        tmp = Feistel.s_func(tmp);
        tmp = Feistel.permutFunc(tmp, P);
        return tmp;
    }
    
    
    
    
    private static byte[] s_func(byte[] ina) 
    {
        ina = Feistel.separateBytes(ina, 6);
        byte[] outa = new byte[ina.Length / 2];
        int halfByte = 0;
        
        unchecked
        {
            for (int b = 0; b < ina.Length; b++) 
            {
                byte valByte = ina[b];
                int r = 2 * (valByte >> 7 & 0x0001) + (valByte >> 2 & 0x0001);
                int c = valByte >> 3 & 0x000F;
                int val = sboxes[b,r,c];
                if (b % 2 == 0)
                    halfByte = val;
                else
                    outa[b / 2] = (byte) (16 * halfByte + val);
            }
        }
        
        return outa;
    }
    
    
    
    
    private static byte[] separateBytes(byte[] ina, int len) 
    {
        int numOfBytes = (8 * ina.Length - 1) / len + 1; 
        byte[] outa = new byte[numOfBytes];
        
        for (int i = 0; i < numOfBytes; i++) 
        {
            for (int j = 0; j < len; j++) 
            {
                int val = Utilities.extractBit(ina, len * i + j);
                Utilities.setBit(outa, 8 * i + j, val);
            }
        }
        
        return outa;
    }
    
    
    
    
    
    
    
    
    
    private static byte[][] generateSubKeys(byte[] key) 
    {
        byte[][] tmp = new byte[16][];
        byte[] tmpK = Feistel.permutFunc(key, PC1);
        
        byte[] C = Utilities.extractBits(tmpK, 0, PC1.Length/2);
        byte[] D = Utilities.extractBits(tmpK, PC1.Length/2, PC1.Length/2);
        
        for (int i = 0; i < 16; i++) 
        {
            C = Feistel.rotLeft(C, 28, keyShift[i]);
            D = Feistel.rotLeft(D, 28, keyShift[i]);
            
            byte[] cd = Utilities.concatBits(C, 28, D, 28);
            
            tmp[i] = Feistel.permutFunc(cd, PC2);
        }
        
        return tmp;
    }
    
    
    
    
    public static byte[] form_des_key(byte[] input)
    {
        byte[] key_des = new byte[8];
        int i = 0, j = 0;
        
        for(i=0;i<16;i++)
        {
            if(i%2==0)
            {
                key_des[j] = input[i];
                j++;
            }
        }
        return key_des;
    }
    
    
    
    
    private static byte[] encrypt64Bloc(byte[] bloc,byte[][] subkeys, bool isDecrypt) 
    {
        byte[] R = new byte[4];
        byte[] L = new byte[4];
        
        //tmp = Feistel.permutFunc(bloc, IP);
        
        L = Utilities.extractBits(bloc, 0, 32);
        R = Utilities.extractBits(bloc, 32, 32);
        
        for (int i = 0; i < 4; i++) 
        {
            byte[] tmpR = R;
            if(isDecrypt)
                R = Feistel.f_func(R, subkeys[3-i]);
            else
                R = Feistel.f_func(R,subkeys[i]);
            
            R = Utilities.xor(L, R);
            L = tmpR;
        }
        
        bloc = Utilities.concatBits(R,32, L, 32);
        // tmp = Feistel.permutFunc(tmp, invIP);
        return bloc;
    }
    
    
    
    
    
    // Permutation P (in f(Feistel) function)
    private static int[] P = new int[]{
        16, 7, 20, 21, 29, 12, 28, 17, 1, 15, 23, 26, 5, 18, 31, 10, 
        2, 8, 24, 14, 32, 27, 3, 9, 19, 13, 30, 6, 22, 11, 4, 25
    };
    
    // initial key permutation 64 => 56 biti
    private static int[] PC1 = new int[]{ 
        57, 49, 41, 33, 25, 17, 9, 1, 58, 50, 42, 34,
        26, 18, 10, 2, 59, 51, 43, 35, 27, 19, 11, 3, 60, 52, 44, 36, 63,
        55, 47, 39, 31, 23, 15, 7, 62, 54, 46, 38, 30, 22, 14, 6, 61, 53,
        45, 37, 29, 21, 13, 5, 28, 20, 12, 4 
    };
    
    // key permutation at round i 56 => 48
    private static int[] PC2 =new int[] { 
        14, 17, 11, 24, 1, 5, 3, 28, 15, 6, 21, 10, 23, 19, 12, 4, 
        26, 8, 16, 7, 27, 20, 13, 2, 41, 52, 31, 37, 47, 55, 30, 40, 
        51, 45, 33, 48, 44, 49, 39, 56, 34, 53, 46, 42, 50, 36, 29, 32
    };
    
    // key shift for each round
    private static int[] keyShift =new int[] { 
        1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1 
    };
    
    // expansion permutation from function f
    private static int[] expandTbl =new int[] { 
        32, 1, 2, 3, 4, 5, 4, 5, 6, 7, 8, 9, 8,
        9, 10, 11, 12, 13, 12, 13, 14, 15, 16, 17, 16, 17, 18, 19, 20, 21,
        20, 21, 22, 23, 24, 25, 24, 25, 26, 27, 28, 29, 28, 29, 30, 31, 32, 1 
    };
    
    // substitution boxes
    private static int[,,] sboxes =new int[,,] {
        {
            { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 },
            { 0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8 },
            { 4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0 },
            { 15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13 } 
        },
        {
            { 15, 1, 8, 14, 6, 11, 3, 2, 9, 7, 2, 13, 12, 0, 5, 10 },
            { 3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5 },
            { 0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15 },
            { 13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9 } 
        },
        {
            { 10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8 },
            { 13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1 },
            { 13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7 },
            { 1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12 } 
        },
        {
            { 7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15 },
            { 13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9 },
            { 10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4 },
            { 3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14 } 
        },
        {
            { 2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9 },
            { 14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6 },
            { 4, 2, 1, 11, 10, 13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14 },
            { 11, 8, 12, 7, 1, 14, 2, 12, 6, 15, 0, 9, 10, 4, 5, 3 } 
        },
        {
            { 12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11 },
            { 10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8 },
            { 9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6 },
            { 4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13 }
        },
        {
            { 4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1 },
            { 13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6 },
            { 1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0, 5, 9, 2 },
            { 6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12 }
        },
        {
            { 13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7 },
            { 1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2 },
            { 7, 11, 4, 1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8 },
            { 2, 1, 14, 7, 4, 10, 18, 13, 15, 12, 9, 0, 3, 5, 6, 11 }
        }
    };

    // holds subkeys(3 because we'll implement triple DES also
    //private static byte[][] K;
    //private static byte[] key_des;
}
