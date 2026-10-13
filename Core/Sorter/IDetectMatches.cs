namespace Core.Sorter
{
    interface IDetectMatches
    {
        public List<FileInfo> MatchingFiles { get; set; }
        public bool IsMatch(FileInfo fileInfo);
    }
}
