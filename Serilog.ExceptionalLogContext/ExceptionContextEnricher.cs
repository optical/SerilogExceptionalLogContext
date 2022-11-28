using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.ExceptionalLogContext {
	public class ExceptionContextEnricher : ILogEventEnricher {
		private readonly ConditionalWeakTable<Exception, ILogEventEnricher> _exceptionToContextLookup;

		public ExceptionContextEnricher() {
			_exceptionToContextLookup = new ConditionalWeakTable<Exception, ILogEventEnricher>();
			AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
		}

		private void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs exceptionEvent) {
			try {
				if (!_exceptionToContextLookup.TryGetValue(exceptionEvent.Exception, out var _))
				{
					var context = LogContext.Clone();
					// Call Add, not AddOrUpdate. If an exception is logged twice, the context from the original callsite will be preserved.
					// This is desireable as subsequent throws/logs of the exception will unwind the stack, removing the context we're trying to preserve
					// Note: this can throw, which absoutely necessitates the try/catch
					_exceptionToContextLookup.Add(exceptionEvent.Exception, context);
				}
			} catch {
				// Any exceptions raised in here cannot be propagated, or the whole application will be taken down
			}
		}

		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
			if (logEvent.Exception != null && _exceptionToContextLookup.TryGetValue(logEvent.Exception, out ILogEventEnricher capturedContext)) {
				capturedContext.Enrich(logEvent, propertyFactory);
			}
		}
	}
}
