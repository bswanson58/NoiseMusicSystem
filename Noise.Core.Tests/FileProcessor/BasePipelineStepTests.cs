using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.Core.FileProcessor;

namespace Noise.Core.Tests.FileProcessor {
	[TestFixture]
	public class BasePipelineStepTests {
		internal class TestPipelineStep : BasePipelineStep {
			public TestPipelineStep() :
				base( ePipelineStep.Completed ) { }

			public override void ProcessStep( PipelineContext context ) { }
		}

		[Test]
		public void CanAppendNextStep() {
			var sut = new TestPipelineStep();
			var nextStep = new TestPipelineStep();
			var context = new PipelineContext( null, null, null, null, null );

			sut.AppendStep( nextStep );

			var result = sut.Process( context );

			result.Should().Be( nextStep );
		}

		[Test]
		public void AppendedStepIsLastInChain() {
			var sut = new TestPipelineStep();
			var step2 = new TestPipelineStep();
			var step3 = new TestPipelineStep();
			var context = new PipelineContext( null, null, null, null, null );

			sut.AppendStep( step2 );
			sut.AppendStep( step3 );

			var result = sut.Process( context );
			result = result.Process( context );

			result.Should().Be( step3 );
		}
	}
}
