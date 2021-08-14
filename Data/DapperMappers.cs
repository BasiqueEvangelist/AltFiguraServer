using System;
using System.Data;
using System.Linq.Expressions;
using Dapper;

namespace AltFiguraServer.Data
{
    public static class DapperMappers
    {
        public static void Register()
        {
            SqlMapper.RemoveTypeMap(typeof(Guid));
            SqlMapper.AddTypeHandler(new GuidHandler());
            SqlMapper.AddTypeHandler(new GuidListHandler());
        }

        private class GuidHandler : SqlMapper.TypeHandler<Guid>
        {
            public override Guid Parse(object value) => Guid.Parse((string)value);

            public override void SetValue(IDbDataParameter parameter, Guid value) => parameter.Value = value.ToString("N");
        }

        private class GuidListHandler : SqlMapper.TypeHandler<GuidList>
        {
            public override GuidList Parse(object value) => GuidList.FromString((string)value);

            public override void SetValue(IDbDataParameter parameter, GuidList value) => parameter.Value = value.ToString();
        }
    }
}