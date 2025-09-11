namespace FlowManager.Client.Deserialization
{
    public class FormContent
    {
        public string Layout { get; set; } = "";
        public List<FormElement> Elements { get; set; } = new();
    }
}
