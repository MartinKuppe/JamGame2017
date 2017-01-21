using UnityEngine;

public struct ParticleControls {

    public bool IsValid { get { return _key != 0 && _inst != null && _inst.Key == _key; } }
    private uint _key;
    private ParticleInstance _inst;

    public ParticleControls(ParticleInstance instance)
    {
        _key = instance.Key;
        _inst = instance;
    }

    public void Stop()
    {
        if (IsValid)
            _inst.Stop();
    }

    public void MoveTo(Vector3 position)
    {
        if (IsValid)
            _inst.MoveTo(position);
    }

    public void RotateTo(Vector3 rotation)
    {
        if (IsValid)
            _inst.RotateTo(rotation);
    }

    public void RotateTo(Quaternion rotation)
    {
        if (IsValid)
            _inst.RotateTo(rotation);
    }

    public void ScaleTo(Vector3 scale)
    {
        if (IsValid)
            _inst.ScaleTo(scale);
    }


}
