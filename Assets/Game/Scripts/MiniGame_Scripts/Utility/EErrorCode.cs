public enum EErrorCode
{
    None,
    DiskFull,                    //磁盘已满，没法保存下载下来的资源，提醒玩家请清理一下
    FileLost,                    //本地错误，未找到对应文件
    Unknown,                     //未知错误
}