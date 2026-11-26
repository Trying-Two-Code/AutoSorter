using Core.Sorter.Creator;

namespace Core.PromptUser
{
    public class PromptResponse
    {
        public PromptResponse(Code? _code) 
        {
            MyCode = _code;
        }

        public Code? MyCode { get; set; }
        public List<InfoWindow> OpenedInfoWindows { get; set; }
        public List<FileMoveInfo> UndoMoves { get; set; } = new();

        public enum Code
        {
            UserOk,
            UserNo,
            UserUndo,
            DefaultAgree,
            Closed,
            Timeout,
            NeverPrompted
        }

        public static PromptResponse ResponseOk { 
            get { return new(Code.UserOk); } }
        public static PromptResponse ResponseNo { 
            get { return new(Code.UserNo); } }
        public static PromptResponse ResponseUndo { 
            get { return new(Code.UserUndo); } }
        public static PromptResponse ResponseDefault { 
            get { return new(Code.DefaultAgree); } }
        public static PromptResponse ResponseClosed { 
            get { return new(Code.Closed); } }
        public static PromptResponse ResponseTimeout {  
            get { return new(Code.Timeout); } }
        public static PromptResponse NeverPrompted { 
            get { return new(Code.NeverPrompted); } }

        public Mark? MarkedAs { get; set; } = null;

        public enum Mark { 
            Spam,
            Muted,
            Blocked,
            Unreadable
        }

        public static PromptResponse SpamPrompt
        {
            get { return new(Code.NeverPrompted) { MarkedAs = Mark.Spam }; }
        }
        public static PromptResponse MutedPrompt
        {
            get { return new(Code.NeverPrompted) { MarkedAs = Mark.Muted }; }
        }
        public static PromptResponse BlockedPrompt
        {
            get { return new(Code.NeverPrompted) { MarkedAs = Mark.Blocked }; }
        }
        public static PromptResponse UnreadablePrompt
        {
            get { return new(Code.NeverPrompted) { MarkedAs = Mark.Unreadable }; }
        }


        public enum InfoWindow
        {
            Settings,
            History,
            OtherRules,
            ThisRule
        }
    }
}
