// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Commands;
using AzureMcp.Core.Models.Option;
using AzureMcp.LoadTesting.Models.LoadTestRun;
using AzureMcp.LoadTesting.Options.LoadTestRun;
using AzureMcp.LoadTesting.Services;
using Microsoft.Extensions.Logging;

namespace AzureMcp.LoadTesting.Commands.LoadTestRun;
public sealed class TestRunListCommand(ILogger<TestRunListCommand> logger)
    : BaseLoadTestingCommand<TestRunListOptions>
{
    private const string _commandTitle = "Test Run List";
    private readonly ILogger<TestRunListCommand> _logger = logger;
    private readonly Option<string> _loadTestIdOption = OptionDefinitions.LoadTesting.Test;
    public override string Name => "list";
    public override string Description =>
        $"""
        Retrieves a comprehensive list of all test run executions for a specific load test configuration. 
        This command provides an overview of test execution history, allowing you to track performance 
        trends, compare results across multiple runs, and analyze testing patterns over time.
        """;
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_loadTestIdOption);
    }

    protected override TestRunListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestId = parseResult.GetValueForOption(_loadTestIdOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            // Required validation step using the base Validate method
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }
            // Get the appropriate service from DI
            var service = context.GetService<ILoadTestingService>();
            // Call service operation(s)
            var results = await service.GetLoadTestRunsFromTestIdAsync(
                options.Subscription!,
                options.TestResourceName!,
                options.TestId!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy);
            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new TestRunListCommandResult(results), LoadTestJsonContext.Default.TestRunListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            // Log error with context information
            _logger.LogError(ex, "Error in {Operation}. Options: {Options}", Name, options);
            // Let base class handle standard error processing
            HandleException(context, ex);
        }
        return context.Response;
    }
    internal record TestRunListCommandResult(List<TestRun> TestRun);
}
