using System;
using System.Collections.Generic;
using Ionic.Zip;
using System.IO;
using System.Web;
using System.Data.SqlServerCe;
using System.Data;
using WebMatrix.Data;

/// <summary>
/// Summary description for Zipper
/// </summary>
public class Zipper
{
    public Zipper()
    {
        //
        // TODO: Add constructor logic here
        //
    }
    public static void createZip(string[] files, string saveToFileName)
    {
        
        // ZIP the uploaded contents
        ZipFile zip = new ZipFile();
        string x = System.Guid.NewGuid().ToString();
        foreach(string file in files)
        {
            zip.AddFile(file, x +"encrypted");
        }
        zip.Save(saveToFileName);
    }
    public static void unzip(string zipToUnpack, string unpackDirectory, string s2)
    {
        var db = Database.Open("kryptokraft_db");
        var sql="";
        var fileSavePath1="";
        var fileSavePath3="";
        string s3="";
        string x="";
        using (var zip1 = ZipFile.Read(zipToUnpack))
        {
            zip1.ExtractAll(unpackDirectory, ExtractExistingFileAction.OverwriteSilently);
            DirectoryInfo d = new DirectoryInfo(unpackDirectory);
            foreach(DirectoryInfo d1 in d.GetDirectories())
            {
                x = d1.Name;
            }
        }
        unpackDirectory += x +"/";
        DirectoryInfo d2 = new DirectoryInfo(unpackDirectory);
        foreach(FileInfo file in d2.GetFiles())
        {
            if(file.Name.Contains("af_key") == false)
            {
                fileSavePath1 = System.Web.Hosting.HostingEnvironment.MapPath("~/unzip/" + x +"/" + file.Name);
            }
            else
            {
                fileSavePath3 = System.Web.Hosting.HostingEnvironment.MapPath("~/unzip/" + x +"/" + file.Name);
            }
        }
        s3=File.ReadAllText(fileSavePath3);
        KryptoKraft.decrypt(fileSavePath1, s3, s2);

        FileInfo file3 = new FileInfo(fileSavePath1);
        sql = "insert into decrypt(FILENAME, CRT_DT) values(@0, @1)";
        db.Execute(sql, file3.FullName, Convert.ToDateTime(file3.CreationTime));
        
        FileInfo file4 = new FileInfo(fileSavePath3);
        sql = "insert into decrypt(FILENAME, CRT_DT) values(@0, @1)";
        db.Execute(sql, file4.FullName, Convert.ToDateTime(file4.CreationTime));

    }
}
