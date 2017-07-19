using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEditor.Utils;

namespace AssetBundles.Manager
{
	internal class LaunchAssetBundleServer : ScriptableSingleton<LaunchAssetBundleServer>
	{
		[SerializeField]
		private int 	m_ServerPID = 0;

        [SerializeField]
        private ServerSetting m_launchedSetting;

        [SerializeField]
        private string     m_args;

		public static void ToggleLocalAssetBundleServer ()
		{
			bool isRunning = IsRunning();
			if (!isRunning)
			{
				Run ();
			}
			else
			{
				KillRunningAssetBundleServer ();
			}
		}

		public static bool IsRunning ()
		{
			if (instance.m_ServerPID == 0)
				return false;

            try
            {
                var lastProcess = Process.GetProcessById (instance.m_ServerPID);
                if (lastProcess == null) {
                    return false;
                }
                return !lastProcess.HasExited;
            }
            catch
            {
            }

            return false;
		}

        public static string GetServerArgs () {
            return instance.m_args;
        }

        public static ServerSetting GetServerSetting() {
            return instance.m_launchedSetting;
        }

        public static void Restart() {
            KillRunningAssetBundleServer ();
            Run ();
        }

		public static void KillRunningAssetBundleServer ()
		{
			// Kill the last time we ran
			try
			{
				if (instance.m_ServerPID == 0)
					return;

				var lastProcess = Process.GetProcessById (instance.m_ServerPID);
				lastProcess.Kill();
				instance.m_ServerPID = 0;
			}
			catch
			{
			}
		}
		
		static void Run ()
		{
            var serverSetting = Settings.CurrentSetting;

            string pathToAssetServer = Path.Combine(Settings.Path.BaseFullPath, "Editor/AssetBundleServer.exe");
	
			KillRunningAssetBundleServer();
			
            string bundleFolder = serverSetting.AssetBundleDirectory;
			
            if(!Directory.Exists(bundleFolder)) {
                instance.m_args = "Directory does not exist. Build asset bundles first and create directory to run local server:" + bundleFolder;
                UnityEngine.Debug.LogError(instance.m_args);
				return;
			}

			#if UNITY_2017_1_OR_NEWER
			string monoProfile = "4.5";
			#elif UNITY_5_5_OR_NEWER
			string monoProfile = "v4.0.30319";
			#else
			string monoProfile = "4.0";
			#endif

            var args = string.Format("\"{0}\" {1}", bundleFolder, Process.GetCurrentProcess().Id);
			ProcessStartInfo startInfo = ExecuteInternalMono.GetProfileStartInfoForMono(MonoInstallationFinder.GetMonoInstallation("MonoBleedingEdge"), monoProfile, pathToAssetServer, args, true);
            startInfo.WorkingDirectory = bundleFolder;
			startInfo.UseShellExecute = false;
			Process launchProcess = Process.Start(startInfo);
			if (launchProcess == null || launchProcess.HasExited == true || launchProcess.Id == 0)
			{
				//Unable to start process
				UnityEngine.Debug.LogError ("Unable Start AssetBundleServer process");
                instance.m_args = "Not running.";
                instance.m_launchedSetting = null;
			}
			else
			{
				UnityEngine.Debug.LogFormat("Local Server started with arg:{0}", args);
				//We seem to have launched, let's save the PID
				instance.m_ServerPID = launchProcess.Id;
                instance.m_args = args;
                instance.m_launchedSetting = serverSetting;
			}
		}
	}
}