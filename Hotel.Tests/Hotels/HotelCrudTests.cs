using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Hotel.Server.Helpers;
using Hotel.Server.Hotels;
using Xunit;

namespace Hotel.Tests.Hotels;

[Collection("Api")]
public class HotelCrudTests
{
    private readonly ApiFixture _api;

    public HotelCrudTests(ApiFixture api)
    {
        _api = api;
    }

    [Fact]
    public async void Hotel_Crud_Success()
    {
        var (_, headers) = await _api.CreateUser();

        // create a new hotel 
        var hotelAddData = new HotelAddRequest
        {
            Name = $"Test  {Guid.NewGuid()}", Price = 100, GeoLat = 45.781322, GeoLng = 15.986307,
        };

        var result = await _api.Request<HotelResponse>("/api/v1/hotels", HttpMethod.Post,
            headers, hotelAddData, HttpStatusCode.Created);

        Assert.Equal(hotelAddData.Name, result.Name);

        var hotelId = result.Id;

        // update hotel 
        var hotelUpdateData = new HotelUpdateRequest
        {
            Name = $"Some other  name {Guid.NewGuid()}", Price = 200, GeoLat = 55.781322, GeoLng = 25.986307,
        };

        await _api.Request<HotelResponse>($"/api/v1/hotels/{hotelId}", HttpMethod.Put, headers,
            hotelUpdateData, HttpStatusCode.OK);

        // get hotel  by id
        var hotel = await _api.Request<HotelResponse>($"/api/v1/hotels/{hotelId}", HttpMethod.Get,
            headers,
            null, HttpStatusCode.OK);

        Assert.Equal(hotelUpdateData.Name, hotel.Name);

        // find hotel  in all hotel categories list
        var hotelList = await _api.Request<PagedApiResponse<HotelResponse>>(
            "/api/v1/hotels?pageSize=999999", HttpMethod.Get, headers,
            null, HttpStatusCode.OK);

        Assert.NotNull(hotelList.Data.FirstOrDefault(e => e.Id == hotelId));

        // delete hotel 
        await _api.Request<HotelResponse>($"/api/v1/hotels/{hotelId}", HttpMethod.Delete, headers,
            null, HttpStatusCode.OK);

        // fail to get hotel  by id
        await _api.Request<HotelResponse>($"/api/v1/hotels/{hotelId}", HttpMethod.Get, headers,
            null, HttpStatusCode.NotFound);
    }
}
