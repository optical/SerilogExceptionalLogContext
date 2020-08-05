using Serilog.Configuration;

namespace Serilog.ExceptionalLogContext {
	public static class ExtensionMethods {
		public static LoggerConfiguration WithExceptionalLogContext(this LoggerEnrichmentConfiguration enrichmentConfiguration) {
			return enrichmentConfiguration.With(new ExceptionContextEnricher());
		}
	}
}
