// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.Core.Commands;
using AzureMcp.Core.Services.Telemetry;
using AzureMcp.Kusto.Commands;
using AzureMcp.Kusto.Options;
using AzureMcp.Kusto.Services;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Kusto.Commands;

public sealed class TableListCommand(ILogger<TableListCommand> logger) : BaseDatabaseCommand<TableListOptions>
{
    private const string CommandTitle = "List Kusto Tables";
    private readonly ILogger<TableListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        """
        List all tables in a specific Kusto database.
        Required `cluster-uri` (or `subscription` and `cluster-name`) and `database-name`.
        Returns table names as a JSON array.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            context.Activity?.WithSubscriptionTag(options);

            var kusto = context.GetService<IKustoService>();
            List<string> tableNames = [];

            if (UseClusterUri(options))
            {
                tableNames = await kusto.ListTables(
                    options.ClusterUri!,
                    options.Database!,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy);
            }
            else
            {
                tableNames = await kusto.ListTables(
                    options.Subscription!,
                    options.ClusterName!,
                    options.Database!,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy);
            }

            context.Response.Results = tableNames?.Count > 0 ?
                ResponseResult.Create(new TableListCommandResult(tableNames), KustoJsonContext.Default.TableListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing tables. Cluster: {Cluster}, Database: {Database}.", options.ClusterUri ?? options.ClusterName, options.Database);
            HandleException(context, ex);
        }
        return context.Response;
    }

    public record TableListCommandResult(List<string> Tables);
}
