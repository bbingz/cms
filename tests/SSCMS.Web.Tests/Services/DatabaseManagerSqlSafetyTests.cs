using SSCMS.Core.Services;
using Xunit;

namespace SSCMS.Web.Tests.Services
{
    public class DatabaseManagerSqlSafetyTests
    {
        [Theory]
        [InlineData("select * from siteserver_Content")]
        [InlineData("WITH recent AS (SELECT * FROM siteserver_Content) SELECT * FROM recent")]
        [InlineData("select 'delete from table' as Text from siteserver_Content")]
        public void IsReadOnlySelectSqlAllowsSingleSelectQueries(string sql)
        {
            Assert.True(DatabaseManager.IsReadOnlySelectSql(sql));
        }

        [Theory]
        [InlineData("delete from siteserver_Administrator")]
        [InlineData("select * from siteserver_Content; delete from siteserver_Administrator")]
        [InlineData("update siteserver_Administrator set Password = 'x'")]
        [InlineData("drop table siteserver_Administrator")]
        [InlineData("select * into backup_table from siteserver_Administrator")]
        [InlineData("select * from siteserver_Content into outfile '/tmp/data.txt'")]
        public void IsReadOnlySelectSqlRejectsMutatingOrMultiStatementSql(string sql)
        {
            Assert.False(DatabaseManager.IsReadOnlySelectSql(sql));
        }
    }
}
