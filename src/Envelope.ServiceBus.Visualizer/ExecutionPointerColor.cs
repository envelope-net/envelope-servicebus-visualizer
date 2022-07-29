using Envelope.ServiceBus.Orchestrations.Execution;
using QuikGraph.Graphviz.Dot;

namespace Envelope.ServiceBus.Visualizer;

public class ExecutionPointerColor
{
	public IExecutionPointer ExecutionPointer { get; }
	public GraphvizColor Color { get; }
	public int Order { get; set; }

	public ExecutionPointerColor(IExecutionPointer executionPointer)
	{
		ExecutionPointer = executionPointer ?? throw new ArgumentNullException(nameof(executionPointer));

		Color = ExecutionPointer.Status switch
		{
			PointerStatus.Pending => GraphvizColor.Orange,
			PointerStatus.InProcess => GraphvizColor.Orange,
			PointerStatus.Completed => GraphvizColor.LightGreen,
			PointerStatus.Retrying => GraphvizColor.Red,
			PointerStatus.WaitingForEvent => GraphvizColor.Orange,
			PointerStatus.Suspended => GraphvizColor.Red,
			_ => GraphvizColor.Orange,
		};
	}

	public ExecutionPointerColor SetOrder(int order)
	{
		Order = order;
		return this;
	}

	public override string ToString()
		=> ExecutionPointer.ToString()!;
}
