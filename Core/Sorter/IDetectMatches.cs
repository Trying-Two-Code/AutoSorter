namespace Core.Sorter
{
    interface IDetectMatches
    {
        public List<string> MatchingFiles { get; set; }
        public bool IsMatch(FileInfo fileInfo);
    }
}
