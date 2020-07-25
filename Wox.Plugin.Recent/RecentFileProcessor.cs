using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Shell32;

namespace Wox.Plugin.Recent
{

    

    public class TargetDescriptor
    {

        public string Extension { get; set; }

        public string Arguments { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

    }

    public class RecentActionDescriptor
    {
        public string ActionLink { get; set; }

        public string ActionName { get; set; }

        public DateTime CreationTime { get; set; }

        public TargetDescriptor Target { get; set; }

    }


    public class RecentFileProcessor
    {


        #region api

        public static class Paths
        {
            public static string WindowsRecentDirectory => Environment.GetFolderPath(Environment.SpecialFolder.Recent);
        }

        public void Reload()
        {
            GrabRecentFiles();
        }

        public List<RecentActionDescriptor> RecentFilesList => _recentFileList;

        //public IEnumerable<String> IndexedFolderPaths
        //{
        //    get
        //    {
        //        XmlDocument doc = new XmlDocument();
        //        doc.LoadXml(File.ReadAllText(Paths.ConfigFile));

        //        XmlNodeList folders = doc.DocumentElement.SelectNodes("/settings/indexed_folders/folder");

        //        foreach (XmlNode folder in folders)
        //        {
        //            string path =  mroot.substitue_enviro_vars(folder.Attributes["path"].Value);
        //            if (Directory.Exists(path))
        //            {
        //                yield return path;
        //            }
        //        }
        //    }
        //}

        #endregion

        #region private

        private void GrabRecentFiles()
        {
            this._recentFileList.Clear();

            DirectoryInfo recentDir = new DirectoryInfo(Paths.WindowsRecentDirectory);

            foreach (FileInfo fileInfo in recentDir.GetFiles())
            {
                this._recentFileList.Add(new RecentActionDescriptor()
                {
                    ActionLink = fileInfo.FullName,
                    CreationTime = fileInfo.CreationTime,
                    ActionName = Path.GetFileNameWithoutExtension(fileInfo.FullName)
                    //Target = GetShortcutTargetDescriptor(fileInfo.FullName)
                }); ;
            }
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

                return desc;
            }

            return null;
        }
        private readonly List<RecentActionDescriptor> _recentFileList = new List<RecentActionDescriptor>();

        #endregion
    }
}
