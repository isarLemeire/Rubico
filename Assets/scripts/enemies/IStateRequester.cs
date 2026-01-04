using System;

public interface IStateRequester
{
    void RequestAnimationState(String triggerName);
    void CancelAnimationState(String triggerName);
    void RequestAnimationBool(bool value, String boolName);

    void RequestState(object stateTag);
}