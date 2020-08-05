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
			var context = LogContext.Clone();
			_exceptionToContextLookup.AddOrUpdate(exceptionEvent.Exception, context);
		}

		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
			if (logEvent.Exception != null && _exceptionToContextLookup.TryGetValue(logEvent.Exception, out ILogEventEnricher capturedContext)) {
				capturedContext.Enrich(logEvent, propertyFactory);
			}
		}
	}
}
