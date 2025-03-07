﻿using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace RemnantSaveGuardian
{
    internal class UpdateCheck
    {
        private static string repo = "Razzmatazzz/RemnantSaveGuardian";
        private static readonly HttpClient client = new();
        private static DateTime lastUpdateCheck = DateTime.MinValue;

        public static event EventHandler<NewVersionEventArgs> NewVersion;

        public static bool OpenDownloadPage { get; set; } = true;

        public static async void CheckForNewVersion()
        {
            try
            {
                if (lastUpdateCheck.AddMinutes(5) > DateTime.Now)
                {
                    Logger.Warn(Loc.T("You must wait 5 minutes between update checks"));
                    return;
                }
                lastUpdateCheck = DateTime.Now;
                GameInfo.CheckForNewGameInfo();
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{repo}/releases/latest");
                request.Headers.Add("user-agent", "remnant-save-guardian");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                JsonNode latestRelease = JsonNode.Parse(await response.Content.ReadAsStringAsync());

                Version remoteVersion = new Version(latestRelease["tag_name"].ToString());
                Version localVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                if (localVersion.CompareTo(remoteVersion) == -1)
                {
                    NewVersion?.Invoke(null, new() { Version = remoteVersion, Uri = new(latestRelease["html_url"].ToString()) });
                    if (OpenDownloadPage)
                    {
                        Process.Start("explorer", "https://github.com/Razzmatazzz/RemnantSaveGuardian/releases/latest");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{Loc.T("Error checking for new version")}: {ex.Message}");
            }
        }
    }

    public class NewVersionEventArgs : EventArgs
    {
        public Version Version { get; set; }
        public Uri Uri { get; set; }
    }
    public class UpdateCheckErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
    }
}
