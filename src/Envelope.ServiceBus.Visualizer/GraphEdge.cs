using QuikGraph;

namespace Envelope.ServiceBus.Visualizer;

public class GraphEdge : IEdge<Orchestrations.Graphing.Vertex>
{
	private readonly Orchestrations.Graphing.Edge _edge;

	public Orchestrations.Graphing.Vertex Source => _edge.From;

	public Orchestrations.Graphing.Vertex Target => _edge.To;

	public string Title => _edge.Title;

	public GraphEdge(Orchestrations.Graphing.Edge edge)
	{
		_edge = edge ?? throw new ArgumentNullException(nameof(edge));
	}

	public override string ToString()
		=> $"{Source.Step.Name} -> {Target.Step.Name}";
}
