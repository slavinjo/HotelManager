using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Hotel.Server.Helpers;
using Hotel.Server.Users;

namespace Hotel.Server.Hotels;

[ApiController]
[Route("/api/v1/hotels")]
public class HotelController : ControllerBaseExtended
{
    private readonly HotelService _hotelService;
    private readonly DateTimeProvider _dateTimeProvider;
    private readonly IMapper _mapper;

    public HotelController(HotelService hotelService, DateTimeProvider dateTimeProvider, IMapper mapper)
    {
        _hotelService = hotelService;
        _dateTimeProvider = dateTimeProvider;
        _mapper = mapper;
    }

    [HttpGet]
    [PermissionLevel(UserRole.Any)]
    public async Task<ActionResult<PagedApiResponse<HotelResponse>>> GetAll(
        [FromQuery] double? lat, [FromQuery] double? lng,
        [FromQuery] RequestParameters requestParams)
    {
        var hotels = await _hotelService.GetHotelsApi(requestParams, lat, lng);
        return Ok(hotels);
    }

    [HttpGet("{id}")]
    [PermissionLevel(UserRole.Any)]
    public async Task<ActionResult<HotelResponse>> GetById(Guid id)
    {
        var hotel = await _hotelService.GetHotelApi(id) ?? throw new NotFoundException();

        return Ok(hotel);
    }

    [HttpPost]
    [PermissionLevel(UserRole.Any)]
    public async Task<ActionResult<HotelResponse>> Add([FromBody] HotelAddRequest model)
    {
        var hotel = _mapper.Map<Hotel>(model);
        await _hotelService.AddHotel(hotel);

        var result = await _hotelService.GetHotelApi(hotel.Id);
        return Created("", result);
    }

    [HttpPut("{id}")]
    [PermissionLevel(UserRole.Any)]
    public async Task<ActionResult<HotelResponse>> Update([FromBody] HotelUpdateRequest model, Guid id)
    {
        var hotel = await _hotelService.GetHotelById(id) ?? throw new NotFoundException();

        _mapper.Map(model, hotel);
        await _hotelService.UpdateHotel(hotel);

        var result = await _hotelService.GetHotelApi(hotel.Id);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [PermissionLevel(UserRole.Any)]
    public async Task<ActionResult<HotelResponse>> Delete(Guid id)
    {
        var hotel = await _hotelService.GetHotelById(id) ?? throw new NotFoundException();

        var result = await _hotelService.GetHotelApi(hotel.Id);
        await _hotelService.DeleteHotel(hotel);

        return Ok(result);
    }
}
