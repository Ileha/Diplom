using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace IOTClient
{
	public class BashExecutor
	{
		public static Task<string> ExecuteBashCommand(string command) {
			command = command.Replace("\"", "\"\"");

			Process proc = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "/bin/bash",
					Arguments = String.Format("-c \"{0}\"", command),
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};

			TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

			ThreadPool.QueueUserWorkItem((object state) => {
				proc.Start();
				proc.WaitForExit();

				if (proc.ExitCode != 0) {
					tcs.SetException(new Exception(proc.StandardOutput.ReadToEnd()));
				}
				else {
					tcs.SetResult(proc.StandardOutput.ReadToEnd());
				}
			});

			return tcs.Task;
		}
	}
}
