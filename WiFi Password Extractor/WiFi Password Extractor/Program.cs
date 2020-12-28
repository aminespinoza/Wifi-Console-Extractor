using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace WiFi_Password_Extractor
{
    class Program
    {
        static int count_names = 0;
        static int count = 0;
        static void Main(string[] args)
        {
            string wifiCollection = GetWifiList();
            ParseLines(wifiCollection);
        }

        private static string GetWifiList()
        {
            Process processWifi = new Process();
            processWifi.StartInfo.FileName = "netsh";
            processWifi.StartInfo.Arguments = "wlan show profile";
            processWifi.StartInfo.RedirectStandardOutput = true;
            processWifi.Start();

            string output = processWifi.StandardOutput.ReadToEnd();
            processWifi.WaitForExit();
            return output;
        }

        private static void ParseLines(string input)
        {
            using (StringReader reader = new StringReader(input))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    count++;
                    RegexLines(line);
                }
            }
        }

        private static string GetWifiPassword(string wifiname)
        {
            string argument = "wlan show profile name=\"" + wifiname + "\" key=clear";
            Process processWifi = new Process();
            processWifi.StartInfo.FileName = "netsh";
            processWifi.StartInfo.Arguments = argument;
            processWifi.StartInfo.RedirectStandardOutput = true;
            processWifi.Start();
            
            string output = processWifi.StandardOutput.ReadToEnd();
            processWifi.WaitForExit();
            return output;
        }

        private static void RegexLines(string input2)
        {
            Regex regex1 = new Regex(@"All User Profile * : (?<after>.*)");
            Match match1 = regex1.Match(input2);

            if (match1.Success)
            {
                count_names++;
                string current_name = match1.Groups["after"].Value;
                string password = HandleSinglePassword(current_name);

                Console.WriteLine($"{count_names.ToString().PadRight(5)}{current_name.PadRight(25)}{password}");
            }
        }

        private static string HandleSinglePassword(string wifiname)
        {
            string get_password = GetWifiPassword(wifiname);        
            using (StringReader reader = new StringReader(get_password))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Regex regex2 = new Regex(@"Key Content * : (?<after>.*)");
                    Match match2 = regex2.Match(line);

                    if (match2.Success)
                    {
                        string current_password = match2.Groups["after"].Value;
                        return current_password;
                    }
                }
            }
            return "Open Network";
        }
    }
}
