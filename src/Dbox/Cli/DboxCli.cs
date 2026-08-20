using System.CommandLine;
using Dbox.Activities;
using Dbox.Database;
using Dbox.Output;

namespace Dbox.Cli;

public static class DboxCli
{
    public static async Task<int> InvokeAsync(
        IReadOnlyList<string> args,
        TextWriter output,
        TextWriter error,
        string currentDirectory,
        CancellationToken cancellationToken = default)
    {
        var normalizedArgs = NormalizeSchemaAlias(args);
        var writer = new OutputWriter(output, error);
        var root = BuildRootCommand(writer, () => currentDirectory);
        var parseResult = root.Parse(normalizedArgs);

        if (parseResult.Errors.Count > 0)
        {
            var details = parseResult.Errors
                .Select(_ => new ErrorDetail("command", "Invalid command syntax."))
                .ToArray();
            var format = DetectErrorFormat(normalizedArgs);
            writer.WriteError(
                new CliError("validation_error", "Invalid command.", ExitCodes.ValidationError, details),
                format);
            return ExitCodes.ValidationError;
        }

        var configuration = new InvocationConfiguration
        {
            Output = output,
            Error = error,
            EnableDefaultExceptionHandler = false
        };

        return await parseResult.InvokeAsync(configuration, cancellationToken);
    }

    public static RootCommand BuildRootCommand(OutputWriter writer, Func<string> currentDirectoryProvider)
    {
        var runtime = new CommandRuntime(writer, currentDirectoryProvider);
        var outputOption = new Option<string?>("--output")
        {
            Description = "Output format: text or json.",
            DefaultValueFactory = _ => "text",
            Recursive = true
        };

        var root = new RootCommand("Local project activity database CLI.")
        {
            TreatUnmatchedTokensAsErrors = true
        };
        root.Options.Add(outputOption);
        root.SetAction((parseResult, cancellationToken) => runtime.RunRootAsync(parseResult, outputOption, cancellationToken));

        var init = new Command("init", "Initialize the database in the current directory.");
        init.SetAction((parseResult, cancellationToken) => runtime.RunInitAsync(parseResult, outputOption, cancellationToken));
        root.Add(init);

        var schema = new Command("schema", "Show the public activity contract.");
        var schemaJsonOption = new Option<bool>("--json") { Description = "Render the schema as JSON." };
        schema.Options.Add(schemaJsonOption);
        schema.SetAction((parseResult, cancellationToken) => runtime.RunSchemaAsync(parseResult, outputOption, schemaJsonOption, cancellationToken));
        root.Add(schema);

        var add = new Command("add", "Create an activity.");
        var addTypeOption = StringOption("--type", "Activity type.");
        var addTitleOption = StringOption("--title", "Activity title.");
        var addDescriptionOption = StringOption("--description", "Optional activity description.");
        var addStatusOption = StringOption("--status", "Activity status.");
        var addJsonOption = StringOption("--json", "Activity input as a JSON object.");
        add.Options.Add(addTypeOption);
        add.Options.Add(addTitleOption);
        add.Options.Add(addDescriptionOption);
        add.Options.Add(addStatusOption);
        add.Options.Add(addJsonOption);
        add.SetAction((parseResult, cancellationToken) => runtime.RunAddAsync(
            parseResult,
            outputOption,
            addTypeOption,
            addTitleOption,
            addDescriptionOption,
            addStatusOption,
            addJsonOption,
            cancellationToken));
        root.Add(add);

        var list = new Command("list", "List activities.");
        var listTypeOption = StringOption("--type", "Filter by activity type.");
        var listStatusOption = StringOption("--status", "Filter by activity status.");
        list.Options.Add(listTypeOption);
        list.Options.Add(listStatusOption);
        list.SetAction((parseResult, cancellationToken) => runtime.RunListAsync(
            parseResult,
            outputOption,
            listTypeOption,
            listStatusOption,
            cancellationToken));
        root.Add(list);

        var get = new Command("get", "Get one activity.");
        var getIdArgument = new Argument<long>("id") { Description = "Activity id." };
        get.Arguments.Add(getIdArgument);
        get.SetAction((parseResult, cancellationToken) => runtime.RunGetAsync(
            parseResult,
            outputOption,
            getIdArgument,
            cancellationToken));
        root.Add(get);

        var update = new Command("update", "Update an activity.");
        var updateIdArgument = new Argument<long>("id") { Description = "Activity id." };
        var updateTypeOption = StringOption("--type", "New activity type.");
        var updateTitleOption = StringOption("--title", "New activity title.");
        var updateDescriptionOption = StringOption("--description", "New activity description.");
        var updateStatusOption = StringOption("--status", "New activity status.");
        var updateJsonOption = StringOption("--json", "Activity update as a JSON object.");
        update.Arguments.Add(updateIdArgument);
        update.Options.Add(updateTypeOption);
        update.Options.Add(updateTitleOption);
        update.Options.Add(updateDescriptionOption);
        update.Options.Add(updateStatusOption);
        update.Options.Add(updateJsonOption);
        update.SetAction((parseResult, cancellationToken) => runtime.RunUpdateAsync(
            parseResult,
            outputOption,
            updateIdArgument,
            updateTypeOption,
            updateTitleOption,
            updateDescriptionOption,
            updateStatusOption,
            updateJsonOption,
            cancellationToken));
        root.Add(update);

        var delete = new Command("delete", "Delete an activity.");
        var deleteIdArgument = new Argument<long>("id") { Description = "Activity id." };
        delete.Arguments.Add(deleteIdArgument);
        delete.SetAction((parseResult, cancellationToken) => runtime.RunDeleteAsync(
            parseResult,
            outputOption,
            deleteIdArgument,
            cancellationToken));
        root.Add(delete);

        return root;
    }

    public static string[] NormalizeSchemaAlias(IReadOnlyList<string> args)
    {
        var schemaIndex = -1;
        for (var index = 0; index < args.Count; index++)
        {
            if (string.Equals(args[index], "--schema", StringComparison.Ordinal))
            {
                schemaIndex = index;
                break;
            }
        }

        if (schemaIndex < 0)
        {
            return args.ToArray();
        }

        return [.. args.Take(schemaIndex), "schema", .. args.Skip(schemaIndex + 1)];
    }

    private static Option<string?> StringOption(string name, string description) =>
        new(name) { Description = description };

    private static OutputFormat DetectErrorFormat(IReadOnlyList<string> args)
    {
        for (var index = 0; index < args.Count; index++)
        {
            if (args[index] == "--output" &&
                index + 1 < args.Count &&
                string.Equals(args[index + 1], "json", StringComparison.OrdinalIgnoreCase))
            {
                return OutputFormat.Json;
            }

            if (args[index].StartsWith("--output=", StringComparison.Ordinal) &&
                string.Equals(args[index]["--output=".Length..], "json", StringComparison.OrdinalIgnoreCase))
            {
                return OutputFormat.Json;
            }
        }

        if (args.Count > 0 && args[0] == "schema" && args.Contains("--json", StringComparer.Ordinal))
        {
            return OutputFormat.Json;
        }

        return OutputFormat.Text;
    }

    private sealed class CommandRuntime
    {
        private readonly OutputWriter writer;
        private readonly Func<string> currentDirectoryProvider;
        private readonly DboxLocator locator;
        private readonly DboxDbContextFactory contextFactory;
        private readonly ActivityRepository repository;
        private readonly DboxDatabase database;

        public CommandRuntime(OutputWriter writer, Func<string> currentDirectoryProvider)
        {
            this.writer = writer;
            this.currentDirectoryProvider = currentDirectoryProvider;
            locator = new DboxLocator();
            contextFactory = new DboxDbContextFactory();
            repository = new ActivityRepository();
            database = new DboxDatabase(locator, contextFactory);
        }

        public Task<int> RunRootAsync(ParseResult parseResult, Option<string?> outputOption, CancellationToken cancellationToken)
        {
            return RunAsync(
                parseResult,
                outputOption,
                forceJson: false,
                (_, _) => Task.FromResult<object?>(null),
                cancellationToken,
                rootCommand: true);
        }

        public Task<int> RunInitAsync(ParseResult parseResult, Option<string?> outputOption, CancellationToken cancellationToken)
        {
            return RunAsync(
                parseResult,
                outputOption,
                forceJson: false,
                async (_, token) => (object?)await database.InitializeAsync(currentDirectoryProvider(), token),
                cancellationToken);
        }

        public Task<int> RunSchemaAsync(
            ParseResult parseResult,
            Option<string?> outputOption,
            Option<bool> jsonOption,
            CancellationToken cancellationToken)
        {
            var forceJson = parseResult.GetValue(jsonOption);
            return RunAsync(
                parseResult,
                outputOption,
                forceJson,
                (_, token) => database.ExecuteAsync(
                    currentDirectoryProvider(),
                    (_, _) => Task.FromResult<object?>(ActivitySchema.CreateDocument()),
                    token),
                cancellationToken);
        }

        public Task<int> RunAddAsync(
            ParseResult parseResult,
            Option<string?> outputOption,
            Option<string?> typeOption,
            Option<string?> titleOption,
            Option<string?> descriptionOption,
            Option<string?> statusOption,
            Option<string?> jsonOption,
            CancellationToken cancellationToken)
        {
            return RunAsync(
                parseResult,
                outputOption,
                forceJson: false,
                (_, token) => AddAsync(parseResult, typeOption, titleOption, descriptionOption, statusOption, jsonOption, token),
                cancellationToken);
        }

        public Task<int> RunListAsync(
            ParseResult parseResult,
            Option<string?> outputOption,
            Option<string?> typeOption,
            Option<string?> statusOption,
            CancellationToken cancellationToken)
        {
            return RunAsync(
                parseResult,
                outputOption,
                forceJson: false,
                (_, token) => ListAsync(parseResult, typeOption, statusOption, token),
                cancellationToken);
        }

        public Task<int> RunGetAsync(
            ParseResult parseResult,
            Option<string?> outputOption,
            Argument<long> idArgument,
            CancellationToken cancellationToken)
        {
            return RunAsync(
                parseResult,
                outputOption,
                forceJson: false,
                (_, token) => GetAsync(parseResult.GetValue(idArgument), token),
                cancellationToken);
        }

        public Task<int> RunUpdateAsync(
            ParseResult parseResult,
            Option<string?> outputOption,
            Argument<long> idArgument,
            Option<string?> typeOption,
            Option<string?> titleOption,
            Option<string?> descriptionOption,
            Option<string?> statusOption,
            Option<string?> jsonOption,
            CancellationToken cancellationToken)
        {
            return RunAsync(
                parseResult,
                outputOption,
                forceJson: false,
                (_, token) => UpdateAsync(
                    parseResult,
                    idArgument,
                    typeOption,
                    titleOption,
                    descriptionOption,
                    statusOption,
                    jsonOption,
                    token),
                cancellationToken);
        }

        public Task<int> RunDeleteAsync(
            ParseResult parseResult,
            Option<string?> outputOption,
            Argument<long> idArgument,
            CancellationToken cancellationToken)
        {
            return RunAsync(
                parseResult,
                outputOption,
                forceJson: false,
                (_, token) => DeleteAsync(parseResult.GetValue(idArgument), token),
                cancellationToken);
        }

        private async Task<object?> AddAsync(
            ParseResult parseResult,
            Option<string?> typeOption,
            Option<string?> titleOption,
            Option<string?> descriptionOption,
            Option<string?> statusOption,
            Option<string?> jsonOption,
            CancellationToken cancellationToken)
        {
            var typeProvided = parseResult.GetResult(typeOption) is not null;
            var titleProvided = parseResult.GetResult(titleOption) is not null;
            var descriptionProvided = parseResult.GetResult(descriptionOption) is not null;
            var statusProvided = parseResult.GetResult(statusOption) is not null;
            var jsonProvided = parseResult.GetResult(jsonOption) is not null;
            var input = ActivityInputParser.ParseCreate(
                jsonProvided,
                parseResult.GetValue(jsonOption),
                typeProvided,
                parseResult.GetValue(typeOption),
                titleProvided,
                parseResult.GetValue(titleOption),
                descriptionProvided,
                parseResult.GetValue(descriptionOption),
                statusProvided,
                parseResult.GetValue(statusOption));

            ThrowIfInvalid(input.Issues);
            var validation = ActivityValidator.ValidateCreate(input.Value!);
            ThrowIfInvalid(validation.Issues);

            var activity = new Activity
            {
                Type = input.Value!.Type!,
                Title = input.Value.Title!,
                Description = input.Value.Description,
                Status = input.Value.Status ?? ActivitySchema.DefaultStatus,
                CreatedAt = DateTime.UtcNow
            };

            return await database.ExecuteAsync(
                currentDirectoryProvider(),
                async (context, token) => ActivityView.FromEntity(await repository.AddAsync(context, activity, token)),
                cancellationToken);
        }

        private async Task<object?> ListAsync(
            ParseResult parseResult,
            Option<string?> typeOption,
            Option<string?> statusOption,
            CancellationToken cancellationToken)
        {
            var type = parseResult.GetValue(typeOption);
            var status = parseResult.GetValue(statusOption);
            var issues = new List<ValidationIssue>();
            if (type is not null && !ActivitySchema.IsType(type))
            {
                issues.Add(new ValidationIssue("type", $"Value must be one of: {string.Join(", ", ActivitySchema.Types)}."));
            }

            if (status is not null && !ActivitySchema.IsStatus(status))
            {
                issues.Add(new ValidationIssue("status", $"Value must be one of: {string.Join(", ", ActivitySchema.Statuses)}."));
            }

            ThrowIfInvalid(issues);
            var activities = await database.ExecuteAsync(
                currentDirectoryProvider(),
                (context, token) => repository.ListAsync(context, type, status, token),
                cancellationToken);
            return activities.Select(ActivityView.FromEntity).ToList();
        }

        private async Task<object?> GetAsync(long id, CancellationToken cancellationToken)
        {
            var activity = await database.ExecuteAsync(
                currentDirectoryProvider(),
                (context, token) => repository.GetAsync(context, id, token),
                cancellationToken);
            return activity is null
                ? throw CliException.ResourceNotFound(id)
                : ActivityView.FromEntity(activity);
        }

        private async Task<object?> UpdateAsync(
            ParseResult parseResult,
            Argument<long> idArgument,
            Option<string?> typeOption,
            Option<string?> titleOption,
            Option<string?> descriptionOption,
            Option<string?> statusOption,
            Option<string?> jsonOption,
            CancellationToken cancellationToken)
        {
            var typeProvided = parseResult.GetResult(typeOption) is not null;
            var titleProvided = parseResult.GetResult(titleOption) is not null;
            var descriptionProvided = parseResult.GetResult(descriptionOption) is not null;
            var statusProvided = parseResult.GetResult(statusOption) is not null;
            var jsonProvided = parseResult.GetResult(jsonOption) is not null;
            var input = ActivityInputParser.ParseUpdate(
                jsonProvided,
                parseResult.GetValue(jsonOption),
                typeProvided,
                parseResult.GetValue(typeOption),
                titleProvided,
                parseResult.GetValue(titleOption),
                descriptionProvided,
                parseResult.GetValue(descriptionOption),
                statusProvided,
                parseResult.GetValue(statusOption));

            ThrowIfInvalid(input.Issues);
            var validation = ActivityValidator.ValidateUpdate(input.Value!);
            ThrowIfInvalid(validation.Issues);

            var activity = await database.ExecuteAsync(
                currentDirectoryProvider(),
                (context, token) => repository.UpdateAsync(context, parseResult.GetValue(idArgument), input.Value!, token),
                cancellationToken);
            return activity is null
                ? throw CliException.ResourceNotFound(parseResult.GetValue(idArgument))
                : ActivityView.FromEntity(activity);
        }

        private async Task<object?> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            var deleted = await database.ExecuteAsync(
                currentDirectoryProvider(),
                (context, token) => repository.DeleteAsync(context, id, token),
                cancellationToken);
            return deleted ? new DeleteResponse(id, true) : throw CliException.ResourceNotFound(id);
        }

        private async Task<int> RunAsync(
            ParseResult parseResult,
            Option<string?> outputOption,
            bool forceJson,
            Func<OutputFormat, CancellationToken, Task<object?>> operation,
            CancellationToken cancellationToken,
            bool rootCommand = false)
        {
            var format = forceJson ? OutputFormat.Json : OutputFormat.Text;
            try
            {
                var configuredOutput = parseResult.GetValue(outputOption);
                if (!OutputFormatParser.TryParse(configuredOutput, out var parsedFormat))
                {
                    ThrowIfInvalid([new ValidationIssue("output", "Value must be text or json.")]);
                }

                if (!forceJson)
                {
                    format = parsedFormat;
                }

                if (rootCommand)
                {
                    ThrowIfInvalid([new ValidationIssue("command", "A command is required.")]);
                }

                var result = await operation(format, cancellationToken);
                if (result is not null)
                {
                    writer.WriteSuccess(result, format);
                }

                return ExitCodes.Success;
            }
            catch (CliException exception)
            {
                writer.WriteError(exception.Error, format);
                return exception.Error.ExitCode;
            }
            catch (OperationCanceledException)
            {
                return ExitCodes.UnexpectedError;
            }
            catch (Exception)
            {
                writer.WriteError(new CliError("unexpected_error", "Unexpected error.", ExitCodes.UnexpectedError), format);
                return ExitCodes.UnexpectedError;
            }
        }

        private static void ThrowIfInvalid(IReadOnlyList<ValidationIssue> issues)
        {
            if (issues.Count == 0)
            {
                return;
            }

            throw CliException.Validation(
                issues.Select(issue => new ErrorDetail(issue.Field, issue.Message)).ToArray());
        }
    }
}
