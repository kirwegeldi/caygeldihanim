using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CAY_Weighing
{
    public static class WTM
    {


        public static void listenHat1(bool plcStartHat1)
        {
            Thread.Sleep(10);
            while (plcStartHat1)
            {
                bool flag = false;                          // bütün siloların completed olup olmadığına göre while döngüsünden çıkarır
                for (int i = 0; i < Silo.Hat1.Count; i++)
                {
                    var silo = Silo.Hat1[i];

                    if (silo.Connected && silo._isActive && !silo.Completed)
                    {
                        flag = true;
                        int[] value = silo.modbusComm.GetMessage();
                        if (value != null && value[1] / 10.0 > silo._valueLow)
                        {
                            silo.Completed = true;
                            PLC.WriteCoil(8268 + silo._ıd, true);
                            Thread.Sleep(30);
                        }
                    }
                }
                if (!flag) 
                {
                    plcStartHat1 = false;
                    foreach (var silo in Silo.Hat1)
                    {
                        silo.Completed = true;
                        PLC.WriteCoil(8268 + silo._ıd, false);
                    }
                }
            }
        }
        public static void listenHat2(bool plcStartHat2)
        {
            Thread.Sleep(10);
            while (plcStartHat2)
            {
                bool flag = false;                          // bütün siloların completed olup olmadığına göre while döngüsünden çıkarır
                for (int i = 0; i < Silo.Hat2.Count; i++)
                {
                    var silo = Silo.Hat2[i];

                    if (silo.Connected && silo._isActive && !silo.Completed)
                    {
                        flag = true;
                        if (silo.modbusComm.GetMessage()[1] / 10.0 < silo._valueLow)
                        {
                            silo.Completed = true;
                            PLC.WriteCoil(8268 + silo._ıd, true);
                        }
                    }
                }
                if (!flag)
                {
                    plcStartHat2 = false;
                    foreach (var silo in Silo.Hat2)
                    {
                        silo.Completed = false;
                        PLC.WriteCoil(8268 + silo._ıd, false);
                    }
                }
            }

        }

    }
}
