using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMqttSubApp.Helpers
{
    public static class Common
    {
        //DB연결 connectString
        public static readonly string CONNSTR = $"Server={App.configuration.Database.Server};Database={App.configuration.Database.Database};Uid={App.configuration.Database.User};Pwd={App.configuration.Database.Password};Charset=utf8";

    }
}
