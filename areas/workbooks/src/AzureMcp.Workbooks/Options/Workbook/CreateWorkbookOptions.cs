// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Core.Options;

namespace AzureMcp.Workbooks.Options.Workbook;

public class CreateWorkbookOptions : SubscriptionOptions
{
    [JsonPropertyName(WorkbooksOptionDefinitions.DisplayNameText)]
    public string? DisplayName { get; set; }

    [JsonPropertyName(WorkbooksOptionDefinitions.SerializedContentText)]
    public string? SerializedContent { get; set; }

    [JsonPropertyName(WorkbooksOptionDefinitions.SourceIdText)]
    public string? SourceId { get; set; }
}
