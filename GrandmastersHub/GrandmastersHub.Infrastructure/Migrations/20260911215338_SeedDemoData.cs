using Microsoft.EntityFrameworkCore.Migrations;

namespace GrandmastersHub.Infrastructure.Migrations;

// Keep the EF-generated .Designer.cs alongside this file.
// Data copied from Seed-DemoData.sql. EF owns the transaction and history entry.
public partial class SeedDemoData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (migrationBuilder.ActiveProvider != "Microsoft.EntityFrameworkCore.SqlServer")
        {
            throw new System.NotSupportedException("SeedDemoData requires SQL Server.");
        }

        migrationBuilder.Sql(
            """
            -- Fictional assignment/demo catalog. Applies to the database selected by EF.
            -- EF owns the transaction. Do not add BEGIN/COMMIT/ROLLBACK or GO here.
            SET NOCOUNT ON;
            SET XACT_ABORT ON;

            IF @@TRANCOUNT = 0
                THROW 51004, 'SeedDemoData requires an EF-managed transaction.', 1;

            IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
               OR OBJECT_ID(N'dbo.Products', N'U') IS NULL
               OR OBJECT_ID(N'dbo.ProductImages', N'U') IS NULL
               OR OBJECT_ID(N'dbo.ProductVariants', N'U') IS NULL
               OR OBJECT_ID(N'dbo.Inventory', N'U') IS NULL
                THROW 51001, 'Required catalog tables are missing. Apply the existing migrations first.', 1;

            DECLARE @DemoProducts TABLE (
                [Slug] nvarchar(200) NOT NULL PRIMARY KEY,
                [Name] nvarchar(200) NOT NULL,
                [CategoryName] nvarchar(100) NOT NULL,
                [Price] decimal(18,2) NOT NULL,
                [Description] nvarchar(2000) NOT NULL,
                [ImageUrl] nvarchar(500) NOT NULL,
                [VariantName] nvarchar(100) NOT NULL,
                [Quantity] int NOT NULL
            );

            INSERT INTO @DemoProducts
                ([Slug], [Name], [CategoryName], [Price], [Description], [ImageUrl], [VariantName], [Quantity])
            VALUES
                (N'demo-classic-walnut-chess-board', N'Classic Walnut Chess Board', N'Boards', 895.00, N'A warm walnut-style board with clear coordinates and a felt-lined base. A welcoming centrepiece for weekly games.', N'/images/Shopping-plain-chess-board.jpg', N'Walnut finish', 24),
                (N'demo-ebony-tournament-board', N'Ebony Tournament Board', N'Boards', 1295.00, N'Bold black-and-white squares, a broad playing area and a clean border. Built for focused club nights and longer matches.', N'/images/shopping-black-white-board.png', N'Tournament size', 12),
                (N'demo-folding-travel-chess-board', N'Folding Travel Chess Board', N'Boards', 549.00, N'A compact folding board for holidays, lunch breaks and games on the move. Easy to pack and quick to set up.', N'/images/Shopping-plain-chess-board.jpg', N'Travel size', 32),
                (N'demo-club-practice-chess-board', N'Club Practice Chess Board', N'Boards', 349.00, N'An affordable everyday board with easy-to-read squares. A practical choice for beginners, schools and chess clubs.', N'/images/shopping-black-white-board.png', N'Standard', 48),
                (N'demo-grandmaster-walnut-board', N'Grandmaster Walnut Board', N'Boards', 1895.00, N'A large-format walnut-style board with a wide frame and a smooth finish. Extra room for analysis sessions and tournament pieces.', N'/images/Shopping-plain-chess-board.jpg', N'Large format', 8),
                (N'demo-championship-display-board', N'Championship Display Board', N'Boards', 2495.00, N'A striking monochrome display board with a generous frame. Made to look at home on a study desk or games-room table.', N'/images/shopping-black-white-board.png', N'Display edition', 5),
                (N'demo-club-digital-chess-clock', N'Club Digital Chess Clock', N'Clocks', 459.00, N'A straightforward digital timer with two clear displays. Set a time control, press start and get straight into the game.', N'/images/Digital-clock.png', N'Digital', 36),
                (N'demo-blitz-tournament-timer', N'Blitz Tournament Timer', N'Clocks', 699.00, N'Large buttons and an easy-to-read display for fast games. A dependable companion for blitz evenings with friends.', N'/images/Digital-clock.png', N'Digital', 18),
                (N'demo-classic-analogue-chess-clock', N'Classic Analogue Chess Clock', N'Clocks', 595.00, N'Traditional twin dials with a simple mechanical feel. An old-school finish for a wooden board and a quiet afternoon of chess.', N'/images/Shopping-clock-1.png', N'Analogue', 14),
                (N'demo-dual-display-competition-clock', N'Dual Display Competition Clock', N'Clocks', 999.00, N'A competition-style digital timer with clear remaining-time displays. Suitable for practice sessions with custom time controls.', N'/images/Digital-clock.png', N'Competition edition', 10),
                (N'demo-rapid-play-digital-timer', N'Rapid Play Digital Timer', N'Clocks', 379.00, N'A compact timer for casual rapid games and classroom practice. Simple controls make it approachable for new players.', N'/images/Digital-clock.png', N'Digital', 42),
                (N'demo-heritage-match-clock', N'Heritage Match Clock', N'Clocks', 1495.00, N'A classic twin-dial clock with a refined tabletop presence. Pair it with a traditional board for a complete study set.', N'/images/Shopping-clock-1.png', N'Heritage edition', 6),
                (N'demo-club-players-handbook', N'The Club Player''s Handbook', N'Books', 295.00, N'A practical handbook covering opening habits, practical plans and common mistakes. A friendly starting point for new club players.', N'/images/book-1.png', N'Paperback', 40),
                (N'demo-tactics-before-coffee', N'Tactics Before Coffee', N'Books', 249.00, N'A collection of bite-sized tactical puzzles. Practise forks, pins and discovered attacks in short daily sessions.', N'/images/book-1.png', N'Paperback', 55),
                (N'demo-endgames-made-practical', N'Endgames Made Practical', N'Books', 379.00, N'A guide to king activity, pawn races and rook endings. Work through practical positions one idea at a time.', N'/images/book-1.png', N'Paperback', 22),
                (N'demo-opening-workshop', N'The Opening Workshop', N'Books', 429.00, N'An opening study guide focused on plans rather than memorised moves. Includes exercises for building a personal repertoire.', N'/images/book-1.png', N'Hardcover', 16),
                (N'demo-positional-chess-explained', N'Positional Chess Explained', N'Books', 349.00, N'A guide to weak squares, open files and better piece placement. Designed for players ready to look beyond immediate tactics.', N'/images/book-1.png', N'Paperback', 28),
                (N'demo-tournament-preparation-journal', N'Tournament Preparation Journal', N'Books', 199.00, N'A study journal with space for game notes, training goals and post-match analysis. Keep your preparation in one place.', N'/images/book-1.png', N'Paperback', 64),
                (N'demo-volcanic-obsidian-chess-set', N'Volcanic Obsidian Chess Set', N'Bespoke', 4995.00, N'A dramatic black-stone-style set with contrasting pieces and a sculpted finish. A statement centrepiece for the dedicated chess enthusiast.', N'/images/Volcanic.png', N'Obsidian finish', 4),
                (N'demo-midnight-collector-chess-set', N'Midnight Collector Chess Set', N'Bespoke', 6495.00, N'Deep tones and sculptural pieces give this collector-style set its character. Intended for display as well as slow, thoughtful games.', N'/images/Volcanic.png', N'Midnight finish', 3),
                (N'demo-heritage-artisan-chess-set', N'Heritage Artisan Chess Set', N'Bespoke', 3995.00, N'An artisan-inspired set with traditional silhouettes and a substantial board. A memorable gift for a player building a personal collection.', N'/images/Volcanic.png', N'Heritage finish', 7),
                (N'demo-royal-contrast-chess-set', N'Royal Contrast Chess Set', N'Bespoke', 5495.00, N'Light and dark pieces create a strong visual contrast across the board. A collector-style set with a formal tabletop presence.', N'/images/Volcanic.png', N'Royal edition', 5),
                (N'demo-monochrome-signature-chess-set', N'Monochrome Signature Chess Set', N'Bespoke', 2995.00, N'A restrained monochrome set with clean piece shapes. A simple display option for a modern office or study.', N'/images/Volcanic.png', N'Signature edition', 9),
                (N'demo-grandmaster-commission-chess-set', N'Grandmaster Commission Chess Set', N'Bespoke', 8995.00, N'A commission-style showcase set with a distinctive board and sculpted pieces. A premium centrepiece for a dedicated games room.', N'/images/Volcanic.png', N'Commission edition', 2);

            -- Serialize simultaneous runs of this seed without holding a session lock.
            DECLARE @LockResult int;
            EXEC @LockResult = sys.sp_getapplock
                @Resource = N'SEN371-GrandmastersHub-DemoCatalog-v1',
                @LockMode = N'Exclusive',
                @LockOwner = N'Transaction',
                @LockTimeout = 10000;
            IF @LockResult < 0
                THROW 51002, 'Another demo seed is running. Retry after it finishes.', 1;

            INSERT INTO [dbo].[Categories] ([Name])
            SELECT DISTINCT seed.[CategoryName]
            FROM @DemoProducts AS seed
            WHERE NOT EXISTS (
                SELECT 1 FROM [dbo].[Categories] AS existing
                WHERE existing.[Name] = seed.[CategoryName]
            );

            INSERT INTO [dbo].[Products] ([Name], [Slug], [Description], [Price], [CategoryId], [CreatedAt])
            SELECT seed.[Name], seed.[Slug], seed.[Description], seed.[Price], category.[CategoryId], SYSUTCDATETIME()
            FROM @DemoProducts AS seed
            JOIN [dbo].[Categories] AS category ON category.[Name] = seed.[CategoryName]
            WHERE NOT EXISTS (
                SELECT 1 FROM [dbo].[Products] AS existing
                WHERE existing.[Slug] = seed.[Slug]
            );

            INSERT INTO [dbo].[ProductImages] ([ProductId], [ImageUrl])
            SELECT product.[ProductId], seed.[ImageUrl]
            FROM @DemoProducts AS seed
            JOIN [dbo].[Products] AS product ON product.[Slug] = seed.[Slug]
            WHERE NOT EXISTS (
                SELECT 1 FROM [dbo].[ProductImages] AS existing
                WHERE existing.[ProductId] = product.[ProductId]
            );

            INSERT INTO [dbo].[ProductVariants] ([ProductId], [Name], [Price])
            SELECT product.[ProductId], seed.[VariantName], product.[Price]
            FROM @DemoProducts AS seed
            JOIN [dbo].[Products] AS product ON product.[Slug] = seed.[Slug]
            WHERE NOT EXISTS (
                SELECT 1 FROM [dbo].[ProductVariants] AS existing
                WHERE existing.[ProductId] = product.[ProductId]
                  AND existing.[Name] = seed.[VariantName]
            );

            INSERT INTO [dbo].[Inventory] ([ProductVariantId], [Quantity], [UpdatedAt])
            SELECT variant.[ProductVariantId], seed.[Quantity], SYSUTCDATETIME()
            FROM @DemoProducts AS seed
            JOIN [dbo].[Products] AS product ON product.[Slug] = seed.[Slug]
            JOIN [dbo].[ProductVariants] AS variant
                ON variant.[ProductId] = product.[ProductId]
               AND variant.[Name] = seed.[VariantName]
            WHERE NOT EXISTS (
                SELECT 1 FROM [dbo].[Inventory] AS existing
                WHERE existing.[ProductVariantId] = variant.[ProductVariantId]
            );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Matching rows may have been inserted by the earlier standalone seed.
        // Do not delete pre-existing catalog data or break cart/order references.
        throw new System.NotSupportedException(
            "SeedDemoData cannot be automatically rolled back. Demo rows may predate " +
            "this migration. Use a reviewed cleanup migration or restore a backup.");
    }
}
