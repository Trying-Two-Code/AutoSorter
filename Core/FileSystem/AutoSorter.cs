using Helper.FileSystem;

namespace Core.FileSystem;

public class AutoSorter
{
    private readonly FileSystemManager _fileSystem;

    public AutoSorter(string path)
    {
        _fileSystem = new FileSystemManager(path);

        SetCallbacks();
    }

    private void SetCallbacks()
    {
        _fileSystem.FileChanged += OnFileChanged;
    }

    private void OnFileChanged(FileChange change)
    {
        switch (change.Type)
        {
            case FileChangeType.Created:
                OnFileCreated(change.Path);
                break;

            case FileChangeType.Modified:
                OnFileModified(change.Path);
                break;

            case FileChangeType.Deleted:
                OnFileDeleted(change.Path);
                break;

            case FileChangeType.Renamed:
                OnFileRenamed(change.Path);
                break;
        }
    }

    private void OnFileCreated(string path)
    {
        // Data Gather

        // Algorithm
    }

    private void OnFileModified(string path)
    {
        // Data Gather

        // Algorithm
    }

    private void OnFileDeleted(string path)
    {
        // Data Gather

        // Algorithm
    }

    private void OnFileRenamed(string path)
    {
        // Data Gather

        // Algorithm
    }

    //Idea: PsuedoCode for a datagathering function
    //What file should handle this?
    /// <DataGatherFunctionSummary>
    /// 
    /// struct fileStruct{
    ///     //data needed for files
    /// }
    /// 
    /// public const string dataFilePath = "\data\userData.json"
    /// 
    /// public fileStructInstance gatherData(string path){
    ///     //find file using path
    ///     //gather data using file
    ///     //gather existing data from getFileDataFromJSON(dataFilePath)[file.name]
    ///     //return data
    /// }
    /// 
    /// public file getFileDataFromJSON(string JSONPath){
    ///     //return file data from the JSON file
    /// }
    /// 
    /// </DataGatherFunctionSummary>

    public void Start()
    {
        _fileSystem.Start();
    }

    public void Stop()
    {
        _fileSystem.Stop();
    }
}