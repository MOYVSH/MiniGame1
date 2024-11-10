public struct ApplicationPauseEvent
{
    public bool pauseStatus;

    public ApplicationPauseEvent(bool pauseStatus)
    {
        this.pauseStatus = pauseStatus;
    }
}