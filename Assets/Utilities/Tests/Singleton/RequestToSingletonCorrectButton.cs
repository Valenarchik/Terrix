namespace CustomUtilities.Tests
{
    public class RequestToSingletonCorrectButton: ButtonClickHandler
    {
        protected override void OnClick()
        {
            var singleton = SingletonCorrect.Instance;
        }
    }
}