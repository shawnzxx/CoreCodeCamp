﻿using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    //indicate we really use below action as an actual API(not view or HTML page)
    //like auto impletmened body binding from post request (For the past we were using [FromBody])
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this._campRepository = campRepository;
            this._mapper = mapper;
            this._linkGenerator = linkGenerator;
        }

        //IActionResult is interface have a lot of pre-defined return result can use: https://exceptionnotfound.net/asp-net-core-demystified-action-results/
        //whereas ActionResult<T> is specific return type need to return: https://joonasw.net/view/aspnet-core-2-1-actionresult-of-t
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

        [HttpPost]
        public async Task<ActionResult<CampModel>> Post(CampModel model) {
            try
            {
                //check for dupe on business logic, we can also set on database level unique constrain
                var excisting = await _campRepository.GetCampAsync(model.Moniker);
                if (excisting != null) {
                    return BadRequest("Moniker is in use");
                }

                var url = _linkGenerator.GetPathByAction("GetCampByMoniker", "Camps", values: new { moniker = model.Moniker });
                if (string.IsNullOrWhiteSpace(url)) {
                    return BadRequest("Could not use current moniker");
                }
                
                //Create new camp
                var camp = _mapper.Map<Camp>(model);
                _campRepository.Add(camp);

                if (await _campRepository.SaveChangesAsync()) {
                    return Created(url, _mapper.Map<CampModel>(camp));
                }
                else
                {
                    return BadRequest("Failed to save new camp");
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> UpdateCamp(string moniker, CampModel model) {
            try
            {
                var oldCamp = await _campRepository.GetCampAsync(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find camp with moniker of {moniker}");
                }

                _mapper.Map(model, oldCamp);

                await _campRepository.SaveChangesAsync();
                
                return Ok(_mapper.Map<CampModel>(oldCamp));
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }

        [HttpDelete("{moniker}")]
        //no need to return payload, so IActionResult no type defination
        public async Task<IActionResult> DeleteCamp(string moniker) {
            try
            {
                var oldCamp = await _campRepository.GetCampAsync(moniker);
                if (oldCamp == null) {
                    return NotFound($"Could not find camp with moniker of {moniker}");
                }

                _campRepository.Delete(oldCamp);

                if (await _campRepository.SaveChangesAsync()) {
                    return Ok();
                }

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
            return BadRequest();
        }
    }
}
