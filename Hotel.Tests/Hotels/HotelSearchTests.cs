using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Hotel.Server.Helpers;
using Hotel.Server.Hotels;
using Xunit;

namespace Hotel.Tests.Hotels;

[Collection("Api")]
public class HotelSearchTests
{
    private readonly ApiFixture _api;

    public HotelSearchTests(ApiFixture api)
    {
        _api = api;
    }

    [Fact]
    public async void Hotel_Search_Success()
    {
        var (_, headers) = await _api.CreateUser();

        // to filter only hotels from this test run and avoid flaky tests
        var searchToken = Guid.NewGuid().ToString();
        
        // create hotels 
        var hotelAddData = new HotelAddRequest
        {
            Name = $"Test {searchToken} {Guid.NewGuid()}", Price = 100, GeoLat = 45.781322, GeoLng = 15.986307,
        };

        var hotelClose = await _api.Request<HotelResponse>("/api/v1/hotels", HttpMethod.Post,
            headers, hotelAddData, HttpStatusCode.Created);
        
        hotelAddData.GeoLat = 46.781322;
        hotelAddData.Price = 50;
        
        var hotelMediumCheap = await _api.Request<HotelResponse>("/api/v1/hotels", HttpMethod.Post,
            headers, hotelAddData, HttpStatusCode.Created);
        
        hotelAddData.GeoLat = 46.781322;
        hotelAddData.Price = 150;
        
        var hotelMediumExpensive = await _api.Request<HotelResponse>("/api/v1/hotels", HttpMethod.Post,
            headers, hotelAddData, HttpStatusCode.Created);

        hotelAddData.GeoLat = 50.781322;
        hotelAddData.Price = 50;
        
        var hotelFar = await _api.Request<HotelResponse>("/api/v1/hotels", HttpMethod.Post,
            headers, hotelAddData, HttpStatusCode.Created);
        
        // search hotels from 45.781322, 15.986307 coords
        var hotelList = await _api.Request<PagedApiResponse<HotelResponse>>(
            $"/api/v1/hotels?pageSize=999999&filter=name[like]={searchToken}&sortBy=distance,price&lat=45.781322&lng=15.986307", HttpMethod.Get, headers,
            null, HttpStatusCode.OK);

        Assert.Equal(4, hotelList.Data.Count);
        
        Assert.Equal(hotelClose.Id, hotelList.Data[0].Id);
        Assert.Equal(hotelMediumCheap.Id, hotelList.Data[1].Id);
        Assert.Equal(hotelMediumExpensive.Id, hotelList.Data[2].Id);
        Assert.Equal(hotelFar.Id, hotelList.Data[3].Id);
        
        // search hotels from 60.781322, 15.986307 coords
        hotelList = await _api.Request<PagedApiResponse<HotelResponse>>(
            $"/api/v1/hotels?pageSize=999999&filter=name[like]={searchToken}&sortBy=distance,price&lat=60.781322&lng=15.986307", HttpMethod.Get, headers,
            null, HttpStatusCode.OK);

        Assert.Equal(4, hotelList.Data.Count);
        
        Assert.Equal(hotelFar.Id, hotelList.Data[0].Id);
        Assert.Equal(hotelMediumCheap.Id, hotelList.Data[1].Id);
        Assert.Equal(hotelMediumExpensive.Id, hotelList.Data[2].Id);
        Assert.Equal(hotelClose.Id, hotelList.Data[3].Id);
    }
}
