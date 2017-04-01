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

    public static string unzip(string zipToUnpack, string privateKeyXml)
    {
        var db = Database.Open("kryptokraft_db");
        var sql="";
        var enc_af_key_path=""; //enc_af_key
        var encrypted_file_path=""; //encrypted_file
        string unpackDirectory = System.Web.Hosting.HostingEnvironment.MapPath("~/unzip/" + Path.GetFileNameWithoutExtension(zipToUnpack));
        string enc_af_key = "";
        string dirInsideZip = "";

        bool exists = System.IO.Directory.Exists(unpackDirectory);

        if(exists)
        {
            System.IO.Directory.Delete(unpackDirectory, true);
        }
            
        System.IO.Directory.CreateDirectory(unpackDirectory);

        db.Execute("insert into FILES(FILENAME, FILETYPE, CRT_DT) values(@0, 'D', getdate())",unpackDirectory);

        using (var zip1 = ZipFile.Read(System.Web.Hosting.HostingEnvironment.MapPath(zipToUnpack)))
        {
            zip1.ExtractAll(unpackDirectory, ExtractExistingFileAction.OverwriteSilently);
            DirectoryInfo d = new DirectoryInfo(unpackDirectory);
            foreach(DirectoryInfo d1 in d.GetDirectories())
            {
                dirInsideZip = d1.Name;
            }
        }
        
        unpackDirectory = unpackDirectory + "/" + dirInsideZip + "/";
        
        DirectoryInfo d2 = new DirectoryInfo(unpackDirectory);
        
        foreach(FileInfo file in d2.GetFiles())
        {
            if(file.Name.Contains("af_key"))
            {
                enc_af_key_path = unpackDirectory + file.Name;
            }
            else
            {
                encrypted_file_path = unpackDirectory + file.Name;
            }
        }
        
        enc_af_key = File.ReadAllText(enc_af_key_path);
        string decrypted_filepath = KryptoKraft.decrypt(encrypted_file_path, enc_af_key, privateKeyXml);

        FileInfo file3 = new FileInfo(enc_af_key_path);
        
        db.Execute("insert into FILES(FILENAME, FILETYPE, CRT_DT) values(@0, 'D', getdate())",
            System.Web.Hosting.HostingEnvironment.MapPath("~/unzip/" + dirInsideZip +"/")
        );
        
        
        db.Execute("insert into FILES(FILENAME, FILETYPE, CRT_DT) values(@0, 'D', getdate())",
            System.Web.Hosting.HostingEnvironment.MapPath(decrypted_filepath)
        );

        return decrypted_filepath;
    }
}
