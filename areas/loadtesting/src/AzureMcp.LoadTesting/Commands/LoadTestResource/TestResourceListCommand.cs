// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Commands;
using AzureMcp.LoadTesting.Models.LoadTestResource;
using AzureMcp.LoadTesting.Options.LoadTestResource;
using AzureMcp.LoadTesting.Services;
using Microsoft.Extensions.Logging;

namespace AzureMcp.LoadTesting.Commands.LoadTestResource;
public sealed class TestResourceListCommand(ILogger<TestResourceListCommand> logger)
    : BaseLoadTestingCommand<TestResourceListOptions>
{
    private const string _commandTitle = "Test Resource List";
    private readonly ILogger<TestResourceListCommand> _logger = logger;
    public override string Name => "list";
    public override string Description =>
        $"""
        Fetches the Load Testing resources for the current selected subscription, resource group in the logged in tenant.
        Returns a list of Load Testing resources.
        """;
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

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
            var results = await service.GetLoadTestResourcesAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.TestResourceName,
                options.Tenant,
                options.RetryPolicy);
            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new TestResourceListCommandResult(results), LoadTestJsonContext.Default.TestResourceListCommandResult) :
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
    internal record TestResourceListCommandResult(List<TestResource> LoadTest);
}
