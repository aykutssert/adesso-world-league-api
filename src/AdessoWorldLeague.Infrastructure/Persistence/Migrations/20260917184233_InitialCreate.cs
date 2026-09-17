using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AdessoWorldLeague.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "league");

            migrationBuilder.CreateTable(
                name: "countries",
                schema: "league",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    iso_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_countries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "draws",
                schema: "league",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    drawn_by_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    drawn_by_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    group_count = table.Column<int>(type: "integer", nullable: false),
                    drawn_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_draws", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                schema: "league",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    country_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.id);
                    table.ForeignKey(
                        name: "FK_teams_countries_country_id",
                        column: x => x.country_id,
                        principalSchema: "league",
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "draw_groups",
                schema: "league",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_draw_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_draw_groups_draws_draw_id",
                        column: x => x.draw_id,
                        principalSchema: "league",
                        principalTable: "draws",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "draw_group_teams",
                schema: "league",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    draw_id = table.Column<Guid>(type: "uuid", nullable: false),
                    draw_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    selection_order = table.Column<int>(type: "integer", nullable: false),
                    pick_number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_draw_group_teams", x => x.id);
                    table.ForeignKey(
                        name: "FK_draw_group_teams_draw_groups_draw_group_id",
                        column: x => x.draw_group_id,
                        principalSchema: "league",
                        principalTable: "draw_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_draw_group_teams_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "league",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "league",
                table: "countries",
                columns: new[] { "id", "iso_code", "name" },
                values: new object[,]
                {
                    { new Guid("17dab60d-d721-56b3-a834-b7645828442d"), "IT", "İtalya" },
                    { new Guid("1a41e4e2-a53d-56fa-8644-e195e098b30b"), "NL", "Hollanda" },
                    { new Guid("37d83f25-109b-587d-ae37-55694ced4a05"), "TR", "Türkiye" },
                    { new Guid("52a55e9a-2500-55ae-ad07-5f1956c744d5"), "PT", "Portekiz" },
                    { new Guid("564af7e8-ae13-5ecf-8728-2a21ace4df85"), "BE", "Belçika" },
                    { new Guid("9c4dbb6e-b6bc-530b-8876-fc5bdb557d4b"), "DE", "Almanya" },
                    { new Guid("be44b6e4-3aa2-574f-ab5d-adc7b235a2a3"), "ES", "İspanya" },
                    { new Guid("cb4407c3-e25b-5ebf-9198-5195f35a91e3"), "FR", "Fransa" }
                });

            migrationBuilder.InsertData(
                schema: "league",
                table: "teams",
                columns: new[] { "id", "country_id", "name" },
                values: new object[,]
                {
                    { new Guid("014dcc59-d254-5eb1-acbe-faf893c09f99"), new Guid("17dab60d-d721-56b3-a834-b7645828442d"), "Adesso Napoli" },
                    { new Guid("05b22c2b-0613-5d2c-a6d5-3f423e343b16"), new Guid("1a41e4e2-a53d-56fa-8644-e195e098b30b"), "Adesso Amsterdam" },
                    { new Guid("0920f705-2765-51a9-a3b8-550f5ebef4cd"), new Guid("52a55e9a-2500-55ae-ad07-5f1956c744d5"), "Adesso Coimbra" },
                    { new Guid("0f5e15f6-817f-5828-9352-94e3223d6c36"), new Guid("be44b6e4-3aa2-574f-ab5d-adc7b235a2a3"), "Adesso Madrid" },
                    { new Guid("1cf8670a-8e1e-5ea1-9439-1bb7519fce76"), new Guid("9c4dbb6e-b6bc-530b-8876-fc5bdb557d4b"), "Adesso Berlin" },
                    { new Guid("263f0560-f2d6-5263-9584-c79ecfe009d3"), new Guid("9c4dbb6e-b6bc-530b-8876-fc5bdb557d4b"), "Adesso Münih" },
                    { new Guid("299ad215-7b39-55bc-90df-82c8c3502a36"), new Guid("cb4407c3-e25b-5ebf-9198-5195f35a91e3"), "Adesso Lyon" },
                    { new Guid("31196768-f6e5-5860-98d7-a4e223ba01eb"), new Guid("37d83f25-109b-587d-ae37-55694ced4a05"), "Adesso İstanbul" },
                    { new Guid("4fb9028e-2360-59d8-b186-915772af8511"), new Guid("37d83f25-109b-587d-ae37-55694ced4a05"), "Adesso Antalya" },
                    { new Guid("56e2818b-c9ce-5f0a-926b-c226ff68eb26"), new Guid("564af7e8-ae13-5ecf-8728-2a21ace4df85"), "Adesso Brugge" },
                    { new Guid("61847517-744b-5075-a3cf-1c26749d8705"), new Guid("be44b6e4-3aa2-574f-ab5d-adc7b235a2a3"), "Adesso Barselona" },
                    { new Guid("677e29b4-5b5d-533e-ad5c-d0991724b4d9"), new Guid("1a41e4e2-a53d-56fa-8644-e195e098b30b"), "Adesso Eindhoven" },
                    { new Guid("6d050f2f-8dfc-51a8-9c03-7f796f926e39"), new Guid("17dab60d-d721-56b3-a834-b7645828442d"), "Adesso Milano" },
                    { new Guid("6d41b3b5-05f1-5214-a719-bd0733233918"), new Guid("52a55e9a-2500-55ae-ad07-5f1956c744d5"), "Adesso Braga" },
                    { new Guid("6e8def07-d547-5453-8d91-2fb1acf71b4d"), new Guid("52a55e9a-2500-55ae-ad07-5f1956c744d5"), "Adesso Lisbon" },
                    { new Guid("70ca042b-5dfe-59c3-ae55-118a9565dcfe"), new Guid("cb4407c3-e25b-5ebf-9198-5195f35a91e3"), "Adesso Nice" },
                    { new Guid("714acb33-72e2-5341-8a99-ca9e18d7b3af"), new Guid("564af7e8-ae13-5ecf-8728-2a21ace4df85"), "Adesso Gent" },
                    { new Guid("7a0c8e50-dc0f-539b-bfee-9beeda4e71a3"), new Guid("37d83f25-109b-587d-ae37-55694ced4a05"), "Adesso Ankara" },
                    { new Guid("819d826c-62e9-5ee3-8c45-91f43c54719d"), new Guid("564af7e8-ae13-5ecf-8728-2a21ace4df85"), "Adesso Brüksel" },
                    { new Guid("904b1362-f0c3-5b31-9512-c08518e474d2"), new Guid("be44b6e4-3aa2-574f-ab5d-adc7b235a2a3"), "Adesso Sevilla" },
                    { new Guid("a2c6e326-b2f5-5826-a54f-deed4e365804"), new Guid("9c4dbb6e-b6bc-530b-8876-fc5bdb557d4b"), "Adesso Frankfurt" },
                    { new Guid("a600e592-8bb0-5837-a35b-643d63ac776e"), new Guid("1a41e4e2-a53d-56fa-8644-e195e098b30b"), "Adesso Rotterdam" },
                    { new Guid("b34a64bf-2cbb-5221-8254-2a554622616a"), new Guid("cb4407c3-e25b-5ebf-9198-5195f35a91e3"), "Adesso Marsilya" },
                    { new Guid("caf75675-3c55-58cc-994d-eef51cc2419d"), new Guid("cb4407c3-e25b-5ebf-9198-5195f35a91e3"), "Adesso Paris" },
                    { new Guid("cafbd70f-3de0-55fe-9d20-041b49bbeaf0"), new Guid("1a41e4e2-a53d-56fa-8644-e195e098b30b"), "Adesso Lahey" },
                    { new Guid("d3fc2a84-63fb-58e6-ad08-1d9d9ce9b19e"), new Guid("17dab60d-d721-56b3-a834-b7645828442d"), "Adesso Venedik" },
                    { new Guid("d8140222-2a55-57f1-8142-5c3abc1728ec"), new Guid("be44b6e4-3aa2-574f-ab5d-adc7b235a2a3"), "Adesso Granada" },
                    { new Guid("dbcb3903-5256-5fe9-b8ed-bfb20d0bf165"), new Guid("37d83f25-109b-587d-ae37-55694ced4a05"), "Adesso İzmir" },
                    { new Guid("e6ca3b36-86f3-5b7c-8ae4-417655d7d5ce"), new Guid("564af7e8-ae13-5ecf-8728-2a21ace4df85"), "Adesso Anvers" },
                    { new Guid("efaf34c6-b7c4-5801-b796-7283f6a4d7b4"), new Guid("17dab60d-d721-56b3-a834-b7645828442d"), "Adesso Roma" },
                    { new Guid("f9efcffe-4d6b-5f04-9520-012c0012ebd3"), new Guid("52a55e9a-2500-55ae-ad07-5f1956c744d5"), "Adesso Porto" },
                    { new Guid("fa336017-2a50-54de-9818-61d8479fcf65"), new Guid("9c4dbb6e-b6bc-530b-8876-fc5bdb557d4b"), "Adesso Dortmund" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_countries_iso_code",
                schema: "league",
                table: "countries",
                column: "iso_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_draw_group_teams_draw_group_id_selection_order",
                schema: "league",
                table: "draw_group_teams",
                columns: new[] { "draw_group_id", "selection_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_draw_group_teams_draw_id_team_id",
                schema: "league",
                table: "draw_group_teams",
                columns: new[] { "draw_id", "team_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_draw_group_teams_team_id",
                schema: "league",
                table: "draw_group_teams",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_draw_groups_draw_id_label",
                schema: "league",
                table: "draw_groups",
                columns: new[] { "draw_id", "label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_draws_drawn_at_utc",
                schema: "league",
                table: "draws",
                column: "drawn_at_utc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_teams_country_id",
                schema: "league",
                table: "teams",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_name",
                schema: "league",
                table: "teams",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "draw_group_teams",
                schema: "league");

            migrationBuilder.DropTable(
                name: "draw_groups",
                schema: "league");

            migrationBuilder.DropTable(
                name: "teams",
                schema: "league");

            migrationBuilder.DropTable(
                name: "draws",
                schema: "league");

            migrationBuilder.DropTable(
                name: "countries",
                schema: "league");
        }
    }
}
