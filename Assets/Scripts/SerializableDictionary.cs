using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// !!! THIS CLASS IS NOT YET THREAD SAFE !!!
/// 
/// As the name suggests, this is just a regular C# Dictionary that can be serialized by the Unity Editor.
/// 
/// There may be race conditions when updating values in the editor, so please refrain from doing so! 
/// The main purpose of this class is to allow values to be seen in the editor, not modified.
/// </summary>
[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> _keys = null;
    [SerializeField] private List<TValue> _values = null;

    public SerializableDictionary() : base() {
        _keys = new List<TKey>();
        _values = new List<TValue>();
    }

    public void OnBeforeSerialize()
    {
        _keys.Clear();
        _values.Clear();

        _keys = Keys.ToList<TKey>();
        _values = Values.ToList<TValue>();
    }

    public void OnAfterDeserialize()
    {
        if (_keys.Count != _values.Count)
            throw new System.Exception(string.Format("SerializableDictionary.OnAfterDeserialize() error: Incompatible key {0} and value {1} counts!", 
                                        _keys.Count, _values.Count));

        this.Clear();
        for (int i = 0; i < _keys.Count; i++)
            this.Add(_keys[i], _values[i]);
    }
}
