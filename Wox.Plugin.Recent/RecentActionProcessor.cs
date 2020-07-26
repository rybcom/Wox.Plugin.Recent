using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Shell32;

namespace Wox.Plugin.Recent
{



    public class TargetDescriptor
    {

        public string Extension { get; set; }

        public string Arguments { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public string WorkingDirectory { get; set; }

        public bool IsDirectory { get; set; }

    }

    public class RecentActionDescriptor
    {
        public string ActionLink { get; set; }

        public string ActionName { get; set; }

        public DateTime CreationTime { get; set; }

        public TargetDescriptor Target { get; set; }


    }


    public class RecentActionProcessor
    {


        #region api

        public static class Paths
        {
            public static string WindowsRecentDirectory => Environment.GetFolderPath(Environment.SpecialFolder.Recent);
        }

        public void Reload()
        {
            GrabRecentActions();
        }

        public List<RecentActionDescriptor> RecentActionList => _recentActionList;

        #endregion

        #region private

        private void GrabRecentActions()
        {
            this._recentActionList.Clear();

            DirectoryInfo recentDir = new DirectoryInfo(Paths.WindowsRecentDirectory);

            foreach (FileInfo fileInfo in recentDir.GetFiles())
            {
                TargetDescriptor target = null;
                Start_STA_Thread(() => target = GetShortcutTargetDescriptor(fileInfo.FullName));

                this._recentActionList.Add(new RecentActionDescriptor()
                {
                    ActionLink = fileInfo.FullName,
                    CreationTime = fileInfo.CreationTime,
                    ActionName = Path.GetFileNameWithoutExtension(fileInfo.FullName),
                    Target = target
                }); ;
            }
        }

        private static void Start_STA_Thread(ThreadStart method)
        {
            Thread thread = new Thread(method);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private static TargetDescriptor GetShortcutTargetDescriptor(string shortcutFilename)
        {
            string pathOnly = Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = Path.GetFileName(shortcutFilename);

            Shell shell = new Shell();
            Folder folder = shell.NameSpace(pathOnly);
            FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;

                TargetDescriptor desc = new TargetDescriptor();
                desc.Path = link.Path;
                desc.Extension = Path.GetExtension(desc.Path);
                desc.Name = Path.GetFileName(desc.Path);
                desc.Arguments = link.Arguments;
                desc.WorkingDirectory = link.WorkingDirectory;

                desc.IsDirectory = Directory.Exists(link.Path);

                return desc;
            }

            return new TargetDescriptor();
        }
        private readonly List<RecentActionDescriptor> _recentActionList = new List<RecentActionDescriptor>();

        #endregion
    }
}
