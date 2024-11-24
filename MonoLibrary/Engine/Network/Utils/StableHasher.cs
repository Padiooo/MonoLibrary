namespace MonoLibrary.Engine.Network.Utils;

public static class StableHasher
{
    const int string_hash_offset = 13337;

    public static int GetStableHash(this string s)
    {
        int hash = string_hash_offset;
        s = s.ToLowerInvariant();
        unchecked
        {
            for (int i = 0; i < s.Length; i++)
                hash *= s[i] - string_hash_offset;
        }

        return hash;
    }
}
