public interface PlayerInfoData
{
    bool SaveData<T>(string RelativePath, T Data, bool Encrypted);

    T LoadData<T>(string RelativePath, bool Encrypted);
}