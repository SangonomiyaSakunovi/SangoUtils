// See https://aka.ms/new-console-template for more information
using SangoLog;

LogTool.InitSettings();
LogTool.LogInfo("{0} Start...", "ServerPELog");
LogTool.ColorLog(LogColor.Blue,"这是颜色的测试");
LogTool.LogWarn("这里输出了警告");
LogTool.LogError("这是一个错误信息");
LogTool.LogProcessing("这是正在运行中");
LogTool.LogDone("这是完成信息");
Testing testing = new Testing();
testing.Init();

class Testing
{
    public void Init()
    {
        Root root = new Root();
        root.Init();
    }
}


class Root
{
    public void Init()
    {
        this.ColorLog(LogColor.None, "颜色测试");
        this.ColorLog(LogColor.Cyan, "颜色测试");
        this.ColorLog(LogColor.Magenta, "颜色测试");
        this.ColorLog(LogColor.Green, "颜色测试");
        this.ColorLog(LogColor.Red, "颜色测试");
        this.ColorLog(LogColor.Yellow, "颜色测试");
        this.ColorLog(LogColor.Blue, "颜色测试");
        this.LogTraceInfo("测试堆栈输出");
    }
}

