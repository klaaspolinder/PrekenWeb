﻿using Prekenweb.Models;
using Prekenweb.Models.Identity;
using Prekenweb.Models.Repository;
using PrekenWeb.Security;
using Prekenweb.Website.Controllers;
using Prekenweb.Website.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using System.Web.UI;
using Prekenweb.Website.Properties;
using Prekenweb.Website.ViewModels;

namespace Prekenweb.Website.Areas.Website.Controllers
{
    public class HomeController : ApplicationController
    {
        private readonly IMailingRepository _mailingRepository;
        private readonly ITekstRepository _tekstRepository;
        private readonly IPrekenRepository _prekenRepository;
        private readonly ISpotlightRepository _spotlightRepository;

        private readonly IPrekenwebCache _cache;
        private readonly IPrekenWebUserManager _prekenWebUserManager;
        private readonly IHuidigeGebruiker _huidigeGebruiker;

        public HomeController(IMailingRepository mailingRepository,
                              ITekstRepository tekstRepository,
                              IPrekenRepository prekenRepository,
                              ISpotlightRepository spotlightRepository,
                              IPrekenwebCache cache,
                              IPrekenWebUserManager prekenWebUserManager,
                              IHuidigeGebruiker huidigeGebruiker)
        {
            _mailingRepository = mailingRepository;
            _tekstRepository = tekstRepository;
            _prekenRepository = prekenRepository;
            _spotlightRepository = spotlightRepository;
            _cache = cache;
            _prekenWebUserManager = prekenWebUserManager;
            _huidigeGebruiker = huidigeGebruiker;

            ViewBag.Taalkeuze = true;
        }

        [OutputCache(Duration = 1800, VaryByCustom="user")] // 30 minuten
        public async Task<ActionResult> Index()
        {
            return View(await GetHomeIndexViewModel());
        }

        private async Task<HomeIndex> GetHomeIndexViewModel()
        {
            var viewModel = new HomeIndex
            {
                NieuwePreken = await GetNieuwPreken(new NieuwePreken()),
                Teksten = _cache.GetCached("HomePageTeksten", TaalId, User.Identity.Name, () => _tekstRepository.GetHomepageTeksten(TaalId)),
                SpotlightItems = _cache.GetCached("SpotlightItems", TaalId, User.Identity.Name, () => _spotlightRepository.GetSpotlightItemsForHomepage(TaalId)),
                WelkomsTekst = _cache.GetCached("WelkomsTekst", TaalId, User.Identity.Name, () => _tekstRepository.GetTekstPagina("home-welkom", TaalId)),
                Taal = Taal
            };
            return viewModel;
        }

        private async Task<NieuwePreken> GetNieuwPreken(NieuwePreken nieuwePreken)
        {
            var preekTypIds = new List<int>();
            if (nieuwePreken.AudioPreken) preekTypIds.Add((int)PreekTypeEnum.Peek);
            if (nieuwePreken.LeesPreken) preekTypIds.Add((int)PreekTypeEnum.LeesPreek);
            if (nieuwePreken.Lezingen) preekTypIds.Add((int)PreekTypeEnum.Lezing);

            var cacheKey = string.Format("{0}.{1}", string.Join(".", preekTypIds), "NieuwePreken");

            nieuwePreken.Preken = await _cache.GetCached(cacheKey, TaalId, User.Identity.Name, async () => await _prekenRepository.GetNieuwePreken(preekTypIds, TaalId, _huidigeGebruiker.Id));
            return nieuwePreken;
        }

        [OutputCache(Duration = 1209600, VaryByParam = "Id", Location = OutputCacheLocation.ServerAndClient)] // 14 dagen
        public ActionResult HomepageImage(int id)
        {
            var afbeelding = _spotlightRepository.GetAfbeelding(id);
            var rootFolder = Settings.Default.AfbeeldingenFolder;
            var filename = Server.MapPath(string.Format(@"{0}\Homepage_{1}.jpg", rootFolder, afbeelding.Id));
            try
            {
                return File(System.IO.File.ReadAllBytes(filename), "image/jpeg", afbeelding.Bestandsnaam);
            }
            catch //(FileNotFoundException ex)
            {
                return File(System.IO.File.ReadAllBytes(Server.MapPath("~/Content/Images/DefaultHomeImage.jpg")), "image/jpeg", "DefaultImage.jpg");
            }
        }

        public ActionResult InschrijvenNieuwsbrief()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> InschrijvenNieuwsbrief(HomeIndex viewModel)
        {
            var defaultHomeIndexViewModel = await GetHomeIndexViewModel();
            if (!ModelState.IsValid)
            {
                viewModel.InschrijvenNieuwsbriefForm = defaultHomeIndexViewModel.InschrijvenNieuwsbriefForm;
                return View("Index", defaultHomeIndexViewModel);
            }
            var gebruiker = await _prekenWebUserManager.FindByEmailAsync(viewModel.InschrijvenNieuwsbriefForm.Email);
            var mailing = (await _mailingRepository.GetAlleMailings()).FirstOrDefault(x => x.TaalId == TaalId);

            if (gebruiker == null && mailing != null)
            {
                // nieuwe gebruiker aanmaken met 1 inschrijving

                try
                {
                    MailChimpController.Subscribe(viewModel.InschrijvenNieuwsbriefForm.Email, viewModel.InschrijvenNieuwsbriefForm.Naam, mailing.MailChimpId);

                    var nieuweGebruiker = new Gebruiker
                    {
                        UserName = viewModel.InschrijvenNieuwsbriefForm.Email,
                        Email = viewModel.InschrijvenNieuwsbriefForm.Email,
                        Naam = viewModel.InschrijvenNieuwsbriefForm.Naam,
                        LaatstIngelogd = DateTime.Now
                    };

                    var result = await _prekenWebUserManager.CreateAsync(nieuweGebruiker);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("InschrijvenNieuwsbriefForm.Email", string.Join(Environment.NewLine, result.Errors));
                    }
                    await _mailingRepository.NieuwsbriefToevoegenAanGebruiker(nieuweGebruiker.Id, mailing.Id);
                }
                catch
                {
                    ModelState.AddModelError("InschrijvenNieuwsbriefForm.Email", "Unable to subscribe, please report this to us by sending us an email");
                    viewModel.InschrijvenNieuwsbriefForm = defaultHomeIndexViewModel.InschrijvenNieuwsbriefForm;
                    return View("Index", defaultHomeIndexViewModel);
                }
            }
            else if (gebruiker != null && mailing != null)
            {
                // bestaande gebruiker

                if (gebruiker.Mailings.Any(x => x.Id == mailing.Id))
                {
                    // al ingeschreven!

                    ModelState.AddModelError("InschrijvenNieuwsbriefForm.Email", "U bent al ingeschreven!");
                    viewModel.InschrijvenNieuwsbriefForm = defaultHomeIndexViewModel.InschrijvenNieuwsbriefForm;
                    return View("Index", defaultHomeIndexViewModel);
                }

                // nog niet ingeschreven
                try
                {
                    MailChimpController.Subscribe(viewModel.InschrijvenNieuwsbriefForm.Email, viewModel.InschrijvenNieuwsbriefForm.Naam, mailing.MailChimpId);
                    await _mailingRepository.NieuwsbriefToevoegenAanGebruiker(gebruiker.Id, mailing.Id);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("InschrijvenNieuwsbriefForm.Email", string.Format("Unable to subscribe, please report this to us by sending us an email ({0})", ex.Message));
                    viewModel.InschrijvenNieuwsbriefForm = defaultHomeIndexViewModel.InschrijvenNieuwsbriefForm;
                    return View("Index", defaultHomeIndexViewModel);
                }
            }
            return RedirectToRoute("TekstPagina", new { pagina = "InschrijvenNieuwsbrief" });
        }

        public async Task<ActionResult> NieuwePreken(NieuwePreken nieuwePreken)
        {
            return PartialView(await GetNieuwPreken(nieuwePreken));
        }

        [Obsolete("Old URL known by Apple")] //todo update URL @ Apple :)
        public ActionResult ITunesPodcast()
        {
            return RedirectToActionPermanent("ITunesPodcast", "Feed");
        }

        [Obsolete("Moved to FeedController, still intact for backwards compatibility")]
        public ActionResult RssFeed(Guid id)
        {
            return RedirectToActionPermanent("RssFeed", "Feed", new { id });
        }
    }
}
