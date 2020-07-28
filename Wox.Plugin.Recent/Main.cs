
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using mroot_lib;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using Wox.Infrastructure.Storage;

namespace Wox.Plugin.Recent
{

    public class Settings
    {
        public string DirectoryManager { get; set; }
        public string FileProcessor { get; set; }
    }

    public class Main : IPlugin, IContextMenu, ISavable,ISettingProvider
    {
        #region members

        private readonly RecentActionProcessor _recentActionProcessor = new RecentActionProcessor();
        private PluginJsonStorage<Settings> _settingsStorage;
        private Settings _settings;

        #endregion

        #region wox overrides

        public System.Windows.Controls.Control CreateSettingPanel()
        {
            return new SettingsPanel();
        }

        public void loadSettings()
        {
            _settingsStorage = new PluginJsonStorage<Settings>();
            _settings = _settingsStorage.Load();
        }

        public void Save()
        {
            _settingsStorage.Save();
        }

        public void Init(PluginInitContext context)
        {
            MessageBox.Show("attach point for debugging");

            loadSettings()
            _recentActionProcessor.Reload();
        }

        public List<Result> Query(Query query)
        {
            UpdateRecentFilesAtBeginning(query);

            List<Result> resultList = new List<Result>();
            this.AddCommands(resultList, query);
            return resultList;
        }

        public void UpdateRecentFilesAtBeginning(Query query)
        {
            if (String.IsNullOrWhiteSpace(query.Search))
            {
                _recentActionProcessor.Reload();
            }
        }

        #endregion

        #region commands

        private void AddCommands(List<Result> resultList, Query query)
        {
            var enumeration_actions = _recentActionProcessor.RecentActionList.OrderByDescending(x => x.CreationTime);

            foreach (var recentActionDescriptor in enumeration_actions)
            {
                TargetDescriptor target = recentActionDescriptor.Target;

                Result commandResult = new Result();

                if (target == null)
                {
                    commandResult.Title = recentActionDescriptor.ActionName;
                    commandResult.SubTitle = recentActionDescriptor.ActionLink;
                    commandResult.IcoPath = "Images\\recent_icon.png";

                }
                else if (target.IsDirectory)
                {
                    commandResult.Title = target.Name;
                    commandResult.SubTitle = $"Directory : {target.Path} ";
                    commandResult.IcoPath = "Images\\recent_dir.png";
                }
                else
                {
                    var subtitle = String.IsNullOrEmpty(target.Arguments) ?
                        target.Path : $"{target.Path} ,Args: {target.Arguments}";

                    commandResult.Title = target.Name;
                    commandResult.SubTitle = subtitle;
                    commandResult.IcoPath = "Images\\recent.png";
                }

                commandResult.Score = (int)(1000 * StringTools.FuzzyMatch(query.Search, commandResult.Title));
                //commandResult.Title += " " + commandResult.Score;

                commandResult.Action = e =>
                {
                    void thread_execution()
                    {
                        Thread.Sleep(100);

                        Process process = new Process();
                        process.StartInfo.FileName = recentActionDescriptor.ActionLink;
                        process.Start();
                    }

                    new Thread(thread_execution).Start();

                    return true;
                };

                resultList.Add(commandResult);
            }
        }

        public List<Result> LoadContextMenus(Result selectedResult)
        {
            return new List<Result>
            {
                new Result()
                {
                    Title = "Open in text editor",
                    SubTitle = selectedResult.SubTitle,
                    IcoPath = "Images\\sublime_logo.png",
                    Action = e =>
                    {
                        string param = mroot.substitue_enviro_vars(selectedResult.SubTitle);
                        string exec_path = mroot.substitue_enviro_vars("||default_file_processor||");

                        Process process = new Process();
                        process.StartInfo.FileName = exec_path;
                        process.StartInfo.Arguments = param;
                        process.Start();
                        return true;
                    }
                }
            };
        }
    }

    #endregion

}
