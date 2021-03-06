﻿using Implem.DefinitionAccessor;
using Implem.Libraries.DataSources.SqlServer;
using Implem.Libraries.Utilities;
using Implem.Pleasanter.Libraries.DataSources;
using Implem.Pleasanter.Libraries.General;
using Implem.Pleasanter.Libraries.Requests;
using Implem.Pleasanter.Libraries.Security;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
namespace Implem.Pleasanter.Libraries.Responses
{
    public static class FileContentResults
    {
        public static FileContentResult Download(Context context, string guid)
        {
            DataRow dataRow = GetBinariesTable(context, guid);
            switch (dataRow.String("BinaryType"))
            {
                case "Images":
                    return dataRow != null
                        ? new ResponseFile(
                            fileContent: new MemoryStream(
                                Bytes(
                                    dataRow: dataRow,
                                    thumbnail: context.QueryStrings.Bool("thumbnail")),
                                false),
                            fileDownloadName: dataRow.String("FileName"),
                            contentType: dataRow.String("ContentType")).FileStream()
                        : null;
                default:
                    return dataRow != null
                        ? new ResponseFile(
                            fileContent: new MemoryStream(Bytes(dataRow: dataRow), false),
                            fileDownloadName: dataRow.String("FileName"),
                            contentType: dataRow.String("ContentType")).FileStream()
                        : null;
            }
        }

        public static ContentResult DownloadByApi(Context context, string guid)
        {
            var dataRow = GetBinariesTable(context, guid);
            return dataRow != null
                ? new ResponseFile(
                    new MemoryStream(Bytes(dataRow: dataRow), false),
                    dataRow.String("FileName"),
                    dataRow.String("ContentType"))
                        .ToContentResult(
                            id: dataRow.Long("BinaryId"),
                            referenceId: dataRow.Long("ReferenceId"),
                            binaryType: dataRow.String("BinaryType"),
                            guid: dataRow.String("Guid"),
                            extension: dataRow.String("Extension"),
                            size: dataRow.Long("Size"),
                            creator: dataRow.Long("Creator"),
                            updator: dataRow.Long("Updator"),
                            createdTime: dataRow.String("CreatedTime"),
                            updatedTime: dataRow.String("UpdatedTime"))
                : ApiResults.Error(
                    context: context,
                    errorData: new ErrorData(type: Error.Types.NotFound));
        }

        private static DataRow GetBinariesTable(Context context, string guid)
        {
            if (guid.IsNullOrEmpty()) return null;
            return Repository.ExecuteTable(
                context: context,
                statements: new SqlStatement[]
                {
                    Rds.SelectBinaries(
                        column: Rds.BinariesColumn()
                            .BinaryId()
                            .ReferenceId()
                            .Guid()
                            .BinaryType()
                            .Bin()
                            .Thumbnail()
                            .FileName()
                            .ContentType()
                            .Extension()
                            .Size()
                            .Creator()
                            .Updator()
                            .CreatedTime()
                            .UpdatedTime(),
                        join: Rds.BinariesJoinDefault()
                            .Add(new SqlJoin(
                                tableBracket: "\"Items\"",
                                joinType: SqlJoin.JoinTypes.Inner,
                                joinExpression: "\"Binaries\".\"ReferenceId\"=\"Items\".\"ReferenceId\""))
                            .Add(new SqlJoin(
                                tableBracket: "\"Sites\"",
                                joinType: SqlJoin.JoinTypes.Inner,
                                joinExpression: "\"Items\".\"SiteId\"=\"Sites\".\"SiteId\"")),
                        where: Rds.BinariesWhere()
                            .TenantId(context.TenantId)
                            .Guid(guid)
                            .CanRead(
                                context: context,
                                idColumnBracket: "\"Binaries\".\"ReferenceId\"",
                                _using: !context.Publish)),
                    Rds.SelectBinaries(
                        column: Rds.BinariesColumn()
                            .BinaryId()
                            .ReferenceId()
                            .Guid()
                            .BinaryType()
                            .Bin()
                            .Thumbnail()
                            .FileName()
                            .ContentType()
                            .Extension()
                            .Size()
                            .Creator()
                            .Updator()
                            .CreatedTime()
                            .UpdatedTime(),
                        join: Rds.BinariesJoinDefault()
                            .Add(new SqlJoin(
                                tableBracket: "\"Items\"",
                                joinType: SqlJoin.JoinTypes.Inner,
                                joinExpression: "\"Binaries\".\"ReferenceId\"=\"Items\".\"ReferenceId\""))
                            .Add(new SqlJoin(
                                tableBracket: "\"Sites\"",
                                joinType: SqlJoin.JoinTypes.Inner,
                                joinExpression: "\"Items\".\"SiteId\"=\"Sites\".\"SiteId\"")),
                        where: Rds.BinariesWhere()
                            .TenantId(context.TenantId)
                            .Guid(guid)
                            .Add(raw: $"(\"Binaries\".\"CreatedTime\"=\"Binaries\".\"UpdatedTime\" and \"Binaries\".\"Creator\"={context.UserId})"),
                        unionType: Sqls.UnionTypes.UnionAll)})
                            .AsEnumerable()
                            .FirstOrDefault();
        }

        private static byte[] Bytes(DataRow dataRow, bool thumbnail = false)
        {
            switch (Parameters.BinaryStorage.Provider)
            {
                case "Local":
                    return BytesforLocal(
                         dataRow: dataRow,
                         thumbnail: thumbnail)
                             ?? BytesforLocal(
                                 dataRow: dataRow,
                                 thumbnail: false);
                default:
                    return thumbnail && dataRow["Thumbnail"] != DBNull.Value
                        ? dataRow.Bytes("Thumbnail")
                        : dataRow.Bytes("Bin");
            }
        }

        private static byte[] BytesforLocal(DataRow dataRow, bool thumbnail = false)
        {
            return Files.Bytes(Path.Combine(
                Directories.BinaryStorage(),
                dataRow.String("BinaryType"),
                dataRow.String("Guid")
                    + (thumbnail
                        ? "_thumbnail"
                        : string.Empty)));
        }

        public static FileContentResult DownloadTemp(string guid)
        {
            var folderPath = Path.Combine(Path.Combine(Directories.Temp(), guid));
            var files = Directory.GetFiles(folderPath);
            using (var fs = new FileStream(files[0], FileMode.Open, FileAccess.Read))
            {
                var res = new ResponseFile(fs, Path.GetFileName(files[0]));
                return res.FileStream();
            }
        }
    }
}