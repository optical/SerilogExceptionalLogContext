using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.ExceptionalLogContext.Tests {
	[TestFixture]
	public class ExceptionContextEnricherTests {
		private TestSink _logEventSink;
		private Logger _logger;

		[SetUp]
		public void Setup() {

			_logEventSink = new TestSink();
			_logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.Enrich.WithExceptionalLogContext()
				.WriteTo.Sink(_logEventSink)
				.CreateLogger();
		}

		[Test]
		public void TestExceptionNonAsync() {
			try {
				using (LogContext.PushProperty("Key", "Value")) {
					throw new Exception("Explosions, ahh");
				}
			} catch (Exception exception) {
				_logger.Error(exception, "Something bad happened");
			}

			Assert.That(_logEventSink.Events.Count, Is.EqualTo(1));
			var logEvent = _logEventSink.Events.Single();
			Assert.That(logEvent.Properties, Contains.Key("Key"));
			var logEventPropertyValue = (ScalarValue)logEvent.Properties["Key"];
			Assert.That(logEventPropertyValue.Value, Is.EqualTo("Value"));
		}

		[Test]
		public async Task TestExceptionAsync() {
			const string exceptionKey = "Key";
			try {
				using (LogContext.PushProperty(exceptionKey, "Value")) {
					await Task.Yield();
					throw new Exception("Explosions, ahh");
				}
			} catch (Exception exception) {
				_logger.Error(exception, "Something bad happened");
			}

			Assert.That(_logEventSink.Events.Count, Is.EqualTo(1));
			var logEvent = _logEventSink.Events.Single();
			Assert.That(logEvent.Properties, Contains.Key(exceptionKey));
			var logEventPropertyValue = (ScalarValue)logEvent.Properties[exceptionKey];
			Assert.That(logEventPropertyValue.Value, Is.EqualTo("Value"));
		}

		[Test]
		public async Task TestNestedThrowsAreOk() {
			const string exceptionKey = "Key";
			try {
				try {
					using (LogContext.PushProperty(exceptionKey, "Value")) {
						await Task.Yield();
						throw new Exception("Explosions, ahh");
					}
				} catch (Exception exception) {
					_logger.Error(exception, "First line of defense");
					throw;
				}
			} catch (Exception exception) {
				_logger.Error(exception, "Last line of defense");
			}

			Assert.That(_logEventSink.Events.Count, Is.EqualTo(2));
			
			var firstEvent = _logEventSink.Events[0];
			Assert.That(firstEvent.Properties, Contains.Key(exceptionKey));
			var firstEventPropertyValue = (ScalarValue)firstEvent.Properties[exceptionKey];
			Assert.That(firstEventPropertyValue.Value, Is.EqualTo("Value"));

			var secondEvent = _logEventSink.Events[0];
			Assert.That(secondEvent.Properties, Contains.Key(exceptionKey));
			var secondEventPropertyValue = (ScalarValue)secondEvent.Properties[exceptionKey];
			Assert.That(secondEventPropertyValue.Value, Is.EqualTo("Value"));
		}
	}
}
