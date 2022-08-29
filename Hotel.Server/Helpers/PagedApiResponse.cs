using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Dapper;
using Hotel.Server.Data;

namespace Hotel.Server.Helpers;

public class PagedApiResponseMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRows { get; set; }
    public int TotalPages { get; set; }
    public string SortBy { get; set; }
    public string SortDirection { get; set; }

    public PagedApiResponseMeta() { }

    public PagedApiResponseMeta(int page, int pageSize, int totalRows, string sortBy, string sortDirection)
    {
        this.Page = page;
        this.PageSize = pageSize;
        this.TotalRows = totalRows;
        this.TotalPages = Convert.ToInt32(Math.Ceiling((decimal)totalRows / pageSize));
        this.SortBy = sortBy;
        this.SortDirection = sortDirection;
    }
}

public class PagedApiResponse<T>
{
    private const int DEFAULT_PAGE_SIZE = 20;

    public List<T> Data { get; set; }
    public PagedApiResponseMeta Meta { get; set; }

    private static string generateSearchSql<TT>(RequestParameters parameters)
    {
        var sqlSearch = "";

        if (!string.IsNullOrEmpty(parameters?.Search))
        {
            var searchColumnList = (from pi in typeof(TT).GetProperties()
                where Attribute.IsDefined(pi, typeof(QuickSearchable))
                select pi.Name).ToList();

            var searchValue = parameters.Search.Replace("'", "");

            sqlSearch = " (";

            sqlSearch += $"id::text ILIKE '%{searchValue}%'";

            for (int i = 0; i < searchColumnList.Count; i++)
            {
                sqlSearch += " OR " + searchColumnList[0].ToSnakeCase() + $" ILIKE '%{searchValue}%'";
            }

            sqlSearch += ") ";
        }

        return sqlSearch;
    }

    private static string generateFilterSql<TT>(RequestParameters parameters)
    {
        var sqlFilter = "";

        if (parameters?.FilterList.Count > 0)
        {
            var filterColumnList = (from pi in typeof(TT).GetProperties() select pi.Name.ToSnakeCase()).ToList();

            if (filterColumnList.Count > 0)
            {
                sqlFilter = " (";

                var i = 0;

                foreach (var key in parameters.FilterList.Keys)
                {
                    var column = key
                        .Replace("'", "")
                        .Replace("[gt]", "")
                        .Replace("[lt]", "")
                        .Replace("[like]", "")
                        .ToSnakeCase();

                    if (!filterColumnList.Contains(column))
                        throw new BadRequestException($"Invalid filter: {column}");

                    var value = parameters.FilterList[key].Replace("'", "");

                    if (i > 0) sqlFilter += " AND ";

                    var sqlOperator = "=";
                    var likeOperator = "";

                    if (key.Contains("[lt]")) sqlOperator = "<";
                    if (key.Contains("[gt]")) sqlOperator = ">=";
                    if (key.Contains("[like]"))
                    {
                        sqlOperator = " ilike ";
                        likeOperator = "%";
                    }

                    if (value == "$null")
                    {
                        sqlFilter += column + $" is null";
                    }
                    else if (value == "$notnull")
                    {
                        sqlFilter += column + $" is not null";
                    }
                    else
                    {
                        //sqlFilter += column + $"::varchar {sqlOperator} '{likeOperator}{value}{likeOperator}'";
                        sqlFilter += column + $" {sqlOperator} '{likeOperator}{value}{likeOperator}'";
                    }

                    i++;
                }

                sqlFilter += ") ";
            }
        }

        return sqlFilter;
    }

    public static async Task<PagedApiResponse<T>> GetFromSql(DbContext context, string sql, object sqlParams,
        RequestParameters parameters)
    {
        var page = parameters?.Page ?? 1;
        var pageSize = parameters?.PageSize ?? DEFAULT_PAGE_SIZE;

        var offset = (page - 1) * pageSize;
        var limit = pageSize;

        var sqlOrder = "";
        var sortDirection = parameters?.SortDirection == "desc" ? "desc" : "asc";

        if (!string.IsNullOrEmpty(parameters?.SortBy))
        {
            foreach (var sortKey in parameters?.SortBy.Split(","))
            {
                var sanitizedSortKey = sortKey.Replace("'", "''").ToSnakeCase();
                var keySortDirection = parameters?.SortDirection == "desc" ? "desc" : "asc";

                if (sanitizedSortKey.Contains("[asc]"))
                {
                    sanitizedSortKey = sanitizedSortKey.Replace("[asc]", "");
                    keySortDirection = "asc";
                }
                else if (sanitizedSortKey.Contains("[desc]"))
                {
                    sanitizedSortKey = sanitizedSortKey.Replace("[desc]", "");
                    keySortDirection = "desc";
                }

                if(sqlOrder == "") sqlOrder = $@" ORDER BY ";
                else sqlOrder += ", ";
                
                sqlOrder += $@"{sanitizedSortKey} {keySortDirection}";
            }
        }

        var sqlPaging = $@" OFFSET {offset.ToString()} LIMIT {limit.ToString()}";

        var sqlWhere = "";
        var sqlSearch = generateSearchSql<T>(parameters);
        var sqlFilter = generateFilterSql<T>(parameters);

        if (!string.IsNullOrEmpty(sqlSearch) || !string.IsNullOrEmpty(sqlFilter))
        {
            sqlWhere = $" WHERE ";
            if (!string.IsNullOrEmpty(sqlSearch)) sqlWhere += sqlSearch;
            if (!string.IsNullOrEmpty(sqlSearch) && !string.IsNullOrEmpty(sqlFilter)) sqlWhere += " AND ";
            if (!string.IsNullOrEmpty(sqlFilter)) sqlWhere += sqlFilter;
        }

        var sqlWithConditions = $@"SELECT * FROM ( {sql} ) as result " + sqlWhere + sqlOrder + sqlPaging;
        var sqlCount = $@"SELECT COUNT(*) as row_count FROM ( {sql} ) as result {sqlWhere}";

        var items = await context.Database.GetDbConnection().QueryAsync<T>(sqlWithConditions, sqlParams);
        var totalRows = await context.Database.GetDbConnection().QuerySingleAsync<int>(sqlCount, sqlParams);

        var response = new PagedApiResponse<T>
        {
            Data = items.ToList(),
            Meta = new PagedApiResponseMeta(page, pageSize, totalRows, parameters?.SortBy, sortDirection)
        };

        return response;
    }
}
