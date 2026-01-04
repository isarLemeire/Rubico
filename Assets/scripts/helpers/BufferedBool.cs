using UnityEngine;


public class BufferedBool
{
    private float _timer;
    private readonly float _duration;
    private bool _currentValue;

    public BufferedBool(float duration)
    {
        this._duration = duration;
        _timer = -1f;
    }

    public void Set(bool pressed)
    {
        if (pressed)
        {
            _timer = _duration;
        }
        _currentValue = pressed;
    }

    public void Update(float deltaTime)
    {
        if (_timer > 0f)
            _timer -= deltaTime;
    }

    public void Clear()
    {
        _timer = -1f;
    }

    public bool IsActive => _timer > 0f;

    public bool IsTrue => _currentValue;
}

