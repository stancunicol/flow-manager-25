namespace FlowManager.Client.ViewModels.GraphView
{
    public class GraphConnection
    {
        public string Id { get; set; } = default!;
        public double FromX { get; set; }
        public double FromY { get; set; }
        public double ToX { get; set; }
        public double ToY { get; set; }
    }
}
