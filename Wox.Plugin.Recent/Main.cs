﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using mroot_lib;
using System.Windows.Forms;
using System.Linq;
using System.IO;

namespace Wox.Plugin.Recent
{
    public class Main : IPlugin, IContextMenu
    {
        #region members

        private readonly RecentFileProcessor _recentFileProcessor = new RecentFileProcessor();

        #endregion

        #region wox overrides

        public void Init(PluginInitContext context)
        {
            //MessageBox.Show("start point");
            _recentFileProcessor.Reload();
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
                _recentFileProcessor.Reload();
            }
        }

        #endregion

        #region commands

        private void AddCommands(List<Result> resultList, Query query)
        {
            var enumeration_files = _recentFileProcessor.RecentFilesList.OrderByDescending(x => x.CreationTime);

            foreach (var recentActionDescriptor in enumeration_files)
            {
                if (IsEqualOnStart(query.FirstSearch, recentActionDescriptor.ActionName) == false)
                {
                    continue;
                }
                Result commandResult = new Result();
                //commandResult.Title = recentActionDescriptor.Target.Name;
                //commandResult.SubTitle = $"{recentActionDescriptor.Target.Path} {recentActionDescriptor.Target.Arguments}";
                commandResult.Title = recentActionDescriptor.ActionName;
                commandResult.SubTitle = recentActionDescriptor.ActionLink;
                commandResult.Score = 1000;
                commandResult.IcoPath = "Images\\recent_icon.png";
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

        private static bool IsEqualOnStart(string query, params string[] values)
        {
            int lengthQuery = query.Length;
            foreach (var pattern in values)
            {
                int lengthPattern = pattern.Length;
                if (lengthPattern > lengthQuery)
                {
                    if (query.Equals(pattern.Substring(0, lengthQuery), StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }
                else
                {
                    if (pattern.Equals(query.Substring(0, lengthQuery), StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
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