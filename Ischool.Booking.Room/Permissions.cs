﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ischool.Booking.Room
{
    class Permissions
    {
        public static string 場地管理單位 { get { return "8EFBEC7D-D438-44EA-81E3-6AFA00862429"; } }
        public static bool 設定場地管理單位權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[場地管理單位].Executable;
            }
        }

        public static string 管理場地 { get { return "26751E07-00A0-4500-BC31-F2E57EE1C6F2"; } }
        public static bool 管理場地權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[管理場地].Executable;
            }
        }

        public static string 系統管理員 { get { return "74E0D4FA-F698-400D-B8A8-60F4DF304BBA"; } }
        public static bool 設定系統管理員權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[系統管理員].Executable;
            }
        }

        public static string 單位管理員 { get { return "24821EBA-426E-4811-95B8-DBF8D9AEEFA2"; } }
        public static bool 設定單位管理員權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[單位管理員].Executable;
            }
        }

        public static string 審核作業 { get { return "AB164E2A-516E-4427-ADB0-79D27F1685CA"; } }
        public static bool 審核作業權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[審核作業].Executable;
            }
        }


    }
}
