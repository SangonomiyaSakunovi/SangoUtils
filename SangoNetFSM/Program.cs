// See https://aka.ms/new-console-template for more information

using SangoNetFSM;

SangoLog.LogTool.InitSettings();

NetFSMExample netFSM = new NetFSMExample();
netFSM.InitFSM("TestStater");
