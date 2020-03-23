﻿using Implem.Libraries.Utilities;
using Implem.Pleasanter.Libraries.General;
using Implem.Pleasanter.Libraries.Requests;
using System.Linq;
namespace Implem.Pleasanter.Libraries.Responses
{
    public static class ApiResponses
    {
        public static ApiResponse Success(long id, string message, int? limitPerDate = null, int? limitRemaining = null)
        {
            return new ApiResponse(id, 200, message, limitPerDate, limitRemaining);
        }

        public static ApiResponse Error(Context context, ErrorData errorData, params string[] data)
        {
            switch (errorData.Type)
            {
                case General.Error.Types.BadRequest:
                    return BadRequest(context);
                case General.Error.Types.Unauthorized:
                    return Unauthorized(context);
                case General.Error.Types.NotFound:
                    return NotFound(context);
                case General.Error.Types.HasNotPermission:
                    return Forbidden(context);
                default:
                    return new ApiResponse(500, data?.Any() == true
                        ? Displays.Get(
                            context: context,
                            id: errorData.Type.ToString()).Params(data)
                        : Displays.Get(
                            context: context,
                            id: errorData.Type.ToString()));
            }
        }

        public static ApiResponse BadRequest(Context context)
        {
            return new ApiResponse(400, Displays.BadRequest(context: context));
        }

        public static ApiResponse Unauthorized(Context context)
        {
            return new ApiResponse(401, Displays.Unauthorized(context: context));
        }

        public static ApiResponse NotFound(Context context)
        {
            return new ApiResponse(404, Displays.NotFound(context: context));
        }

        public static ApiResponse Forbidden(Context context)
        {
            return new ApiResponse(403, Displays.HasNotPermission(context: context));
        }

        public static ApiResponse OverLimitApi(Context context, long siteId, int limitPerSite)
        {
            return new ApiResponse(
                id: context.Id,
                statusCode: 429,
                message: Displays.OverLimitApi(
                    context: context, siteId.ToString(),
                    limitPerSite.ToString()));
        }

        public static ApiResponse OverLimitQuantity(Context context, decimal? maxSize)
        {
            return new ApiResponse(
                id: context.Id,
                statusCode: 441,
                message: Displays.OverLimitQuantity(
                    context: context,
                    data: maxSize.ToString()));
        }

        public static ApiResponse OverLimitSize(Context context, decimal? maxSize)
        {
            return new ApiResponse(
                id: context.Id,
                statusCode: 442,
                message: Displays.OverLimitSize(
                    context: context,
                    data: maxSize.ToString()));
        }

        public static ApiResponse OverTotalLimitSize(Context context, decimal? maxSize)
        {
            return new ApiResponse(
                id: context.Id,
                statusCode: 443,
                message: Displays.OverTotalLimitSize(
                    context: context,
                    data: maxSize.ToString()));
        }

        public static ApiResponse OverTenantStorageSize(Context context, decimal? maxSize)
        {
            return new ApiResponse(
                id: context.Id,
                statusCode: 444,
                message: Displays.OverTenantStorageSize(
                    context: context,
                    data: maxSize.ToString()));
        }
    }
}