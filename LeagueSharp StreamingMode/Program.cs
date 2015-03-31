using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;

namespace LeagueSharp_StreamingMode
{
    class Program
    {
        //StreamingMode Assembly of stefsot based.

        static Assembly lib;
        static int LeaguesharpCore;
        static Dictionary<int, int> offsets;
        static string CoreName;

        static void Main(string[] args)
        {
            lib = Assembly.Load(Properties.Resources.LeaguesharpStreamingModelib);
            
            looop();
        }

        static void looop()
        {
            Console.Clear();

            Console.Title = "[xcsoft] LeagueSharp StreamingMode For 5.6";

            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine("[*] {0}", Console.Title);
            Console.WriteLine("-------------------------------------------------------------------------------\n");

            try
            {
                var WhereisCore = Directory.GetFiles(Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.None)).Where(x => x.Substring(x.Length - 10).Contains("LS")).FirstOrDefault() + @"\1\").Where(x => x.Substring(x.Length - 4) == ".dll" && new FileInfo(x).Length >= 2616320).FirstOrDefault();
                CoreName = WhereisCore.Substring(WhereisCore.Length - 10);

                Console.WriteLine("[*] L# Core Path: " + WhereisCore);
                Console.WriteLine("[*] L# Core Name: " + CoreName + "\n");

                Console.WriteLine("[*] Waiting for LoL.exe and L# Core Inject...");
            }
            catch
            {
                Console.WriteLine("[*] L# Core file could not be found.");
                Console.Beep();
                Console.ReadLine();
                Process.GetCurrentProcess().Kill();
                return;
            }

            while (LeaguesharpCore == 0)
            {
                LeaguesharpCore = GetModuleAddress();
                System.Threading.Thread.Sleep(500);
            }

            Console.WriteLine("[*] L# Core inject Detected.");

            try
            {
                Program.SetUpOffsets();

                if (!IsEnabled())
                    Program.Enable();
            }
            catch
            {
                Console.WriteLine("[*] Error!");
                Console.Beep();
                Console.ReadLine();
                Process.GetCurrentProcess().Kill();
                return;
            }

            Console.WriteLine("[*] StreamingMode is Enabled.\n");

            Console.Beep(260, 100);
            Console.Beep(290, 100);
            Console.Beep(330, 100);
            Console.Beep(340, 100);
            Console.Beep(380, 100);
            Console.Beep(430, 100);
            Console.Beep(490, 100);
            Console.Beep(510, 500);

            while (LeaguesharpCore != 0)
            {
                LeaguesharpCore = GetModuleAddress();
                System.Threading.Thread.Sleep(500);
            }

            Console.WriteLine("\n[*] GameOver.\n");
            System.Threading.Thread.Sleep(3000);

            looop();
        }

        static int GetModuleAddress()
        {
            try
            {
                Process process = Process.GetProcessesByName("League of Legends")[0];

                for (int i = 0; i < process.Modules.Count; i++)
                {
                    if (process.Modules[i].ModuleName.Contains(CoreName))
                    {
                        return (int)process.Modules[i].BaseAddress;
                    }
                }
            }
            catch
            {
                return 0;
            }

            return 0;
        }

        static byte[] ReadMemory(int address, int length)
        {
            MethodInfo methodInfo = Program.lib.GetType("LeaguesharpStreamingModelib.MemoryModule").GetMethods()[2];
            return (byte[])methodInfo.Invoke(null, new object[]
			{
				address,
				length
			});
        }

        static void WriteMemory(int address, byte value)
        {
            MethodInfo methodInfo = Program.lib.GetType("LeaguesharpStreamingModelib.MemoryModule").GetMethods()[4];
            methodInfo.Invoke(null, new object[]
			{
				address,
				value
			});
        }

        static void WriteMemory(int address, byte[] array)
        {
            MethodInfo methodInfo = Program.lib.GetType("LeaguesharpStreamingModelib.MemoryModule").GetMethods()[4];
            methodInfo.Invoke(null, new object[]
			{
				address,
				array
			});
        }

        static int SignatureScan(int start, int length, int[] pattern)
        {
            byte[] array = Program.ReadMemory(start, length);
            int result;
            for (int i = 0; i < array.Length - pattern.Length; i++)
            {
                if ((int)array[i] == pattern[0])
                {
                    for (int j = 1; j < pattern.Length; j++)
                    {
                        if (pattern[j] >= 0 && (int)array[i + j] != pattern[j])
                        {
                            break;
                        }
                        if (j == pattern.Length - 1)
                        {
                            result = i;
                            return result;
                        }
                    }
                }
            }
            result = -1;
            return result;
        }

        static void SetUpOffsets()
        {
            Program.offsets = new Dictionary<int, int>();
            int[] pattern = new int[]
			{
				85,
				139,
				236,
				106,
				255,
				104,
				-1,
				-1,
				-1,
				-1,
				100,
				161,
				0,
				0,
				0,
				0,
				80,
				131,
				236,
				12,
				86,
				161,
				-1,
				-1,
				-1,
				-1,
				51,
				197
			};
            int[] pattern2 = new int[]
			{
				85,
				139,
				236,
				141,
				69,
				20,
				80
			};
            int length = 327680;
            int value = Program.SignatureScan(Program.LeaguesharpCore, length, pattern);
            int num = Program.SignatureScan(Program.LeaguesharpCore, length, pattern2);
            Program.offsets.Add(0, value);
            Program.offsets.Add(1, num);
            Program.offsets.Add(2, num - 123);
        }

        static void Enable()
        {
            Program.WriteMemory(Program.LeaguesharpCore + Program.offsets[0], 195);
            Program.WriteMemory(Program.LeaguesharpCore + Program.offsets[1], 195);
            Program.WriteMemory(Program.LeaguesharpCore + Program.offsets[2], new byte[]
			{
				144,
				144,
				144,
				144,
				144,
				144
			});
        }

        static void Disable()
        {
            Program.WriteMemory(Program.LeaguesharpCore + Program.offsets[0], 85);
            Program.WriteMemory(Program.LeaguesharpCore + Program.offsets[1], 85);
        }

        static bool IsEnabled()
        {
            return Program.ReadMemory(Program.LeaguesharpCore + Program.offsets[1], 1)[0] == 195;
        }
    }
}
