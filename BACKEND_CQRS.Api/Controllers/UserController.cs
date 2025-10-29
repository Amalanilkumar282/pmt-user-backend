using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Query.User;
using BACKEND_CQRS.Application.Query.Users;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet("all")]
        public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
        {
            var query = new GetAllUsersQuery();
            var result = await _mediator.Send(query);


            return Ok(result);
        }

        [HttpGet("{userId}/activities")]
        public async Task<ApiResponse<List<ActivityLogDto>>> GetUserActivities(
            [FromRoute] int userId,
            [FromQuery] int take = 50)
        {
            var query = new GetUserActivitiesQuery(userId, take);
            return await _mediator.Send(query);
        }
    }
}
