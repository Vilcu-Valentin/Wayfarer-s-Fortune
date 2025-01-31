/// <summary>
/// This class exists to encapsulate structs in containers in a way that allows member-wise modification of the structs,
/// which is in many (if not all) cases not allowed in C# for no apparent reason other than its insistence on 
/// being annoying and inferior to C++.
/// 
/// 'Why not just make the structs classes then?'
/// Because there would be a ton of tiny files with long weird names whose (class endowed) functionality will only be used in 1 or 2 other big scripts.
/// 
/// Idfk if this is bad practice or whatever, but if it is please tell me despite my visible antagonism towards non-C/C++ languages.
/// </summary>
/// <typeparam name="T"></typeparam>

[System.Serializable]
public class MutableHolder<T>
{
    public T value;
    public MutableHolder(T value) {
        this.value = value;
    }
}
