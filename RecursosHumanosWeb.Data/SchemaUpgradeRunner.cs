using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;

namespace RecursosHumanosWeb.Services;

public static class SchemaUpgradeRunner
{
    private const string UpgradeFolder = "upgrade";
    private const string FilePrefix = "upgrade_";

    public static void ApplyPendingUpgrades(HumanResourcesContext context, ILogger logger, string baseDirectory)
    {
        EnsureTrackingTable(context);

        var upgradeDirectory = Path.Combine(baseDirectory, "db", UpgradeFolder);
        if (!Directory.Exists(upgradeDirectory)) return;

        var applied = context.Database
            .SqlQueryRaw<string>("SELECT [Version] AS [Value] FROM [dbo].[SchemaVersion]")
            .Select(v => v.Trim())
            .ToHashSet(StringComparer.Ordinal);

        var pendingFiles = Directory
            .GetFiles(upgradeDirectory, $"{FilePrefix}*.sql")
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

        foreach (var file in pendingFiles)
        {
            var version = Path.GetFileNameWithoutExtension(file)[FilePrefix.Length..].Trim();
            if (string.IsNullOrEmpty(version) || applied.Contains(version)) continue;

            var sql = File.ReadAllText(file);

            // Split into GO-separated batches (ExecuteSqlRaw cannot handle GO separators)
            var batches = System.Text.RegularExpressions.Regex.Split(
                sql, @"^\s*GO\s*$",
                System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            using var transaction = context.Database.BeginTransaction();
            try
            {
                foreach (var batch in batches)
                {
                    if (!string.IsNullOrWhiteSpace(batch))
                        context.Database.ExecuteSqlRaw(batch);
                }

                context.Database.ExecuteSqlRaw(
                    "INSERT INTO [dbo].[SchemaVersion] ([Version]) VALUES ({0})",
                    version);
                transaction.Commit();
                logger.LogInformation("Upgrade de esquema {Version} aplicado correctamente.", version);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    private static void EnsureTrackingTable(HumanResourcesContext context)
    {
        context.Database.ExecuteSqlRaw(
            """
            IF OBJECT_ID(N'dbo.SchemaVersion', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[SchemaVersion] (
                    [Version]   NVARCHAR(64) NOT NULL CONSTRAINT [PK_SchemaVersion] PRIMARY KEY,
                    [AppliedAt] DATETIME     NOT NULL CONSTRAINT [DF_SchemaVersion_AppliedAt] DEFAULT GETDATE()
                );
            END

            IF OBJECT_ID(N'dbo.SchemaVersion', N'U') IS NOT NULL
            AND EXISTS (
                SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID(N'dbo.SchemaVersion')
                  AND name = N'Version'
                  AND system_type_id = 239 -- nchar
            )
            BEGIN
                ALTER TABLE [dbo].[SchemaVersion] DROP CONSTRAINT [PK_SchemaVersion];
                ALTER TABLE [dbo].[SchemaVersion] ALTER COLUMN [Version] NVARCHAR(64) NOT NULL;
                ALTER TABLE [dbo].[SchemaVersion] ADD CONSTRAINT [PK_SchemaVersion] PRIMARY KEY ([Version]);
            END
            """);
    }
}
