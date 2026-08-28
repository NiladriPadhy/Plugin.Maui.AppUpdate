namespace Plugin.Maui.AppUpdate;

sealed class FilePostponeStore
{
    readonly string _path;
    readonly object _gate = new();

    public FilePostponeStore(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        _path = Path.Combine(directory, AppUpdateDefaults.PostponeFileName);
    }

    public PostponeState? Load()
    {
        lock (_gate)
        {
            if (!File.Exists(_path))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(_path);
                return JsonSerializer.Deserialize(json, AppUpdateJsonContext.Default.PostponeState);
            }
            catch
            {
                return null;
            }
        }
    }

    public void Save(PostponeState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        lock (_gate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var json = JsonSerializer.Serialize(state, AppUpdateJsonContext.Default.PostponeState);
            var temp = _path + ".tmp";
            File.WriteAllText(temp, json);
            File.Copy(temp, _path, overwrite: true);
            File.Delete(temp);
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }
    }
}
