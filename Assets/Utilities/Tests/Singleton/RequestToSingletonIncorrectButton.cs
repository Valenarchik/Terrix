namespace CustomUtilities.Tests
{
    public class RequestToSingletonIncorrectButton: ButtonClickHandler
    {
        protected override void OnClick()
        {
            var singleton = SingletonIncorrect.Instance;
        }
    }
}