using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
internal class MemoryScan
{
	public struct MEMORY_BASIC_INFORMATION
	{
		public int BaseAddress;
		public int AllocationBase;
		public int AllocationProtect;
		public int RegionSize;
		public int State;
		public int Protect;
		public int lType;
	}
	public struct SYSTEM_INFO
	{
		public ushort processorArchitecture;
		private ushort reserved;
		public uint pageSize;
		public IntPtr minimumApplicationAddress;
		public IntPtr maximumApplicationAddress;
		public IntPtr activeProcessorMask;
		public uint numberOfProcessors;
		public uint processorType;
		public uint allocationGranularity;
		public ushort processorLevel;
		public ushort processorRevision;
	}
	public class MemoryScanResult
	{
		public int BaseAddress;
		public int Address;
		public int offset;
		public byte[] buffer;
		public MemoryScanResult(int BaseAddress, int Address, int offset, byte[] buffer)
		{
			this.BaseAddress = BaseAddress;
			this.Address = Address;
			this.offset = offset;
			this.buffer = buffer;
		}
	}
	private const int PROCESS_QUERY_INFORMATION = 1024;
	private const int MEM_COMMIT = 4096;
	private const int PAGE_READWRITE = 4;
	private const int PROCESS_WM_READ = 16;
	private Process process = null;
	[DllImport("kernel32.dll")]
	public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
	[DllImport("kernel32.dll")]
	public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
	[DllImport("kernel32.dll")]
	private static extern void GetSystemInfo(out MemoryScan.SYSTEM_INFO lpSystemInfo);
	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MemoryScan.MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);
	public MemoryScan(Process P)
	{
		this.process = P;
	}
	public int SingleSignatureScan(int[] pattern, byte[] buffer)
	{
		int result;
		for (int i = 0; i < buffer.Length - pattern.Length; i++)
		{
			if ((int)buffer[i] == pattern[0])
			{
				for (int j = 1; j < pattern.Length; j++)
				{
					if (pattern[j] >= 0 && (int)buffer[i + j] != pattern[j])
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
	public MemoryScan.MemoryScanResult SingleScan(int[] pattern)
	{
		MemoryScan.SYSTEM_INFO sYSTEM_INFO = default(MemoryScan.SYSTEM_INFO);
		MemoryScan.GetSystemInfo(out sYSTEM_INFO);
		IntPtr minimumApplicationAddress = sYSTEM_INFO.minimumApplicationAddress;
		IntPtr maximumApplicationAddress = sYSTEM_INFO.maximumApplicationAddress;
		long num = (long)minimumApplicationAddress;
		long num2 = (long)maximumApplicationAddress;
		IntPtr intPtr = MemoryScan.OpenProcess(1040, false, this.process.Id);
		MemoryScan.MEMORY_BASIC_INFORMATION mEMORY_BASIC_INFORMATION = default(MemoryScan.MEMORY_BASIC_INFORMATION);
		int num3 = 0;
		MemoryScan.MemoryScanResult result;
		while (num < num2)
		{
			MemoryScan.VirtualQueryEx(intPtr, minimumApplicationAddress, out mEMORY_BASIC_INFORMATION, 28u);
			if (mEMORY_BASIC_INFORMATION.Protect == 4 && mEMORY_BASIC_INFORMATION.State == 4096)
			{
				byte[] array = new byte[mEMORY_BASIC_INFORMATION.RegionSize];
				MemoryScan.ReadProcessMemory((int)intPtr, mEMORY_BASIC_INFORMATION.BaseAddress, array, mEMORY_BASIC_INFORMATION.RegionSize, ref num3);
				int num4 = this.SingleSignatureScan(pattern, array);
				if (num4 >= 0)
				{
					result = new MemoryScan.MemoryScanResult(mEMORY_BASIC_INFORMATION.BaseAddress, mEMORY_BASIC_INFORMATION.BaseAddress + num4, num4, array);
					return result;
				}
			}
			num += (long)mEMORY_BASIC_INFORMATION.RegionSize;
			minimumApplicationAddress = new IntPtr(num);
		}
		result = null;
		return result;
	}
	public MemoryScan.MemoryScanResult[] Scan(int[] pattern)
	{
		MemoryScan.SYSTEM_INFO sYSTEM_INFO = default(MemoryScan.SYSTEM_INFO);
		MemoryScan.GetSystemInfo(out sYSTEM_INFO);
		IntPtr minimumApplicationAddress = sYSTEM_INFO.minimumApplicationAddress;
		IntPtr maximumApplicationAddress = sYSTEM_INFO.maximumApplicationAddress;
		long num = (long)minimumApplicationAddress;
		long num2 = (long)maximumApplicationAddress;
		IntPtr intPtr = MemoryScan.OpenProcess(1040, false, this.process.Id);
		MemoryScan.MEMORY_BASIC_INFORMATION mEMORY_BASIC_INFORMATION = default(MemoryScan.MEMORY_BASIC_INFORMATION);
		int num3 = 0;
		List<MemoryScan.MemoryScanResult> list = new List<MemoryScan.MemoryScanResult>();
		while (num < num2)
		{
			MemoryScan.VirtualQueryEx(intPtr, minimumApplicationAddress, out mEMORY_BASIC_INFORMATION, 28u);
			if (mEMORY_BASIC_INFORMATION.Protect == 4 && mEMORY_BASIC_INFORMATION.State == 4096)
			{
				byte[] array = new byte[mEMORY_BASIC_INFORMATION.RegionSize];
				MemoryScan.ReadProcessMemory((int)intPtr, mEMORY_BASIC_INFORMATION.BaseAddress, array, mEMORY_BASIC_INFORMATION.RegionSize, ref num3);
				int num4 = this.SingleSignatureScan(pattern, array);
				if (num4 >= 0)
				{
					list.Add(new MemoryScan.MemoryScanResult(mEMORY_BASIC_INFORMATION.BaseAddress, mEMORY_BASIC_INFORMATION.BaseAddress + num4, num4, array));
				}
			}
			num += (long)mEMORY_BASIC_INFORMATION.RegionSize;
			minimumApplicationAddress = new IntPtr(num);
		}
		return list.ToArray();
	}
}
