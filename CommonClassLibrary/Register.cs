using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//custom
using Microsoft.Win32;  //注册表操作


namespace CommonClassLibrary
{
    public class Register
    {
        private static string APPNAMEREG = "LandResources";  //在注册表中，本程序的名称为“LandResouces”
        private static string CONFIGFILEREG = "AppConfig";  //本软件初试配置路径


        //读取指定名称的注册表的值
        //读取的注册表中HKEY_LOCAL_MACHINE\SOFTWARE目录下的XXX目录中名称为name的注册表值；
        public static string GetConfigReg()
        {
            string registData;
            RegistryKey hkml = Registry.LocalMachine;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);
            RegistryKey aimdir = software.OpenSubKey(APPNAMEREG, true);
            registData = aimdir.GetValue(CONFIGFILEREG).ToString();
            return registData;
        }

        
        //写入本软件的注册表项        
        public static void WriteAppReg()
        {
            RegistryKey hklm = Registry.LocalMachine;
            RegistryKey software = hklm.OpenSubKey("SOFTWARE", true);
            RegistryKey aimdir = software.CreateSubKey(APPNAMEREG);
            //aimdir.SetValue(CONFIGFILEREG, tovalue);
        }

        //写入配置文件注册表项        
        public static void WriteConfigReg(string configFilePath)
        {
            RegistryKey hklm = Registry.LocalMachine;
            RegistryKey software = hklm.OpenSubKey("SOFTWARE", true);
            RegistryKey aimdir = software.CreateSubKey(APPNAMEREG);
            aimdir.SetValue(CONFIGFILEREG, configFilePath);
        }

        //删除注册表中指定的注册表项
        //在注册表中HKEY_LOCAL_MACHINE\SOFTWARE目录下XXX目录中删除名称为name注册表项；
        public static void DeleteRegist()
        {
            string[] aimnames;
            RegistryKey hkml = Registry.LocalMachine;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);
            RegistryKey aimdir = software.OpenSubKey(APPNAMEREG, true);
            aimnames = aimdir.GetSubKeyNames();
            foreach (string aimKey in aimnames)
            {
                if (aimKey == CONFIGFILEREG)
                    aimdir.DeleteSubKeyTree(CONFIGFILEREG);
            }
        }
        

        //判断指定注册表项是否存在
        public static bool IsRegeditExit()
        {
            bool _exit = false;
            string[] subkeyNames;
            RegistryKey hkml = Registry.LocalMachine;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);
            RegistryKey aimdir = software.OpenSubKey(APPNAMEREG, true);
            subkeyNames = aimdir.GetSubKeyNames();
            foreach (string keyName in subkeyNames)
            {
                if (keyName == CONFIGFILEREG)
                {
                    _exit = true;
                    return _exit;
                }
            }
            return _exit;
        }

        //判断本软件的注册表项是否存在
        public static bool IsAppRegeditExit()
        {
            bool _exit = false;
            string[] subkeyNames;
            RegistryKey hkml = Registry.LocalMachine;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);            
            subkeyNames = software.GetSubKeyNames();
            foreach (string keyName in subkeyNames)
            {
                if (keyName == APPNAMEREG)
                {
                    _exit = true;
                    return _exit;
                }
            }
            return _exit;
        }
    }
}
