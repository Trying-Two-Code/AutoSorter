using System.Diagnostics;

namespace Core.Sorter
{
    /// <summary>
    /// Sends prompts to the user via a UITask.
    /// </summary>
    public class PromptUser
    {
        public UITask UIEvent;

        public PromptUser(UITask _uIEvent)
        {
            UIEvent = _uIEvent;
        }

        public bool Prompt()
        {
            Task<bool> uiTask = UIEvent(this, new());
            Task<bool> uiResult = Task<bool>.Factory.StartNew(() =>
            {
                return uiTask.Result;
            });
            bool result = uiResult.Result;

            return result;
        }
        public static bool PromptUserMethod(PromptUser? _promptUserInst, EventArgs args)
        {
            try
            {
                Task promptUserTask = _promptUserInst.UIEvent.Invoke(null, args);
                promptUserTask.Wait();

                if (promptUserTask.IsCompletedSuccessfully)
                {
                    return true;
                }
            }
            catch
            {
                Debug.WriteLine("Prompt User Failed.");
            }
            return false;
        }
    }
}
