# Serilog.ExceptionalLogContext
An enricher for Serilog which will enhance your exceptions logged with their LogContext at the time they were thrown. If you use LogContext to enrich your logs with additional ambient details, but are frustrated that your low level exception logs lack this context, this enricher is for you.

This solves the issue described in [serilog/#984](https://github.com/serilog/serilog/issues/895). There is no need to use an exception filter and it also works for async/await too, all without changing any of your existing exception handling code

The problem described in code:
```
try {
	LogContext.Push("InstanceId", Guid.NewGuid()) {
		await RunMyapplication();
	}
} catch (Exception exception) {
	// Without Serilog.ExceptionalLogContext this log line won't have any of our LogContext properties as the stack has been unwound already, disposing of them :(
	Log.Error(exception, "Oh no, something bad happened");
}
```

With Serilog.ExceptionalLogContext your log LogContext state is cloned immediately when an exception is thrown, so that when your application eventually gets around to logging it, you'll still get to keep all that key information.

## Usage
It's as simple as adding `.Enrich.WithExceptionalLogContext()` and you're done!

```
var logger = new LoggerConfiguration()
	.Enrich.WithExceptionalLogContext()
	.CreateLogger();
```

## Acquistion
Download from Nuget