using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TesterClient
{
    class Program
    {
        private static HttpClient _client;

        private static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}");
        }

        private static async Task<DirectoryInfo> DownloadArchive(string url, string name)
        {
            DirectoryInfo dir = new DirectoryInfo(name);
            if (dir.Exists)
            {
                dir.Delete(true);
                dir.Refresh();
            }

            dir.Create();
            dir.Refresh();

            using (var zip = new ZipArchive(await _client.GetStreamAsync(url)))
                zip.ExtractToDirectory(dir.FullName);
            return dir;
        }

        private static Task SetStatus(int submissionId, double score, string title, string message)
        {
            return _client.GetAsync($"SetStatus/?submissionId={submissionId}&score={score}&title={title}&message={message}");
        }

        private static Task RunTester(int submissionId, DirectoryInfo testerDir, DirectoryInfo submissionDir)
        {
            FileInfo[] runFiles = testerDir.GetFiles("run.*");

            if (runFiles.Length != 1)
            {
                Log("Invalid tester archive");
                return SetStatus(submissionId, -1, "CF", "Check failed. Only one 'run.*' files allowed");
            }

            FileInfo runFile = runFiles[0];
            string resultPath = Path.Combine(testerDir.FullName, "result.txt");

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = runFile.FullName,
                WorkingDirectory = testerDir.FullName,
                Arguments = $"\"{submissionDir.FullName}\" \"{resultPath}\""
            };

            Log("Executing tester...");
            try
            {
                Process tester = Process.Start(startInfo);
                tester.WaitForExit();
                tester.Dispose();
            }
            catch
            {
                Log("Tester failed to run");
            }

            if (!File.Exists(resultPath))
            {
                Log("Tester didn't generate resulting file");
                return SetStatus(submissionId, -1, "CF", "No result file was generated found.");
            }

            string title, message;
            double score;
            using (StreamReader reader = File.OpenText(resultPath))
            {
                score = Convert.ToDouble(reader.ReadLine());
                title = reader.ReadLine();
                message = reader.ReadToEnd();
            }

            Log($"Tester answer: {title} (Score: {score})");
            return SetStatus(submissionId, score, title, message);
        }

        private static async Task LoopAsync()
        {
            {
                Log("Querying submission...");
                var response = await _client.GetAsync("GetPendingSubmission");

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Log("No available submissions...");
                    await Task.Delay(15000);
                    return;
                }

                var info = JObject.Parse(await response.Content.ReadAsStringAsync());

                int submissionId = info.Value<int>("submissionId");
                int taskVariantId = info.Value<int>("taskVariantId");

                Log("Downloading tester...");
                DirectoryInfo testerDir = await DownloadArchive($"GetTester/{taskVariantId}", "tester");
                Log("Downloading submission...");
                DirectoryInfo submissionDir = await DownloadArchive($"GetSubmission/{submissionId}", "tester/submission");

                await RunTester(submissionId, testerDir, submissionDir);

                testerDir.Delete(true);
            }
        }

        static void Main(string[] args)
        {
            _client = new HttpClient { BaseAddress = new Uri(new Uri(args[0]), "Testing/") };

            while (true)
            {
                try
                {
                    LoopAsync().Wait();
                }
                catch (Exception e)
                {
                    Log($"General loop failure\n{e}");
                    Thread.Sleep(5000);
                }
            }
        }
    }
}