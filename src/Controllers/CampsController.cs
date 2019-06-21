using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;

        public CampsController(ICampRepository campRepository, IMapper mapper)
        {
            this._campRepository = campRepository;
            this._mapper = mapper;
        }

        //return action from this method, action could be success or failed, here is status code coming from
        //[HttpGet]
        //public async Task<IActionResult> GetCamps() {
        //    try
        //    {
        //        var results = await _campRepository.GetAllCampsAsync();

        //        CampModel[] models = this._mapper.Map<CampModel[]>(results);

        //        return Ok(models);
        //    }
        //    catch (Exception)
        //    {
        //        return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
        //    }
        //}

        //we code refactoring from IActionResult to ActionResult<T>, return templated version
        [HttpGet]
        //query string includeTalks default value = false means may or may not excist
        public async Task<ActionResult<CampModel[]>> GetCamps(bool includeTalks = false)
        {
            try
            {
                var results = await _campRepository.GetAllCampsAsync(includeTalks);

                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }

        //detail route explaination
        //https://docs.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2
        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> GetCampByMoniker(string moniker) {
            try
            {
                var result = await _campRepository.GetCampAsync(moniker);

                if (result == null) {
                    return NotFound();
                }

                return _mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }

        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchCampByDate(DateTime theDate, bool includeTalks = false) {
            try
            {
                var result = await _campRepository.GetAllCampsByEventDate(theDate, includeTalks);
                if (!result.Any()) {
                    return NotFound();
                }
                return _mapper.Map<CampModel[]>(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }
    }
}
