using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAY_Weighing
{
    public class DbSchema
    {
        public class Result
        {
            public const string NAME = "Result";
            public static class ResultTittle
            {
                public const string ID = "ID";
                public const string SILO1 = "SILO1";
                public const string SILO2 = "SILO2";
                public const string SILO3 = "SILO3";
                public const string SILO4 = "SILO4";
                public const string SILO5 = "SILO5";
                public const string SILO6 = "SILO6";
                public const string SILO7 = "SILO7";
                public const string SILO8 = "SILO8";
                public const string HAT1 = "HAT1";
                public const string HAT2 = "HAT2";
                public const string TARIH = "TARIH";
            }
        }

        public class User
        {
            public const string NAME = "LivaUserInfo";


            public static class UserTittle
            {
                public const string ID = "ID";
                public const string Password = "Password";
                
            }
        }
    }
}