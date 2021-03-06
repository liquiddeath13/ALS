﻿using System;
using System.Linq;
using System.Threading.Tasks;
using ALS.EntityСontext;
using Generator.MainGen;
using Generator.MainGen.Structs;
using Generator.Parsing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ALS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Teacher")]
    public class GeneratorController : Controller
    {
        private Gen _gen;
        private ApplicationContext _db;

        public GeneratorController(ApplicationContext db)//, IParser pr, IParamsContainer paramsContainer)
        {
            _db = db;
            _gen = new Gen();
        }

        // Подумать над вариантом - откуда брать
        [HttpPost]
        public async Task<IActionResult> GenNewTask([FromHeader]int lrId, [FromHeader]int var)
        {
            var TemplateLaboratoryWorkId = _db.LaboratoryWorks.FirstOrDefault(lr => lr.LaboratoryWorkId == lrId).TemplateLaboratoryWorkId;
            if (TemplateLaboratoryWorkId == null) return BadRequest("TemplateLaboratoryWorkId is null");

            var tlwPath = _db.TemplateLaboratoryWorks.FirstOrDefault(twl => twl.TemplateLaboratoryWorkId == TemplateLaboratoryWorkId).TemplateTask;
            if (tlwPath == null) return BadRequest("TemplateLaboratoryWorkPath is null");

            try
            {
                var res = await _gen.Run(new Uri(tlwPath).AbsolutePath, lrId, var, true);
                //if (res == null) return BadRequest("Result of generation is null");

                _db.Variants.Add(new Variant { LaboratoryWorkId = lrId, VariantNumber = var, Description = res.task, LinkToModel = res.code, InputDataRuns = res.tests });
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok("New variant has created!");
        }
    }
}
