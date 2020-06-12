using KoyashiroKohaku.VrcMetaTool;
using System;
using System.IO;

namespace KoyashiroKohaku.VrcMetaTool.Reader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("VrcMetaTool.NET.Reader.exeに画像ファイルをドラッグアンドドロップしてください。");
                Console.ReadLine();
            }

            var path = args[0];

            if (!File.Exists(path))
            {
                Console.WriteLine("ファイルを読み込めませんでした。");
                Console.ReadLine();
            }

            VrcMetaData vrcMetaData;

            try
            {
                vrcMetaData = VrcMetaDataReader.Read(File.ReadAllBytes(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            if (vrcMetaData.Date != null)
            {
                Console.WriteLine($"Date: {vrcMetaData.Date}");
            }

            if (vrcMetaData.Photographer != null)
            {
                Console.WriteLine($"Photo by: {vrcMetaData.Photographer}");
            }

            if (vrcMetaData.World != null)
            {
                Console.WriteLine($"World: {vrcMetaData.World}");
            }

            foreach (var user in vrcMetaData.Users)
            {
                if (user.HasTwitterScreenName)
                {
                    Console.WriteLine($"User: {user.UserName} : {user.TwitterScreenName}");
                }
                else
                {
                    Console.WriteLine($"User: {user.UserName}");
                }
            }

            Console.ReadLine();
        }
    }
}
