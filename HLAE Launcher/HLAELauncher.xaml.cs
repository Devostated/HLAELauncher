// Left to do:
// Markdown parser

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Path = System.IO.Path;

namespace HLAE_Launcher
{
    public partial class MainWindow : Window
    {
        private const string toolName = "HLAE"; // Your tools name
        private const string urlRepo = "https://github.com/advancedfx/advancedfx"; // Link to the repository
        private const string exe = toolName + ".exe";
        private string location = AppDomain.CurrentDomain.BaseDirectory;
        private JToken result = null;
        private WebClient client = null;

        public MainWindow()
        {
            InitializeComponent();
            CheckUpdate();
            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
        }
        private void ButtonClose(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void ButtonCancel(object sender, RoutedEventArgs e)
        {
            Process.Start(exe);
            Close();
        }

        private void CheckUpdate()
        {
            // web stuff
            client = new WebClient();
            client.Headers.Add("User-Agent", "C# program");

            string username = null, repository = null;
            Match m = Regex.Match(urlRepo, @"(?i:https://github.com[^\s/]*)/(?<username>[^\s/]*)/(?<repository>[^\s/]*)?");
            if (m.Success)
            {
                username = m.Groups["username"].Value;
                repository = m.Groups["repository"].Value;
            }
            string url = "https://api.github.com/repos/" + username + "/" + repository + "/releases/latest";
            string content = client.DownloadString(url);
            result = JsonConvert.DeserializeObject<JToken>(content);

            // Changelog - has to be changed to use real MD text
            string changelogTxt = result["body"].ToString();

            changelogTxt = changelogTxt.Replace("#  ", "");
            changelogTxt = changelogTxt.Replace("# ", "");
            changelogTxt = changelogTxt.Replace("#", "");
            changelogTxt = changelogTxt.Replace("**", "\t-");
            changelogTxt = changelogTxt.Replace("*", "-");
            Changelogbox.Text = changelogTxt;

            string changelogVersion = result["tag_name"].ToString();
            ChangelogLabel.Content = "Changelog: " + changelogVersion;

            // Version
            var versionInfo = FileVersionInfo.GetVersionInfo(@location + exe);
            string releaseVer = result["tag_name"].ToString();
            string localVer = versionInfo.FileVersion;
            string checkVer = "v" + localVer.Remove(localVer.Length - 2);

            if (checkVer == releaseVer)
            {
                // Launch HLAE
                Process[] processes = Process.GetProcessesByName(toolName);
                if (processes.Length == 0)
                {
                    Process.Start(exe);
                    System.Environment.Exit(1);
                }
            }
        }

        private void ButtonUpdate(object sender, RoutedEventArgs e)
        {
            
            string downloadUrl = result["assets"][0]["browser_download_url"].ToString();

            // Zip stuff
            string zipName = result["assets"][0]["name"].ToString();
            string zipPath = @location + zipName;
            string extractPath = @location;

            // Download release
            client.DownloadFile(downloadUrl, @location + zipName);

            // Kill running process
            Process[] workers = Process.GetProcessesByName(toolName);
            foreach (Process worker in workers)
            {
                worker.Kill();
                worker.WaitForExit();
                worker.Dispose();
            }

            // Extract and overwrite files
            bool overwrite = true;
            if (overwrite == false)
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            }
            else
            {
                using ZipArchive archive = new ZipArchive(File.OpenRead(zipPath));
                foreach (ZipArchiveEntry file in archive.Entries)
                {
                    if (file.Name == "")
                    {
                        continue;
                    }

                    string filepath = Path.Combine(extractPath, file.FullName);
                    FileInfo outputFile = new FileInfo(filepath);
                    if (!outputFile.Directory.Exists)
                    {
                        outputFile.Directory.Create();
                    }
                    file.ExtractToFile(outputFile.FullName, true);
                }
            }

            File.Delete(@location + zipName);

            // Restart HLAE
            Process.Start(exe);

            Close();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
