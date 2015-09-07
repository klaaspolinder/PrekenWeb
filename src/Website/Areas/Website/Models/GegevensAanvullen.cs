﻿using PrekenWeb.Data.Tables;
using PrekenWeb.Data.ViewModels;
using Prekenweb.Models;
using System.ComponentModel.DataAnnotations;

namespace Prekenweb.Website.ViewModels
{
    public class GegevensAanvullen
    {
        public TekstPagina TekstPagina { get; set; }
        public TekstPagina TekstPaginaCompleet { get; set; }

        [Display(Name = "Naam", ResourceType = typeof(Resources.Resources))]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "NaamIsVerplicht", ErrorMessageResourceType = typeof(Resources.Resources))]
        public string Naam { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email", ResourceType = typeof(Resources.Resources))]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "EmailIsVerplicht", ErrorMessageResourceType = typeof(Resources.Resources))]
        public string Email { get; set; }

        [Display(Name = "Onderwerp", ResourceType = typeof(Resources.Resources)) ]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "OnderwerpIsVerplicht", ErrorMessageResourceType = typeof(Resources.Resources))]
        public string Onderwerp { get; set; }

        public int PreekId { get; set; }

        [Display(Name = "Preek", ResourceType = typeof(Resources.Resources))]
        public Preek Preek { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Aanvulling", ResourceType = typeof(Resources.Resources))]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = "AanvullingIsVerplicht", ErrorMessageResourceType = typeof(Resources.Resources))]
        public string Aanvulling { get; set; }

        public bool Verzonden { get; set; }

    }
}
