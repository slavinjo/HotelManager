using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Hotel.Server.Data;
using Hotel.Server.Helpers;

namespace Hotel.Server.Hotels;

public class HotelService
{
    private readonly HotelContext _context;
    private readonly IMapper _mapper;

    // Distance formula from: https://stackoverflow.com/questions/61135374/postgresql-calculate-distance-between-two-points-without-using-postgis
    private readonly string _selectSql = $@"
             SELECT DISTINCT ON (h.id)
                 h.*,
                 CASE 
                    WHEN @lat is null OR @lng is null THEN null
                    ELSE SQRT(POW(69.1 * (h.geo_lat::float -  @lat::float), 2) + POW(69.1 * (@lng::float - h.geo_lng::float) * COS(h.geo_lat::float / 57.3), 2))
                 END AS distance
             FROM
                 hotel h
             WHERE
                 (@id is null OR h.id = @id)";

    // postgres sql to calculate distance between two coordinates
    private object getSelectSqlParams(Guid? id = null, double? lat = null, double? lng = null)
    {
        return new {id, lat, lng};
    }

    public HotelService(HotelContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<Hotel>> GetHotels(Guid? id = null)
    {
        var items = await _context.Database.GetDbConnection()
            .QueryAsync<Hotel>(_selectSql, getSelectSqlParams(id: id));
        return items.ToList();
    }

    public async Task<Hotel> GetHotelById(Guid id)
    {
        var hotel = await _context.Database.GetDbConnection().QueryFirstOrDefaultAsync<Hotel>(_selectSql, getSelectSqlParams(id: id));

        return hotel;
    }

    public async Task<Hotel> AddHotel(Hotel hotel)
    {
        var newHotel = _context.Hotels.Add(hotel).Entity;
        await _context.SaveChangesAsync();

        return newHotel;
    }

    public async Task<Hotel> UpdateHotel(Hotel hotel)
    {
        var newHotel = _context.Hotels.Update(hotel).Entity;
        await _context.SaveChangesAsync();

        return newHotel;
    }

    public async Task DeleteHotel(Hotel hotel)
    {
        _context.Hotels.Remove(hotel);
        await _context.SaveChangesAsync();
    }

    public async Task<HotelResponse> GetHotelApi(Guid id)
    {
        var hotel = await GetHotelById(id);
        var result = _mapper.Map<HotelResponse>(hotel);
        return result;
    }

    public async Task<PagedApiResponse<HotelResponse>> GetHotelsApi(RequestParameters requestParameters = null,
        double? lat = null, double? lng = null)
    {
        var hotels = await PagedApiResponse<Hotel>.GetFromSql(_context, _selectSql, getSelectSqlParams(lat: lat, lng: lng), requestParameters);

        var result = new PagedApiResponse<HotelResponse>
        {
            Meta = hotels.Meta, Data = hotels.Data.Select(e => _mapper.Map<HotelResponse>(e)).ToList()
        };

        return result;
    }
}
