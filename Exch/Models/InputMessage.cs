namespace Exch.Controllers
{
    public class InputMessage
    {
        public int channel { get; set; }
        public string text { get; set; }
        public InputMessage(int channel, string text)
        {
            this.channel = channel;
            this.text = text;
        }
    }
}