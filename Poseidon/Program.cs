﻿using System;
using System.IO;

namespace Poseidon
{
    public class Program
    {
        private static string KEY;
        private static string SIGNATURE;
        private static Kraken kraken;

        private static void Main()
        {
            Console.Title = "Poseidon";
            CheckKeyFile();
            LoadKeys();

            kraken = new Kraken(KEY, SIGNATURE);
            Console.WriteLine(kraken.GetServerTime().result.rfc1123);

            var balances = kraken.GetAccountBalance().balances;
            Console.WriteLine(balances.ToStringTable(new[] {"Currency", "Amount"}, a => a.Key, a => a.Value));

            Console.ReadLine();
        }

        public static void CheckKeyFile()
        {
            if (!File.Exists("API.txt"))
                try
                {
                    using (var sw = File.CreateText("API.txt"))
                    {
                        sw.WriteLine("KEY=");
                        sw.WriteLine("SIGNATURE=");
                    }
                    Console.WriteLine("Please enter your credentials in API.txt");

                    ExitProgram();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);

                    ExitProgram();
                }
        }

        public static void LoadKeys()
        {
            using (var sr = new StreamReader("API.txt"))
            {
                var line = sr.ReadLine();
                if (line != null && line.Substring(0, 4) == "KEY=")
                    KEY = line.Substring(4);
                line = sr.ReadLine();
                if (line != null && line.Substring(0, 10) == "SIGNATURE=")
                    SIGNATURE = line.Substring(10);
            }
        }

        public static void ExitProgram()
        {
            Console.WriteLine("Press enter to close application");
            Console.ReadLine();
            Environment.Exit(-1);
        }

        public static void WriteToFile(string text)
        {
            using (var sw = File.CreateText("temp.txt"))
            {
                sw.Write(text);
            }
            Console.WriteLine("File written");
        }

        public static void WriteToFile(string fileText, string text)
        {
            using (var sw = File.CreateText(fileText + "_" + DateTime.Now.ToFileTime() + ".txt"))
            {
                sw.Write(text);
            }
            Console.WriteLine("File written");
        }

        public static string UnixTimeToString(decimal unix)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds((double) unix).ToLocalTime();
            return dtDateTime.ToString();
        }

        public static Kraken GetKraken()
        {
            return kraken;
        }
    }
}