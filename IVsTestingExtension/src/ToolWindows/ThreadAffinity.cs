namespace IVsTestingExtension.ToolWindows
{
    public enum ThreadAffinity
    {
        ASYNC_FROM_UI,
        ASYNC_FROM_BACKGROUND,
        SYNC_JTFRUN_BLOCKING,
        SYNC_THREADPOOL_TASKRUN,
        SYNC_JTFRUNASYNC_FIRE_FORGET,
        SYNC_BLOCKING_TASKRUN,
    }
}
