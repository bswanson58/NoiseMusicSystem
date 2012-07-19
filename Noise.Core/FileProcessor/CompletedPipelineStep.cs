namespace Noise.Core.FileProcessor {
	internal class CompletedPipelineStep : BasePipelineStep {
		public CompletedPipelineStep() :
			base( ePipelineStep.Completed) { }

		public override void ProcessStep( PipelineContext context ) {
			context.Trigger = ePipelineTrigger.Completed;
		}
	}
}
