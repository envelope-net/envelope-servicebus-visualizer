using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using System.Text;

namespace Envelope.ServiceBus.Visualizer;

public class OrchestrationGraphvizGenerator
{
	private readonly ClusteredAdjacencyGraph<Orchestrations.Graphing.Vertex, GraphEdge> _graph;
	private readonly Dictionary<Guid, List<ExecutionPointerColor>>? _executionPointerColors;

	public OrchestrationGraphvizGenerator(
		Orchestrations.Graphing.IOrchestrationGraph orchestrationGraph,
		IEnumerable<ExecutionPointerColor>? executionPointerColors = null)
	{
		if (orchestrationGraph == null)
			throw new ArgumentNullException(nameof(orchestrationGraph));

		if (executionPointerColors != null)
		{
			var epc = executionPointerColors.ToList();
			var i = 0;
			foreach (var executionPointer in epc)
			{
				executionPointer.Order = ++i;
			}

			_executionPointerColors = epc.GroupBy(x => x.ExecutionPointer.IdStep).ToDictionary(x => x.Key, x => x.ToList());
		}
		var graph = new AdjacencyGraph<Orchestrations.Graphing.Vertex, GraphEdge>();
		var clusteredGraph = new ClusteredAdjacencyGraph<Orchestrations.Graphing.Vertex, GraphEdge>(graph);
		_graph = CreateAdjacencyGraph(orchestrationGraph, clusteredGraph);
	}

	public string CreateDotGraph()
	{
		var algorithm = new GraphvizAlgorithm<Orchestrations.Graphing.Vertex, GraphEdge>(_graph);
		algorithm.FormatVertex += VertexStyler;
		algorithm.FormatEdge += EdgeStyler;
		algorithm.FormatCluster += ClusterStyler;
		return algorithm.Generate();
	}

	private void VertexStyler(object sender, FormatVertexEventArgs<Orchestrations.Graphing.Vertex> args)
	{
		var order = "";
		var fillColor = GraphvizColor.Transparent;
		if (_executionPointerColors?.TryGetValue(args.Vertex.Step.IdStep, out var executionPointerColors) == true)
		{
			fillColor = executionPointerColors.Last().Color;
			var sb = new StringBuilder();
			sb.Append('\n');

			var i = 0;
			foreach (var epOrder in executionPointerColors.Select(x => x.Order).OrderBy(x => x))
			{
				var mod = i % 3;

				if (mod == 0)
				{
					sb.Append($"#{epOrder}.");
				}
				else if (mod == 1)
				{
					sb.Append($" #{epOrder}.");
				}
				else
				{
					sb.Append($" #{epOrder}.");
					if (i <= executionPointerColors.Count - 1)
						sb.Append('\n');
				}

				i++;
			}
			order = sb.ToString();
		}

		args.VertexFormat.Label = $"{args.Vertex.Step.Name}{(args.Vertex.Step.BranchController != null ? $" ({args.Vertex.Step.BranchController.Name})" : "")}{(string.IsNullOrWhiteSpace(order) ? "" : order)}";

		args.VertexFormat.Style = GraphvizVertexStyle.Filled;

		args.VertexFormat.FillColor = fillColor;

		if (args.Vertex.VertextType == Orchestrations.Graphing.VertexType.Root)
		{
			args.VertexFormat.Shape = GraphvizVertexShape.Octagon;
			args.VertexFormat.FontColor = GraphvizColor.Green;
			args.VertexFormat.StrokeColor = GraphvizColor.Green;
		}
		else if (args.Vertex.VertextType == Orchestrations.Graphing.VertexType.Branch)
		{
			args.VertexFormat.Shape = GraphvizVertexShape.Ellipse;
			args.VertexFormat.FontColor = GraphvizColor.Blue;
			args.VertexFormat.StrokeColor = GraphvizColor.Blue;
		}
		else if (args.Vertex.VertextType == Orchestrations.Graphing.VertexType.BranchController)
		{
			args.VertexFormat.Shape = GraphvizVertexShape.Diamond;
			if (fillColor == GraphvizColor.LightGreen
				|| fillColor == GraphvizColor.Transparent)
			{
				args.VertexFormat.FontColor = GraphvizColor.Purple;
				args.VertexFormat.StrokeColor = GraphvizColor.Purple;
			}
			else
			{
				args.VertexFormat.FontColor = GraphvizColor.Black;
				args.VertexFormat.StrokeColor = GraphvizColor.Black;
			}
		}
		else if (args.Vertex.VertextType == Orchestrations.Graphing.VertexType.End)
		{
			args.VertexFormat.Shape = GraphvizVertexShape.House;
			args.VertexFormat.FontColor = GraphvizColor.Green;
			args.VertexFormat.StrokeColor = GraphvizColor.Green;
		}
		else //next
		{
			args.VertexFormat.Shape = GraphvizVertexShape.Rectangle;
			args.VertexFormat.FontColor = GraphvizColor.Black;
			args.VertexFormat.StrokeColor = GraphvizColor.Black;
		}
	}

	private static void EdgeStyler(object sender, FormatEdgeEventArgs<Orchestrations.Graphing.Vertex, GraphEdge> args)
	{
		args.EdgeFormat.Label.Value = $" {args.Edge.Title}";

		args.EdgeFormat.FontColor = GraphvizColor.Black;
		args.EdgeFormat.StrokeColor = GraphvizColor.Black;

		if (0 < args.Edge.Source.Step.Branches.Count
			&& args.Edge.Target.Step.IsStartingStep)
		{
			args.EdgeFormat.FontColor = GraphvizColor.Purple;
			args.EdgeFormat.StrokeColor = GraphvizColor.Purple;
		}
	}

	private void ClusterStyler(object sender, FormatClusterEventArgs<Orchestrations.Graphing.Vertex, GraphEdge> args)
	{
		//args.GraphFormat.Label = "Cluster Name";
		args.GraphFormat.BackgroundColor = GraphvizColor.WhiteSmoke;
	}

	private static ClusteredAdjacencyGraph<Orchestrations.Graphing.Vertex, GraphEdge> CreateAdjacencyGraph(
		Orchestrations.Graphing.IOrchestrationGraph orchestrationGraph,
		ClusteredAdjacencyGraph<Orchestrations.Graphing.Vertex, GraphEdge> clusteredGraph)
	{
		//clusteredGraph.AddVertexRange(orchestrationGraph.Vertices);
		clusteredGraph.AddVerticesAndEdgeRange(orchestrationGraph.Edges.Select(x => new GraphEdge(x)));

		foreach (var branch in orchestrationGraph.Branches)
		{
			var branchCluster = clusteredGraph.AddCluster();
			CreateAdjacencyGraph(branch, branchCluster);
		}

		return clusteredGraph;
	}
}
