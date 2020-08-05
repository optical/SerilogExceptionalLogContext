using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.ExceptionalLogContext.Tests {
	public class TestSink : ILogEventSink {
		public List<LogEvent> Events { get; } = new List<LogEvent>();
		public void Emit(LogEvent logEvent) {
			Events.Add(logEvent);
		}
	}
}