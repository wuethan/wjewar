using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WjeWar
{
    class FileManage
    {
        //安装模型
        public static bool SetupModel(byte[] modelByte,string path,string modelName)
        {
            if (!File.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                    FileCreate(modelByte, path, modelName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                    return false;
                }
            }
            return true;
        }

        //安装文件
        public static void FileCreate(byte[] fileByte, string path, string fileName) 
        {
            try
            {
                FileStream fs = new FileStream(path + "\\" + fileName, FileMode.Create, FileAccess.ReadWrite);
                fs.Write(fileByte, 0, fileByte.Length);
                fs.Flush();
                fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        //删除文件
        public static bool deleteFile(string filePath) 
        {
            if (File.Exists(filePath)) 
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }

                if (!File.Exists(filePath))
                {
                    return true;
                }
            }
            return false;
        }

        //比较文件大小
        public static void compareFileSize(string filePath) 
        {
            int length = (int)new FileInfo(filePath).Length;
            System.Windows.Forms.MessageBox.Show(length.ToString());
        } 
    }
}
