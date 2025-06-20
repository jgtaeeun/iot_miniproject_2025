using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMrpSimulatorApp.Helpers
{
    //프로젝트 내에서 공통으로 사용하는 정적 클래스
    //클래스 자체가 static일 필요는 없음. 사용할 변수들이 static이어야 함!!
    public static class Common
    {
        public static readonly string CONNSTR = "Server=localhost;Database=mydb;Uid=root;Password=12345;Charset=utf8";
        public static IDialogCoordinator DIALOGCOORDINATOR;

    }
}
