﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ALS.EntityСontext;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ALS.Pages
{
    public class LaboratoryWorksModel : PageModel
    {
        private ApplicationContext _context;
        public List<LaboratoryWork> LaboratoryWorks;
        public List<SelectListItem> Themes;
        public List<SelectListItem> Disciplines;
        public List<SelectListItem> TemplateLWs;
        public List<SelectListItem> Evaluations = new List<SelectListItem>();

        public LaboratoryWorksModel(ApplicationContext context)
        {
            _context = context;
            // руссификация Evaluation
            var folderDetails = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "EvaluationRU.json");
            var json = System.IO.File.ReadAllText(folderDetails);
            var evaluationsRU = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            foreach (Evaluation item in Enum.GetValues(typeof(Evaluation)))
            {
                Evaluations.Add(new SelectListItem { Value = ((int)item).ToString(), Text = evaluationsRU[item.ToString()] });
            }
        }

        public async Task OnGetAsync()
        {
            LaboratoryWorks = await Task.Run( () => _context.LaboratoryWorks
                                            .Include(x => x.Theme)
                                            .Include(x => x.User)
                                            .Include(x => x.TemplateLaboratoryWork)
                                            .Where( _ => true)
                                            .OrderBy( x => x.LaboratoryWorkId).ToList());

            Themes = await Task.Run(() => _context.Themes.Select(x =>
                     new SelectListItem
                     {
                         Value = x.ThemeId.ToString(),
                         Text = x.Name
                     }).ToList()
                 );
            Disciplines = await Task.Run(() => _context.Disciplines.Select(x =>
                     new SelectListItem
                     {
                         Value = x.Cipher,
                         Text = x.Name
                     }).ToList()
                 );
            TemplateLWs = await Task.Run(() => _context.TemplateLaboratoryWorks.Select(x =>
                     new SelectListItem
                     {
                         Value = x.TemplateLaboratoryWorkId.ToString(),
                         Text = $"#{x.TemplateLaboratoryWorkId}  {x.TemplateTask.Substring(x.TemplateTask.LastIndexOf('/') + 1)}"
                     }).ToList()
                 );
            TemplateLWs.Add(new SelectListItem
            {
                Value = "-1",
                Text = "-"
            });
        }
    }
}