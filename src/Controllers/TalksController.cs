using AutoMapper;
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
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public TalksController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this._campRepository = campRepository;
            this._mapper = mapper;
            this._linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> GetTalks(string moniker) {
            try
            {
                var talks = await _campRepository.GetTalksByMonikerAsync(moniker, true);
                return _mapper.Map<TalkModel[]>(talks);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> GetTalkById(string moniker, int id) {
            try
            {
                var talk = await _campRepository.GetTalkByMonikerAsync(moniker, id, true);
                return _mapper.Map<TalkModel>(talk);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model) {
            try
            {
                var camp = await _campRepository.GetCampAsync(moniker);
                if (camp == null) {
                    return BadRequest("camp does not exist");
                }

                var talk = _mapper.Map<Talk>(model);
                talk.Camp = camp;
                if (model.Speaker == null) {
                    return BadRequest("Speaker ID is required");
                }

                var speaker = await _campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) {
                    return BadRequest("Speaker could not be found");
                }
                talk.Speaker = speaker;

                _campRepository.Add(talk);
                if (await _campRepository.SaveChangesAsync())
                {
                    var url = _linkGenerator.GetPathByAction(HttpContext,
                            "GetTalkById",
                            values: new { moniker, id = talk.TalkId });
                    return Created(url, _mapper.Map<TalkModel>(talk));
                }
                else {
                    return BadRequest("Failed to save new talk");
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id, TalkModel model) {
            try
            {
                var talk = await _campRepository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null) {
                    return NotFound("Couldn't find the talk");
                }

                _mapper.Map(model, talk);

                if (model.Speaker != null) {
                    var speaker = await _campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null) {
                        talk.Speaker = speaker;
                    }
                }

                if (await _campRepository.SaveChangesAsync())
                {
                    return _mapper.Map<TalkModel>(talk);
                }
                else {
                    return BadRequest("Failed to update talk");
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failed");
            }
        }
    }
}
