using System;

public static class CustomFunctions
{
    public static double GetUnixTime()
    {
        // 获取当前 UTC 时间
        DateTime currentTime = DateTime.UtcNow;

        // 计算自1970年1月1日以来的秒数
        DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan timeSpan = currentTime - unixEpoch;

        // 返回以秒为单位的时间戳
        return timeSpan.TotalSeconds;
    }
}