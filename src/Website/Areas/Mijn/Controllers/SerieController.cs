﻿using PrekenWeb.Data;
using PrekenWeb.Data.Identity;
using PrekenWeb.Data.Tables;
using Prekenweb.Website.Areas.Mijn.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Prekenweb.Website.Lib;

namespace Prekenweb.Website.Areas.Mijn.Controllers
{
    [Authorize(Roles = "Stamgegevens")]
    public class SerieController : Controller
    {
        private readonly IPrekenwebContext<Gebruiker> _context;

        public SerieController(IPrekenwebContext<Gebruiker> context)
        {
            _context = context;
        } 

        public ActionResult Index()
        {
            return View(new SerieIndexViewModel
            {
                Series = _context.Series.Where(s => s.TaalId == TaalInfoHelper.FromRouteData(RouteData).Id).OrderBy(s => s.Omschrijving).ToList()
            });
        }

        public ActionResult Verwijder(int id)
        {
            var serie = _context.Series.Single(s => s.Id == id);
            _context.Series.Remove(serie);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Bewerk(int id)
        {
            var serie = _context.Series.Single(s => s.Id == id);
            return View(new SerieEditViewModel
            {
                Serie = serie
            });
        }

        [HttpPost]
        public ActionResult Bewerk(SerieEditViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            _context.Entry(viewModel.Serie).State = EntityState.Modified;
            _context.SaveChanges();
            
            return View(viewModel);
        }

        public ActionResult Maak()
        {
            return View(new SerieEditViewModel
            {
                Serie = new Serie { TaalId = TaalInfoHelper.FromRouteData(RouteData).Id }
            });
        }

        [HttpPost]
        public ActionResult Maak(SerieEditViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);
            
            _context.Series.Add(viewModel.Serie);
            _context.SaveChanges();
            
            return RedirectToAction("Bewerk", new { viewModel.Serie.Id });
        }
    }
}
