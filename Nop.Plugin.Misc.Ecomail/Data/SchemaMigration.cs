using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.Ecomail.Domain;

namespace Nop.Plugin.Misc.Ecomail.Data
{
    [NopMigration("2023/04/27 12:00:00", "Misc.Ecomail base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        #region Methods

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            Create.TableFor<EcomailOrder>();
        }

        #endregion
    }
}