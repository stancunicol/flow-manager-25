namespace FlowManager.Client.ViewModels.GraphView
{
    public class GraphNode
    {
        public double X { get; set; }
        public double Y { get; set; }
        public FlowStepItemVM FlowStepItem { get; set; } = default!;
        public int LevelIndex { get; set; }
        public int NodeIndex { get; set; }
    }
}
