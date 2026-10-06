using System.Diagnostics;

namespace Core.Sorter
{
    public class PromptUser
    {
        //Use with BeginInvoke(DispatcherPrioity, Delegate)
        //Priority = DispatcherPrioirity.Normal, and Delagate = callback (new NextPrimeDelagate(CheckNextNumber));

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

            Debug.WriteLine("result is: " + result);

            return result;
        }
    }
}
